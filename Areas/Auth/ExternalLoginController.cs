using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using AccountsData.Models.DataModels;
using IdentityServer4.Models;
using IdentityServer4.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;

namespace VLO_BOARDS.Areas.Auth
{
    [ApiController]
    [Area("Auth")]
    [Route("[area]/[controller]")]
    public class ExternalLoginController : ControllerBase
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEmailSender _emailSender;
        private readonly ILogger<ExternalLoginController> _logger;
        private readonly IIdentityServerInteractionService _interaction;
        private readonly EmailTemplates _emailTemplates;

        public ExternalLoginController(
            SignInManager<ApplicationUser> signInManager,
            UserManager<ApplicationUser> userManager,
            ILogger<ExternalLoginController> logger,
            IEmailSender emailSender,
            IIdentityServerInteractionService interaction,
            EmailTemplates emailTemplates)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _logger = logger;
            _emailSender = emailSender;
            _interaction = interaction;
            _emailTemplates = emailTemplates;
        }
        
        /// <summary>
        /// Challenges user using external provider
        /// </summary>
        /// <param name="provider"></param>
        /// <param name="returnUrl"></param>
        /// <returns></returns>
        [HttpGet]
        public IActionResult OnGet(string provider, string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");

            //!= true for semantic reasons
            if (_interaction.IsValidReturnUrl(returnUrl) != true)
            {
                returnUrl = Url.Content("~/");
            }
            
            // Request a redirect to the external login provider.
            var redirectUrl = _emailTemplates.GenerateUrl("Auth/ExternalLogin/Callback",  new Dictionary<string, string>() {{"returnUrl", returnUrl }}).ToString();
            var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
            return new ChallengeResult(provider, properties);
        }
        
        /// <summary>
        /// Callback executed when external auth provider redirects back to app
        /// </summary>
        /// <param name="returnUrl"></param>
        /// <param name="remoteError"></param>
        /// <returns>Redirects to pages</returns>
        [HttpGet]
        [Route("Callback")]
        public async Task<IActionResult> OnGetCallbackAsync(string returnUrl = null, string remoteError = null)
        {
            returnUrl ??= Url.Content("~/");

            //!= true for semantic reasons
            if (_interaction.IsValidReturnUrl(returnUrl) != true)
            {
                returnUrl = Url.Content("~/");
            }
            
            if (remoteError != null)
            {
                var redirectUrl = _emailTemplates.GenerateUrl("/Login", new Dictionary<string, string>{{"returnUrl", returnUrl}, {"error", $"Błąd po stronie zewnętrznego dostawcy auth: {remoteError}"}}).ToString();
                return Redirect(redirectUrl);
            }
            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                var redirectUrl = _emailTemplates.GenerateUrl("/Login", new Dictionary<string, string>{{"returnUrl", returnUrl}, {"error", $"Nie udało się zalogować przez zewnętrzne konto, ponieważ serwer nie dostał żadnej informacji o koncie"}}).ToString();
                return Redirect(redirectUrl);
            }

            // Sign in the user with this external login provider if the user already has a login.
            var result = await _signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, isPersistent: false, bypassTwoFactor : false);
            if (result.Succeeded)
            {
                var user = await _userManager.FindByLoginAsync(info.LoginProvider, info.ProviderKey);
                
                user.HandleExternalAuth(info);
                
                _logger.LogInformation("{Name} logged in with {LoginProvider} provider.", info.Principal.Identity.Name, info.LoginProvider);
                return LocalRedirect(returnUrl);
            }

            if (result.IsNotAllowed)
            {
                var redirectUrl = _emailTemplates.GenerateUrl("/Login", new Dictionary<string, string>{{"returnUrl", returnUrl}, {"error", $"Konto nie zostało potwierdzone (email) lub utraciłeś możliwość logowania się"}}).ToString();
                return Redirect(redirectUrl);
            }

            if (result.RequiresTwoFactor)
            {
                var redirectUrl = _emailTemplates.GenerateUrl("/Login", new Dictionary<string, string>{{"returnUrl", returnUrl}, {"prompt2Fa", "true"}}).ToString();
                return Redirect(redirectUrl);
            }
            if (result.IsLockedOut)
            {
                var redirectUrl = _emailTemplates.GenerateUrl("/Login", new Dictionary<string, string>() {{"error", $"Po ostatniej nieudanej próbie logowania konto zostało zablokowane na 5 minut"}}).ToString();
                return Redirect(redirectUrl);
            }
            else
            {
                var Email = "";
                var Username = "";
                
                // If the user does not have an account, then ask the user to create an account.
                if (info.Principal.HasClaim(c => c.Type == ClaimTypes.Email))
                {
                    Email = info.Principal.FindFirstValue(ClaimTypes.Email);
                }

                if (info.Principal.HasClaim(c => c.Type == ClaimTypes.Name))
                {
                    Username = info.Principal.FindFirstValue(ClaimTypes.Name);
                }
                
                
                var redirectUrl = _emailTemplates.GenerateUrl("/RegisterExternalLogin", new Dictionary<string, string>{{"returnUrl", returnUrl}, {"providerDisplayName", info.ProviderDisplayName}, {"email", Email}, {"username", Username} }).ToString();

                return Redirect(redirectUrl);
            }
        }
        
        /// <summary>
        /// Endpoint used for creating accounts when there's no account associated with external login
        /// </summary>
        /// <param name="Input"></param>
        /// <param name="returnUrl"></param>
        /// <returns>Bad request with modelstate or ok success</returns>
        [HttpPost]
        [Route("CreateAccount")]
        public async Task<IActionResult> OnPostConfirmationAsync(InputModel Input, string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");
            
            //!= true for semantic reasons
            if (_interaction.IsValidReturnUrl(returnUrl) != true)
            {
                returnUrl = Url.Content("~/");
            }
            
            // Get the information about the user from the external login provider
            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                ModelState.AddModelError(string.Empty, "Brak informacji o zewnętrznym dostawcy auth");
                return BadRequest(ModelState);
            }
            
            var user = new ApplicationUser { UserName = Input.Username, Email = Input.Email };

            var result = await _userManager.CreateAsync(user);
            if (result.Succeeded)
            {
                result = await _userManager.AddLoginAsync(user, info);
                if (result.Succeeded)
                {
                    _logger.LogInformation("User created an account using {Name} provider.", info.LoginProvider);

                    var userId = await _userManager.GetUserIdAsync(user);
                    var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                    code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                    var callbackUrl =
                        _emailTemplates.GenerateUrl("ConfirmEmail", new Dictionary<string, string> {{"code", code}, {"userId", user.Id}});

                    var body = await _emailTemplates.RenderFluid("Email.liquid", new {Link = callbackUrl});
            
                    await _emailSender.SendEmailAsync(
                        Input.Email,
                        "Potwierdź swój adres email",
                        body);

                    return Ok("ConfirmEmail");
                }
            }
            
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return BadRequest(ModelState);
        }
        
        public class InputModel
        {
            [Required]
            [EmailAddress]
            public string Email { get; set; }
            
            [Required]
            [DataType(DataType.Text)]
            public string Username { get; set; }
        }
    }
}