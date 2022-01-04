using System;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Security.Policy;
using System.Threading.Tasks;
using AccountsData.Models.DataModels;
using IdentityServer4.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace VLO_BOARDS.Areas.Auth
{
    [ApiController]
    [Area("Auth")]
    [Route("api/[area]/[controller]")]
    public class Login : ControllerBase
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<Login> _logger;
        private readonly IEventService _events;
        private readonly IIdentityServerInteractionService _interaction;
        private readonly Captcha _captcha;

        public Login(SignInManager<ApplicationUser> signInManager, UserManager<ApplicationUser> userManager, ILogger<Login> logger,
            IIdentityServerInteractionService interaction,
            IEventService events,
            Captcha captcha)
        {
            _signInManager = signInManager;
            _logger = logger;
            _userManager = userManager;
            _events = events;
            _interaction = interaction;
            _captcha = captcha;
        }

        /// <summary>
        /// Endpoint used to log user in based on username, password
        /// </summary>
        /// <param name="loginInput"></param>
        /// <param name="returnUrl"></param>
        /// <returns>Ok login result, unprocessable entity (2fa required), forbidden or bad request with model state</returns>
        [HttpPost]
        [ProducesResponseType(typeof(LoginResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(StatusCodes.Status423Locked)]
        public async Task<ActionResult> OnPostAsync(LoginInputModel loginInput, string returnUrl = null)
        {
            if (await _captcha.VerifyCaptcha(loginInput.CaptchaResponse) < 0.7) 
            {
                ModelState.AddModelError(Captcha.ErrorName, "Bad captcha");
                return BadRequest(ModelState);
            }
            
            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

            var context = await _interaction.GetAuthorizationContextAsync(returnUrl);
            
            returnUrl ??= Url.Content("~/");

            //!= true for semantic reasons
            if (_interaction.IsValidReturnUrl(returnUrl) != true)
            {
                returnUrl = Url.Content("~/");
            }

            var result = await _signInManager.PasswordSignInAsync(loginInput.Username, loginInput.Password, loginInput.RememberMe, lockoutOnFailure: true);
            
            if (result.Succeeded)
            {
                _logger.LogInformation("User logged in.");
                return Ok(new LoginResult("Success", true, returnUrl));
            }
            if (result.RequiresTwoFactor)
            {
                return UnprocessableEntity("2FA Required");
            }
            if (result.IsLockedOut)
            {
                return StatusCode((int) HttpStatusCode.Locked, "Locked Out");
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Niepoprawny login lub hasło");
                return BadRequest(ModelState);
            }
            
        }

        [HttpPost]
        [Route("ClearExternalCookies")]
        public async Task<ActionResult> ClearExternalCookies()
        {
            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

            return Ok();
        }
    }
    
    public class LoginInputModel
    {
        [Required]
        [DataType(DataType.Text)]
        public string Username { get; set; }

        [Required] 
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Required]
        [Display(Name = "Zapamiętaj mnie")]
        public bool RememberMe { get; set; }
            
        [Required]
        public string CaptchaResponse { get; set; }
    }
    
    public class LoginResult
    {
        public string message { get; set; }
        public bool redirect { get; set; }
        public string returnUrl { get; set; }

        public LoginResult(string message, bool redirect = false, string returnUrl = "")
        {
            this.message = message;
            this.redirect = redirect;
            this.returnUrl = returnUrl;
        }
    }
}