using System;
using System.Collections.Generic;
using System.Data.Linq;
using System.Linq;
using System.Web.Mvc;
using System.Web.Security;
using System.Web.UI.WebControls;
using WebsiteBlogCMS.Classes;
using WebsiteBlogCMS.Data;
using WebsiteBlogCMS.Models;
using WebsiteBlogCMS.Models.Validation;
using static WebsiteBlogCMS.Classes.DataHelper;
using static WebsiteBlogCMS.Classes.Enums;

namespace WebsiteBlogCMS.Controllers
{
    public class AdminController : Controller
    {
        // GET: Admin
        [Authorize]
        public ActionResult Index()
        {
            using (var ctx = DbHelper.DataContext)
            {
                User user = UserUtils.GetUser(ctx, User.Identity.Name);
                ViewBag.IsUserConfigured = UserUtils.IsUserConfigured(user);

                AdminStatistics statistics = new AdminStatistics();
                statistics.TotalUsers = ctx.Users.Where(x => x.login != null).Count();
                statistics.TotalPosts = ctx.Posts.Count();
                statistics.TotalCategories = ctx.Categories.Count();
                statistics.TotalTags = ctx.Tags.Count();
                statistics.TotalComments = ctx.Comments.Count();
                return View(statistics);
            }
        }

