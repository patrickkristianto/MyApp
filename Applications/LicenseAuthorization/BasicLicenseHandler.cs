using Applications.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Applications.LicenseAuthorization
{
    public class BasicLicenseHandler : AuthorizationHandler<BasicLicense>
    {
        private readonly DBContext _context;

        public BasicLicenseHandler(DBContext context)
        {
            _context = context;
        }

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, BasicLicense requirement)
        {
            var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return;
            }

            var hasBasicLicense = await _context.Licenses
                .AnyAsync(l => l.UserId == userId && l.IsActive && (l.SubscriptionLevel == "Basic" || l.SubscriptionLevel == "Premium")&& l.ExpirationDate > DateTime.UtcNow);

            if (hasBasicLicense)
            {
                context.Succeed(requirement);
            }
        }
    }
}
