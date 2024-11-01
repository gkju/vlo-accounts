﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using AccountsData.Models.DataModels;
using Fluid;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Razor.Language;
using Microsoft.AspNetCore.WebUtilities;
using VLO_BOARDS.Extensions;

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
        private readonly Captcha _captcha;

        public ResendEmailConfirmationController(UserManager<ApplicationUser> userManager, IEmailSender emailSender,
            EmailTemplates emailTemplates, Captcha captcha)
        {
            _userManager = userManager;
            _emailSender = emailSender;
            _emailTemplates = emailTemplates;
            _captcha = captcha;
        }

        /// <summary>
        /// Resends email confirmation
        /// </summary>
        /// <response code="200"> Success </response>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult> OnPostAsync(ResendEmailConfirmationInputModel resendEmailConfirmationInput)
        {
            if (await _captcha.VerifyCaptcha(resendEmailConfirmationInput.CaptchaResponse) < Captcha.Threshold)
            {
                ModelState.AddModelError(Captcha.ErrorName, Captcha.ErrorStatus);
                return this.GenBadRequestProblem();
            }
            
            var user = await _userManager.FindByEmailAsync(resendEmailConfirmationInput.Email);
            if (user == null || user.EmailConfirmed)
            {
                //Do not reveal user doesn't exist/exists
                return Ok("Success");
            }
            
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
        [Required] [EmailAddress]
        public string Email { get; set; }
        
        [Required]
        public string CaptchaResponse { get; set; }
    }
}