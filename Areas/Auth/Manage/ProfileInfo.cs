using System;
using System.Threading.Tasks;
using AccountsData.Models.DataModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace VLO_BOARDS.Areas.Auth.Manage;

[ApiController]
[Area("Auth")]
[Route("api/[area]/[controller]")]
[Authorize]
public class ProfileInfo : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<GenerateRecoveryCodes> _logger;

    public ProfileInfo(
        UserManager<ApplicationUser> userManager,
        ILogger<GenerateRecoveryCodes> logger)
    {
        _userManager = userManager;
        _logger = logger;
    }

    /// <summary>
    /// Get the current user's profile information.
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    [ProducesResponseType(typeof(ApplicationUser), 200)]
    public async Task<IActionResult> OnGetAsync()
    {
        var user = await _userManager.GetUserAsync(User);
        user.PasswordHash = "-";
        return Ok(user);
    }
}