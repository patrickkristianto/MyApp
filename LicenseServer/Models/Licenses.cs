using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace LicenseServer.Models
{
    public class Licenses
    {
        public int LicenseId { get; set; }
        public string LicenseKey { get; set; }
        public string SubscriptionLevel { get; set; }
        public DateTime ExpirationDate { get; set; }
        public bool IsActive { get; set; }
        public string? UserId { get; set; }
        public virtual AspNetUser? User { get; set; }
    }
}
