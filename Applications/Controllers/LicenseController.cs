using Applications.Models;
using Applications.ViewModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

public class LicenseController : Controller
{
    private readonly HttpClient _httpClient;
    private readonly UserManager<Users> uM;


    public LicenseController(HttpClient httpClient, UserManager<Users> uM)
    {
        _httpClient = httpClient;
        this.uM = uM;
    }
    [Authorize(Roles = "Admin")]
    [HttpGet]
    public async Task<IActionResult> LicenseList()
    {
        var licenses = await _httpClient.GetFromJsonAsync<List<LicenseVM>>("https://localhost:7010/api/licenses/list");
        var licensesWithFullName = new List<LicenseVM>();

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

        var response = await _httpClient.PostAsJsonAsync("https://localhost:7010/api/licenses/generate", request); 
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
        var content = new
        {
            Keys = licensekey
        };
        var response = await _httpClient.PostAsJsonAsync("https://localhost:7010/api/licenses/revoke", content);
        if (response.IsSuccessStatusCode)
        {
            return RedirectToAction("LicenseList", "License");
        }
        ViewBag.Message = "Error revoking license.";
        return RedirectToAction("LicenseList", "License");
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
        var response = await _httpClient.PostAsJsonAsync("https://localhost:7010/api/licenses/assign", content);
        if (response.IsSuccessStatusCode)
        {
            return RedirectToAction("LicenseList", "License");
        }
        ViewBag.Message = "Error assigning license.";
        return RedirectToAction("LicenseList", "License");
    }
}