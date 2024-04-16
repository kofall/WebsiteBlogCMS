using System.ComponentModel.DataAnnotations;

namespace WebsiteBlogCMS.Models.Validation
{
    public class EditSettingsModel
    {
        [Key]
        public int Id { get; set; }
        
        public string Login { get; set; }

        public int RoleId { get; set; }

        [MaxLength(50, ErrorMessage = "Imię nie może przekraczać 50 znaków.")]
        [Required(ErrorMessage = "Imię jest wymagane.")]
        public string FirstName { get; set; }

        [MaxLength(50, ErrorMessage = "Nazwisko nie może przekraczać 50 znaków.")]
        [Required(ErrorMessage = "Nazwisko jest wymagane.")]
        public string LastName { get; set; }

        [MaxLength(100, ErrorMessage = "Wprowadzenie nie może przekraczać 100 znaków.")]
        [Required(ErrorMessage = "Wprowadzenie jest wymagane.")]
        public string Intro { get; set; } = "";

        [MaxLength(ErrorMessage = "Opis profilu jest zbyt długi.")]
        [Required(ErrorMessage = "Opis profilu jest wymagany.")]
        public string Profile { get; set; } = "";

        [MaxLength(ErrorMessage = "Zdjęcie profilowe jest za duże.")]
        public byte[] ProfileImage { get; set; }
    }
}
