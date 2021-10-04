using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using AccountsData.Models.DataModels;
using Fluid;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Razor.Language;
using Microsoft.AspNetCore.WebUtilities;

namespace VLO_BOARDS.Areas.Auth
{
    [ApiController]
    [Area("Auth")]
    [Route("api/[area]/[controller]")]
    public class ResendEmailConfirmationController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEmailSender _emailSender;
        private readonly EmailTemplates _emailTemplates;

        public ResendEmailConfirmationController(UserManager<ApplicationUser> userManager, IEmailSender emailSender, EmailTemplates emailTemplates)
        {
            _userManager = userManager;
            _emailSender = emailSender;
            _emailTemplates = emailTemplates;
        }

        /// <summary>
        /// Resends email confirmation
        /// </summary>
        /// <response code="200">Success</response>
        [HttpPost]
        public async Task<IActionResult> OnPostAsync(ResendEmailConfirmationInputModel resendEmailConfirmationInput)
        {
            
            var user = await _userManager.FindByEmailAsync(resendEmailConfirmationInput.Email);
            if (user == null)
            {
                //Do not reveal user doesn't exist
                return Ok("Success");
            }

            var userId = await _userManager.GetUserIdAsync(user);
            var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
            
            var callbackUrl =
                _emailTemplates.GenerateUrl("ConfirmEmail", new Dictionary<string, string> {{"code", code}, {"userId", user.Id}});
            
            await _emailSender.SendEmailAsync(
                resendEmailConfirmationInput.Email,
                "Potwierdź swój adres email",
                await _emailTemplates.RenderFluid("Email.liquid", new {Link = callbackUrl}));

            return Ok("Success");
        }
    }
    
    public class ResendEmailConfirmationInputModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }
}