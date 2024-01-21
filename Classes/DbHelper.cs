using System;
using System.Data.Entity;
using System.Data.Linq;
using System.Transactions;
using WebsiteBlogCMS.Data;

namespace WebsiteBlogCMS.Classes
{
    public static class DbHelper
    {
        private static string ConnectionString = "Data Source=#server#;Initial Catalog=#dbname#;User ID=#userid#;Password=#password#";

        public static string ConnectionBlog
        {
            get
            {
                return ConnectionString
                    .Replace("#server#", ServerName)
                    .Replace("#dbname#", DatabaseName)
                    .Replace("#userid#", ServerUserId)
                    .Replace("#password#", ServerPassword);
            }
        }

        private static readonly string ServerName = "(localdb)\\Local";
        private static readonly string DatabaseName = "BlogDB";
        private static readonly string ServerUserId = "admin";
        private static readonly string ServerPassword = "admin";

        private static void LoadOptions(BlogDBDataContext context)
        {
            DataLoadOptions options = new DataLoadOptions();
            options.LoadWith<Post>(x => x.User);
            options.LoadWith<Post>(x => x.Comments);
            options.LoadWith<PostCategory>(x => x.Category);
            options.LoadWith<PostCategory>(x => x.Post);
            options.LoadWith<PostTag>(x => x.Tag);
            options.LoadWith<PostTag>(x => x.Post);
            options.LoadWith<User>(x => x.Role);
            options.LoadWith<User>(x => x.Comments);
            options.LoadWith<User>(x => x.UserTokens);
            options.LoadWith<TopPick>(x => x.Post);
            options.LoadWith<SliderPick>(x => x.Slider);
            options.LoadWith<TopMonthPick>(x => x.Post);
            options.LoadWith<EditorsPick>(x => x.Post);
            options.LoadWith<CategorySetting>(x => x.Category);
            context.LoadOptions = options;
        }

        public static BlogDBDataContext DataContext
        {
            get
            {
                BlogDBDataContext context = new BlogDBDataContext(ConnectionBlog);
                LoadOptions(context);
                return context;
            }
        }
    }
}