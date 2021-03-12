using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using AccountsData.Models.DataModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using RazorLight;

namespace VLO_BOARDS.Areas.Auth
{
    [ApiController]
    [Area("Auth")]
    [Route("[area]/[controller]")]
    public class ForgotPasswordController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEmailSender _emailSender;
        private readonly RazorLightEngine _razorLightEngine;

        public ForgotPasswordController(UserManager<ApplicationUser> userManager, IEmailSender emailSender, RazorLightEngine engine)
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

        /// <summary>
        /// Sends password reset email based on given email
        /// </summary>
        /// <param name="Input"></param>
        /// <returns>Ok success or bad request with model state</returns>
        [HttpPost]
        public async Task<IActionResult> OnPostAsync(InputModel Input)
        {

                var user = await _userManager.FindByEmailAsync(Input.Email);
                if (user == null || !(await _userManager.IsEmailConfirmedAsync(user)))
                {
                    // Don't reveal that the user does not exist or is not confirmed
                    return Ok("Success");
                }
                
                var code = await _userManager.GeneratePasswordResetTokenAsync(user);
                code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));

                var callbackUrl = Url.Page(
                    "/ResetPassword",
                    values: new { code });
                
                var body = PreMailer.Net.PreMailer.MoveCssInline(await _razorLightEngine.CompileRenderAsync("ResetPassword.cshtml",
                    new {Link = callbackUrl})).Html;

                await _emailSender.SendEmailAsync(
                    Input.Email,
                    "Zresetuj swoje hasło",
                    body);

                return Ok("Success");
        }
    }
}