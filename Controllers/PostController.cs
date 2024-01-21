using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using WebsiteBlogCMS.Classes;
using WebsiteBlogCMS.Data;
using WebsiteBlogCMS.Models;
using static WebsiteBlogCMS.Classes.DataHelper;
using static WebsiteBlogCMS.Classes.Enums;

namespace WebsiteBlogCMS.Controllers
{
    public class PostController : Controller
    {
        public ActionResult PostsOfCategory(int id)
        {
            using (var ctx = DbHelper.DataContext)
            {
                Category category = CategoryUtils.GetCategory(ctx, id);

                if (category == null)
                {
                    return View("HttpNotFound");
                }

                List<Post> posts = PostUtils.GetPostsByCategory(ctx, category);
                ViewBag.Title = category.title;
                return View(posts);
            }
        }

        public ActionResult PostsOfTag(int id)
        {
            using (var ctx = DbHelper.DataContext)
            {
                Tag tag = TagUtils.GetTag(ctx, id);

                if (tag == null)
                {
                    return View("HttpNotFound");
                }

                List<Post> posts = PostUtils.GetPostsByTag(ctx, tag);
                ViewBag.Title = tag.title;
                return View(posts);
            }
        }

        public ActionResult Details(int id)
        {
            using (var ctx = DbHelper.DataContext)
            {
                Post post = PostUtils.GetPost(ctx, id);

                if (post == null)
                {
                    return View("HttpNotFound");
                }

                ViewBag.Categories = CategoryUtils.GetRootCategories(ctx);
                ViewBag.UserPosts = PostUtils.GetUserPosts(ctx, post.authorId);
                return View(post);
            }
        }

        [Authorize]
        public ActionResult Confirm(int id)
        {
            if (!UserUtils.HasPermission(2, User.Identity.Name))
            {
                TempData[Message(MessageType.Warning)] = "Nie posiadasz uprawnień do korzystania z tego widoku.";
                return RedirectToAction("Index", "Admin");
            }

            using (var ctx = DbHelper.DataContext)
            {
                Post post = PostUtils.GetPost(ctx, id);

                if (post == null)
                {
                    return View("HttpNotFound");
                }

                post.published = true;
                post.publishedAt = DateTime.Now;
                post.isVisible = true;
                ctx.SubmitChanges();

                TempData[Message(MessageType.Success)] = "Pomyślnie opublikowano post.";
                return RedirectToAction("Posts", "Admin");
            }
        }

        [Authorize]
        public ActionResult Preview(int id)
        {
            if (!UserUtils.HasPermission(3, User.Identity.Name))
            {
                TempData[Message(MessageType.Warning)] = "Nie posiadasz uprawnień do korzystania z tego widoku.";
                return RedirectToAction("Index", "Admin");
            }

            using (var ctx = DbHelper.DataContext)
            {
                Post post = PostUtils.GetPost(ctx, id);

                User me = UserUtils.GetUser(ctx, User.Identity.Name);
                User author = UserUtils.GetUser(ctx, post.authorId);
                if (!(me.id == author.id || me.roleId < 3))
                {
                    TempData[Message(MessageType.Warning)] = "Nie posiadasz uprawnień do wyświetlenia tego artykułu.";
                    return RedirectToAction("Index", "Admin");
                }

                ViewBag.Categories = PostUtils.GetPostCategories(ctx, id);
                ViewBag.Tags = PostUtils.GetPostTags(ctx, id);

                return View(post);
            }
        }

