using PagedList;
using System;
using System.Collections.Generic;
using System.Data.Linq;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Web;
using System.Web.Mvc;
using System.Web.Services.Description;
using WebsiteBlogCMS.Classes;
using WebsiteBlogCMS.Data;
using WebsiteBlogCMS.Models;
using WebsiteBlogCMS.Models.Validation;
using static WebsiteBlogCMS.Classes.DataHelper;
using static WebsiteBlogCMS.Classes.Enums;

namespace WebsiteBlogCMS.Controllers
{
    public class PostController : Controller
    {
        public ActionResult PostsOfCategory(int id, int? page)
        {
            int pageSize = 10;
            int pageNumber = (page ?? 1);

            using (var ctx = DbHelper.DataContext)
            {
                DataLoadOptions options = new DataLoadOptions();
                options.LoadWith<Post>(x => x.User);
                options.LoadWith<Post>(x => x.PostTags);
                options.LoadWith<Post>(x => x.PostCategories);
                options.LoadWith<PostTag>(x => x.Tag);
                options.LoadWith<PostCategory>(x => x.Category);
                ctx.LoadOptions = options;

                Category category = CategoryUtils.GetCategory(ctx, id);

                if (category == null)
                {
                    return View("HttpNotFound");
                }

                List<Post> posts = PostUtils.GetPostsByCategory(ctx, category);

                PostsOfCategoryModel model = new PostsOfCategoryModel();
                model.Category = category;
                model.Posts = posts.ToPagedList(pageNumber, pageSize);

                if (category.parentId != null)
                {
                    model.ParentCategory = CategoryUtils.GetCategory(ctx, (int)category.parentId);
                }
                model.SubCategories = CategoryUtils.GetSubCats(ctx, category);

                return View(model);
            }
        }

        public ActionResult PostsOfTag(int id, int? page)
        {
            int pageSize = 10;
            int pageNumber = (page ?? 1);

            using (var ctx = DbHelper.DataContext)
            {
                DataLoadOptions options = new DataLoadOptions();
                options.LoadWith<Post>(x => x.User);
                options.LoadWith<Post>(x => x.PostTags);
                options.LoadWith<Post>(x => x.PostCategories);
                options.LoadWith<PostTag>(x => x.Tag);
                options.LoadWith<PostCategory>(x => x.Category);
                ctx.LoadOptions = options;

                Tag tag = TagUtils.GetTag(ctx, id);

                if (tag == null)
                {
                    return View("HttpNotFound");
                }

                List<Post> posts = PostUtils.GetPostsByTag(ctx, tag);

                PostsOfTagModel model = new PostsOfTagModel();
                model.Tag = tag;
                model.Posts = posts.ToPagedList(pageNumber, pageSize);

                return View(model);
            }
        }

        private PostDetailsModel GetPostDetails(int id, bool onlyVisible = true)
        {
            using (var ctx = DbHelper.DataContext)
            {
                DataLoadOptions options = new DataLoadOptions();
                options.LoadWith<Post>(x => x.User);
                options.LoadWith<Post>(x => x.PostTags);
                options.LoadWith<Post>(x => x.PostCategories);
                options.LoadWith<Post>(x => x.Comments);
                options.LoadWith<PostTag>(x => x.Tag);
                options.LoadWith<PostCategory>(x => x.Category);
                ctx.LoadOptions = options;

                Post post = PostUtils.GetPost(ctx, id);

                if (post == null)
                {
                    throw new Exception("HttpNotFound");
                }

                PostDetailsModel model = new PostDetailsModel();
                model.Post = post;
                model.UserPosts = PostUtils.GetUserPosts(ctx, post.authorId).Where(x => !x.id.Equals(id)).Take(3).ToList();
                model.Comments = CommentUtils.GetRootComments(ctx, post);
                model.CommentsCount = CommentUtils.GetPostComments(ctx, post, onlyVisible).Count();

                return model;
            }
        }

