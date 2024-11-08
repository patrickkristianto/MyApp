using Applications.Authorization;
using Applications.Data;
using Applications.LicenseAuthorization;
using Applications.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddHttpClient();

builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<DBContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
);
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("BasicLicensePolicy", policy =>
        policy.Requirements.Add(new BasicLicense()));

    options.AddPolicy("PremiumLicensePolicy", policy =>
        policy.Requirements.Add(new PremiumLicense()));
});
builder.Services.AddControllersWithViews();

builder.Services.AddIdentity<Users, IdentityRole>(options =>
{
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 8;
    options.Password.RequireUppercase = false;
    options.Password.RequireLowercase = false;
    options.User.RequireUniqueEmail = true;
    options.SignIn.RequireConfirmedAccount = false;
    options.SignIn.RequireConfirmedEmail = false;
    options.SignIn.RequireConfirmedPhoneNumber = false;
})
.AddEntityFrameworkStores<DBContext>()
.AddDefaultTokenProviders();

builder.Services.AddScoped<IAuthorizationHandler, BasicLicenseHandler>();
builder.Services.AddScoped<IAuthorizationHandler, PremiumLicenseHandler>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var configuration = app.Configuration;

    await SuperUser.SeedSuperUser(services, configuration);
}

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}");
app.MapControllerRoute(
    name: "accessDenied",
    pattern: "AccessDenied",
    defaults: new { controller = "Account", action = "AccessDenied" });


app.Run();