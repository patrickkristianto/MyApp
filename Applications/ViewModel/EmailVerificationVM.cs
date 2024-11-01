using System.ComponentModel.DataAnnotations;

namespace Applications.ViewModel
{
    public class EmailVerificationVM
    {
        [Required(ErrorMessage ="Email is required. ")]
        [Display(Name = "Email")]
        [EmailAddress]
        public string email { get;set; }
    }
}
