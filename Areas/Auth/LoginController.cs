using System;
using System.ComponentModel.DataAnnotations;
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
    [Route("[area]/[controller]")]
    public class LoginController : ControllerBase
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<LoginController> _logger;
        private readonly IEventService _events;
        private readonly IIdentityServerInteractionService _interaction;
        private readonly Captcha _captcha;

        public LoginController(SignInManager<ApplicationUser> signInManager, UserManager<ApplicationUser> userManager, ILogger<LoginController> logger,
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
        
        public class InputModel
        {
            [Required]
            [DataType(DataType.Text)]
            public string UserName { get; set; }

            [Required] 
            [DataType(DataType.Password)]
            public string Password { get; set; }

            [Required]
            [Display(Name = "Zapamiętaj mnie")]
            public bool RememberMe { get; set; }
            
            [Required]
            public string CaptchaResponse { get; set; }
        }

        /// <summary>
        /// Endpoint used to log user in based on username, password
        /// </summary>
        /// <param name="Input"></param>
        /// <param name="returnUrl"></param>
        /// <returns>Ok login result, unprocessable entity (2fa required), forbidden or bad request with model state</returns>
        [HttpPost]
        [ProducesResponseType(typeof(LoginResult), StatusCodes.Status200OK)]
        public async Task<IActionResult> OnPostAsync(InputModel Input, string returnUrl = null)
        {
            if (await _captcha.verifyCaptcha(Input.CaptchaResponse) > 0.3)
            {
                ModelState.AddModelError(Captcha.ErrorName, "Bad captcha");
                return BadRequest(ModelState);
            }
            
            var context = await _interaction.GetAuthorizationContextAsync(returnUrl);
            
            returnUrl ??= Url.Content("~/");

            //!= true for semantic reasons
            if (_interaction.IsValidReturnUrl(returnUrl) != true)
            {
                returnUrl = Url.Content("~/");
            }

            var result = await _signInManager.PasswordSignInAsync(Input.UserName, Input.Password, Input.RememberMe, lockoutOnFailure: true);
            
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
                return Forbid("Locked Out");
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Niepoprawny login lub hasło");
                return BadRequest(ModelState);
            }
            
        }

        [HttpPost]
        [Route("/ClearExternalCookies")]
        public async Task<IActionResult> ClearExternalCookies()
        {
            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

            return Ok();
        }
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