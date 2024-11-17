using Azure.Core;
using LicenseServer.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel;

namespace LicenseServer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LicensesController : ControllerBase
    {
        private readonly ApplicationContext _context;

        public LicensesController(ApplicationContext context)
        {
            _context = context;
        }

        [HttpGet("getstatus")]
        public IActionResult GetStatus()
        {
            return Ok("License Service is running.");
        }

        [HttpGet("list")]
        public IActionResult GetLicenses([FromQuery] string? userId = null)
        {
            var result = new List<Licenses>();
            
            var licenses = _context.Licenses.ToListAsync().Result.ToList();
            if (licenses != null)
            {
                result = licenses;
                if (!string.IsNullOrEmpty(userId))
                {
                    result = result.Where(_ => _.UserId != null && _.UserId == userId && !_.IsRevoked).ToList();
                }
            }
            
            return Ok(result);
        }

        [HttpPost("generate")]
        public async Task<IActionResult> GenerateLicense([FromBody] LicenseRequest request)
        {
            try
            {
                Licenses newLicense = new Licenses
                {
                    LicenseKey = Guid.NewGuid().ToString(),
                    SubscriptionLevel = request.SubscriptionLevel,
                    UserId = null,
                    IsRevoked = false
                };

                if (!string.IsNullOrEmpty(request.UserId))
                {
                    var user = await _context.Users.FindAsync(request.UserId);
                    if (user != null)
                        newLicense.UserId = user.Id;
                }
                _context.Licenses.Add(newLicense);
                await _context.SaveChangesAsync();
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            return Ok(new { isSuccess = true, errorMessage = string.Empty });
        }

        //[HttpPost("revoke")]
        //public async Task<IActionResult> RevokeLicense([FromBody] Key licenseKey)
        //{
        //    var license = await _context.Licenses.FirstOrDefaultAsync(l => l.LicenseKey == licenseKey.Keys);
        //    if (license == null) return NotFound("License not found");

        //    license.IsActive = false;
        //    license.UserId = null;
        //    license.UserId = null;
        //    await _context.SaveChangesAsync();

        //    return Ok("License revoked successfully");
        //}

        [HttpPost("assign")]
        public async Task<IActionResult> AssignLicense([FromBody] LicenseAssignRequest request)
        {
            var license = await _context.Licenses.FirstOrDefaultAsync(l => l.LicenseKey == request.LicenseKey);
            var user = await _context.Users.FindAsync(request.UserId);

            if (license == null || user == null)
                return NotFound("License or User not found");

            license.UserId = user.Id;
            await _context.SaveChangesAsync();

            return Ok("License assigned to user successfully");
        }

        [HttpPost("checkinglicense")]
        public async Task<IActionResult> checkinglicense([FromBody] LicenseRequest request)
        {
            bool data = false;
            if(request.SubscriptionLevel == "Premium")
            {
                data = await _context.Licenses
                .AnyAsync(l => l.UserId == request.UserId && l.IsActive && l.SubscriptionLevel == "Premium" && l.ExpirationDate > DateTime.UtcNow && !l.IsRevoked);
            }
            else if(request.SubscriptionLevel == "Basic")
            {
                data = await _context.Licenses
                .AnyAsync(l => l.UserId == request.UserId && l.IsActive && (l.SubscriptionLevel == "Basic" || l.SubscriptionLevel == "Premium") && l.ExpirationDate > DateTime.UtcNow && !l.IsRevoked);
            }
            else
            {
                return BadRequest();
            }
            return Ok(new
            {
                IsAuthorized = data
            });
        }
        [HttpPost("activatekey")]
        public async Task<IActionResult> activatelicense([FromBody] Key key)
        {
            var license = await _context.Licenses.FirstOrDefaultAsync(l => l.LicenseKey == key.Keys);
            if (license == null)
            {
                var result = new
                {
                    isSuccess = false,
                    errorMessage = "License not found"
                };
                return Ok(result);

            }
            license.IsActive = true;
            license.ExpirationDate = DateTime.UtcNow.AddMonths(1);
            await _context.SaveChangesAsync();
            return Ok(new { isSuccess = true, errorMessage = string.Empty});
        }

        [HttpPost("renewallicense")]
        public async Task<IActionResult> renewlicenses([FromBody] RenewLicenses param)
        {
            var license = await _context.Licenses.FirstOrDefaultAsync(l => l.LicenseKey == param.Keys);
            if (license == null)
            {
                var result = new
                {
                    isSuccess = false,
                    errorMessage = "License not found"
                };
                return Ok(result);

            }
            license.ExpirationDate = param.duration;
            await _context.SaveChangesAsync();
            return Ok(new { isSuccess = true, errorMessage = string.Empty });
        }

        [HttpPost("revokelicense")]
        public async Task<IActionResult> revokelicenses([FromBody] RenewLicenses param)
        {
            var license = await _context.Licenses.FirstOrDefaultAsync(l => l.LicenseKey == param.Keys);
            if (license == null)
            {
                var result = new
                {
                    isSuccess = false,
                    errorMessage = "License not found"
                };
                return Ok(result);

            }
            license.IsRevoked = true;
            license.IsActive = false;
            await _context.SaveChangesAsync();
            return Ok(new { isSuccess = true, errorMessage = string.Empty });
        }
    }
}
