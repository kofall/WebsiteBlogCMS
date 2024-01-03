using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebsiteBlogCMS.Classes
{
    sealed public class Enums
    {
        public enum DatabaseType
        {
            Local,
            Public
        }

        public enum CompanyType
        {
            Production,
            Test
        }
    }
}