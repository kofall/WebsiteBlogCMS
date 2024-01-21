using System.Data.Linq;
using System.Linq;
using System.Web.Mvc;
using WebsiteBlogCMS.Classes;
using WebsiteBlogCMS.Data;
using WebsiteBlogCMS.Models;

namespace WebsiteBlogCMS.Controllers
{
    public class HomeController : Controller
    {
        // GET: Home
        public ActionResult Index()
        {
            using (var ctx = DbHelper.DataContext)
            {
                HomeContent content = new HomeContent();

                content.SliderPicks = (from data in ctx.Sliders
                                       join dataPicks in ctx.SliderPicks on data.id equals dataPicks.sliderId
                                       select data).ToList();

                content.CategoryPicks = (from data in ctx.Categories
                                         join dataPicks in ctx.CategorySettings on data.id equals dataPicks.categoryId
                                         select data).ToList();

                content.EditorsPicks = (from data in ctx.Posts
                                        join dataPicks in ctx.EditorsPicks on data.id equals dataPicks.postId
                                        select data).ToList();

                content.TopMonthPicks = (from data in ctx.Posts
                                         join dataPicks in ctx.TopMonthPicks on data.id equals dataPicks.postId
                                         select data).ToList();

                content.TopPicks = (from data in ctx.Posts
                                    join dataPicks in ctx.TopPicks on data.id equals dataPicks.postId
                                    select data).ToList();

                return View(content);
            }
        }
    }
}