        [Authorize]
        public ActionResult Create()
        {
            if (!UserUtils.HasPermission(3, User.Identity.Name))
            {
                TempData[Message(MessageType.Warning)] = "Nie posiadasz uprawnień do korzystania z tego widoku.";
                return RedirectToAction("Index", "Admin");
            }

            using (var ctx = DbHelper.DataContext)
            {
                var jsonSettings = new Newtonsoft.Json.JsonSerializerSettings
                {
                    ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore
                };

                var categories = ctx.Categories.Select(c => new DropDownItem
                (
                    c.title,
                    c.id,
                    false
                ));
                ViewBag.Categories = Newtonsoft.Json.JsonConvert.SerializeObject(categories, jsonSettings);

                var tags = ctx.Tags.Select(t => new DropDownItem
                (
                    t.title,
                    t.id,
                    false
                ));
                ViewBag.Tags = Newtonsoft.Json.JsonConvert.SerializeObject(tags, jsonSettings);

                return View();
            }
        }

        [HttpPost]
        [ValidateInput(false)]
        [Authorize]
        public ActionResult Create(Post post, string selectedCategories, string selectedTags)
        {
            if (!ModelState.IsValid)
            {
                return View("HttpNotFound");
            }

            using (var ctx = DbHelper.DataContext)
            {
                if (!PostUtils.IsPostValid(ctx, post))
                {
                    TempData[Message(MessageType.Warning)] = "Post o takim tytule już istnieje.";
                    return RedirectToAction("Posts", "Admin");
                }

                using (var transaction = new System.Transactions.TransactionScope())
                {
                    User user = UserUtils.GetUser(ctx, User.Identity.Name);
                    post.authorId = user.id;
                    post.createdAt = DateTime.Now;

                    ctx.Posts.InsertOnSubmit(post);
                    ctx.SubmitChanges();

                    Post data = PostUtils.GetPost(ctx, post.title);

                    if (!string.IsNullOrEmpty(selectedCategories))
                    {
                        foreach (var id in selectedCategories.Split(',').Select(x => int.Parse(x)))
                        {
                            PostCategory postCategory = new PostCategory();
                            postCategory.postId = data.id;
                            postCategory.categoryId = id;
                            ctx.PostCategories.InsertOnSubmit(postCategory);
                        }
                    }

                    if (!string.IsNullOrEmpty(selectedTags))
                    {
                        foreach (var id in selectedTags.Split(',').Select(x => int.Parse(x)))
                        {
                            PostTag postTag = new PostTag();
                            postTag.postId = data.id;
                            postTag.tagId = id;
                            ctx.PostTags.InsertOnSubmit(postTag);
                        }
                    }

                    ctx.SubmitChanges();
                    transaction.Complete();

                    TempData[Message(MessageType.Success)] = "Pomyślnie utworzono post.";
                    return RedirectToAction("Posts", "Admin");
                }
            }
        }

        [Authorize]
        public ActionResult Edit(int id)
        {
            if (!UserUtils.HasPermission(3, User.Identity.Name))
            {
                TempData[Message(MessageType.Warning)] = "Nie posiadasz uprawnień do korzystania z tego widoku.";
                return RedirectToAction("Index", "Admin");
            }

            using (var ctx = DbHelper.DataContext)
            {
                Post data = PostUtils.GetPost(ctx, id);

                if (data == null)
                {
                    return HttpNotFound();
                }

                User me = UserUtils.GetUser(ctx, User.Identity.Name);
                User author = UserUtils.GetUser(ctx, data.authorId);
                if (!(me.id == author.id || me.roleId < 3))
                {
                    TempData[Message(MessageType.Warning)] = "Nie posiadasz uprawnień do edycji tego artykułu.";
                    return RedirectToAction("Index", "Admin");
                }

                var jsonSettings = new Newtonsoft.Json.JsonSerializerSettings
                {
                    ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore
                };

                var categories = ctx.Categories.Select(c => new DropDownItem
                (
                    c.title,
                    c.id,
                    ctx.PostCategories
                        .Where(x => x.postId.Equals(id) && x.categoryId.Equals(c.id))
                        .Select(x => x.categoryId).Any()
                ));
                ViewBag.Categories = Newtonsoft.Json.JsonConvert.SerializeObject(categories, jsonSettings);

                var tags = ctx.Tags.Select(t => new DropDownItem
                (
                    t.title,
                    t.id,
                    ctx.PostTags
                        .Where(x => x.postId.Equals(id) && x.tagId.Equals(t.id))
                        .Select(x => x.tagId).Any()
                ));
                ViewBag.Tags = Newtonsoft.Json.JsonConvert.SerializeObject(tags, jsonSettings);

                var content = ctx.Posts.Where(p => p.id.Equals(id)).Select(p => p.content).SingleOrDefault();
                ViewBag.Content = Newtonsoft.Json.JsonConvert.SerializeObject(content, jsonSettings);

                return View(data);
            }
        }

