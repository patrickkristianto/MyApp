using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Threading.Tasks;
using Applications.Data;
using Applications.LicenseAuthorization;

namespace Applications.Authorization
{
    public class PremiumLicenseHandler : AuthorizationHandler<PremiumLicense>
    {
        private readonly DBContext _context;

        public PremiumLicenseHandler(DBContext context)
        {
            _context = context;
        }

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, PremiumLicense requirement)
        {
            var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return;
            }

            var hasPremiumLicense = await _context.Licenses
                .AnyAsync(l => l.UserId == userId && l.IsActive && l.SubscriptionLevel == "Premium" && l.ExpirationDate > DateTime.UtcNow);

            if (hasPremiumLicense)
            {
                context.Succeed(requirement);
            }
        }
    }
}