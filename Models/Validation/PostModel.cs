using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace WebsiteBlogCMS.Models.Validation
{
    public class PostModel
    {
        [Key]
        public int Id { get; set; }

        public bool Published { get; set; }

        public bool IsVisible { get; set; }

        [MaxLength(100, ErrorMessage = "Tytuł nie może być dłuższy niż 255 znaków.")]
        [Required(ErrorMessage = "Tytuł jest wymagany.")]
        public string Title { get; set; }

        [Required(ErrorMessage = "Treść jest wymagana.")]
        public string Content { get; set; }

        [MaxLength(ErrorMessage = "Zdjęcie postu jest wymagane.")]
        public byte[] Image { get; set; }
    }
}