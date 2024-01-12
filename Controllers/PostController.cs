using System;
using System.Linq;
using System.Web.Mvc;
using WebsiteBlogCMS.Classes;
using WebsiteBlogCMS.Data;
using WebsiteBlogCMS.Models;

namespace WebsiteBlogCMS.Controllers
{
    public class PostController : Controller
    {
        [HttpPost]
        public ActionResult Create(Post post, string selectedCategories, string selectedTags)
        {
            if (ModelState.IsValid)
            {
                using (var transaction = new System.Transactions.TransactionScope())
                using (var ctx = new BlogDBDataContext(DbHelper.ConnectionBlog))
                {
                    bool isValid = !ctx.Posts.Where(x => x.title.Equals(post.title)).Any();

                    if (!isValid)
                    {
                        ViewBag.ErrorMessage = "Post o takim tytule już istnieje!";
                        return View("HttpNotFound");
                    }

                    post.authorId = 1;
                    post.createdAt = DateTime.Now;

                    ctx.Posts.InsertOnSubmit(post);
                    ctx.SubmitChanges();

                    Post data = ctx.Posts.Where(x => x.title.Equals(post.title)).Single();

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
                    return RedirectToAction("Posts", "Admin");
                }
            }
            return View("HttpNotFound");
        }

        [HttpPost]
        public ActionResult Edit(Post model, string selectedCategories, string selectedTags)
        {
            if (ModelState.IsValid)
            {
                using (var transaction = new System.Transactions.TransactionScope())
                using (var ctx = new BlogDBDataContext(DbHelper.ConnectionBlog))
                {
                    bool isValid = !ctx.Posts.Where(x => x.title.Equals(model.title) && x.id != model.id).Any();

                    if (!isValid)
                    {
                        ViewBag.ErrorMessage = "Kategoria o takiej nazwie już istnieje!";
                        return View("HttpNotFound");
                    }

                    Post data = ctx.Posts.Where(x => x.id.Equals(model.id)).SingleOrDefault();

                    if (data == null)
                    {
                        return View("HttpNotFound");
                    }

                    data.title = model.title;
                    data.content = model.content;
                    data.updatedAt = DateTime.Now;
                    data.published = model.published;
                    if (data.published && data.publishedAt == null)
                    {
                        data.publishedAt = DateTime.Now;
                    }

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
                    return RedirectToAction("Posts", "Admin");
                }
            }
            return View("HttpNotFound");
        }

        public ActionResult Delete(int id)
        {
            using (var ctx = new BlogDBDataContext(DbHelper.ConnectionBlog))
            {
                Post data = ctx.Posts.Where(x => x.id.Equals(id)).SingleOrDefault();

                if (data != null)
                {
                    ctx.Posts.DeleteOnSubmit(data);
                    ctx.SubmitChanges();
                }
            }
            return RedirectToAction("Posts", "Admin");
        }

        public ActionResult LoadPostPartialView(string operation, int id)
        {
            using (var ctx = new BlogDBDataContext(DbHelper.ConnectionBlog))
            {
                Post data = new Post();

                if (operation != "Create")
                {
                    data = ctx.Posts.Where(x => x.id.Equals(id)).SingleOrDefault();

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

                var tags = ctx.Tags.Select(c => new DropDownItem
                (
                    c.title,
                    c.id,
                    ctx.PostTags
                        .Where(x => x.postId.Equals(id) && x.tagId.Equals(c.id))
                        .Select(x => x.tagId).Any()
                ));
                ViewBag.Tags = Newtonsoft.Json.JsonConvert.SerializeObject(tags, jsonSettings);

                return PartialView($"_{operation}PostPartialView", data);
            }
        }
    }
}
