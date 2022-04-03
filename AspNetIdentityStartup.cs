using System.Net;
using System.Threading.Tasks;
using AccountsData.Data;
using AccountsData.Models.DataModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace VLO_BOARDS;

public static class AspNetIdentityStartup
{
    public static string AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._+&*()";

    public static IServiceCollection AddAspNetIdentity(this IServiceCollection services)
    {
        services.AddIdentity<ApplicationUser, IdentityRole>(options => options.SignIn.RequireConfirmedAccount = true)
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddErrorDescriber<Internationalization.PolishIdentityErrorDescriber>()
            .AddDefaultTokenProviders();
            
        services.Configure<IdentityOptions>(options =>
        {
            options.Password.RequireDigit = true;
            options.Password.RequireLowercase = true;
            options.Password.RequireNonAlphanumeric = true;
            options.Password.RequireUppercase = true;
            options.Password.RequiredLength = 8;
            options.Password.RequiredUniqueChars = 4;
        });

        services.Configure<IdentityOptions>(options =>
        {
            options.User.AllowedUserNameCharacters = AllowedUserNameCharacters;
            options.User.RequireUniqueEmail = true;
        });

        services.ConfigureApplicationCookie(options =>
        {
            options.LoginPath = "/Login";
            options.LogoutPath = "/Logout";
            options.Events.OnRedirectToLogin = context =>
            {
                context.Response.StatusCode = (int) HttpStatusCode.Unauthorized;
                return Task.CompletedTask;
            };
            options.Events.OnRedirectToAccessDenied = context =>
            {
                context.Response.StatusCode = (int) HttpStatusCode.Forbidden;
                return Task.CompletedTask;
            };
        });
        
        return services;
    }
}