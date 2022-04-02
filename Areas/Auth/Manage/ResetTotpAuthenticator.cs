using System.Threading.Tasks;
using AccountsData.Models.DataModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using VLO_BOARDS.Extensions;

namespace VLO_BOARDS.Areas.Auth.Manage;

[ApiController]
[Area("Auth")]
[Route("api/[area]/[controller]")]
[Authorize]
public class ResetTotpAuthenticator : ControllerBase
{
    UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    ILogger<ResetTotpAuthenticator> _logger;

    public ResetTotpAuthenticator(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        ILogger<ResetTotpAuthenticator> logger)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _logger = logger;
    }

    [HttpPost]
    public async Task<IActionResult> OnPostAsync()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return this.GenInternalError();
        }

        await _userManager.SetTwoFactorEnabledAsync(user, false);
        await _userManager.ResetAuthenticatorKeyAsync(user);
        _logger.LogInformation("User with ID '{UserId}' has reset their authentication app key.", user.Id);
            
        await _signInManager.RefreshSignInAsync(user);
        return Ok();
    }
}