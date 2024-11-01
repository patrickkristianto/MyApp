using System.ComponentModel.DataAnnotations;

namespace Applications.ViewModel
{
    public class RegisterVM
    {
        [Required(ErrorMessage ="Input your name.")]
        [Display(Name = "Name")]
        public string name { get; set; }

        [Required(ErrorMessage = "Input your email.")]
        [Display(Name = "Email")]
        [EmailAddress]
        public string email { get; set; }

        [Required(ErrorMessage = "Password is required.")]
        [StringLength(40, MinimumLength = 8, ErrorMessage = "The {0} must be at {2} and at max {1} characters long.")]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        [Compare("confirmpassword", ErrorMessage ="Password does not match")]
        public string password { get; set; }

        [Required(ErrorMessage ="Confirm password is required.")]
        [DataType (DataType.Password)]
        [Display(Name = "Confirm Password")]
        public string confirmpassword { get; set; }
    }
}
