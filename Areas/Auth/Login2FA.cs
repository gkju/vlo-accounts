﻿using System;
using System.ComponentModel.DataAnnotations;
using System.Net;
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
    [Route("api/[area]/[controller]")]
    public class Login2FA : ControllerBase
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ILogger<Login2FA> _logger;
        private readonly IIdentityServerInteractionService _interaction;

        public Login2FA(SignInManager<ApplicationUser> signInManager, ILogger<Login2FA> logger, IIdentityServerInteractionService interaction)
        {
            _signInManager = signInManager;
            _logger = logger;
            _interaction = interaction;
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
                return StatusCode((int) HttpStatusCode.Locked, "Locked Out");
            }
            else
            {
                _logger.LogWarning("Invalid authenticator code entered for user with ID '{UserId}'.", user.Id);
                ModelState.AddModelError(string.Empty, "Invalid authenticator code.");
                return BadRequest(ModelState);
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
    }
}