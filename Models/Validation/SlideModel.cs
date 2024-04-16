using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace WebsiteBlogCMS.Models.Validation
{
    public class SlideModel
    {
        [Key]
        public int Id { get; set; }

        [MaxLength(255, ErrorMessage = "Tytuł nie może być dłuższy niż 255 znaków.")]
        [Required(ErrorMessage = "Tytuł jest wymagany.")]
        public string Title { get; set; }

        [MaxLength(ErrorMessage = "Opis nie może być dłuższy.")]
        [Required(ErrorMessage = "Opis jest wymagany.")]
        public string Description { get; set; }

        [MaxLength(ErrorMessage = "Zdjęcie slajdu jest wymagane.")]
        public byte[] Image { get; set; }
    }
}