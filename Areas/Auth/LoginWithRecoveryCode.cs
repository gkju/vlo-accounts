using System;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Net.Http;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using AccountsData.Models.DataModels;
using IdentityServer4.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using VLO_BOARDS.Extensions;

namespace VLO_BOARDS.Areas.Auth
{
    [ApiController]
    [Area("Auth")]
    [Route("api/[area]/[controller]")]
    public class LoginWithRecoveryCodeController : ControllerBase
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ILogger<LoginWithRecoveryCodeController> _logger;
        private readonly IEventService _events;
        private readonly IIdentityServerInteractionService _interaction;

        public LoginWithRecoveryCodeController(
            SignInManager<ApplicationUser> signInManager, 
            ILogger<LoginWithRecoveryCodeController> logger,
            IIdentityServerInteractionService interaction,
            IEventService events)
        {
            _signInManager = signInManager;
            _logger = logger;
            _events = events;
            _interaction = interaction;
        }


        /// <summary>
        /// Logins user using 2fa recovery code
        /// </summary>
        /// <returns> Login with recovery code result </returns>
        /// <response code="200"> Returns LoginWithRecoveryCodeResult </response>
        /// <response code="400"> Default invalid model response </response>
        /// <response code="423"> User is locked out of his account </response> 
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status423Locked)]
        public async Task<ActionResult> OnPostAsync(LoginWithRecoveryCodeInputModel loginWithRecoveryCodeInput, string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");
            
            if (!_interaction.IsValidReturnUrl(returnUrl))
            {
                returnUrl = Url.Content("~/");
            }

            var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();
            if (user == null)
            {
                throw new InvalidOperationException($"Unable to load two-factor authentication user.");
            }

            var recoveryCode = loginWithRecoveryCodeInput.RecoveryCode.Replace(" ", string.Empty).Replace("-", string.Empty);

            var result = await _signInManager.TwoFactorRecoveryCodeSignInAsync(recoveryCode);

            if (result.Succeeded)
            {
                _logger.LogInformation("User with ID '{UserId}' logged in with a recovery code.", user.Id);
                return Ok(new LoginWithRecoveryCodeResult("Redirect", true, returnUrl));
            }

            if (result.IsLockedOut)
            {
                _logger.LogWarning("User with ID '{UserId}' account locked out.", user.Id);
                ModelState.AddModelError(Constants.AccountError, Constants.LockedOutStatus);
                return this.GenLockedProblem();
            }
            else
            {
                _logger.LogWarning("Invalid recovery code entered for user with ID '{UserId}' ", user.Id);
                ModelState.AddModelError(Constants.TwoFaError, Constants.InvalidRecoveryCodeStatus);
                return this.GenBadRequestProblem();
            }
        }
    }
    
    public class LoginWithRecoveryCodeInputModel
    {
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
}