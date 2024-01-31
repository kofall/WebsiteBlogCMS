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

        public static BlogDBDataContext DataContext => new BlogDBDataContext(ConnectionBlog);
    }
}