using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using AccountsData.Models.DataModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using VLO_BOARDS.Extensions;

namespace VLO_BOARDS.Areas.Auth
{
    [ApiController]
    [Area("Auth")]
    [Route("api/[area]/[controller]")]
    public class ResetPasswordController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly Captcha _captcha;

        public ResetPasswordController(UserManager<ApplicationUser> userManager, Captcha captcha)
        {
            _userManager = userManager;
            _captcha = captcha;
        }

        /// <summary>
        /// Changes the password to a new one based on input
        /// </summary>
        /// <param name="resetPasswordInput"></param>
        /// <returns> Either ok success or bad request with modelstate </returns>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult> OnPostAsync(ResetPasswordInputModel resetPasswordInput) {
            if (await _captcha.VerifyCaptcha(resetPasswordInput.CaptchaResponse) < Captcha.Threshold)
            {
                ModelState.AddModelError(Captcha.ErrorName, "Bad captcha");
                return this.GenBadRequestProblem();
            }
            
            
            var user = await _userManager.FindByEmailAsync(resetPasswordInput.Email);
            if (user == null)
            {
                // Don't reveal that the user does not exist
                return Ok("Success");
            }

            var result = await _userManager.ResetPasswordAsync(user, resetPasswordInput.Code, resetPasswordInput.Password);
            if (result.Succeeded)
            {
                return Ok("Success");
            }
            
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(error.Code, error.Description);
            }
            
            return this.GenBadRequestProblem();
        }
    }
    
    public class ResetPasswordInputModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "{0} musi mieć co najmniej {2} i maksymalnie {1} znaków.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "Hasło i potwierdzenie hasła nie zgadzają się")]
        public string ConfirmPassword { get; set; }

        [Required]
        public string Code { get; set; }
            
        [Required]
        public string CaptchaResponse { get; set; }
    }
}