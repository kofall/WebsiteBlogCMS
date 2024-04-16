using System.Collections.Generic;
using System.Data.Linq;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Services.Description;
using WebsiteBlogCMS.Classes;
using WebsiteBlogCMS.Data;
using WebsiteBlogCMS.Models.Validation;
using static WebsiteBlogCMS.Classes.DataHelper;
using static WebsiteBlogCMS.Classes.Enums;

namespace WebsiteBlogCMS.Controllers
{
    public class SliderController : Controller
    {
        [Authorize]
        public ActionResult Index()
        {
            if (!UserUtils.HasPermission(2, User.Identity.Name))
            {
                TempData[Message(MessageType.Warning)] = "Nie posiadasz uprawnień do korzystania z tego widoku.";
                return RedirectToAction("Index", "Admin");
            }

            return View();
        }

        [Authorize]
        public ActionResult Create()
        {
            if (!UserUtils.HasPermission(2, User.Identity.Name))
            {
                TempData[Message(MessageType.Warning)] = "Nie posiadasz uprawnień do korzystania z tego widoku.";
                return RedirectToAction("Index", "Admin");
            }

            return View();
        }

        [HttpPost]
        [Authorize]
        public ActionResult Create(SlideModel model, HttpPostedFileBase image)
        {
            if (image != null && image.ContentLength > 0)
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    image.InputStream.CopyTo(ms);
                    model.Image = ms.ToArray();
                }
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            using (var ctx = DbHelper.DataContext)
            {
                Slider slider = new Slider();
                slider.title = model.Title;
                slider.description = model.Description;

                if (!SliderUtils.IsSliderValid(ctx, slider))
                {
                    TempData[Message(MessageType.Error)] = "Slajd o takim tytule już istnieje.";
                    return View(model);
                }

                if (model.Image != null && model.Image.Length > 0)
                {
                    slider.image = new Binary(model.Image);
                }
                else if (model.Image == null)
                {
                    TempData[Message(MessageType.Warning)] = "Zdjęcie slajdu jest wymagane.";
                    return View(model);
                }

                ctx.Sliders.InsertOnSubmit(slider);
                ctx.SubmitChanges();

                TempData[Message(MessageType.Success)] = "Pomyślnie utworzono slajd.";
                return RedirectToAction("Sliders", "Admin");
            }
        }

        [Authorize]
        public ActionResult Edit(int id)
        {
            if (!UserUtils.HasPermission(2, User.Identity.Name))
            {
                TempData[Message(MessageType.Warning)] = "Nie posiadasz uprawnień do korzystania z tego widoku.";
                return RedirectToAction("Index", "Admin");
            }

            using (var ctx = DbHelper.DataContext)
            {
                Slider data = SliderUtils.GetSlider(ctx, id);

                if (data == null)
                {
                    return HttpNotFound();
                }

                SlideModel model = new SlideModel();
                model.Title = data.title;
                model.Description = data.description;
                model.Image = data.image?.ToArray() ?? null;

                return View(model);
            }
        }

        [HttpPost]
        [Authorize]
        public ActionResult Edit(SlideModel model, HttpPostedFileBase image)
        {
            if (image != null && image.ContentLength > 0)
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    image.InputStream.CopyTo(ms);
                    model.Image = ms.ToArray();
                }
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            using (var ctx = DbHelper.DataContext)
            {
                Slider data = SliderUtils.GetSlider(ctx, model.Id);

                if (data == null)
                {
                    return View("HttpNotFound");
                }

                data.title = model.Title;
                data.description = model.Description;

                if (!SliderUtils.IsSliderValid(ctx, data))
                {
                    TempData[Message(MessageType.Warning)] = "Slajd o takim tytule już istnieje.";
                    return View(model);
                }

                if (model.Image != null && model.Image.Length > 0)
                {
                    data.image = new Binary(model.Image);
                }
                else if (model.Image == null)
                {
                    TempData[Message(MessageType.Warning)] = "Zdjęcie slajdu jest wymagane.";
                    return View(model);
                }

                ctx.SubmitChanges();

                TempData[Message(MessageType.Success)] = "Pomyślnie zapisano zmiany.";
                return RedirectToAction("Sliders", "Admin");
            }
        }

        [Authorize]
        [HttpPost]
        public ActionResult Delete(int id)
        {
            using (var ctx = DbHelper.DataContext)
            {
                Slider data = SliderUtils.GetSlider(ctx, id);

                if (data != null)
                {
                    ctx.Sliders.DeleteOnSubmit(data);
                    ctx.SubmitChanges();

                    TempData[Message(MessageType.Success)] = "Pomyślnie usunięto slajd.";
                }

                return RedirectToAction("Sliders", "Admin");
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
                var deletePicks = ctx.SliderPicks.ToList();
                ctx.SliderPicks.DeleteAllOnSubmit(deletePicks);
                ctx.SubmitChanges();

                int counter = 1;
                foreach (var id in rowCheckbox)
                {
                    SliderPick pick = new SliderPick();
                    pick.sliderId = id;
                    pick.position = counter++;
                    ctx.SliderPicks.InsertOnSubmit(pick);
                }

                ctx.SubmitChanges();
                transaction.Complete();

                TempData[Message(MessageType.Success)] = "Pomyślnie zapisano zmiany.";
            }
            return RedirectToAction("SlidersPick", "Admin");
        }

        [Authorize]
        public ActionResult LoadPartialView(string operation, int id)
        {
            using (var ctx = DbHelper.DataContext)
            {
                Slider data = SliderUtils.GetSlider(ctx, id);

                if (data == null)
                {
                    return HttpNotFound();
                }

                return PartialView($"_{operation}SliderPartialView", data);
            }
        }
    }
}