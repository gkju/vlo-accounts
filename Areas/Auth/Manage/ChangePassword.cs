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
public class ChangePassword : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly ILogger<ChangePassword> _logger;

    public ChangePassword(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager, 
        ILogger<ChangePassword> logger)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _logger = logger;
    }

    public class InputModel
    {
        [Microsoft.Build.Framework.Required]
        [DataType(DataType.Password)]
        public string OldPassword { get; set; }
        
        [Required]
        [DataType(DataType.Password)]
        public string NewPassword { get; set; }
    }

    /// <summary>
    /// Changes the password (changes as in there was a different password previously)
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
        
        var changePasswordResult = await _userManager.ChangePasswordAsync(user, input.OldPassword, input.NewPassword);
        
        if (!changePasswordResult.Succeeded)
        {
            foreach (var error in changePasswordResult.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return this.GenBadRequestProblem();
        }
        
        _logger.LogInformation("User with ID '{UserId}' has changed his password.", user.Id);

        return Ok();
    }
}