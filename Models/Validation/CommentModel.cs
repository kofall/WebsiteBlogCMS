using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using WebsiteBlogCMS.Data;


namespace WebsiteBlogCMS.Models.Validation
{
    public class CommentModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Imię jest wymage.")]
        [MaxLength(100, ErrorMessage = "Imię nie może przekraczać 100 znaków.")]
        public string AuthorName { get; set; }

        [Required(ErrorMessage = "Proszę wprowadzić swój komentarz")]
        [MaxLength(5000, ErrorMessage = "Komentarz nie może przekraczać 5000 znaków.")]
        public string Content { get; set; }

        public int? ParentId { get; set; }
        public int PostId { get; set; }
        public int? AuthorId { get; set; }

        public bool Published { get; set; }
        public DateTime PublishedAt { get; set; }

        public bool IsVisible { get; set; }

        // Navigation properties
        public User Author { get; set; }
        public Comment ParentComment { get; set; }
        public Post Post { get; set; }
    }
}