        [HttpPost]
        [ValidateInput(false)]
        [Authorize]
        public ActionResult Edit(Post post, string selectedCategories, string selectedTags)
        {
            if (!ModelState.IsValid)
            {
                return View("HttpNotFound");
            }

            using (var ctx = DbHelper.DataContext)
            {
                if (!PostUtils.IsPostValid(ctx, post))
                {
                    TempData[Message(MessageType.Warning)] = "Post o takim tytule już istnieje.";
                    return View("HttpNotFound");
                }

                Post data = PostUtils.GetPost(ctx, post.id);

                if (data == null)
                {
                    return View("HttpNotFound");
                }

                using (var transaction = new System.Transactions.TransactionScope())
                {
                    data.title = post.title;
                    data.content = post.content;
                    data.isVisible = post.isVisible;
                    data.updatedAt = DateTime.Now;

                    var deleteCategories = ctx.PostCategories.Where(x => x.postId.Equals(data.id)).ToList();
                    ctx.PostCategories.DeleteAllOnSubmit(deleteCategories);
                    ctx.SubmitChanges();

                    if (!string.IsNullOrEmpty(selectedCategories))
                    {
                        foreach (var id in selectedCategories.Split(',').Select(x => int.Parse(x)))
                        {
                            PostCategory postCategory = new PostCategory();
                            postCategory.postId = data.id;
                            postCategory.categoryId = id;
                            ctx.PostCategories.InsertOnSubmit(postCategory);
                        }
                    }

                    var deleteTags = ctx.PostTags.Where(x => x.postId.Equals(data.id)).ToList();
                    ctx.PostTags.DeleteAllOnSubmit(deleteTags);
                    ctx.SubmitChanges();

                    if (!string.IsNullOrEmpty(selectedTags))
                    {
                        foreach (var id in selectedTags.Split(',').Select(x => int.Parse(x)))
                        {
                            PostTag postTag = new PostTag();
                            postTag.postId = data.id;
                            postTag.tagId = id;
                            ctx.PostTags.InsertOnSubmit(postTag);
                        }
                    }

                    ctx.SubmitChanges();
                    transaction.Complete();

                    TempData[Message(MessageType.Success)] = "Pomyślnie zapisano zmiany.";
                    return RedirectToAction("Posts", "Admin");
                }
            }
        }

        [Authorize]
        [HttpPost]
        public ActionResult Delete(int id)
        {
            using (var ctx = DbHelper.DataContext)
            {
                Post data = PostUtils.GetPost(ctx, id);

                if (data != null)
                {
                    ctx.Posts.DeleteOnSubmit(data);
                    ctx.SubmitChanges();

                    TempData[Message(MessageType.Success)] = "Pomyślnie usunięto post.";
                }
                return RedirectToAction("Posts", "Admin");
            }
        }

        [Authorize]
        [HttpPost]
        public ActionResult SavePickEditors(IEnumerable<int> rowCheckbox)
        {
            if (rowCheckbox == null)
            {
                rowCheckbox = new List<int>();
            }

            using (var transaction = new System.Transactions.TransactionScope())
            using (var ctx = DbHelper.DataContext)
            {
                var deletePicks = ctx.EditorsPicks.ToList();
                ctx.EditorsPicks.DeleteAllOnSubmit(deletePicks);
                ctx.SubmitChanges();

                int counter = 1;
                foreach (var id in rowCheckbox)
                {
                    EditorsPick pick = new EditorsPick();
                    pick.postId = id;
                    pick.position = counter++;
                    ctx.EditorsPicks.InsertOnSubmit(pick);
                }

                ctx.SubmitChanges();
                transaction.Complete();

                TempData[Message(MessageType.Success)] = "Pomyślnie zapisano zmiany.";
            }
            return RedirectToAction("EditorsPick", "Admin");
        }

