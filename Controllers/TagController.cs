﻿using System.Web.Mvc;
using WebsiteBlogCMS.Classes;
using WebsiteBlogCMS.Data;
using WebsiteBlogCMS.Models.Validation;
using static WebsiteBlogCMS.Classes.DataHelper;
using static WebsiteBlogCMS.Classes.Enums;

namespace WebsiteBlogCMS.Controllers
{
    public class TagController : Controller
    {
        [HttpPost]
        [Authorize]
        public ActionResult Create(TagModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData[Message(MessageType.Warning)] = ModelUtils.GetErrorMessages(ModelState);
                return RedirectToAction("Tags", "Admin");
            }

            using (var ctx = DbHelper.DataContext)
            {
                Tag tag = new Tag();
                tag.title = model.Title;

                if (!TagUtils.IsTagValid(ctx, tag))
                {
                    TempData[Message(MessageType.Error)] = "Tag o takiej nazwie już istnieje.";
                    return RedirectToAction("Tags", "Admin");
                }

                ctx.Tags.InsertOnSubmit(tag);
                ctx.SubmitChanges();

                TempData[Message(MessageType.Success)] = "Pomyślnie utworzono tag.";
                return RedirectToAction("Tags", "Admin");
            }
        }

        [HttpPost]
        [Authorize]
        public ActionResult Edit(TagModel model)
        {
            if (!ModelState.IsValid)
            {
                return View("HttpNotFound");
            }

            using (var ctx = DbHelper.DataContext)
            {
                Tag tag = TagUtils.GetTag(ctx, model.Id);

                if (tag == null)
                {
                    return View("HttpNotFound");
                }

                tag.title = model.Title;

                if (!TagUtils.IsTagValid(ctx, tag))
                {
                    TempData[Message(MessageType.Warning)] = "Tag o takiej nazwie już istnieje.";
                    return View("HttpNotFound");
                }

                ctx.SubmitChanges();

                TempData[Message(MessageType.Success)] = "Pomyślnie zapisano zmiany.";
                return RedirectToAction("Tags", "Admin");
            }
        }

        [Authorize]
        [HttpPost]
        public ActionResult Delete(int id)
        {
            using (var ctx = DbHelper.DataContext)
            {
                Tag tag = TagUtils.GetTag(ctx, id);

                if (tag != null)
                {
                    ctx.Tags.DeleteOnSubmit(tag);
                    ctx.SubmitChanges();

                    TempData[Message(MessageType.Success)] = "Pomyślnie usunięto tag.";
                }
                return RedirectToAction("Tags", "Admin");
            }
        }

        [Authorize]
        public ActionResult LoadPartialView(string operation, int id)
        {
            using (var ctx = DbHelper.DataContext)
            {
                TagModel model = new TagModel();

                if (operation != "Create")
                {
                    Tag tag = TagUtils.GetTag(ctx, id);

                    if (tag == null)
                    {
                        return HttpNotFound();
                    }

                    model.Id = tag.id;
                    model.Title = tag.title;
                }

                return PartialView($"_{operation}TagPartialView", model);
            }
        }
    }
}