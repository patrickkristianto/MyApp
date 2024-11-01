using Applications.Models;

namespace Applications.ViewModel
{
    public class LicenseVM
    {
        public int licenseId { get; set; }
        public string licenseKey { get; set; }
        public string subscriptionLevel { get;set; }
        public DateTime expirationDate { get;set; }
        public bool isActive { get;set; }
        public string userId { get;set; }
        public Users users { get;set; }
    }
}