        [Authorize]
        [HttpPost]
        public ActionResult SavePickTopMonth(IEnumerable<int> rowCheckbox)
        {
            if (rowCheckbox == null)
            {
                rowCheckbox = new List<int>();
            }

            using (var transaction = new System.Transactions.TransactionScope())
            using (var ctx = DbHelper.DataContext)
            {
                var deletePicks = ctx.TopMonthPicks.ToList();
                ctx.TopMonthPicks.DeleteAllOnSubmit(deletePicks);
                ctx.SubmitChanges();

                int counter = 1;
                foreach (var id in rowCheckbox)
                {
                    TopMonthPick pick = new TopMonthPick();
                    pick.postId = id;
                    pick.position = counter++;
                    ctx.TopMonthPicks.InsertOnSubmit(pick);
                }

                ctx.SubmitChanges();
                transaction.Complete();

                TempData[Message(MessageType.Success)] = "Pomyślnie zapisano zmiany.";
            }
            return RedirectToAction("MonthPostsPick", "Admin");
        }

        [Authorize]
        [HttpPost]
        public ActionResult SavePickTrends(IEnumerable<int> rowCheckbox)
        {
            if (rowCheckbox == null)
            {
                rowCheckbox = new List<int>();
            }

            using (var transaction = new System.Transactions.TransactionScope())
            using (var ctx = DbHelper.DataContext)
            {
                var deletePicks = ctx.TopPicks.ToList();
                ctx.TopPicks.DeleteAllOnSubmit(deletePicks);
                ctx.SubmitChanges();

                int counter = 1;
                foreach (var id in rowCheckbox)
                {
                    TopPick pick = new TopPick();
                    pick.postId = id;
                    pick.position = counter++;
                    ctx.TopPicks.InsertOnSubmit(pick);
                }

                ctx.SubmitChanges();
                transaction.Complete();

                TempData[Message(MessageType.Success)] = "Pomyślnie zapisano zmiany.";
            }
            return RedirectToAction("Trends", "Admin");
        }

        [Authorize]
        public ActionResult LoadPartialView(string operation, int id)
        {
            using (var ctx = DbHelper.DataContext)
            {
                Post data = new Post();

                if (operation != "Create")
                {
                    data = PostUtils.GetPost(ctx, id);

                    if (data == null)
                    {
                        return HttpNotFound();
                    }
                }

                var jsonSettings = new Newtonsoft.Json.JsonSerializerSettings
                {
                    ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore
                };

                var categories = ctx.Categories.Select(c => new DropDownItem
                (
                    c.title,
                    c.id,
                    ctx.PostCategories
                        .Where(x => x.postId.Equals(id) && x.categoryId.Equals(c.id))
                        .Select(x => x.categoryId).Any()
                ));
                ViewBag.Categories = Newtonsoft.Json.JsonConvert.SerializeObject(categories, jsonSettings);

                var tags = ctx.Tags.Select(t => new DropDownItem
                (
                    t.title,
                    t.id,
                    ctx.PostTags
                        .Where(x => x.postId.Equals(id) && x.tagId.Equals(t.id))
                        .Select(x => x.tagId).Any()
                ));
                ViewBag.Tags = Newtonsoft.Json.JsonConvert.SerializeObject(tags, jsonSettings);

                var content = ctx.Posts.Where(p => p.id.Equals(id)).Select(p => p.content).SingleOrDefault();
                ViewBag.Content = Newtonsoft.Json.JsonConvert.SerializeObject(content, jsonSettings);

                return PartialView($"_{operation}PostPartialView", data);
            }
        }
    }
}
