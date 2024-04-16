using System.ComponentModel.DataAnnotations;

namespace WebsiteBlogCMS.Models
{
    public class UserAccount
    {
        [Required]
        public string Login { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        public UserAccount() { }

        public UserAccount(string login, string password)
        {
            Login = login;
            Password = password;
        }
    }
}