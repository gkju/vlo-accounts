using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net.Mail;
using System.Runtime.Serialization;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using AccountsData.Models.DataModels;
using CanonicalEmails;
using IdentityServer4.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using VLO_BOARDS.Extensions;

namespace VLO_BOARDS.Areas.Auth
{
    [ApiController]
    [Area("Auth")]
    [Route("api/[area]/[controller]")]
    public class RegisterController : ControllerBase
    {
    private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<RegisterController> _logger;
        private readonly IEmailSender _emailSender;
        private readonly IEventService _events;
        private readonly IIdentityServerInteractionService _interaction;
        private readonly Captcha _captcha;
        private readonly EmailTemplates _emailTemplates;

        public RegisterController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ILogger<RegisterController> logger,
            IEmailSender emailSender,
            IIdentityServerInteractionService interaction,
            IEventService events, 
            Captcha captcha,
            EmailTemplates emailTemplates)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _emailSender = emailSender;
            _events = events;
            _interaction = interaction;
            _captcha = captcha;
            _emailTemplates = emailTemplates;
        }

        /// <summary>
        /// Registers user based on the provided input
        /// </summary>
        /// <param name="registerInput"></param>
        /// <returns></returns>
        [HttpPost]
        [ProducesResponseType(typeof(RegistrationResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult> OnPostAsync(RegisterInputModel registerInput)
        {
            if (await _captcha.VerifyCaptcha(registerInput.CaptchaResponse) < Captcha.Threshold)
            {
                ModelState.AddModelError(Captcha.ErrorName, Captcha.ErrorStatus);
                return this.GenBadRequestProblem();
            }

            registerInput.Email = Normalizer.Normalize(new MailAddress(registerInput.Email)).ToString();
            
            var externalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
            var user = new ApplicationUser { UserName = registerInput.Username, Email = registerInput.Email };
            
            var result = await _userManager.CreateAsync(user, registerInput.Password);
            if (result.Succeeded)
            {
                _logger.LogInformation("User created a new account with password.");

                var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));

                var callbackUrl =
                    _emailTemplates.GenerateUrl("ConfirmEmail", new Dictionary<string, string> {{"code", code}, {"userId", user.Id}});

                await _emailSender.SendEmailAsync(
                    registerInput.Email,
                    "Potwierdź swój adres email",
                    await _emailTemplates.RenderFluid("Email.liquid", new {Link = callbackUrl}));
                    
                
                return Ok(new RegistrationResult("ConfirmRegistration"));
                
                // Configuration will be ignored as unconfirmed email accounts are unsafe, legacy code below
                /*
                if (_userManager.Options.SignIn.RequireConfirmedAccount)
                {
                    return Ok(new RegistrationResult("ConfirmRegistration"));
                }
                else
                {
                    await _signInManager.SignInAsync(user, isPersistent: false);
                    return Ok(new RegistrationResult("Redirect", true, returnUrl));
                }*/
            }
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(error.Code, error.Description);
            }

            return this.GenBadRequestProblem();
        }

    }
    
    public class RegisterInputModel
    {
        [Required] [EmailAddress]
        public string Email { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "{0} musi mieć od {2} do {1} znaków.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [Required]
        [StringLength(20)]
        [DataType(DataType.Text)]
        public string Username { get; set; }
            
        [Required]
        public string CaptchaResponse { get; set; }
    }
    
    public class RegistrationResult
    {
        public string Message { get; set; }
        
        public RegistrationResult(string message)
        {
            Message = message;
        }
    }
}