using Applications.Models;
using Applications.ViewModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Applications.Controllers
{
    public class AccountController : Controller
    {
        private readonly SignInManager<Users> sM;
        private readonly UserManager<Users> uM;
        private readonly RoleManager<IdentityRole> rM;

        public AccountController(SignInManager<Users> sM, UserManager<Users> uM, RoleManager<IdentityRole> rM)
        {
            this.sM = sM;
            this.uM = uM;
            this.rM = rM;
        }

        public IActionResult Login()
        {
            return View();
        }

        public IActionResult AccessDenied()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginVM vm)
        {
            if (ModelState.IsValid)
            {
                var result = await sM.PasswordSignInAsync(vm.email, vm.password, vm.rememberme, false);
                if (result.Succeeded)
                {
                    var user = await uM.FindByEmailAsync(vm.email);
                    if (user != null)
                    {
                        // Check if the user is in the Admin role
                        if (await uM.IsInRoleAsync(user, "Admin"))
                        {
                            return RedirectToAction("Index", "Admin");
                        }
                        else
                        {
                            return RedirectToAction("Index", "User");
                        }
                    }
                }
                else
                {
                    ModelState.AddModelError("", "Please input the correct email and password!");
                    return View(vm);
                }
            }
            return View(vm);
        }
        public IActionResult Register()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Register(RegisterVM vm)
        {
            if (ModelState.IsValid)
            {
                // Create a new user
                Users user = new Users
                {
                    FullName = vm.name,
                    Email = vm.email,
                    UserName = vm.email
                };

                var result = await uM.CreateAsync(user, vm.password);
                if (result.Succeeded)
                {
                    // Check if the "User" role exists and create it if it doesn't
                    string roleName = "User";
                    if (!await rM.RoleExistsAsync(roleName))
                    {
                        await rM.CreateAsync(new IdentityRole(roleName));
                    }

                    // Assign the user to the "User" role
                    await uM.AddToRoleAsync(user, roleName);

                    return RedirectToAction("Login", "Account");
                }
                else
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError("", error.Description);
                    }
                    return View(vm);
                }
            }
            return View(vm);
        }
        public IActionResult Verify()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Verify(EmailVerificationVM vm)
        {
            if (ModelState.IsValid)
            {
                var user = await uM.FindByEmailAsync(vm.email);
                if(user != null)
                {
                    return RedirectToAction("ResetPassword", "Account", new { username = user.UserName });
                }
                else
                {
                    ModelState.AddModelError("", "Email is not registered!");
                    return View(vm);
                }
            }
            return View(vm);
        }
        public IActionResult ResetPassword(string username)
        {
            if (string.IsNullOrEmpty(username))
            {
                return RedirectToAction("Verify", "Accoiunt");
            }
            return View(new ResetPasswordVM { email = username });
        }
        [HttpPost]
        public async Task<IActionResult> ResetPassword(ResetPasswordVM vm)
        {
            if(ModelState.IsValid)
            {
                var user = await uM.FindByNameAsync(vm.email);
                if(user != null)
                {
                    var result = await uM.RemovePasswordAsync(user);
                    if (result.Succeeded)
                    {
                        result = await uM.AddPasswordAsync(user, vm.newpassword);
                        return RedirectToAction("Login", "Account");
                    }
                    else
                    {
                        foreach(var error in result.Errors)
                        {
                            ModelState.AddModelError("", error.Description);
                        }
                        return View(vm);
                    }
                }
                else
                {
                    ModelState.AddModelError("", "Email is not registered!");
                    return View(vm);
                }
            }
            else
            {
                ModelState.AddModelError("", "Something went wrong");
                return View(vm);
            }
        }
        public async Task<IActionResult> Logout()
        {
            await sM.SignOutAsync();
            return RedirectToAction("Login","Account");
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UserList()
        {
            var users = await uM.Users.ToListAsync();
            var usersWithRoles = new List<UserListVM>();

            foreach (var user in users)
            {
                var roles = await uM.GetRolesAsync(user);
                usersWithRoles.Add(new UserListVM
                {
                    User = user,
                    Roles = roles.ToList()
            });
            }
            return View(usersWithRoles);
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> EditUser(string id)
{
            var user = await uM.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            var model = new EditUserVM
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditUser(EditUserVM vm)
        {
            bool isAdmin = User.IsInRole("Admin");
            if (isAdmin && string.IsNullOrEmpty(vm.Password) && string.IsNullOrEmpty(vm.ConfirmPassword))
            {
                ModelState.Remove(nameof(vm.Password));
                ModelState.Remove(nameof(vm.ConfirmPassword));
            }
            if (ModelState.IsValid)
            {
                var user = await uM.FindByIdAsync(vm.Id);
                if (user == null)
                {
                    return NotFound();
                }

                user.FullName = vm.FullName;
                user.Email = vm.Email;
                user.FullName = vm.FullName;
                user.Email = vm.Email;
                if (!string.IsNullOrEmpty(vm.Password) && vm.Password == vm.ConfirmPassword)
                {
                    var passwordHasher = new PasswordHasher<Users>();
                    user.PasswordHash = passwordHasher.HashPassword(user, vm.Password);
                }

                var result = await uM.UpdateAsync(user);
                if (result.Succeeded)
                {
                    return RedirectToAction("UserList");
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }
            return View(vm);
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteUser(string id)
        {
            var user = await uM.FindByIdAsync(id);
            if (user == null) return NotFound();

            await uM.DeleteAsync(user);
            return RedirectToAction("UserList");
        }
    }
}
