﻿using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using AccountsData.Models.DataModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using VLO_BOARDS.Extensions;

namespace VLO_BOARDS.Areas.Auth
{
    [ApiController]
    [Area("Auth")]
    [Route("api/[area]/[controller]")]
    public class ForgotPassword : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEmailSender _emailSender;
        private readonly EmailTemplates _emailTemplates;
        private readonly Captcha _captcha;

        public ForgotPassword(UserManager<ApplicationUser> userManager, IEmailSender emailSender, EmailTemplates emailTemplates, Captcha captcha)
        {
            _userManager = userManager;
            _emailSender = emailSender;
            _emailTemplates = emailTemplates;
            _captcha = captcha;
        }

        /// <summary>
        /// Sends password reset email based on given email
        /// </summary>
        /// <param name="forgotPasswordInput"></param>
        /// <returns> Ok success or bad request with model state </returns>
        [HttpPost]
        public async Task<ActionResult> OnPostAsync(ForgotPasswordInputModel forgotPasswordInput)
        {
            if (await _captcha.VerifyCaptcha(forgotPasswordInput.CaptchaResponse) < Captcha.Threshold)
            {
                ModelState.AddModelError(Captcha.ErrorName, Captcha.ErrorStatus);
                return this.GenBadRequestProblem();
            }

            var user = await _userManager.FindByEmailAsync(forgotPasswordInput.Email);
            if (user == null || !(await _userManager.IsEmailConfirmedAsync(user)))
            {
                // Don't reveal that the user does not exist or is not confirmed
                return Ok("Success");
            }
            
            var code = await _userManager.GeneratePasswordResetTokenAsync(user);
            code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));

            var callbackUrl =
                _emailTemplates.GenerateUrl("/ResetPassword", new Dictionary<string, string> {{"code", code}});

            var body = await _emailTemplates.RenderFluid("ResetPassword.liquid", new {Link = callbackUrl});

            await _emailSender.SendEmailAsync(
                forgotPasswordInput.Email,
                "Zresetuj swoje hasło",
                body);

            return Ok("Success");
        }
    }
    
    public class ForgotPasswordInputModel
    {
        [Required] [EmailAddress]
        public string Email { get; set; }
        
        [Required]
        public string CaptchaResponse { get; set; }
    }
}