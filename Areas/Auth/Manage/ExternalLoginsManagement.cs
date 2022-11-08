using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AccountsData.Models.DataModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
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

    /// <summary>
    /// Returns all the available external login providers as well as the currently used ones
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    [ProducesResponseType(typeof(ExternalLoginInfo), 200)]
    public async Task<ActionResult<ExternalLoginInfo>> OnGetAsync()
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
        var sAvailableLogins = new List<SimplifiedAuthenticationScheme>();
        foreach (var login in availableLogins)
        {
            sAvailableLogins.Add(new SimplifiedAuthenticationScheme(login));
        }
        return Ok(new ExternalLoginInfo {CurrentLogins = currentLogins.ToList(), AvailableLogins = sAvailableLogins});
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
        if (!result.Succeeded || result.Errors.Any())
        {
            ModelState.AddModelError(Constants.AccountError, Constants.IneligibleForLoginRemoval);
            return this.GenBadRequestProblem();
        }

        await _userManager.UpdateAsync(user);
        
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
    [HttpGet]
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
    public async Task<IActionResult> OnCallbackGetAsync(string returnUrl = null, string remoteError = null)
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

        user.HandleExternalAuth(info);
        
        await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

        redirectUrl = _emailTemplates.GenerateUrl("/ExternalLogins").ToString();
        
        _logger.LogInformation("User with ID {UserId} has added an external login with {Provider}", user.Id, info.LoginProvider);
        
        return Redirect(redirectUrl);
    }
}

public class ExternalLoginInfo
{
    public List<UserLoginInfo> CurrentLogins { get; set; }
    public List<SimplifiedAuthenticationScheme> AvailableLogins { get; set; }
}

public class SimplifiedAuthenticationScheme
{
    public string Name { get; set; }
    public string DisplayName { get; set; }
    
    public SimplifiedAuthenticationScheme(AuthenticationScheme scheme)
    {
        Name = scheme.Name;
        DisplayName = scheme.DisplayName;
    }
}