using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace Applications.ViewModel
{
    public class LoginVM
    {
        [Required(ErrorMessage = "Please input the email.")]
        [Display(Name = "Email")]
        [EmailAddress]
        public string email { get; set; }

        [Required (ErrorMessage = "Please input the password.")]
        [Display(Name = "Password")]
        [DataType(DataType.Password)]
        public string password { get; set; }

        [Display(Name = "Remember me?")]
        public bool rememberme {  get; set; }
    }
}
