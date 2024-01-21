using System.Web.Mvc;
using WebsiteBlogCMS.Classes;
using WebsiteBlogCMS.Data;
using static WebsiteBlogCMS.Classes.DataHelper;
using static WebsiteBlogCMS.Classes.Enums;

namespace WebsiteBlogCMS.Controllers
{
    public class TagController : Controller
    {
        [HttpPost]
        [Authorize]
        public ActionResult Create(Tag tag)
        {
            if (!ModelState.IsValid)
            {
                return View("HttpNotFound");
            }

            using (var ctx = DbHelper.DataContext)
            {
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
        public ActionResult Edit(Tag tag)
        {
            if (!ModelState.IsValid)
            {
                return View("HttpNotFound");
            }

            using (var ctx = DbHelper.DataContext)
            {
                if (!TagUtils.IsTagValid(ctx, tag))
                {
                    TempData[Message(MessageType.Warning)] = "Tag o takiej nazwie już istnieje.";
                    return View("HttpNotFound");
                }

                Tag TagData = TagUtils.GetTag(ctx, tag.id);

                if (TagData == null)
                {
                    return View("HttpNotFound");
                }

                TagData.title = tag.title;

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
                Tag tag = new Tag();

                if (operation != "Create")
                {
                    tag = TagUtils.GetTag(ctx, id);

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