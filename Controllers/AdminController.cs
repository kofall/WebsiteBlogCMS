using System;
using System.Collections.Generic;
using System.Data.Linq;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Web;
using System.Web.Mvc;
using WebsiteBlogCMS.Classes;
using WebsiteBlogCMS.Data;
using static System.Collections.Specialized.BitVector32;

namespace WebsiteBlogCMS.Controllers
{
    public class AdminController : Controller
    {
        // GET: Admin
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Login()
        {
            return View();
        }

        public ActionResult Users()
        {
            using (var ctx = new BlogDBDataContext(DbHelper.ConnectionBlog))
            {
                DataLoadOptions options = new DataLoadOptions();
                options.LoadWith<User>(u => u.Role);
                ctx.LoadOptions = options;

                IEnumerable<User> users = ctx.Users.ToList();
                return View(users);
            }
        }

        public ActionResult Categories()
        {
            using (var ctx = new BlogDBDataContext(DbHelper.ConnectionBlog))
            {
                IEnumerable<Category> categories = ctx.Categories.ToList();
                return View(categories);
            }
        }

        public ActionResult Tags()
        {
            using (var ctx = new BlogDBDataContext(DbHelper.ConnectionBlog))
            {
                IEnumerable<Tag> tags = ctx.Tags.ToList();
                return View(tags);
            }
        }

        public ActionResult Posts()
        {
            using (var ctx = new BlogDBDataContext(DbHelper.ConnectionBlog))
            {
                DataLoadOptions options = new DataLoadOptions();
                options.LoadWith<Post>(u => u.User);
                ctx.LoadOptions = options;

                IEnumerable<Post> posts = ctx.Posts.ToList();
                return View(posts);
            }
        }
    }
}