using System;
using System.Linq;
using System.Web.Mvc;
using WebsiteBlogCMS.Classes;
using WebsiteBlogCMS.Data;

namespace WebsiteBlogCMS.Controllers
{

    public class CategoryController : Controller
    {
        [HttpPost]
        public ActionResult Create(Category category)
        {
            if (ModelState.IsValid)
            {
                using (var ctx = new BlogDBDataContext(DbHelper.ConnectionBlog))
                {
                    bool isValid = !ctx.Categories.Where(x => x.title.Equals(category.title)).Any();

                    if(!isValid)
                    {
                        ViewBag.ErrorMessage = "Kategoria o takiej nazwie już istnieje!";
                        return View("HttpNotFound");
                    }

                    ctx.Categories.InsertOnSubmit(category);
                    ctx.SubmitChanges();

                    return RedirectToAction("Categories", "Admin");
                }
            }
            return View("HttpNotFound");
        }

        [HttpPost]
        public ActionResult Edit(Category category)
        {
            if (ModelState.IsValid)
            {
                using (var ctx = new BlogDBDataContext(DbHelper.ConnectionBlog))
                {
                    bool isValid = !ctx.Categories.Where(x => x.title.Equals(category.title) && x.id != category.id).Any();

                    if (!isValid)
                    {
                        ViewBag.ErrorMessage = "Kategoria o takiej nazwie już istnieje!";
                        return View("HttpNotFound");
                    }

                    Category categoryData = ctx.Categories.Where(x => x.id.Equals(category.id)).SingleOrDefault();

                    if(categoryData == null)
                    {
                        return View("HttpNotFound");
                    }

                    categoryData.title = category.title;
                    categoryData.parentId = category.parentId;
                    categoryData.content = category.content;

                    ctx.SubmitChanges();

                    return RedirectToAction("Categories", "Admin");
                }
            }
            return View("HttpNotFound");
        }

        public ActionResult Delete(int id)
        {
            using (var ctx = new BlogDBDataContext(DbHelper.ConnectionBlog))
            {
                Category category = ctx.Categories.Where(x => x.id.Equals(id)).SingleOrDefault();

                if (category != null)
                {
                    ctx.Categories.DeleteOnSubmit(category);
                    ctx.SubmitChanges();
                }
            }
            return RedirectToAction("Categories", "Admin");
        }

        public ActionResult LoadCategoryPartialView(string operation, int id)
        {
            using (var ctx = new BlogDBDataContext(DbHelper.ConnectionBlog))
            {
                Category category = new Category();

                if (operation != "Create")
                {
                    category = ctx.Categories.Where(x => x.id.Equals(id)).SingleOrDefault();

                    if (category == null)
                    {
                        return HttpNotFound();
                    }
                }

                ViewBag.Categories = ctx.Categories.ToList();

                return PartialView($"_{operation}CategoryPartialView", category);
            }
        }
    }
}
