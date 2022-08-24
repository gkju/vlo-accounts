using System.Threading.Tasks;
using AccountsData.Models.DataModels;
using Duende.IdentityServer.Events;
using Duende.IdentityServer.Extensions;
using Duende.IdentityServer.Services;
using IdentityModel;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace VLO_BOARDS.Areas.Auth.Manage;

[ApiController]
[Area("Auth")]
[Route("api/[area]/[controller]")]
public class Logout : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly ILogger<ChangePassword> _logger;
    private readonly Captcha _captcha;
    private readonly IIdentityServerInteractionService _interactionService;
    private readonly IEventService _events;

    public Logout(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager, 
        ILogger<ChangePassword> logger,
        Captcha captcha,
        IIdentityServerInteractionService interactionService,
        IEventService eventsService)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _logger = logger;
        _captcha = captcha;
        _interactionService = interactionService;
        _events = eventsService;
    }
    
    /// <summary>
    /// Returns bad request if user interaction is required for logout
    /// </summary>
    /// <param name="logoutId"></param>
    /// <returns></returns>
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(LogoutResult), 200)]
    public async Task<IActionResult> OnGet(string logoutId)
    {
        var request = await _interactionService.GetLogoutContextAsync(logoutId);
        if (request?.ShowSignoutPrompt == false || !User.Identity.IsAuthenticated)
        {
            return await OnPost(logoutId);
        }

        return BadRequest();
    }

    [HttpPost]
    [Authorize]
    [ProducesResponseType(typeof(LogoutResult), 200)]
    public async Task<IActionResult> OnPost(string logoutId)
    {
        await _signInManager.SignOutAsync();
        _logger.LogInformation("User logged out.");

        var request = await _interactionService.GetLogoutContextAsync(logoutId);
        if (request != null)
        {
            return Ok(new
            {
                PostLogoutRedirectUri = request.PostLogoutRedirectUri,
                SignOutIframeUrl = request.SignOutIFrameUrl
            });
        }

        return Ok();
    }
}

public class LogoutResult
{
    public string PostLogoutRedirectUri { get; set; }
    public string SignOutIFrameUrl { get; set; }
}