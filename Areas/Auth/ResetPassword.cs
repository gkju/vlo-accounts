using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using AccountsData.Models.DataModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Swashbuckle.AspNetCore.Annotations;

namespace VLO_BOARDS.Areas.Auth
{
    [ApiController]
    [Area("Auth")]
    [Route("[area]/[controller]")]
    public class ResetPasswordController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public ResetPasswordController(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }
        
        public class InputModel
        {
            [Required]
            [EmailAddress]
            public string Email { get; set; }

            [Required]
            [StringLength(100, ErrorMessage = "{0} musi mieć co najmniej {2} i maksymalnie {1} znaków.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            public string Password { get; set; }

            [DataType(DataType.Password)]
            [Display(Name = "Confirm password")]
            [Compare("Password", ErrorMessage = "Hasło i potwierdzenie hasła nie zgadzają się")]
            public string ConfirmPassword { get; set; }

            [Required]
            public string Code { get; set; }
        }

        /// <summary>
        /// Changes the password to a new one based on input
        /// </summary>
        /// <param name="Input"></param>
        /// <returns>Either ok success or bad request with modelstate</returns>
        [HttpPost]
        public async Task<ActionResult<string>> OnPostAsync(InputModel Input) {
            var user = await _userManager.FindByEmailAsync(Input.Email);
            if (user == null)
            {
                // Don't reveal that the user does not exist
                return Ok("Success");
            }

            var result = await _userManager.ResetPasswordAsync(user, Input.Code, Input.Password);
            if (result.Succeeded)
            {
                return Ok("Success");
            }
            
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
            
            return BadRequest(ModelState);
        }
    }
}