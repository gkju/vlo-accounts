using System.Text;
using System.Threading.Tasks;
using AccountsData.Models.DataModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using VLO_BOARDS.Extensions;

namespace VLO_BOARDS.Areas.Auth
{
    [ApiController]
    [Area("Auth")]
    [Route("api/[area]/[controller]")]
    public class ConfirmEmail : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly Captcha _captcha;

        public ConfirmEmail(UserManager<ApplicationUser> userManager, Captcha captcha)
        {
            _userManager = userManager;
            _captcha = captcha;
        }

        /// <summary>
        /// Confirms email using provided userid and code
        /// </summary>
        /// <param name="confirmEmailInput"></param>
        /// <returns> Either notfound, ok success or bad request with model state </returns>
        [HttpPost]
        public async Task<ActionResult> OnPostAsync(ConfirmEmailInputModel confirmEmailInput)
        {
            if (await _captcha.VerifyCaptcha(confirmEmailInput.CaptchaResponse) < Captcha.Threshold)
            {
                ModelState.AddModelError(Captcha.ErrorName, Captcha.ErrorStatus);
                return this.GenBadRequestProblem();
            }
            
            var user = await _userManager.FindByIdAsync(confirmEmailInput.userId);
            
            if (user == null)
            {
                // do not reveal user doesn't exist
                return Ok("Success");
            }

            confirmEmailInput.code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(confirmEmailInput.code));
            var result = await _userManager.ConfirmEmailAsync(user, confirmEmailInput.code);

            if (result.Succeeded)
            {
                return Ok("Success");
            }
            else
            {
                ModelState.AddModelError(Constants.InvalidCodeError, Constants.InvalidCodeStatus);
                return this.GenBadRequestProblem();
            }
        }
    }
    
    public class ConfirmEmailInputModel
    {
        public string userId { get; set; }
        public string code { get; set; }
        public string CaptchaResponse { get; set; }
    }
}