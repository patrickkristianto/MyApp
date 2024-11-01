using System;
using System.Collections.Generic;

namespace Applications.Models;

public partial class License
{
    public int LicenseId { get; set; }

    public string LicenseKey { get; set; } = null!;

    public string SubscriptionLevel { get; set; } = null!;

    public DateTime ExpirationDate { get; set; }

    public bool IsActive { get; set; }

    public string? UserId { get; set; }
}
