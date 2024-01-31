using PagedList;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebsiteBlogCMS.Data;

namespace WebsiteBlogCMS.Models
{
    public class UserDetailsModel
    {
        public User User { get; set; }
        public IPagedList<Post> UserPosts { get; set; }
    }
}