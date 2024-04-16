using PagedList;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebsiteBlogCMS.Data;

namespace WebsiteBlogCMS.Models
{
    public class PostsOfCategoryModel
    {
        public Category Category { get; set; }
        public Category ParentCategory { get; set; } = null;
        public List<Category> SubCategories { get; set; } = new List<Category>();
        public IPagedList<Post> Posts { get; set; }
    }
}