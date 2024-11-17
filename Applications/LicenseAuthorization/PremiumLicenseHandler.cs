using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Applications.LicenseAuthorization;
using Applications.Services;
using Applications.Models;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Http;

namespace Applications.Authorization
{
    public class PremiumLicenseHandler : AuthorizationHandler<PremiumLicense>
    {
        private readonly LicenseServices _licenseServices;
        public PremiumLicenseHandler(LicenseServices licenseServices)
        {
            _licenseServices = licenseServices;
        }

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, PremiumLicense requirement)
        {
            try
            {
                var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    return;
                }
                var content = new
                {
                    UserId = userId,
                    SubscriptionLevel = "Premium"
                };
                var response = await _licenseServices.PostData("checkinglicense", content);
                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<CheckingLicense>();
                    if (result != null && result.IsAuthorized)
                    {
                        context.Succeed(requirement);
                    }
                    else
                    {

                        context.Fail();
                    }
                }
                else
                {
                    context.Fail();
                    if (context.Resource is HttpContext httpcontext)
                    {
                        httpcontext.Items["ErrorMessage"] = "The license server is currently unavailable. Please try again later.";
                    }

                }
            }
            catch (Exception ex)
            {
                context.Fail();
                if (context.Resource is HttpContext httpcontext)
                {
                    httpcontext.Items["ErrorMessage"] = "The license server is currently unavailable. Please try again later.";
                }
            }
        }
    }
}