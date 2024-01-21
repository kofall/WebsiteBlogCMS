using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebsiteBlogCMS.Data;

namespace WebsiteBlogCMS.Models
{
    public class HomeContent
    {
        public List<Slider> SliderPicks { get; set; }
        public List<Category> CategoryPicks { get; set; }
        public List<Post> EditorsPicks { get; set; }
        public List<Post> TopMonthPicks { get; set; }
        public List<Post> TopPicks { get; set; }
    }
}