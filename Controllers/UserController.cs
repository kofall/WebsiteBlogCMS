using System;
using System.Linq;
using System.Web.Mvc;
using WebsiteBlogCMS.Classes;
using WebsiteBlogCMS.Data;
using static WebsiteBlogCMS.Classes.DataHelper;
using static WebsiteBlogCMS.Classes.Enums;

namespace WebsiteBlogCMS.Controllers
{
    public class UserController : Controller
    {
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
                return View("HttpNotFound");
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
                return View("HttpNotFound");
            }

            using (var ctx = DbHelper.DataContext)
            {
                var userData = UserUtils.GetUser(ctx, user.id);

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
        public ActionResult EditSettings(User user)
        {
            if (!ModelState.IsValid)
            {
                return View("HttpNotFound");
            }

            using (var ctx = DbHelper.DataContext)
            {
                var userData = UserUtils.GetUser(ctx, user.id);

                if (user == null)
                {
                    return View("HttpNotFound");
                }

                userData.firstName = user.firstName;
                userData.lastName = user.lastName;
                userData.intro = user.intro;
                userData.profile = user.profile;

                ctx.SubmitChanges();

                TempData[Message(MessageType.Success)] = "Pomyślnie zapisano zmiany.";
                return RedirectToAction("Settings", "Admin", new { id = user.id });
            }
        }

        [Authorize]
        [HttpPost]
        public ActionResult Delete(int id)
        {
            using (var ctx = DbHelper.DataContext)
            {
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