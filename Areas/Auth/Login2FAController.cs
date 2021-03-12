using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using AccountsData.Models.DataModels;
using IdentityServer4.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace VLO_BOARDS.Areas.Auth
{
    [ApiController]
    [Area("Auth")]
    [Route("[area]/[controller]")]
    public class Login2FAController : ControllerBase
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ILogger<Login2FAController> _logger;
        private readonly IIdentityServerInteractionService _interaction;

        public Login2FAController(SignInManager<ApplicationUser> signInManager, ILogger<Login2FAController> logger, IIdentityServerInteractionService interaction)
        {
            _signInManager = signInManager;
            _logger = logger;
            _interaction = interaction;
        }
        
        public class InputModel
        {
            [Required]
            [StringLength(7, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
            [DataType(DataType.Text)]
            [Display(Name = "Authenticator code")]
            public string TwoFactorCode { get; set; }

            [Display(Name = "Remember this machine")]
            public bool RememberMachine { get; set; }
            
            public bool RememberMe { get; set; }
        }
        
        /// <summary>
        /// Logs user in based on 2FA code
        /// </summary>
        /// <param name="Input"></param>
        /// <param name="returnUrl"></param>
        /// <returns>Either forbidden, bad request, ok login result or exception</returns>
        /// <exception cref="InvalidOperationException"></exception>
        [HttpPost]
        [ProducesResponseType(typeof(LoginResult), StatusCodes.Status200OK)]
        public async Task<IActionResult> OnPostAsync(InputModel Input, string returnUrl = null)
        {
            returnUrl = returnUrl ?? Url.Content("~/");

            //!= true for semantic reasons
            if ((_interaction.IsValidReturnUrl(returnUrl) || Url.IsLocalUrl(returnUrl)) != true)
            {
                returnUrl = Url.Content("~/");
            }

            var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();
            if (user == null)
            {
                throw new InvalidOperationException($"Unable to load two-factor authentication user.");
            }

            var authenticatorCode = Input.TwoFactorCode.Replace(" ", string.Empty).Replace("-", string.Empty);

            var result = await _signInManager.TwoFactorAuthenticatorSignInAsync(authenticatorCode, Input.RememberMe, Input.RememberMachine);

            if (result.Succeeded)
            {
                _logger.LogInformation("User with ID '{UserId}' logged in with 2fa.", user.Id);
                return Ok(new LoginResult("Success", true, returnUrl));
            }
            else if (result.IsLockedOut)
            {
                _logger.LogWarning("User with ID '{UserId}' account locked out.", user.Id);
                return Forbid("Locked Out");
            }
            else
            {
                _logger.LogWarning("Invalid authenticator code entered for user with ID '{UserId}'.", user.Id);
                ModelState.AddModelError(string.Empty, "Invalid authenticator code.");
                return BadRequest(ModelState);
            }
        }
    }
}