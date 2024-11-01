using System.ComponentModel.DataAnnotations;

namespace Applications.ViewModel
{
    public class ResetPasswordVM
    {
        [Required(ErrorMessage ="Email is required.")]
        [Display(Name = "Email")]
        [EmailAddress]
        public string email { get; set; }

        [Required(ErrorMessage = "Password is reuired.")]
        [StringLength(40, MinimumLength = 8, ErrorMessage = "The {0} must be at {2} and at max {1} characters long.")]
        [DataType(DataType.Password)]
        [Display(Name = "New Password")]
        [Compare("confirmedpassword", ErrorMessage = "Password does not match.")]
        public string newpassword { get;set; }

        [Required(ErrorMessage = "Confirm password is required.")]
        [DataType(DataType.Password)]
        [Display(Name = "Confirm New Password")]
        public string confirmedpassword { get; set; }
    }
}
