using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebsiteBlogCMS.Models
{
    public class AdminStatistics
    {
        public int TotalUsers { get; set; }
        public int TotalPosts { get; set; }
        public int TotalCategories { get; set; }
        public int TotalTags { get; set; }
        public int TotalComments { get; set; }
    }
}