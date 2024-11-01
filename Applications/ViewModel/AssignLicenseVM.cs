using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace Applications.ViewModel
{
    public class AssignLicenseVM
    {
        [Display(Name = "License Key")]
        public string Licensekey { get; set; }
        public List<SelectListItem> UserList { get; set; }
        public string UserId { get; set; }
    }
}
