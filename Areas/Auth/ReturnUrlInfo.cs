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
    [Route("api/[area]/[controller]")]
    public class ReturnUrlInfoController : ControllerBase
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<Login> _logger;
        private readonly IEventService _events;
        private readonly IIdentityServerInteractionService _interaction;

        public ReturnUrlInfoController(SignInManager<ApplicationUser> signInManager, UserManager<ApplicationUser> userManager, ILogger<Login> logger,
            IIdentityServerInteractionService interaction,
            IEventService events)
        {
            _signInManager = signInManager;
            _logger = logger;
            _userManager = userManager;
            _events = events;
            _interaction = interaction;
        }
        
        /// <summary>
        /// Returns information about client associated with returnurl
        /// </summary>
        /// <remarks>
        /// Use every time you need to check whether to redirect to returnurl
        /// </remarks>
        /// <returns> Info on returnurl </returns>
        /// <response code="200"> Returns info on returnurl </response>
        /// <response code="400"> Default invalid model response </response>     
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<ReturnUrlInfo>> OnPost(ReturnUrlInputModel returnUrlInput)
        {
            
            var returnUrlInfo = new ReturnUrlInfo();

            if (_interaction.IsValidReturnUrl(returnUrlInput.returnUrl))
            {
                var context = await _interaction.GetAuthorizationContextAsync(returnUrlInput.returnUrl);
                returnUrlInfo.clientInfo = true;
                returnUrlInfo.clientName = context.Client?.ClientName;
                returnUrlInfo.clientUri = context.Client?.ClientUri;
                returnUrlInfo.clientLogoUrl = context.Client?.LogoUri;
                returnUrlInfo.validReturnUrl = true;
            }
            else
            {
                returnUrlInfo.clientInfo = false;
                returnUrlInput.returnUrl = Url.Content("~/");
                returnUrlInfo.safeReturnUrl = returnUrlInput.returnUrl;
                returnUrlInfo.validReturnUrl = false;
            }
            
            return returnUrlInfo;
        }
    }
    
    public class ReturnUrlInputModel
    {
        [Required] [DataType(DataType.Url)]
        public string returnUrl { get; set; }
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