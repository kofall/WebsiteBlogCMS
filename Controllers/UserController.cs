using System;
using System.Data.Linq;
using System.Linq;
using System.Web.Mvc;
using WebsiteBlogCMS.Classes;
using WebsiteBlogCMS.Data;

namespace WebsiteBlogCMS.Controllers
{
    public class UserController : Controller
    {
        // GET: User
        public ActionResult Index()
        {
            return View();
        }

        private (string, string) GenerateActivationLink()
        {
            var token = TokenService.GenerateToken();
            return (token, $"https://localhost:44313/User/Activate?activationCode={token}");
        }

        [HttpPost]
        public ActionResult Create(User newUser)
        {
            if (ModelState.IsValid)
            {
                using (var ctx = new BlogDBDataContext(DbHelper.ConnectionBlog))
                {
                    var activationLink = GenerateActivationLink();
                    newUser.activationLink = activationLink.Item2;

                    ctx.Users.InsertOnSubmit(newUser);
                    ctx.SubmitChanges();

                    User user = ctx.Users.OrderByDescending(u => u.id).FirstOrDefault();

                    UserToken userToken = new UserToken();
                    userToken.userId = user.id;
                    userToken.token = activationLink.Item1;
                    userToken.expireDate = DateTime.Now.AddDays(1);

                    ctx.UserTokens.InsertOnSubmit(userToken);
                    ctx.SubmitChanges();

                    // Redirect to the user list or perform any other action
                    TempData["ShowActivationLink"] = true;
                    TempData["UserToShow"] = user.id;

                    return RedirectToAction("Users", "Admin");
                }
            }
            return View("HttpNotFound");
        }

        public ActionResult Activate(string activationCode)
        {
            if (!string.IsNullOrEmpty(activationCode))
            {
                using (var ctx = new BlogDBDataContext(DbHelper.ConnectionBlog))
                {
                    if (TokenService.ValidateToken(activationCode))
                    {
                        int userId = ctx.UserTokens.Where(x => x.token.Equals(activationCode)).Select(x => x.userId).Single();
                        var user = ctx.Users.SingleOrDefault(u => u.id == userId);

                        if (user != null)
                        {
                            ViewBag.Token = activationCode;
                            return View(user);
                        }
                    }
                    else
                    {
                        User user = ctx.Users.Where(x => x.activationLink.Contains(activationCode)).SingleOrDefault();
                        if(user != null)
                        {
                            ctx.Users.DeleteOnSubmit(user);
                            ctx.SubmitChanges();
                        }
                    }
                }                
            }

            return View("ActivationFailure");
        }

        [HttpPost]
        public ActionResult Activate(User user, string token)
        {
            if (ModelState.IsValid)
            {
                using (var ctx = new BlogDBDataContext(DbHelper.ConnectionBlog))
                {
                    if (TokenService.ValidateToken(token))
                    {
                        bool isValid = !ctx.Users.Where(u => u.login.Equals(user.login)).Any();

                        if(!isValid)
                        {
                            ViewBag.ErrorMessage = "Użytkownik o takim loginie już istnieje!";
                            ViewBag.Token = token;
                            return View(user);
                        }

                        var userData = ctx.Users.FirstOrDefault(u => u.id == user.id);

                        if (userData != null)
                        {
                            userData.login = user.login;
                            userData.passwordHash = CryptHelper.Encrypt(user.passwordHash);
                            userData.registeredAt = DateTime.Now;
                            userData.activationLink = null;

                            TokenService.RemoveToken(token);

                            ctx.SubmitChanges();

                            return RedirectToAction("Index", "Admin");
                        }
                    }
                    return View("ActivationFailure");
                }
            }

            return View();
        }

        [HttpPost]
        public ActionResult Edit(User user)
        {
            if (ModelState.IsValid)
            {
                using (var ctx = new BlogDBDataContext(DbHelper.ConnectionBlog))
                {
                    var userData = ctx.Users.Where(u => u.id.Equals(user.id)).SingleOrDefault();

                    if (user == null)
                    {
                        return HttpNotFound();
                    }

                    userData.roleId = user.roleId;

                    ctx.SubmitChanges();
                }
            }
            return RedirectToAction("Users", "Admin");
        }

        public ActionResult Delete(int id)
        {
            using (var ctx = new BlogDBDataContext(DbHelper.ConnectionBlog))
            {
                User user = ctx.Users.Where(x => x.id.Equals(id)).SingleOrDefault();

                if(user != null)
                {
                    ctx.Users.DeleteOnSubmit(user);
                    ctx.SubmitChanges();
                }
            }
            return RedirectToAction("Users", "Admin");
        }

        public ActionResult LoadActivationLinkPartialView(int id)
        {
            using (var ctx = new BlogDBDataContext(DbHelper.ConnectionBlog))
            {
                var user = ctx.Users.Where(x => x.id.Equals(id)).Single();

                return PartialView("_ActivationLinkPartialView", user);
            }
        }

        public ActionResult LoadUserPartialView(string operation, int id)
        {
            using (var ctx = new BlogDBDataContext(DbHelper.ConnectionBlog))
            {
                DataLoadOptions options = new DataLoadOptions();
                options.LoadWith<User>(u => u.Role);
                ctx.LoadOptions = options;

                User user = new User();

                if (operation != "Create")
                {
                    user = ctx.Users.Where(x => x.id.Equals(id)).SingleOrDefault();

                    if (user == null)
                    {
                        return HttpNotFound();
                    }
                }

                ViewBag.Roles = ctx.Roles.ToList();

                return PartialView($"_{operation}UserPartialView", user);
            }
        }
    }
}
