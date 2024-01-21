using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using WebsiteBlogCMS.Classes;
using WebsiteBlogCMS.Data;
using static WebsiteBlogCMS.Classes.DataHelper;
using static WebsiteBlogCMS.Classes.Enums;

namespace WebsiteBlogCMS.Controllers
{
    public class CategoryController : Controller
    {
        [HttpPost]
        [Authorize]
        public ActionResult Create(Category category)
        {
            if (!ModelState.IsValid)
            {
                return View("HttpNotFound");
            }

            using (var ctx = DbHelper.DataContext)
            {
                if (!CategoryUtils.IsCategoryValid(ctx, category))
                {
                    TempData[Message(MessageType.Warning)] = "Kategoria o takiej nazwie już istnieje.";
                    return RedirectToAction("Categories", "Admin");
                }

                ctx.Categories.InsertOnSubmit(category);
                ctx.SubmitChanges();

                TempData[Message(MessageType.Success)] = "Pomyślnie utworzono kategorię.";
                return RedirectToAction("Categories", "Admin");
            }
        }

        [HttpPost]
        [Authorize]
        public ActionResult Edit(Category category)
        {
            if (!ModelState.IsValid)
            {
                return View("HttpNotFound");
            }

            using (var ctx = DbHelper.DataContext)
            {
                if (!CategoryUtils.IsCategoryValid(ctx, category))
                {
                    TempData[Message(MessageType.Warning)] = "Kategoria o takiej nazwie już istnieje.";
                    return View("Categories", "Admin");
                }

                Category categoryData = CategoryUtils.GetCategory(ctx, category.id);

                if (categoryData == null)
                {
                    return View("HttpNotFound");
                }

                categoryData.title = category.title;
                categoryData.parentId = category.parentId;
                categoryData.content = category.content;

                ctx.SubmitChanges();

                TempData[Message(MessageType.Success)] = "Pomyślnie utworzono kategorię.";
                return RedirectToAction("Categories", "Admin");
            }
        }

        [Authorize]
        [HttpPost]
        public ActionResult Delete(int id)
        {
            using (var ctx = DbHelper.DataContext)
            {
                Category category = CategoryUtils.GetCategory(ctx, id);

                if (category != null)
                {
                    ctx.Categories.DeleteOnSubmit(category);
                    ctx.SubmitChanges();

                    TempData[Message(MessageType.Success)] = "Pomyślnie usunięto kategorię.";
                }

                return RedirectToAction("Categories", "Admin");
            }
        }

        [Authorize]
        [HttpPost]
        public ActionResult SavePick(IEnumerable<int> rowCheckbox)
        {
            if (rowCheckbox == null)
            {
                rowCheckbox = new List<int>();
            }

            using (var transaction = new System.Transactions.TransactionScope())
            using (var ctx = DbHelper.DataContext)
            {
                var deletePicks = ctx.CategorySettings.ToList();
                ctx.CategorySettings.DeleteAllOnSubmit(deletePicks);
                ctx.SubmitChanges();

                int counter = 1;
                foreach (var id in rowCheckbox)
                {
                    CategorySetting pick = new CategorySetting();
                    pick.categoryId = id;
                    pick.position = counter++;
                    ctx.CategorySettings.InsertOnSubmit(pick);
                }

                ctx.SubmitChanges();
                transaction.Complete();
                TempData[Message(MessageType.Success)] = "Pomyślnie zapisano zmiany.";
            }
            return RedirectToAction("CategoriesPick", "Admin");
        }

        [Authorize]
        public ActionResult LoadPartialView(string operation, int id)
        {
            using (var ctx = DbHelper.DataContext)
            {
                Category category = new Category();

                if (operation != "Create")
                {
                    category = CategoryUtils.GetCategory(ctx, id);

                    if (category == null)
                    {
                        return HttpNotFound();
                    }
                }

                ViewBag.Categories = CategoryUtils.GetCategories(ctx);

                return PartialView($"_{operation}CategoryPartialView", category);
            }
        }
    }
}