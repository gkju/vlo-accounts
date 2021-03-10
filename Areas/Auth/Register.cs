using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using RazorLight;
using VLO_BOARDS.Models.DataModels;

namespace VLO_BOARDS.Areas.Auth
{
    [ApiController]
    [Area("Auth")]
    [Route("[area]/[controller]")]
    public class RegisterController : Controller
    {
    private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<RegisterController> _logger;
        private readonly IEmailSender _emailSender;
        private readonly RazorLightEngine _razorLightEngine;

        public RegisterController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ILogger<RegisterController> logger,
            IEmailSender emailSender, 
            RazorLightEngine engine)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _emailSender = emailSender;
            _razorLightEngine = engine;
        }

        public class InputModel
        {
            [Required]
            [EmailAddress]
            [Display(Name = "Email")]
            public string Email { get; set; }

            [Required]
            [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "Password")]
            public string Password { get; set; }

            [DataType(DataType.Password)]
            [Display(Name = "Confirm password")]
            [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
            public string ConfirmPassword { get; set; }
            
            [Required]
            [StringLength(20)]
            [DataType(DataType.Text)]
            public string Username { get; set; }
        }

        public class RegistrationResult
        {
            public RegistrationResult(string message, bool redirect = false, string returnUrl = "")
            {
                this.message = message;
                this.redirect = redirect;
                this.returnUrl = returnUrl;
            }
            
            public string returnUrl { get; set; } = "";
            public bool redirect { get; set; } = false;
            public string message { get; set; }
        }
        
        [HttpPost]
        public async Task<IActionResult> OnPostAsync(InputModel Input, string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");
            var ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser { UserName = Input.Username, Email = Input.Email };
                var result = await _userManager.CreateAsync(user, Input.Password);
                if (result.Succeeded)
                {
                    _logger.LogInformation("User created a new account with password.");

                    var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                    code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                    var callbackUrl = Url.Page(
                        "/Account/ConfirmEmail",
                        pageHandler: null,
                        values: new { area = "Identity", userId = user.Id, code = code, returnUrl = returnUrl },
                        protocol: Request.Scheme);

                    var body = PreMailer.Net.PreMailer.MoveCssInline(await _razorLightEngine.CompileRenderAsync("Email.cshtml",
                        new {Link = callbackUrl})).Html;
            
                    await _emailSender.SendEmailAsync(
                        Input.Email,
                        "Potwierdź swój adres email",
                        body);
                    
                    return Ok(new RegistrationResult("ConfirmRegistration"));
                    
                    //Configuration will be ignored as unconfirmed email accounts are unsafe, legacy code below
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
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }
            
            return BadRequest(ModelState);
        }

    }
}