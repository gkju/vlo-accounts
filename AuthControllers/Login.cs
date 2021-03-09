using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using VLO_BOARDS.Models.DataModels;

namespace VLO_BOARDS.AuthControllers
{
    [ApiController]
    [Route("auth/[controller]")]
    public class Login : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ILogger<Login> _logger;
        
        public Login(SignInManager<ApplicationUser> signInManager, 
            ILogger<Login> logger,
            UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
        }

        public class InputUser
        {
            [Required]
            [EmailAddress]
            public string Email { get; set; }
            
            [Required]
            [DataType(DataType.Password)]
            public string Password { get; set; }

            public bool RememberMe { get; set; }
        }

        /// <summary>
        /// Login result, http code mimics the http response code. Why? ???
        /// </summary>
        public class LoginResult
        {
            public string Message { get; set; }
            public int HttpCode { get; set; }
            
            public LoginResult(string Message, HttpResponse res)
            {
                this.Message = Message;
                this.HttpCode = res.StatusCode;
            }
        }

        /// <summary>
        /// Logs the user in using the provided password and email
        /// </summary>
        /// <remarks>
        /// If 2FA is enabled you can only use external login w/ providing the totp code
        /// or you can use the 2FA endpoint
        /// </remarks>
        /// <returns>Response to the login request</returns>
        /// <response code="200">Login successful</response>
        /// <response code="403">Locked out, invalid login attempt or 2FA is required</response>
        /// <response code="422">Invalid input</response>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status403Forbidden, Type=typeof(LoginResult))]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity, Type=typeof(LoginResult))]
        public async Task<ActionResult<LoginResult>> OnPostAsync(InputUser Input)
        {
            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

            if (ModelState.IsValid)
            {
                var result =
                    await _signInManager.PasswordSignInAsync(Input.Email, Input.Password, Input.RememberMe, true);
                if (result.Succeeded)
                {
                    _logger.LogInformation("{Email} logged in", Input.Email);
                    return new LoginResult("Success", HttpContext.Response);
                }
                if (result.RequiresTwoFactor)
                {
                    HttpContext.Response.StatusCode = 403;
                    return new LoginResult("2FA Required", HttpContext.Response);
                }
                if (result.IsLockedOut)
                {
                    HttpContext.Response.StatusCode = 403;
                    return new LoginResult("Locked out", HttpContext.Response);
                }
                else
                {
                    HttpContext.Response.StatusCode = 403;
                    return new LoginResult("Invalid login attempt", HttpContext.Response);
                }
            }

            HttpContext.Response.StatusCode = 422;
            return new LoginResult("Invalid input", HttpContext.Response);
        }
    }
}