using System.Linq;
using System.Threading.Tasks;
using AccountsData.Data;
using AccountsData.Models.DataModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using VLO_BOARDS.Extensions;

namespace VLO_BOARDS.Areas.Auth.Manage;

[ApiController]
[Area("Auth")]
[Route("api/[area]/[controller]")]
[Authorize]
public class ChangeUserName : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ApplicationDbContext _db;
    private readonly Captcha _captcha;
    private readonly IEmailSender _emailSender;
    private readonly EmailTemplates _emailTemplates;
    private readonly ILogger<RequestEmailChange> _logger;
    private readonly byte[] _dummySalt = {10, 10, 10};

    public ChangeUserName(
        UserManager<ApplicationUser> userManager, 
        ApplicationDbContext db, 
        Captcha captcha, 
        IEmailSender emailSender, 
        EmailTemplates emailTemplates, 
        ILogger<RequestEmailChange> logger)
    {
        _userManager = userManager;
        _db = db;
        _captcha = captcha;
        _emailSender = emailSender;
        _emailTemplates = emailTemplates;
        _logger = logger;
    }

    [HttpPost]
    public async Task<IActionResult> OnPost(string userName)
    {
        var user = await _userManager.GetUserAsync(User);
        var res = await _userManager.SetUserNameAsync(user, userName);
        if (res.Errors.ToList().Count > 0)
        {
            foreach (var err in res.Errors)
            {
                ModelState.AddModelError(err.Code, err.Description);
            }
            
            return this.GenBadRequestProblem();
        }
        return Ok();
    }
}