namespace LicenseServer.Models
{
    public class LicenseCreateRequest
    {
        public string UserId { get; set; }
        public string SubscriptionLevel { get; set; }
    }
}
