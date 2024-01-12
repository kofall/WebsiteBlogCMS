using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebsiteBlogCMS.Classes;
using WebsiteBlogCMS.Data;

namespace WebsiteBlogCMS.Models
{
    public class PostViewModel
    {
        public Post Post { get; set; }
        public List<CategoryObject> SelectedCategories { get; set; }

        public PostViewModel()
        {
            SelectedCategories = new List<CategoryObject>();
        }
    }

    public class CategoryObject
    {
        public string label { get; set; }
        public int value { get; set; }

        public CategoryObject(string label, int value)
        {
            this.label = label;
            this.value = value;
        }
    }
}