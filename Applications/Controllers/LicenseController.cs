using Applications.Models;
using Applications.Services;
using Applications.ViewModel;
using Azure.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System.Net.Http;

public class LicenseController : Controller
{
    private readonly LicenseServices _licenseServices;
    private readonly UserManager<Users> uM;
    private readonly IMemoryCache _cache;

    public LicenseController(LicenseServices licenseServices, UserManager<Users> uM)
    {
        _licenseServices = licenseServices;
        this.uM = uM;
    }
    [Authorize(Roles = "Admin")]
    [HttpGet]
    public async Task<IActionResult> LicenseList()
    {
        var licensesWithFullName = new List<LicenseVM>();
        var response = await _licenseServices.GetData("list");
        if (response.IsSuccessStatusCode)
        {
            var licenses = await response.Content.ReadFromJsonAsync<List<LicenseVM>>();
            if(licenses != null)
            {
                foreach (var license in licenses)
                {
                    if (!string.IsNullOrEmpty(license.userId))
                    {
                        var user = await uM.Users
                                           .FirstOrDefaultAsync(u => u.Id == license.userId);
                        license.users = user;
                    }
                    licensesWithFullName.Add(license);
                }
            }
        }
        return View(licensesWithFullName);
    }
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Generate()
    {
        var users = await uM.Users.ToListAsync();
        var vm = new GenerateLicenseVM
        {
            UserList = users.Select(u => new SelectListItem
            {
                Value = u.Id,
                Text = u.FullName
            }).ToList()
        };

        return View(vm);
    }
    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<IActionResult> Generate(string userId, string licenseType)
    {
        if(userId == null)
        {
            userId = "";
        }
        var request = new
        {
            UserId = userId,
            SubscriptionLevel = licenseType
        };
        var response = await _licenseServices.PostData("generate", request);
        if (response.IsSuccessStatusCode)
        {
            return RedirectToAction("LicenseList", "License");
        }
        ViewBag.Message = "Error generating license.";
        return RedirectToAction("LicenseList", "License");
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<IActionResult> Revoke(string licensekey)
    {
        try
        {
            var content = new
            {
                Keys = licensekey
            };
            var response = await _licenseServices.PostData("revokelicense", content);
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<ServiceOperation>();
                if (result != null)
                {
                    if (result.isSuccess)
                        return RedirectToAction("LicenseList", "License");
                    else
                    {
                        TempData["ErrorMessage"] = result.errorMessage;
                        return RedirectToAction("LicenseList", "License");
                    }

                }
                else
                {
                    TempData["ErrorMessage"] = "An error happened. Please try again.";
                    return RedirectToAction("LicenseList", "License");
                }
            }
            else
            {
                TempData["ErrorMessage"] = "The license server is currently unavailable. Please try again later.";
                return RedirectToAction("LicenseList", "License");

            }
        }
        catch
        {
            TempData["ErrorMessage"] = "The license server is currently unavailable. Please try again later.";
            return RedirectToAction("LicenseList", "License");
        }
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<IActionResult> Assign(string licenseKey)
    {
        var users = await uM.Users.ToListAsync();
        var vm = new AssignLicenseVM
        {
            UserList = users.Select(u => new SelectListItem
            {
                Value = u.Id,
                Text = u.FullName
            }).ToList(),
            Licensekey = licenseKey

        };

        return View(vm);
    }
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> AssignLicense(string userId, string licenseKey)
    {
        var content = new
        {
            LicenseKey = licenseKey,
            UserId = userId
        };
        var response = await _licenseServices.PostData("assign", content);

        if (response.IsSuccessStatusCode)
        {
            return RedirectToAction("LicenseList", "License");
        }
        ViewBag.Message = "Error assigning license.";
        return RedirectToAction("LicenseList", "License");
    }
    
}