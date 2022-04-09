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
public class Disable2FA : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    public readonly ILogger<Disable2FA> _logger;

    public Disable2FA(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager, 
        ILogger<Disable2FA> logger)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _logger = logger;
    }

    /// <summary>
    /// Disables 2FA :c
    /// </summary>
    /// <returns></returns>
    [HttpPost]
    [ProducesResponseType(200)]
    [ProducesResponseType(typeof(BadRequestResult), 400)]
    [ProducesResponseType(typeof(ObjectResult), 500)]
    public async Task<IActionResult> OnPostAsync()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return this.GenInternalError();
        }

        if (!await _userManager.GetTwoFactorEnabledAsync(user))
        {
            ModelState.AddModelError(Constants.AccountError, Constants.TwoFaNotEnabled);
            return this.GenBadRequestProblem();
        }

        var disable2FAResult = await _userManager.SetTwoFactorEnabledAsync(user, false);
        if (!disable2FAResult.Succeeded)
        {
            return this.GenInternalError();
        }
        
        _logger.LogInformation("User with ID '{UserId}' has disabled 2FA.", user.Id);

        return Ok();
    }
}