using PagedList;
using System.Collections.Generic;
using System.Data.Linq;
using System.Linq;
using System.Web.Mvc;
using WebsiteBlogCMS.Classes;
using WebsiteBlogCMS.Data;
using WebsiteBlogCMS.Models;
using static WebsiteBlogCMS.Classes.DataHelper;

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

                DataLoadOptions options = new DataLoadOptions();
                options.LoadWith<Post>(x => x.User);
                options.LoadWith<Post>(x => x.PostCategories);
                options.LoadWith<Post>(x => x.PostTags);
                options.LoadWith<Post>(x => x.EditorsPicks);
                options.LoadWith<Post>(x => x.TopMonthPicks);
                options.LoadWith<Post>(x => x.TopPicks);
                options.LoadWith<PostCategory>(x => x.Category);
                options.LoadWith<PostTag>(x => x.Tag);
                options.LoadWith<SliderPick>(x => x.Slider);
                ctx.LoadOptions = options;

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

        public ActionResult Categories(int? page)
        {
            int pageSize = 10;
            int pageNumber = (page ?? 1);

            using (var ctx = DbHelper.DataContext)
            {
                IPagedList<Category> list = CategoryUtils.GetAssociatedCategories(ctx).ToPagedList(pageNumber, pageSize);
                return View(list);
            }
        }

        public ActionResult Tags(int? page)
        {
            int pageSize = 10;
            int pageNumber = (page ?? 1);

            using (var ctx = DbHelper.DataContext)
            {
                IPagedList<Tag> list = TagUtils.GetAssociatedTags(ctx).ToPagedList(pageNumber, pageSize);
                return View(list);
            }
        }
    }
}