using System.ComponentModel.DataAnnotations;
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
// 1984 controller
public class GdprDeleteUser : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly ILogger<GdprDeleteUser> _logger;

    public GdprDeleteUser(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager, 
        ILogger<GdprDeleteUser> logger)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _logger = logger;
    }
    
    public class InputModel
    {
        [DataType(DataType.Password)]
        public string? Password { get; set; }
    }

    /// <summary>
    /// Deletes the user and ALL HIS DATA PERMANENTLY
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    [HttpPost]
    public async Task<IActionResult> OnPostAsync(InputModel input)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return this.GenInternalError();
        }

        var requirePassword = await _userManager.HasPasswordAsync(user);
        if (requirePassword)
        {
            if (!await _userManager.CheckPasswordAsync(user, input.Password))
            {
                ModelState.AddModelError(Constants.UsernameOrPasswordError, Constants.InvalidUsernameOrPasswordStatus);
                return this.GenBadRequestProblem();
            }
        }

        var res = await _userManager.DeleteAsync(user);

        if (!res.Succeeded)
        {
            return this.GenInternalError();
        }

        await _signInManager.SignOutAsync();
        
        _logger.LogInformation("User with ID {UserId} has deleted his account", user.Id);

        return Ok();
    }
}