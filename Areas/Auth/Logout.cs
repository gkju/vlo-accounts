using System.Runtime.Serialization;
using System.Threading.Tasks;
using AccountsData.Models.DataModels;
using Duende.IdentityServer.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace VLO_BOARDS.Areas.Auth
{
    [ApiController]
    [Area("Auth")]
    [Route("api/[area]/[controller]")]
    [Authorize]
    public class LogoutController : ControllerBase
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ILogger<LogoutController> _logger;
        private readonly IEventService _events;
        private readonly IIdentityServerInteractionService _interaction;

        public LogoutController(
            SignInManager<ApplicationUser> signInManager, 
            ILogger<LogoutController> logger, 
            IIdentityServerInteractionService interaction,
            IEventService events)
        {
            _signInManager = signInManager;
            _logger = logger;
            _events = events;
            _interaction = interaction;
        }

        /// <summary>
        /// Logs user out
        /// </summary>
        /// <returns> Logout result </returns>
        /// <response code="200"> Logout result </response>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult> OnPost(string returnUrl = null)
        {
            await _signInManager.SignOutAsync();
            _logger.LogInformation("User logged out.");
            if (returnUrl != null)
            {
                //!= true for semantic reasons
                if (_interaction.IsValidReturnUrl(returnUrl) != true)
                {
                    returnUrl = Url.Content("~/");
                }
                
                return Ok(new LogoutResult("Redirect", true, returnUrl));
            }
            else
            {
                return Ok(new LogoutResult("Success"));
            }
        }
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
}