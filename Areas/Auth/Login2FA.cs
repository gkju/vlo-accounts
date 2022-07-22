using System;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Threading.Tasks;
using AccountsData.Models.DataModels;
using Duende.IdentityServer.Services;
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
    public class Login2FA : ControllerBase
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ILogger<Login2FA> _logger;
        private readonly IIdentityServerInteractionService _interaction;
        private readonly Captcha _captcha;

        public Login2FA(SignInManager<ApplicationUser> signInManager, ILogger<Login2FA> logger, IIdentityServerInteractionService interaction, Captcha captcha)
        {
            _signInManager = signInManager;
            _logger = logger;
            _interaction = interaction;
            _captcha = captcha;
        }

        /// <summary>
        /// Logs user in based on 2FA code
        /// </summary>
        /// <param name="login2FaInput"></param>
        /// <param name="returnUrl"></param>
        /// <returns> Login result or exception </returns>
        /// <exception cref="InvalidOperationException"></exception>
        [HttpPost]
        [ProducesResponseType(typeof(LoginResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status423Locked)]
        public async Task<ActionResult> OnPostAsync(Login2FAInputModel login2FaInput, string returnUrl = null)
        {
            if (await _captcha.VerifyCaptcha(login2FaInput.CaptchaResponse) < Captcha.Threshold)
            {
                ModelState.AddModelError(Captcha.ErrorName, Captcha.ErrorStatus);
                return this.GenBadRequestProblem();
            }
            
            returnUrl = returnUrl ?? Url.Content("~/");

            //!= true for semantic reasons
            if (_interaction.IsValidReturnUrl(returnUrl) != true)
            {
                returnUrl = Url.Content("~/");
            }

            var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();
            if (user == null)
            {
                throw new InvalidOperationException($"Unable to load two-factor authentication user.");
            }

            var authenticatorCode = login2FaInput.TwoFactorCode.Replace(" ", string.Empty).Replace("-", string.Empty);

            var result = await _signInManager.TwoFactorAuthenticatorSignInAsync(authenticatorCode, login2FaInput.RememberMe, login2FaInput.RememberMachine);

            if (result.Succeeded)
            {
                _logger.LogInformation("User with ID '{UserId}' logged in with 2fa.", user.Id);
                return Ok(new LoginResult("Success", true, returnUrl));
            }
            else if (result.IsLockedOut)
            {
                _logger.LogWarning("User with ID '{UserId}' account locked out.", user.Id);
                ModelState.AddModelError(Constants.AccountError, Constants.LockedOutStatus);
                return this.GenLockedProblem();
            }
            else
            {
                _logger.LogWarning("Invalid authenticator code entered for user with ID '{UserId}'.", user.Id);
                ModelState.AddModelError(Constants.TwoFaError, Constants.InvalidAuthenticatorCodeStatus);
                return this.GenBadRequestProblem();
            }
        }
    }
    
    public class Login2FAInputModel
    {
        [Required]
        [StringLength(7, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
        [DataType(DataType.Text)]
        public string TwoFactorCode { get; set; }
        
        public bool RememberMachine { get; set; }
            
        public bool RememberMe { get; set; }
        
        [Required]
        public string CaptchaResponse { get; set; }
    }
}