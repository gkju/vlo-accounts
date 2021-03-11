using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using AccountsData.Models.DataModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Razor.Language;
using Microsoft.AspNetCore.WebUtilities;
using RazorLight;

namespace VLO_BOARDS.Areas.Auth
{
    [ApiController]
    [Area("Auth")]
    [Route("[area]/[controller]")]
    public class ResendEmailConfirmationController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEmailSender _emailSender;
        private readonly RazorLightEngine _razorLightEngine;

        public ResendEmailConfirmationController(UserManager<ApplicationUser> userManager, IEmailSender emailSender, RazorLightEngine engine)
        {
            _userManager = userManager;
            _emailSender = emailSender;
            _razorLightEngine = engine;
        }
        
        public class InputModel
        {
            [Required]
            [EmailAddress]
            public string Email { get; set; }
        }
        
        public async Task<IActionResult> OnPostAsync(InputModel Input)
        {
            
            var user = await _userManager.FindByEmailAsync(Input.Email);
            if (user == null)
            {
                //Do not reveal user doesn't exist
                return Ok("Success");
            }

            var userId = await _userManager.GetUserIdAsync(user);
            var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
            var callbackUrl = Url.Page(
                "/Account/ConfirmEmail",
                pageHandler: null,
                values: new { userId = userId, code = code },
                protocol: Request.Scheme);
            
            var body = PreMailer.Net.PreMailer.MoveCssInline(await _razorLightEngine.CompileRenderAsync("Email.cshtml",
                new {Link = callbackUrl})).Html;
            
            await _emailSender.SendEmailAsync(
                Input.Email,
                "Potwierdź swój adres email",
                body);

            return Ok("Success");
        }
    }
}