using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace WebsiteBlogCMS.Models.Validation
{
    public class TagModel
    {
        [Key]
        public int Id { get; set; }

        [MaxLength(100, ErrorMessage = "Tytuł nie może być dłuższy niż 100 znaków.")]
        [Required(ErrorMessage = "Tytuł jest wymagany.")]
        public string Title { get; set; }
    }
}