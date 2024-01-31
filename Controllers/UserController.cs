using PagedList;
using System;
using System.Collections.Generic;
using System.Data.Linq;
using System.IO;
using System.Linq;
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
    public class UserController : Controller
    {
        public ActionResult Details(int id, int? page)
        {
            int pageSize = 6;
            int pageNumber = (page ?? 1);

            using (var ctx = DbHelper.DataContext)
            {
                DataLoadOptions options = new DataLoadOptions();
                options.LoadWith<User>(x => x.Posts);
                options.LoadWith<User>(x => x.Role);
                ctx.LoadOptions = options;

                User user = UserUtils.GetUser(ctx, id);
                List<Post> posts = PostUtils.GetUserPosts(ctx, id);

                UserDetailsModel model = new UserDetailsModel();
                model.User = user;
                model.UserPosts = posts.ToPagedList(pageNumber, pageSize);

                return View(model);
            }
        }

        // GET: User
        [Authorize]
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        [Authorize]
        public ActionResult Create(User newUser)
        {
            if (!ModelState.IsValid)
            {
                TempData[Message(MessageType.Warning)] = "Aby dodać konto, najpierw nadaj mu rolę.";
                return RedirectToAction("Users", "Admin");
            }

            using (var ctx = DbHelper.DataContext)
            {
                var data = UserUtils.GenerateActivationLink();
                newUser.activationLink = data["link"];

                ctx.Users.InsertOnSubmit(newUser);
                ctx.SubmitChanges();

                User user = ctx.Users.OrderByDescending(u => u.id).FirstOrDefault();

                UserToken userToken = new UserToken();
                userToken.userId = user.id;
                userToken.token = data["token"];
                userToken.expireDate = DateTime.Now.AddDays(1);

                ctx.UserTokens.InsertOnSubmit(userToken);
                ctx.SubmitChanges();

                // Redirect to the user list or perform any other action
                TempData["ShowActivationLink"] = true;
                TempData["UserToShow"] = user.id;
                TempData[Message(MessageType.Success)] = "Pomyślnie wygenerowano link aktywacyjny.";

                return RedirectToAction("Users", "Admin");
            }
        }

        [HttpPost]
        [Authorize]
        public ActionResult Edit(User user)
        {
            if (!ModelState.IsValid)
            {
                TempData[Message(MessageType.Warning)] = "Aby edytować rolę konta, najpierw wybierz rolę.";
                return RedirectToAction("Users", "Admin");
            }

            using (var ctx = DbHelper.DataContext)
            {
                DataLoadOptions options = new DataLoadOptions();
                options.LoadWith<User>(x => x.Role);
                ctx.LoadOptions = options;

                User userData = UserUtils.GetUser(ctx, user.id);

                if (user == null)
                {
                    return View("HttpNotFound");
                }

                if (UserUtils.IsLastAdmin(ctx, user.id))
                {
                    TempData[Message(MessageType.Warning)] = "Nie można zmienić roli ostatniego administratora.";
                    return RedirectToAction("Users", "Admin");
                }

                userData.roleId = user.roleId;
                ctx.SubmitChanges();

                TempData[Message(MessageType.Success)] = "Pomyślnie zapisano zmiany.";
                return RedirectToAction("Users", "Admin");
            }
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public ActionResult EditSettings(EditSettingsModel model, HttpPostedFileBase profileImage)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Roles = RoleUtils.GetRoles();
                return View("~/Views/Admin/Settings.cshtml", model);
            }

            if (profileImage != null && profileImage.ContentLength > 0)
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    profileImage.InputStream.CopyTo(ms);
                    model.ProfileImage = ms.ToArray();
                }
            }

            using (var ctx = DbHelper.DataContext)
            {
                User user = UserUtils.GetUser(ctx, model.Id);

                if (user == null)
                {
                    return View("HttpNotFound");
                }

                if (model.ProfileImage != null && model.ProfileImage.Length > 0)
                {
                    user.profileImage = new Binary(model.ProfileImage);
                }
                else if(model.ProfileImage == null)
                {
                    TempData[Message(MessageType.Warning)] = "Zdjęcie profilowe jest wymagane.";
                    ViewBag.Roles = RoleUtils.GetRoles();
                    return View("~/Views/Admin/Settings.cshtml", model);
                }

                user.firstName = model.FirstName;
                user.lastName = model.LastName;
                user.intro = model.Intro;
                user.profile = model.Profile;

                ctx.SubmitChanges();

                TempData[Message(MessageType.Success)] = "Pomyślnie zapisano zmiany.";
                return RedirectToAction("Settings", "Admin", new { login = User.Identity.Name });
            }
        }

        [Authorize]
        [HttpPost]
        public ActionResult Delete(int id)
        {
            using (var ctx = DbHelper.DataContext)
            {
                DataLoadOptions options = new DataLoadOptions();
                options.LoadWith<User>(x => x.Role);
                ctx.LoadOptions = options;

                User user = UserUtils.GetUser(ctx, id);

                if (user == null)
                {
                    TempData[Message(MessageType.Error)] = "Wybrany użytkownik nie istnieje.";
                    return RedirectToAction("Users", "Admin");
                }

                if (UserUtils.IsLastAdmin(ctx, id))
                {
                    TempData[Message(MessageType.Warning)] = "Nie można usunąć ostatniego administratora.";
                    return RedirectToAction("Users", "Admin");
                }

                ctx.Users.DeleteOnSubmit(user);
                ctx.SubmitChanges();

                TempData[Message(MessageType.Success)] = "Pomyślnie usunięto uzytkownika.";
                return RedirectToAction("Users", "Admin");
            }
        }

        [Authorize]
        public ActionResult LoadActivationLinkPartialView(int id)
        {
            using (var ctx = DbHelper.DataContext)
            {
                User user = UserUtils.GetUser(ctx, id);
                return PartialView("_ActivationLinkPartialView", user);
            }
        }

        [Authorize]
        public ActionResult LoadPartialView(string operation, int id)
        {
            using (var ctx = DbHelper.DataContext)
            {
                DataLoadOptions options = new DataLoadOptions();
                options.LoadWith<User>(x => x.Role);
                ctx.LoadOptions = options;

                User user = new User();

                if (operation != "Create")
                {
                    user = UserUtils.GetUser(ctx, id);

                    if (user == null)
                    {
                        return HttpNotFound();
                    }
                }

                ViewBag.Roles = RoleUtils.GetRoles(ctx);

                return PartialView($"_{operation}UserPartialView", user);
            }
        }
    }
}