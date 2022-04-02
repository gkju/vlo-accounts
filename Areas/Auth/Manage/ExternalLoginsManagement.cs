using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AccountsData.Models.DataModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;
using Microsoft.Extensions.Logging;
using VLO_BOARDS.Extensions;

namespace VLO_BOARDS.Areas.Auth.Manage;

[ApiController]
[Area("Auth")]
[Route("api/[area]/[controller]")]
[Authorize]
public class ExternalLoginsManagement : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly EmailTemplates _emailTemplates;
    private readonly ILogger<ExternalLoginsManagement> _logger;

    public ExternalLoginsManagement(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        EmailTemplates emailTemplates, 
        ILogger<ExternalLoginsManagement> logger)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _emailTemplates = emailTemplates;
        _logger = logger;
    }

    public class LoginInfo
    {
        public List<UserLoginInfo> CurrentLogins { get; set; }
        public List<AuthenticationScheme> AvailableLogins { get; set; }
    }

    /// <summary>
    /// Returns all the available external login providers as well as the currently used ones
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    public async Task<ActionResult<LoginInfo>> OnGetAsync()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            this.GenInternalError();
        }

        var currentLogins = await _userManager.GetLoginsAsync(user);
        var availableLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync())
            .Where(auth => currentLogins.All(ul => auth.Name != ul.LoginProvider))
            .ToList();
        return Ok(new LoginInfo {CurrentLogins = currentLogins.ToList(), AvailableLogins = availableLogins.ToList()});
    }

    /// <summary>
    /// Deletes an external login provider
    /// </summary>
    /// <param name="loginProvider"></param>
    /// <param name="providerKey"></param>
    /// <returns></returns>
    [HttpDelete]
    public async Task<IActionResult> OnDeleteAsync(string loginProvider, string providerKey)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            this.GenInternalError();
        }

        var result = await _userManager.RemoveLoginAsync(user, loginProvider, providerKey);
        if (!result.Succeeded)
        {
            ModelState.AddModelError(Constants.AccountError, Constants.IneligibleForLoginRemoval);
            return this.GenBadRequestProblem();
        }
        
        _logger.LogInformation("User with ID {UserId} has deleted {Provider} login from his account", user.Id, loginProvider);

        await _signInManager.RefreshSignInAsync(user);
        return Ok();
    }

    /// <summary>
    /// Requests a new challenge for adding external auth
    /// </summary>
    /// <param name="provider"></param>
    /// <returns></returns>
    [Route("GetChallenge")]
    [HttpPost]
    public async Task<IActionResult> OnPostAsync(string provider)
    {
        await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

        var redirectUrl = _emailTemplates.GenerateUrl("api/Auth/ExternalLoginsManagement/Callback").ToString();
        var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl, _userManager.GetUserId(User));
        return new ChallengeResult(provider, properties);
    }

    /// <summary>
    /// Handles the information returned by the external authentication provider
    /// </summary>
    /// <returns></returns>
    [Route("Callback")]
    [HttpGet]
    public async Task<IActionResult> OnCallbackGetAsync()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return this.GenInternalError();
        }

        var info = await _signInManager.GetExternalLoginInfoAsync(user.Id);
        if (info == null)
        {
            return this.GenInternalError();
        }

        string redirectUrl;
        
        var result = await _userManager.AddLoginAsync(user, info);
        if (!result.Succeeded)
        {
            ModelState.AddModelError(Constants.ExternalError, Constants.MultiAccountError);
            redirectUrl = _emailTemplates.GenerateUrl("/Manage/ExternalLogins",
                new Dictionary<string, string> {{"error", Constants.MultiAccountError}}).ToString();
            return Redirect(redirectUrl);
        }

        await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

        redirectUrl = _emailTemplates.GenerateUrl("/Manage/ExternalLogins").ToString();
        
        _logger.LogInformation("User with ID {UserId} has added an external login with {Provider}", user.Id, info.LoginProvider);
        
        return Redirect(redirectUrl);
    }
}