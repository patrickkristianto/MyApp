using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;

namespace LicenseServer.Models;

public partial class AspNetUser : IdentityUser
{
    public string FullName { get; set; } = null!;
}
