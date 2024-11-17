using Applications.Models;
using Applications.Services;
using Applications.ViewModel;
using Azure.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Polly;
using System.Diagnostics;
using System.Security.Claims;

namespace Applications.Controllers
{
    public class UserController : Controller
    {
        private readonly ILogger<UserController> _logger;
        private readonly LicenseServices _licenseServices; 
        private readonly UserManager<Users> uM;


        public UserController(ILogger<UserController> logger, LicenseServices licenseServices, UserManager<Users> uM)
        {
            _logger = logger;
            _licenseServices = licenseServices;
            this.uM = uM;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> BasicPage()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            try
            {
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
                        return View();
                    }
                    else
                    {
                        return RedirectToAction("AccessDenied", "Account");
                    }
                }
                else
                {
                    TempData["ErrorMessage"] = "The license server is currently unavailable. Please try again later.";
                    return RedirectToAction("ErrorPage", "Account");

                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "The license server is currently unavailable. Please try again later.";
                return RedirectToAction("ErrorPage", "Account");
            }
        }

        public async Task<IActionResult> PremiumPage()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            try
            {
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
                        return View();
                    }
                    else
                    {
                        return RedirectToAction("AccessDenied", "Account");
                    }
                }
                else
                {
                    TempData["ErrorMessage"] = "The license server is currently unavailable. Please try again later.";
                    return RedirectToAction("ErrorPage", "Account");

                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "The license server is currently unavailable. Please try again later.";
                return RedirectToAction("ErrorPage", "Account");
            }
        }

        [Authorize]
        public async Task<IActionResult> BuyLicenses()
        {
            var licensesWithFullName = new List<LicenseVM>();
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            try
            {

                var response = await _licenseServices.GetData($"list?userId={userId}");
                if (response.IsSuccessStatusCode)
                {
                    var licenses = await response.Content.ReadFromJsonAsync<List<LicenseVM>>();
                    if (licenses != null)
                    {
                        licensesWithFullName = licenses;
                    }
                }
                return View(licensesWithFullName);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "The license server is currently unavailable. Please try again later.";
                return RedirectToAction("ErrorPage", "Account");
            }
        }
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Activate(string licenseKey)
        {
            try
            {
                var content = new
                {
                    Keys = licenseKey
                };
                var response = await _licenseServices.PostData("activatekey", content);
                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<ServiceOperation>();
                    if (result != null)
                    {
                        if(result.isSuccess)
                            return RedirectToAction("BuyLicenses", "User");
                        else
                        {
                            TempData["ErrorMessage"] = result.errorMessage;
                            return RedirectToAction("BuyLicenses", "User");
                        }

                    }
                    else
                    {
                        TempData["ErrorMessage"] = "An error happened. Please try again.";
                        return RedirectToAction("BuyLicenses", "User");
                    }
                }
                else
                {
                    TempData["ErrorMessage"] = "The license server is currently unavailable. Please try again later.";
                    return RedirectToAction("BuyLicenses", "User");

                }
            }
            catch
            {
                TempData["ErrorMessage"] = "The license server is currently unavailable. Please try again later.";
                return RedirectToAction("BuyLicenses", "User");
            }
            
        }

        [Authorize]
        public async Task<IActionResult> Buy()
        {
            var user = await uM.GetUserAsync(User);
            var vm = new BuyLicensesVM
            {
                Name = user.FullName
            };

            return View(vm);
        }
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> BuyLKeys(string licenseType)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var content = new
                {
                    UserId = userId,
                    SubscriptionLevel = licenseType
                };
                var response = await _licenseServices.PostData("generate", content);
                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<ServiceOperation>();
                    if (result != null)
                    {
                        if (result.isSuccess)
                            return RedirectToAction("BuyLicenses", "User");
                        else
                        {
                            TempData["ErrorMessage"] = result.errorMessage;
                            return RedirectToAction("BuyLicenses", "User");
                        }

                    }
                    else
                    {
                        TempData["ErrorMessage"] = "An error happened. Please try again.";
                        return RedirectToAction("BuyLicenses", "User");
                    }
                }
                else
                {
                    TempData["ErrorMessage"] = "The license server is currently unavailable. Please try again later.";
                    return RedirectToAction("BuyLicenses", "User");

                }
            }
            catch
            {
                TempData["ErrorMessage"] = "The license server is currently unavailable. Please try again later.";
                return RedirectToAction("BuyLicenses", "User");
            }
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Revoke(string licenseKey)
        {
            try
            {
                var content = new
                {
                    Keys = licenseKey
                };
                var response = await _licenseServices.PostData("revokelicense", content);
                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<ServiceOperation>();
                    if (result != null)
                    {
                        if (result.isSuccess)
                            return RedirectToAction("BuyLicenses", "User");
                        else
                        {
                            TempData["ErrorMessage"] = result.errorMessage;
                            return RedirectToAction("BuyLicenses", "User");
                        }

                    }
                    else
                    {
                        TempData["ErrorMessage"] = "An error happened. Please try again.";
                        return RedirectToAction("BuyLicenses", "User");
                    }
                }
                else
                {
                    TempData["ErrorMessage"] = "The license server is currently unavailable. Please try again later.";
                    return RedirectToAction("BuyLicenses", "User");

                }
            }
            catch
            {
                TempData["ErrorMessage"] = "The license server is currently unavailable. Please try again later.";
                return RedirectToAction("BuyLicenses", "User");
            }
        }

        [Authorize]
        [HttpPost]
        public IActionResult RenewLicenses(string licenseKey, DateTime existExpDate)
        {
            var vm = new RenewLicenseVM
            {
                Licensekey = licenseKey,
                Duration = existExpDate
            };
            return View(vm);

        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> RenewLicenseSave(RenewLicenseVM model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var content = new
                    {
                        Keys = model.Licensekey,
                        duration = model.Duration
                    };
                    var response = await _licenseServices.PostData("renewallicense", content);
                    if (response.IsSuccessStatusCode)
                    {
                        var result = await response.Content.ReadFromJsonAsync<ServiceOperation>();
                        if (result != null)
                        {
                            if (result.isSuccess)
                                return RedirectToAction("BuyLicenses", "User");
                            else
                            {
                                TempData["ErrorMessage"] = result.errorMessage;
                                return RedirectToAction("BuyLicenses", "User");
                            }

                        }
                        else
                        {
                            TempData["ErrorMessage"] = "An error happened. Please try again.";
                            return RedirectToAction("BuyLicenses", "User");
                        }
                    }
                    else
                    {
                        TempData["ErrorMessage"] = "The license server is currently unavailable. Please try again later.";
                        return RedirectToAction("BuyLicenses", "User");

                    }
                }
                catch
                {
                    TempData["ErrorMessage"] = "The license server is currently unavailable. Please try again later.";
                    return RedirectToAction("BuyLicenses", "User");
                }
            }

            TempData["ErrorMessage"] = "An error happened. Please try again.";
            return RedirectToAction("BuyLicenses", "User");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
