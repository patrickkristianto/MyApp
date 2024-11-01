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


        [HttpPost("generate")]
        public async Task<IActionResult> GenerateLicense([FromBody] LicenseCreateRequest request)
        {
            Licenses newLicense = new Licenses
            {
                LicenseKey = Guid.NewGuid().ToString(),
                SubscriptionLevel = request.SubscriptionLevel,
                ExpirationDate = DateTime.UtcNow.AddMonths(1),
                UserId = null
            };

            if (!string.IsNullOrEmpty(request.UserId))
            {
                var user = await _context.Users.FindAsync(request.UserId);
                if (user == null)
                    return NotFound("User not found");

                newLicense.UserId = user.Id;
            }

            _context.Licenses.Add(newLicense);
            await _context.SaveChangesAsync();

            return Ok(new { LicenseKey = newLicense.LicenseKey, ExpirationDate = newLicense.ExpirationDate });
        }

        [HttpPost("revoke")]
        public async Task<IActionResult> RevokeLicense([FromBody] string licenseKey)
        {
            var license = await _context.Licenses.FirstOrDefaultAsync(l => l.LicenseKey == licenseKey);
            if (license == null) return NotFound("License not found");

            license.IsActive = false;
            await _context.SaveChangesAsync();

            return Ok("License revoked successfully");
        }

        [HttpPost("assign")]
        public async Task<IActionResult> AssignLicense([FromBody] LicenseAssignRequest request)
        {
            var license = await _context.Licenses.FirstOrDefaultAsync(l => l.LicenseKey == request.LicenseKey);
            var user = await _context.Users.FindAsync(request.UserId);

            if (license == null || user == null)
                return NotFound("License or User not found");

            var activeLicense = await _context.Licenses
                .Where(l => l.UserId == user.Id && l.IsActive && l.ExpirationDate > DateTime.UtcNow)
                .FirstOrDefaultAsync();

            var expiredLicense = await _context.Licenses
                .Where(l => l.UserId == user.Id && !l.IsActive && l.ExpirationDate <= DateTime.UtcNow)
                .FirstOrDefaultAsync();

            if (activeLicense != null)
            {
                return Conflict("User already has an active license that has not expired.");
            }

            if (expiredLicense != null)
            {
                expiredLicense.IsActive = true;
                expiredLicense.ExpirationDate = DateTime.UtcNow.AddMonths(1);
                await _context.SaveChangesAsync();

                return Ok("Expired license renewed successfully.");
            }

            license.UserId = user.Id;
            license.IsActive = true;
            license.ExpirationDate = DateTime.UtcNow.AddMonths(1);
            await _context.SaveChangesAsync();

            return Ok("License assigned to user successfully");
        }
    }
}
