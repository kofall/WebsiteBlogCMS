using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebsiteBlogCMS.Classes;
using WebsiteBlogCMS.Data;

namespace WebsiteBlogCMS.Controllers
{
    public class TagController : Controller
    {
        [HttpPost]
        public ActionResult Create(Tag tag)
        {
            if (ModelState.IsValid)
            {
                using (var ctx = new BlogDBDataContext(DbHelper.ConnectionBlog))
                {
                    bool isValid = !ctx.Tags.Where(x => x.title.Equals(tag.title)).Any();

                    if (!isValid)
                    {
                        ViewBag.ErrorMessage = "Tag o takiej nazwie już istnieje!";
                        return View("HttpNotFound");
                    }

                    ctx.Tags.InsertOnSubmit(tag);
                    ctx.SubmitChanges();

                    return RedirectToAction("Tags", "Admin");
                }
            }
            return View("HttpNotFound");
        }

        [HttpPost]
        public ActionResult Edit(Tag tag)
        {
            if (ModelState.IsValid)
            {
                using (var ctx = new BlogDBDataContext(DbHelper.ConnectionBlog))
                {
                    bool isValid = !ctx.Tags.Where(x => x.title.Equals(tag.title) && x.id != tag.id).Any();

                    if (!isValid)
                    {
                        ViewBag.ErrorMessage = "Tag o takiej nazwie już istnieje!";
                        return View("HttpNotFound");
                    }

                    Tag TagData = ctx.Tags.Where(x => x.id.Equals(tag.id)).SingleOrDefault();

                    if (TagData == null)
                    {
                        return View("HttpNotFound");
                    }

                    TagData.title = tag.title;

                    ctx.SubmitChanges();

                    return RedirectToAction("Tags", "Admin");
                }
            }
            return View("HttpNotFound");
        }

        public ActionResult Delete(int id)
        {
            using (var ctx = new BlogDBDataContext(DbHelper.ConnectionBlog))
            {
                Tag tag = ctx.Tags.Where(x => x.id.Equals(id)).SingleOrDefault();

                if (tag != null)
                {
                    ctx.Tags.DeleteOnSubmit(tag);
                    ctx.SubmitChanges();
                }
            }
            return RedirectToAction("Tags", "Admin");
        }

        public ActionResult LoadTagPartialView(string operation, int id)
        {
            using (var ctx = new BlogDBDataContext(DbHelper.ConnectionBlog))
            {
                Tag tag = new Tag();

                if (operation != "Create")
                {
                    tag = ctx.Tags.Where(x => x.id.Equals(id)).SingleOrDefault();

                    if (tag == null)
                    {
                        return HttpNotFound();
                    }
                }

                return PartialView($"_{operation}TagPartialView", tag);
            }
        }
    }
}
