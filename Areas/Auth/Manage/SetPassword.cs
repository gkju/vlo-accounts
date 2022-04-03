using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using AccountsData.Models.DataModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using VLO_BOARDS.Extensions;

namespace VLO_BOARDS.Areas.Auth.Manage;

[ApiController]
[Area("Auth")]
[Route("api/[area]/[controller]")]
[Authorize]
public class SetPassword : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;

    public SetPassword(
        UserManager<ApplicationUser> userManager, 
        SignInManager<ApplicationUser> signInManager)
    {
        _userManager = userManager;
        _signInManager = signInManager;
    }

    /// <summary>
    /// Adds a password (for users using only hardware/external authentication)
    /// </summary>
    /// <param name="resetPasswordInput"></param>
    /// <returns></returns>
    [HttpPost]
    public async Task<IActionResult> OnPostAsync(ResetPasswordInputModel resetPasswordInput)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            this.GenInternalError();
        }

        if (await _userManager.HasPasswordAsync(user))
        {
            ModelState.AddModelError(Constants.AccountError, Constants.HasPasswordStatus);
            return this.GenBadRequestProblem();
        }

        var addPasswordResult = await _userManager.AddPasswordAsync(user, resetPasswordInput.NewPassword);
        if (!addPasswordResult.Succeeded)
        {
            foreach (var error in addPasswordResult.Errors)
            {
                ModelState.AddModelError(Constants.AccountError, error.Description);
            }
            return this.GenBadRequestProblem();
        }
        
        await _signInManager.RefreshSignInAsync(user);

        return Ok();
    }
}

public class ResetPasswordInputModel
{
    [Required] [DataType(DataType.Password)]
    public string NewPassword { get; set; }
}