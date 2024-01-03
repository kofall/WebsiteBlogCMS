using System.Collections.Generic;
using static WebsiteBlogCMS.Classes.Enums;
using static WebsiteBlogCMS.Classes.ProgramVariables;

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
                    .Replace("#server#", ServerNames[DATABASE])
                    .Replace("#dbname#", DatabaseNames[COMPANY])
                    .Replace("#userid#", ServerUserId)
                    .Replace("#password#", ServerPassword);
            }
        }

        private static readonly Dictionary<DatabaseType, string> ServerNames = new Dictionary<DatabaseType, string>
        {
            { DatabaseType.Local, "(localdb)\\Local" },
            { DatabaseType.Public, string.Empty }
        };

        private static readonly Dictionary<CompanyType, string> DatabaseNames = new Dictionary<CompanyType, string>
        {
            { CompanyType.Production, "BlogDB" },
            { CompanyType.Test, "BlogDBTest" }
        };

        private static readonly string ServerUserId = "admin";
        private static readonly string ServerPassword = "admin";
    }
}