using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebsiteBlogCMS.Models
{
    public class PickOrderItemViewModel
    {
        public int ItemId { get; set; }
        public string Title { get; set; }
        public bool IsSelected { get; set; }
        public int Position { get; set; }
    }
}