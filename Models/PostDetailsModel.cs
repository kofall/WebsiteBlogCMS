using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebsiteBlogCMS.Data;
using WebsiteBlogCMS.Models.Validation;

namespace WebsiteBlogCMS.Models
{
    public class PostDetailsModel
    {
        public Post Post { get; set; }
        public List<Post> UserPosts { get; set; } = new List<Post>();
        public List<Comment> Comments { get; set; } = new List<Comment>();
        public int CommentsCount { get; set; }
        public CommentModel CommentModel { get; set; }
    }
}