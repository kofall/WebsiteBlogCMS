using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using WebsiteBlogCMS.Classes;
using WebsiteBlogCMS.Data;
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
        public ActionResult Create(Slider slider)
        {
            if (!ModelState.IsValid)
            {
                return View("HttpNotFound");
            }

            using (var ctx = DbHelper.DataContext)
            {
                if (!SliderUtils.IsSliderValid(ctx, slider))
                {
                    TempData[Message(MessageType.Error)] = "Slajd o takim tytule już istnieje.";
                    return View(slider);
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

                return View(data);
            }
        }

        [HttpPost]
        [Authorize]
        public ActionResult Edit(Slider slider)
        {
            if (!ModelState.IsValid)
            {
                return View("HttpNotFound");
            }

            using (var ctx = DbHelper.DataContext)
            {
                if (!SliderUtils.IsSliderValid(ctx, slider))
                {
                    TempData[Message(MessageType.Warning)] = "Slajd o takim tytule już istnieje.";
                    return View(slider);
                }

                Slider data = SliderUtils.GetSlider(ctx, slider.id);

                if (data == null)
                {
                    return View("HttpNotFound");
                }


                data.title = slider.title;
                data.description = slider.description;

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