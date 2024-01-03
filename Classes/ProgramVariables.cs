using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using static WebsiteBlogCMS.Classes.Enums;

namespace WebsiteBlogCMS.Classes
{
    sealed class ProgramVariables
    {
        public static bool DEBUG { get; } = true;
        public static DatabaseType DATABASE { get; } = DatabaseType.Local;
        public static CompanyType COMPANY { get; } = CompanyType.Test;
    }
}