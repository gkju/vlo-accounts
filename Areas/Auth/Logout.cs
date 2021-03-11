using System.Threading.Tasks;
using AccountsData.Models.DataModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace VLO_BOARDS.Areas.Auth
{
    [ApiController]
    [Area("Auth")]
    [Route("[area]/[controller]")]
    public class LogoutController : Controller
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ILogger<LogoutController> _logger;

        public LogoutController(SignInManager<ApplicationUser> signInManager, ILogger<LogoutController> logger)
        {
            _signInManager = signInManager;
            _logger = logger;
        }

        public class LogoutResult
        {
            public string message { get; set; }
            public bool redirect { get; set; }
            public string returnUrl { get; set; }

            public LogoutResult(string message, bool redirect = false, string returnUrl = "")
            {
                this.message = message;
                this.redirect = redirect;
                this.returnUrl = returnUrl;
            }
        }
        
        [HttpPost]
        public async Task<IActionResult> OnPost(string returnUrl = null)
        {
            await _signInManager.SignOutAsync();
            _logger.LogInformation("User logged out.");
            if (returnUrl != null)
            {
                return Ok(new LogoutResult("Redirect", true, returnUrl));
            }
            else
            {
                return Ok(new LogoutResult("Success"));
            }
        }
    }
}