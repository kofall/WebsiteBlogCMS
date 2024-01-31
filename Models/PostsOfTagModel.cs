using PagedList;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebsiteBlogCMS.Data;

namespace WebsiteBlogCMS.Models
{
    public class PostsOfTagModel
    {
        public Tag Tag { get; set; }
        public IPagedList<Post> Posts { get; set; }
    }
}