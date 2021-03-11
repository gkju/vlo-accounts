using System;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using AccountsData.Models.DataModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace VLO_BOARDS.Areas.Auth
{
    [ApiController]
    [Area("Auth")]
    [Route("[area]/[controller]")]
    public class LoginWithRecoveryCodeController : Controller
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ILogger<LoginWithRecoveryCodeController> _logger;

        public LoginWithRecoveryCodeController(SignInManager<ApplicationUser> signInManager, ILogger<LoginWithRecoveryCodeController> logger)
        {
            _signInManager = signInManager;
            _logger = logger;
        }
        
        public class InputModel
        {
            [BindProperty]
            [Required]
            [DataType(DataType.Text)]
            [Display(Name = "Recovery Code")]
            public string RecoveryCode { get; set; }
        }
        
        public class LoginWithRecoveryCodeResult
        {
            public string message { get; set; }
            public bool redirect { get; set; }
            public string returnUrl { get; set; }

            public LoginWithRecoveryCodeResult(string message, bool redirect = false, string returnUrl = "")
            {
                this.message = message;
                this.redirect = redirect;
                this.returnUrl = returnUrl;
            }
        }

        [HttpPost]
        public async Task<IActionResult> OnPostAsync(InputModel Input, string returnUrl = null)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();
            if (user == null)
            {
                throw new InvalidOperationException($"Unable to load two-factor authentication user.");
            }

            var recoveryCode = Input.RecoveryCode.Replace(" ", string.Empty);

            var result = await _signInManager.TwoFactorRecoveryCodeSignInAsync(recoveryCode);

            if (result.Succeeded)
            {
                _logger.LogInformation("User with ID '{UserId}' logged in with a recovery code.", user.Id);
                return Ok(new LoginWithRecoveryCodeResult("Redirect", true, returnUrl ?? Url.Content("~/")));
            }

            if (result.IsLockedOut)
            {
                _logger.LogWarning("User with ID '{UserId}' account locked out.", user.Id);

                return StatusCode((int) HttpStatusCode.Locked, new LoginWithRecoveryCodeResult("Lockedout"));
            }
            else
            {
                _logger.LogWarning("Invalid recovery code entered for user with ID '{UserId}' ", user.Id);
                ModelState.AddModelError(string.Empty, "Invalid recovery code entered.");
                return BadRequest(ModelState);
            }
        }
    }
}