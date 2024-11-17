using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace Applications.ViewModel
{
    public class RenewLicenseVM
    {
        [Display(Name = "License Key")]
        public string Licensekey { get; set; }

        [Display(Name = "Duration")]
        [DataType(DataType.Date)]
        public DateTime Duration { get; set; }
    }
}
