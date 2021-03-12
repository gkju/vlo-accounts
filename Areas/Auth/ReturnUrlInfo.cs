using System.ComponentModel.DataAnnotations;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using AccountsData.Models.DataModels;
using IdentityServer4.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace VLO_BOARDS.Areas.Auth
{
    [ApiController]
    [Area("Auth")]
    [Route("[area]/[controller]")]
    public class ReturnUrlInfoController : ControllerBase
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<LoginController> _logger;
        private readonly IEventService _events;
        private readonly IIdentityServerInteractionService _interaction;

        public ReturnUrlInfoController(SignInManager<ApplicationUser> signInManager, UserManager<ApplicationUser> userManager, ILogger<LoginController> logger,
            IIdentityServerInteractionService interaction,
            IEventService events)
        {
            _signInManager = signInManager;
            _logger = logger;
            _userManager = userManager;
            _events = events;
            _interaction = interaction;
        }

        public class InputModel
        {
            [Required]
            [DataType(DataType.Url)]
            public string returnUrl { get; set; }
        }

        

        //R.I.P rest, I want it in the body!
        /// <summary>
        /// Returns information about client associated with returnurl
        /// </summary>
        /// <remarks>
        /// Use every time you need to check whether to redirect to returnurl
        ///
        /// </remarks>
        /// <returns>Info on returnurl</returns>
        /// <response code="200">Returns info on returnurl</response>
        /// <response code="400">Default invalid model response</response>     
        [HttpPost]
        [ProducesResponseType(typeof(ReturnUrlInfo), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> OnPost(InputModel Input)
        {
            
            var returnUrlInfo = new ReturnUrlInfo();

            if (_interaction.IsValidReturnUrl(Input.returnUrl) != true)
            {
                var context = await _interaction.GetAuthorizationContextAsync(Input.returnUrl);
                returnUrlInfo.clientInfo = true;
                returnUrlInfo.clientName = context.Client?.ClientName;
                returnUrlInfo.clientUri = context.Client?.ClientUri;
                returnUrlInfo.clientLogoUrl = context.Client?.LogoUri;
                returnUrlInfo.validReturnUrl = true;
            }
            else
            {
                Input.returnUrl = Url.Content("~/");
                returnUrlInfo.safeReturnUrl = Input.returnUrl;
                returnUrlInfo.validReturnUrl = false;
                returnUrlInfo.clientInfo = false;
            }
            
            return Ok(returnUrlInfo);
        }
    }
    
    public class ReturnUrlInfo 
    {
        public string safeReturnUrl { get; set; }

        public bool validReturnUrl { get; set; }

        public bool clientInfo { get; set; }

        public string clientName { get; set; }

        public string clientUri { get; set; }

        public string clientLogoUrl { get; set; }

    }
}