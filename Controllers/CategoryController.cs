using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using WebsiteBlogCMS.Classes;
using WebsiteBlogCMS.Data;
using WebsiteBlogCMS.Models.Validation;
using static WebsiteBlogCMS.Classes.DataHelper;
using static WebsiteBlogCMS.Classes.Enums;

namespace WebsiteBlogCMS.Controllers
{
    public class CategoryController : Controller
    {
        [HttpPost]
        [Authorize]
        public ActionResult Create(CategoryModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData[Message(MessageType.Warning)] = ModelUtils.GetErrorMessages(ModelState);
                return RedirectToAction("Categories", "Admin");
            }

            using (var ctx = DbHelper.DataContext)
            {
                Category category = new Category();
                category.title = model.Title;
                category.content = model.Content;
                category.parentId = model.ParentId;

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
        public ActionResult Edit(CategoryModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData[Message(MessageType.Warning)] = ModelUtils.GetErrorMessages(ModelState);
                return RedirectToAction("Categories", "Admin");
            }

            using (var ctx = DbHelper.DataContext)
            {
                Category category = CategoryUtils.GetCategory(ctx, model.Id);

                if (category == null)
                {
                    return View("HttpNotFound");
                }

                category.title = model.Title;
                category.parentId = model.ParentId;
                category.content = model.Content;

                if (!CategoryUtils.IsCategoryValid(ctx, category))
                {
                    TempData[Message(MessageType.Warning)] = "Kategoria o takiej nazwie już istnieje.";
                    return RedirectToAction("Categories", "Admin");
                }

                if(CategoryUtils.WillCreateLoop(category.id, category.parentId))
                {
                    TempData[Message(MessageType.Warning)] = "Wybranie tej kategorii nadrzędnej spowoduje wystąpienie cyklu.";
                    return RedirectToAction("Categories", "Admin");
                }

                ctx.SubmitChanges();

                TempData[Message(MessageType.Success)] = "Pomyślnie zapisano zmiany.";
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
                CategoryModel model = new CategoryModel();

                if (operation != "Create")
                {
                    Category category = CategoryUtils.GetCategory(ctx, id);
                    if (category == null)
                    {
                        return HttpNotFound();
                    }
                    model.Id = category.id;
                    model.Title = category.title;
                    model.Content = category.content;
                    model.ParentId = category.parentId;
                }

                ViewBag.Categories = CategoryUtils.GetCategories(ctx);

                return PartialView($"_{operation}CategoryPartialView", model);
            }
        }
    }
}