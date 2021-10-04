using System.Text;
using System.Threading.Tasks;
using AccountsData.Models.DataModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;

namespace VLO_BOARDS.Areas.Auth
{
    [ApiController]
    [Area("Auth")]
    [Route("api/[area]/[controller]")]
    public class ConfirmEmailController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public ConfirmEmailController(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        /// <summary>
        /// Confirms email using provided userid and code
        /// </summary>
        /// <param name="confirmEmailInput"></param>
        /// <returns>Either notfound, ok success or bad request with model state</returns>
        [HttpPost]
        public async Task<IActionResult> OnPostAsync(ConfirmEmailInputModel confirmEmailInput)
        {
            var user = await _userManager.FindByIdAsync(confirmEmailInput.userId);
            
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{confirmEmailInput.userId}'.");
            }

            confirmEmailInput.code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(confirmEmailInput.code));
            var result = await _userManager.ConfirmEmailAsync(user, confirmEmailInput.code);

            if (result.Succeeded)
            {
                return Ok("Success");
            }
            else
            {
                return BadRequest("Error confirming email");
            }
        }
    }
    
    public class ConfirmEmailInputModel
    {
        public string userId { get; set; }
        public string code { get; set; }
    }
}