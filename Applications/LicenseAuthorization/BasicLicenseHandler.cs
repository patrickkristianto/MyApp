using Applications.Models;
using Applications.Services;
using Azure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Security.Claims;

namespace Applications.LicenseAuthorization
{
    public class BasicLicenseHandler : AuthorizationHandler<BasicLicense>
    {
        private readonly LicenseServices _licenseServices;

        public BasicLicenseHandler(LicenseServices licenseServices)
        {
            _licenseServices = licenseServices;
        }

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, BasicLicense requirement)
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
                    SubscriptionLevel = "Basic"
                };
                var response = await _licenseServices.PostData("checkinglicense", content);
                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<CheckingLicense>();
                    if (result != null && result.IsAuthorized)
                    {
                        context.Succeed(requirement);
                    }
                }
            }
            catch (Exception ex)
            {
                if (context.Resource is AuthorizationFilterContext filterContext)
                {
                    filterContext.HttpContext.Items["ErrorMessage"] = "The license server is currently unavailable. Please try again later.";
                }
                context.Fail();
            }
        }
    }
}