        public ActionResult Details(int id)
        {
            using (var ctx = DbHelper.DataContext)
            {
                PostDetailsModel model = GetPostDetails(id);

                ViewBag.Categories = CategoryUtils.GetRootCategories(ctx);
                ViewBag.UserPosts = PostUtils.GetUserPosts(ctx, model.Post.authorId);
                return View(model);
            }
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteComment(int commentId)
        {
            using (var ctx = DbHelper.DataContext)
            {
                Comment comment = CommentUtils.GetComment(ctx, commentId);
                int postId = comment.postId;
                try
                {
                    if (comment != null)
                    {
                        ctx.Comments.DeleteOnSubmit(comment);
                        ctx.SubmitChanges();

                        TempData[Message(MessageType.Success)] = "Pomyślnie usunięto komentarz.";
                    }
                    return RedirectToAction("Preview", "Post", new { id = postId });
                }
                catch
                {
                    TempData[Message(MessageType.Error)] = "Nie udało się usunąć komentarza.";
                    return RedirectToAction("Preview", "Post", new { id = postId });
                }
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LeaveComment(PostDetailsModel model)
        {
            if (!ModelState.IsValid)
            {
                CommentModel commentModel = model.CommentModel;
                model = GetPostDetails(model.Post.id);
                model.CommentModel = commentModel;
                return View("Details", model);
            }

            using (var ctx = DbHelper.DataContext)
            {
                Comment comment = new Comment();
                comment.postId = model.Post.id;
                comment.parentId = model.CommentModel.ParentId;
                comment.authorName = model.CommentModel.AuthorName;
                comment.published = true;
                comment.publishedAt = DateTime.Now;
                comment.content = model.CommentModel.Content;
                comment.isVisible = true;

                ctx.Comments.InsertOnSubmit(comment);
                ctx.SubmitChanges();
            }

            return RedirectToAction("Details", new { id = model.Post.id });
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LeavePriorityComment(PostDetailsModel model)
        {
            using (var ctx = DbHelper.DataContext)
            {
                if (!ModelState.IsValid)
                {
                    CommentModel commentModel = model.CommentModel;
                    model = GetPostDetails(model.Post.id);
                    model.CommentModel = commentModel;
                    ViewBag.Categories = PostUtils.GetPostCategories(ctx, model.Post.id);
                    ViewBag.Tags = PostUtils.GetPostTags(ctx, model.Post.id);
                    return View("Preview", model);
                }

                Comment comment = new Comment();
                comment.postId = model.Post.id;
                comment.parentId = model.CommentModel.ParentId;
                comment.authorId = UserUtils.GetUser(User.Identity.Name).id;
                comment.authorName = model.CommentModel.AuthorName;
                comment.published = true;
                comment.publishedAt = DateTime.Now;
                comment.content = model.CommentModel.Content;
                comment.isVisible = true;

                ctx.Comments.InsertOnSubmit(comment);
                ctx.SubmitChanges();
                TempData[Message(MessageType.Success)] = "Pomyślnie dodano komentarz.";
            }

            return RedirectToAction("Preview", new { id = model.Post.id });
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
                DataLoadOptions options = new DataLoadOptions();
                options.LoadWith<Post>(x => x.User);
                options.LoadWith<Post>(x => x.Comments);
                ctx.LoadOptions = options;

                PostDetailsModel model = GetPostDetails(id, false);

                User me = UserUtils.GetUser(ctx, User.Identity.Name);
                User author = UserUtils.GetUser(ctx, model.Post.authorId);
                if (!(me.id == author.id || me.roleId < 3))
                {
                    TempData[Message(MessageType.Warning)] = "Nie posiadasz uprawnień do wyświetlenia tego artykułu.";
                    return RedirectToAction("Index", "Admin");
                }

                ViewBag.Categories = PostUtils.GetPostCategories(ctx, id);
                ViewBag.Tags = PostUtils.GetPostTags(ctx, id);

                return View(model);
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
        public ActionResult Create(PostModel model, string selectedCategories, string selectedTags, HttpPostedFileBase image)
        {
            using (var ctx = DbHelper.DataContext)
            {
                string errorMessages = string.Empty;

                var jsonSettings = new Newtonsoft.Json.JsonSerializerSettings
                {
                    ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore
                };

                IEnumerable<DropDownItem> categories = ctx.Categories.ToList().Select(c =>
                {
                    bool isSelected = false;

                    if (selectedCategories != null)
                    {
                        isSelected = selectedCategories.Split(',').ToList().Contains(c.id.ToString());
                    }

                    return new DropDownItem
                    (
                        c.title,
                        c.id,
                        isSelected
                    );
                });
                ViewBag.Categories = Newtonsoft.Json.JsonConvert.SerializeObject(categories, jsonSettings);
                if (string.IsNullOrEmpty(selectedCategories))
                {
                    errorMessages += "Kategoria jest wymagana. ";
                }

                IEnumerable<DropDownItem> tags = ctx.Tags.ToList().Select(t =>
                {
                    bool isSelected = false;

                    if (selectedTags != null)
                    {
                        isSelected = selectedTags.Split(',').ToList().Contains(t.id.ToString());
                    }

                    return new DropDownItem
                    (
                        t.title,
                        t.id,
                        isSelected
                    );
                });
                ViewBag.Tags = Newtonsoft.Json.JsonConvert.SerializeObject(tags, jsonSettings);

                if (image != null && image.ContentLength > 0)
                {
                    using (MemoryStream ms = new MemoryStream())
                    {
                        image.InputStream.CopyTo(ms);
                        model.Image = ms.ToArray();
                    }
                }

                if (!ModelState.IsValid || !string.IsNullOrEmpty(errorMessages))
                {
                    if (!string.IsNullOrEmpty(errorMessages))
                    {
                        TempData[Message(MessageType.Warning)] = errorMessages;
                    }
                    return View(model);
                }

                Post post = new Post();
                post.title = model.Title;
                post.content = model.Content;

                if (!PostUtils.IsPostValid(ctx, post))
                {
                    TempData[Message(MessageType.Warning)] = "Post o takim tytule już istnieje.";
                    return View(model);
                }

                if (model.Image != null && model.Image.Length > 0)
                {
                    post.image = new Binary(model.Image);
                }
                else if (model.Image == null)
                {
                    TempData[Message(MessageType.Warning)] = "Zdjęcie postu jest wymagane.";
                    return View(model);
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

                PostModel model = new PostModel();
                model.Id = id;
                model.Published = data.published;
                model.IsVisible = data.isVisible;
                model.Title = data.title;
                model.Content = data.content;
                model.Image = data.image?.ToArray() ?? null;

                return View(model);
            }
        }

        [HttpPost]
        [ValidateInput(false)]
        [Authorize]
        public ActionResult Edit(PostModel model, string selectedCategories, string selectedTags, HttpPostedFileBase image)
        {
            using (var ctx = DbHelper.DataContext)
            {
                string errorMessages = string.Empty;

                var jsonSettings = new Newtonsoft.Json.JsonSerializerSettings
                {
                    ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore
                };

                IEnumerable<DropDownItem> categories = ctx.Categories.ToList().Select(c =>
                {
                    bool isSelected = false;

                    if (selectedCategories != null)
                    {
                        isSelected = selectedCategories.Split(',').ToList().Contains(c.id.ToString());
                    }

                    return new DropDownItem
                    (
                        c.title,
                        c.id,
                        isSelected
                    );
                });
                ViewBag.Categories = Newtonsoft.Json.JsonConvert.SerializeObject(categories, jsonSettings);
                if (string.IsNullOrEmpty(selectedCategories))
                {
                    errorMessages += "Kategoria jest wymagana. ";
                }

                IEnumerable<DropDownItem> tags = ctx.Tags.ToList().Select(t =>
                {
                    bool isSelected = false;

                    if (selectedTags != null)
                    {
                        isSelected = selectedTags.Split(',').ToList().Contains(t.id.ToString());
                    }

                    return new DropDownItem
                    (
                        t.title,
                        t.id,
                        isSelected
                    );
                });
                ViewBag.Tags = Newtonsoft.Json.JsonConvert.SerializeObject(tags, jsonSettings);

                if (image != null && image.ContentLength > 0)
                {
                    using (MemoryStream ms = new MemoryStream())
                    {
                        image.InputStream.CopyTo(ms);
                        model.Image = ms.ToArray();
                    }
                }

                if (!ModelState.IsValid || !string.IsNullOrEmpty(errorMessages))
                {
                    TempData[Message(MessageType.Warning)] = errorMessages;
                    return View(model);
                }

                Post post = PostUtils.GetPost(ctx, model.Id);

                if (post == null)
                {
                    return View("HttpNotFound");
                }

                if (!PostUtils.IsPostValid(ctx, post))
                {
                    TempData[Message(MessageType.Warning)] = "Post o takim tytule już istnieje.";
                    return View(model);
                }

                if (model.Image != null && model.Image.Length > 0)
                {
                    post.image = new Binary(model.Image);
                }
                else if (model.Image == null)
                {
                    TempData[Message(MessageType.Warning)] = "Zdjęcie postu jest wymagane.";
                    return View(model);
                }

                using (var transaction = new System.Transactions.TransactionScope())
                {
                    post.title = model.Title;
                    post.content = model.Content;
                    post.isVisible = model.IsVisible;
                    post.updatedAt = DateTime.Now;

                    var deleteCategories = ctx.PostCategories.Where(x => x.postId.Equals(post.id)).ToList();
                    ctx.PostCategories.DeleteAllOnSubmit(deleteCategories);
                    ctx.SubmitChanges();

                    if (!string.IsNullOrEmpty(selectedCategories))
                    {
                        foreach (var id in selectedCategories.Split(',').Select(x => int.Parse(x)))
                        {
                            PostCategory postCategory = new PostCategory();
                            postCategory.postId = post.id;
                            postCategory.categoryId = id;
                            ctx.PostCategories.InsertOnSubmit(postCategory);
                        }
                    }

                    var deleteTags = ctx.PostTags.Where(x => x.postId.Equals(post.id)).ToList();
                    ctx.PostTags.DeleteAllOnSubmit(deleteTags);
                    ctx.SubmitChanges();

                    if (!string.IsNullOrEmpty(selectedTags))
                    {
                        foreach (var id in selectedTags.Split(',').Select(x => int.Parse(x)))
                        {
                            PostTag postTag = new PostTag();
                            postTag.postId = post.id;
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
                PostModel model = new PostModel();

                if (operation != "Create")
                {
                    Post post = PostUtils.GetPost(ctx, id);
                    if (post == null)
                    {
                        return HttpNotFound();
                    }
                    model.Id = post.id;
                    model.Title = post.title;
                    model.Content = post.content;
                    model.Published = post.published;
                    model.IsVisible = post.isVisible;
                    model.Image = post.image.ToArray();
                }

                return PartialView($"_{operation}PostPartialView", model);
            }
        }
    }
}