        public ActionResult Login()
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index");
            }
            return View();
        }

        [HttpPost]
        public ActionResult Login(UserAccount account)
        {
            using (var ctx = DbHelper.DataContext)
            {
                if(string.IsNullOrEmpty(account.Login) || string.IsNullOrEmpty(account.Password))
                {
                    TempData[Message(MessageType.Warning)] = "Podany login oraz hasło są niepoprawne.";
                    return View();
                }

                User user = UserUtils.AuthenticateUser(ctx, account.Login, account.Password);

                if (user == null)
                {
                    TempData[Message(MessageType.Warning)] = "Podany login oraz hasło są niepoprawne.";
                    return View();
                }

                FormsAuthentication.SetAuthCookie(user.login, false);
                return RedirectToAction("Index");
            }
        }

        [Authorize]
        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();
            return View("Login");
        }

        public ActionResult Activate(string activationCode)
        {
            using (var ctx = DbHelper.DataContext)
            {
                FormsAuthentication.SignOut();
                if (!string.IsNullOrEmpty(activationCode))
                {
                    if (TokenService.ValidateToken(activationCode))
                    {
                        int userId = ctx.UserTokens.Where(x => x.token.Equals(activationCode)).Select(x => x.userId).Single();
                        User user = UserUtils.GetUser(ctx, userId);

                        if (user != null)
                        {
                            ViewBag.Token = activationCode;
                            return View(user);
                        }
                    }
                    else
                    {
                        User user = ctx.Users.Where(x => x.activationLink.Contains(activationCode)).SingleOrDefault();
                        if (user != null)
                        {
                            ctx.Users.DeleteOnSubmit(user);
                            ctx.SubmitChanges();
                        }
                    }
                }

                return View("ActivationFailure");
            }
        }

        [HttpPost]
        public ActionResult Activate(User user, string token)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }

            if (TokenService.ValidateToken(token))
            {
                using (var ctx = DbHelper.DataContext)
                {
                    if (!UserUtils.IsUserValid(ctx, user))
                    {
                        TempData[Message(MessageType.Warning)] = "Użytkownik o takim loginie już istnieje.";
                        ViewBag.Token = token;
                        return View(user);
                    }

                    var userData = UserUtils.GetUser(ctx, user.id);

                    if (userData != null)
                    {
                        userData.login = user.login;
                        userData.passwordHash = CryptHelper.Encrypt(user.passwordHash);
                        userData.registeredAt = DateTime.Now;
                        userData.activationLink = null;

                        TokenService.RemoveToken(token);

                        ctx.SubmitChanges();

                        FormsAuthentication.SetAuthCookie(user.login, false);

                        TempData[Message(MessageType.Success)] = "Pomyślnie utworzono użytkownika.";
                        return RedirectToAction("Index", "Admin");
                    }
                }
            }
            return View("ActivationFailure");
        }

        [Authorize]
        public ActionResult Settings(string login)
        {
            if (login != User.Identity.Name)
            {
                TempData[Message(MessageType.Warning)] = "Nie posiadasz uprawnień do korzystania z tego widoku.";
                return RedirectToAction("Index", "Admin");
            }

            using (var ctx = DbHelper.DataContext)
            {
                User user = UserUtils.GetUser(ctx, User.Identity.Name);
                EditSettingsModel model = new EditSettingsModel();
                model.Id = user.id;
                model.RoleId = user.roleId;
                model.FirstName = user.firstName;
                model.LastName = user.lastName;
                model.Login = user.login;
                model.Intro = user.intro;
                model.Profile = user.profile;
                model.ProfileImage = user.profileImage?.ToArray() ?? null;

                ViewBag.Roles = RoleUtils.GetRoles(ctx);
                return View(model);
            }
        }

        [Authorize]
        public ActionResult Users()
        {
            if (!UserUtils.HasPermission(1, User.Identity.Name))
            {
                TempData[Message(MessageType.Warning)] = "Nie posiadasz uprawnień do korzystania z tego widoku.";
                return RedirectToAction("Index");
            }

            using (var ctx = DbHelper.DataContext)
            {
                DataLoadOptions options = new DataLoadOptions();
                options.LoadWith<User>(x => x.Role);
                ctx.LoadOptions = options;

                List<User> users = UserUtils.GetUsers(ctx);
                return View(users);
            }
        }

        [Authorize]
        public ActionResult Sliders()
        {
            if (!UserUtils.HasPermission(2, User.Identity.Name))
            {
                TempData[Message(MessageType.Warning)] = "Nie posiadasz uprawnień do korzystania z tego widoku.";
                return RedirectToAction("Index");
            }

            using (var ctx = DbHelper.DataContext)
            {
                List<Slider> sliders = SliderUtils.GetSliders(ctx);
                return View(sliders);
            }
        }

        [Authorize]
        public ActionResult SlidersPick()
        {
            if (!UserUtils.HasPermission(2, User.Identity.Name))
            {
                TempData[Message(MessageType.Warning)] = "Nie posiadasz uprawnień do korzystania z tego widoku.";
                return RedirectToAction("Index");
            }

            using (var ctx = DbHelper.DataContext)
            {
                var data = ctx.Sliders.ToList();
                var dataPicks = ctx.SliderPicks.ToList();

                var viewModel = data.Select(x => new PickOrderItemViewModel
                {
                    ItemId = x.id,
                    Title = x.title,
                    IsSelected = dataPicks.Any(sp => sp.sliderId == x.id),
                    Position = dataPicks.FirstOrDefault(sp => sp.sliderId == x.id)?.position ?? 1000
                }).ToList();

                ViewBag.Header = "Wybór slajdów";
                ViewBag.Controller = "Slider";
                return View("PickPage", viewModel);
            }
        }

        [Authorize]
        public ActionResult CategoriesPick()
        {
            if (!UserUtils.HasPermission(2, User.Identity.Name))
            {
                TempData[Message(MessageType.Warning)] = "Nie posiadasz uprawnień do korzystania z tego widoku.";
                return RedirectToAction("Index");
            }

            using (var ctx = DbHelper.DataContext)
            {
                var data = CategoryUtils.GetAssociatedCategories(ctx);
                var dataPicks = ctx.CategorySettings.ToList();

                var viewModel = data.Select(x => new PickOrderItemViewModel
                {
                    ItemId = x.id,
                    Title = x.title,
                    IsSelected = dataPicks.Any(sp => sp.categoryId == x.id),
                    Position = dataPicks.FirstOrDefault(sp => sp.categoryId == x.id)?.position ?? 1000
                }).ToList();

                ViewBag.Header = "Wybór kategorii";
                ViewBag.Controller = "Category";
                return View("PickPage", viewModel);
            }
        }

        [Authorize]
        public ActionResult EditorsPick()
        {
            if (!UserUtils.HasPermission(2, User.Identity.Name))
            {
                TempData[Message(MessageType.Warning)] = "Nie posiadasz uprawnień do korzystania z tego widoku.";
                return RedirectToAction("Index");
            }

            using (var ctx = DbHelper.DataContext)
            {
                var data = ctx.Posts.Where(x => x.isVisible).ToList();
                var dataPicks = ctx.EditorsPicks.ToList();

                var viewModel = data.Select(x => new PickOrderItemViewModel
                {
                    ItemId = x.id,
                    Title = x.title,
                    IsSelected = dataPicks.Any(sp => sp.postId == x.id),
                    Position = dataPicks.FirstOrDefault(sp => sp.postId == x.id)?.position ?? 1000
                }).ToList();

                ViewBag.Header = "Wybór redakcyjny";
                ViewBag.Controller = "Post";
                ViewBag.Part = "Editors";
                return View("PickPage", viewModel);
            }
        }

        [Authorize]
        public ActionResult MonthPostsPick()
        {
            if (!UserUtils.HasPermission(2, User.Identity.Name))
            {
                TempData[Message(MessageType.Warning)] = "Nie posiadasz uprawnień do korzystania z tego widoku.";
                return RedirectToAction("Index");
            }

            using (var ctx = DbHelper.DataContext)
            {
                var data = ctx.Posts.Where(x => x.isVisible).ToList();
                var dataPicks = ctx.TopMonthPicks.ToList();

                var viewModel = data.Select(x => new PickOrderItemViewModel
                {
                    ItemId = x.id,
                    Title = x.title,
                    IsSelected = dataPicks.Any(sp => sp.postId == x.id),
                    Position = dataPicks.FirstOrDefault(sp => sp.postId == x.id)?.position ?? 1000
                }).ToList();

                ViewBag.Header = "Wybór postów miesiąca";
                ViewBag.Controller = "Post";
                ViewBag.Part = "TopMonth";
                return View("PickPage", viewModel);
            }
        }

        [Authorize]
        public ActionResult Trends()
        {
            if (!UserUtils.HasPermission(2, User.Identity.Name))
            {
                TempData[Message(MessageType.Warning)] = "Nie posiadasz uprawnień do korzystania z tego widoku.";
                return RedirectToAction("Index");
            }

            using (var ctx = DbHelper.DataContext)
            {
                var data = ctx.Posts.Where(x => x.isVisible).ToList();
                var dataPicks = ctx.TopPicks.ToList();

                var viewModel = data.Select(x => new PickOrderItemViewModel
                {
                    ItemId = x.id,
                    Title = x.title,
                    IsSelected = dataPicks.Any(sp => sp.postId == x.id),
                    Position = dataPicks.FirstOrDefault(sp => sp.postId == x.id)?.position ?? 1000
                }).ToList();

                ViewBag.Header = "Wybór trendów";
                ViewBag.Controller = "Post";
                ViewBag.Part = "Trends";
                return View("PickPage", viewModel);
            }
        }

        [Authorize]
        public ActionResult Categories()
        {
            if (!UserUtils.HasPermission(2, User.Identity.Name))
            {
                TempData[Message(MessageType.Warning)] = "Nie posiadasz uprawnień do korzystania z tego widoku.";
                return RedirectToAction("Index");
            }

            using (var ctx = DbHelper.DataContext)
            {
                List<Category> categories = CategoryUtils.GetCategories(ctx);
                return View(categories);
            }
        }

        [Authorize]
        public ActionResult Tags()
        {
            if (!UserUtils.HasPermission(2, User.Identity.Name))
            {
                TempData[Message(MessageType.Warning)] = "Nie posiadasz uprawnień do korzystania z tego widoku.";
                return RedirectToAction("Index");
            }

            using (var ctx = DbHelper.DataContext)
            {
                List<Tag> tags = TagUtils.GetTags(ctx);
                return View(tags);
            }
        }

        [Authorize]
        public ActionResult Posts()
        {
            if (!UserUtils.HasPermission(3, User.Identity.Name))
            {
                TempData[Message(MessageType.Warning)] = "Nie posiadasz uprawnień do korzystania z tego widoku.";
                return RedirectToAction("Index");
            }

            using (var ctx = DbHelper.DataContext)
            {
                DataLoadOptions options = new DataLoadOptions();
                options.LoadWith<Post>(x => x.User);
                ctx.LoadOptions = options;

                User user = UserUtils.GetUser(ctx, User.Identity.Name);

                List<Post> posts = new List<Post>();
                if(user.roleId == 3)
                {
                    posts = PostUtils.GetUserPosts(ctx, user.id, false);
                }
                else
                {
                    posts = PostUtils.GetPosts(ctx, false);
                }
                return View(posts);
            }
        }
    }
}