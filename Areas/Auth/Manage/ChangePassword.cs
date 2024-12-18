﻿using System.ComponentModel.DataAnnotations;
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
    private readonly Captcha _captcha;

    public ChangePassword(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager, 
        ILogger<ChangePassword> logger,
        Captcha captcha)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _logger = logger;
        _captcha = captcha;
    }

    /// <summary>
    /// Changes the password (changes as in there was a different password previously)
    /// </summary>
    /// <param name="changePasswordInput"></param>
    /// <returns></returns>
    [HttpPost]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> OnPostAsync(ChangePasswordInputModel changePasswordInput)
    {
        if (await _captcha.VerifyCaptcha(changePasswordInput.CaptchaResponse) < Captcha.Threshold)
        {
            ModelState.AddModelError(Captcha.ErrorName, Captcha.ErrorStatus);
            return this.GenBadRequestProblem();
        }
        
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return this.GenInternalError();
        }
        
        var changePasswordResult = await _userManager.ChangePasswordAsync(user, changePasswordInput.OldPassword, changePasswordInput.NewPassword);
        
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

public class ChangePasswordInputModel
{
    [Required]
    [DataType(DataType.Password)]
    public string OldPassword { get; set; }
        
    [Required]
    [DataType(DataType.Password)]
    public string NewPassword { get; set; }
    
    [Required]
    public string CaptchaResponse { get; set; }
}