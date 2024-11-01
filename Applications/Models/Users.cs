using Microsoft.AspNetCore.Identity;

namespace Applications.Models
{
    public class Users : IdentityUser
    {
        public string FullName { get; set; }
    }
}
