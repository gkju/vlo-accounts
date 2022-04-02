using System.Linq;
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
public class GenerateRecoveryCodes : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<GenerateRecoveryCodes> _logger;

    public GenerateRecoveryCodes(
        UserManager<ApplicationUser> userManager,
        ILogger<GenerateRecoveryCodes> logger)
    {
        _userManager = userManager;
        _logger = logger;
    }

    /// <summary>
    /// Generates recovery codes for use with TOTP
    /// </summary>
    /// <returns> Recovery codes </returns>
    [HttpGet]
    public async Task<IActionResult> OnGetAsync()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return this.GenInternalError();
        }

        var is2FAEnabled = await _userManager.GetTwoFactorEnabledAsync(user);
        if (!is2FAEnabled)
        {
            ModelState.AddModelError(Constants.AccountError, Constants.TwoFaNotEnabled);
            return this.GenBadRequestProblem();
        }

        var recoveryCodes = await _userManager.GenerateNewTwoFactorRecoveryCodesAsync(user, 10);
        
        _logger.LogInformation("User with ID '{UserId}' has generated new 2FA recovery codes.", user.Id);

        return Ok(recoveryCodes.ToArray());
    }
}