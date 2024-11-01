using Applications.Models;
using Microsoft.AspNetCore.Identity;

namespace Applications.Data
{
    public class SuperUser
    {
        public static async Task SeedSuperUser(IServiceProvider serviceProvider, IConfiguration configuration)
        {
            // Use your custom user class 'Users' instead of IdentityUser
            var uM = serviceProvider.GetRequiredService<UserManager<Users>>();
            var rM = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            string email = configuration["SuperUser:Email"];
            string password = configuration["SuperUser:Password"];
            string roleName = "Admin";

            // Check if the role exists, if not, create it
            if (!await rM.RoleExistsAsync(roleName))
            {
                await rM.CreateAsync(new IdentityRole(roleName));
            }

            // Check if the user already exists
            var user = await uM.FindByEmailAsync(email);
            if (user == null)
            {
                // Create a new user if it does not exist
                user = new Users
                {
                    UserName = email,
                    Email = email,
                    EmailConfirmed = true,
                    FullName = "Super User"
                };
                await uM.CreateAsync(user, password);
            }

            // Check if the user is already in the role, if not, add them
            if (!await uM.IsInRoleAsync(user, roleName))
            {
                await uM.AddToRoleAsync(user, roleName);
            }
        }
    }
}
