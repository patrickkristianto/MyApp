using Microsoft.AspNetCore.Mvc.Rendering;

namespace Applications.ViewModel
{
    public class GenerateLicenseVM
    {
        public string UserId { get; set; }
        public string LicenseType { get; set; }
        public List<SelectListItem> UserList { get; set; }
    }
}
