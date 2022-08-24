using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net.Mail;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using AccountsData.Data;
using AccountsData.Models.DataModels;
using CanonicalEmails;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using VLO_BOARDS.Auth;
using VLO_BOARDS.Extensions;

namespace VLO_BOARDS.Areas.Auth.Manage;

[ApiController]
[Area("Auth")]
[Route("api/[area]/[controller]")]
[Authorize]
public class RequestEmailChange : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ApplicationDbContext _db;
    private readonly Captcha _captcha;
    private readonly IEmailSender _emailSender;
    private readonly EmailTemplates _emailTemplates;
    private readonly ILogger<RequestEmailChange> _logger;
    private readonly byte[] _dummySalt = {10, 10, 10};

    public RequestEmailChange(
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

    /// <summary>
    /// Creates a new email change request
    /// </summary>
    /// <param name="emailInput"></param>
    /// <returns></returns>
    [HttpPost]
    public async Task<ActionResult> OnPostAsync(RequestEmailChangeInput emailInput)
    {
        if (await _captcha.VerifyCaptcha(emailInput.CaptchaResponse) < Captcha.Threshold)
        {
            ModelState.AddModelError(Captcha.ErrorName, Captcha.ErrorStatus);
            return this.GenBadRequestProblem();
        }

        var user = await _userManager.GetUserAsync(User);
        emailInput.Email = new MailAddress(emailInput.Email).Normalize().ToString();
        if (user.Email == emailInput.Email)
        {
            ModelState.AddModelError(Constants.AccountError, "Nie można zmienić maila na ten sam adres mail");
            return this.GenUnprocessableProblem();
        }

        var bytes = new byte[8];
        var codeBytes = new byte[8];
        SodiumLib.randombytes_buf(bytes, bytes.Length);
        
        for (int i = 0; i < bytes.Length; ++i)
        {
            codeBytes[i] = Convert.ToByte(bytes[i] % 26 + 65);
        }
        var code = Encoding.UTF8.GetString(codeBytes);
        
        await _emailSender.SendEmailAsync(
            emailInput.Email,
            "Potwierdź swój adres email",
            await _emailTemplates.RenderFluid("EmailChange.liquid", new {Code = code}));
        
        var codeHash = Encoding.UTF8.GetString(SodiumLib.HashPassword(code, _dummySalt));
        
        var request = new EmailChangeRequest(user, emailInput.Email, codeHash: codeHash);
        _db.EmailChangeRequests.Add(request);
        await _db.SaveChangesAsync();
        
        _logger.LogInformation("User with ID {UserId} has requested an email change", user.Id);
        
        return Ok();
    }
    
    /// <summary>
    /// Handles the email change requested previously
    /// </summary>
    /// <param name="emailChangeInput"></param>
    /// <returns></returns>
    [HttpPut]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult> OnPutAsync(ConfirmEmailChangeInput emailChangeInput)
    {
        if (await _captcha.VerifyCaptcha(emailChangeInput.CaptchaResponse) < Captcha.Threshold)
        {
            ModelState.AddModelError(Captcha.ErrorName, Captcha.ErrorStatus);
            return this.GenBadRequestProblem();
        }

        var user = await _userManager.GetUserAsync(User);

        var requests = await _db.EmailChangeRequests.Where(req => req.User == user).ToArrayAsync();
        requests = requests.Where(req => req.Date.AddMinutes(30) > DateTime.Now.ToUniversalTime()).ToArray();
        if (!(requests.Length > 0))
        {
            ModelState.AddModelError(Constants.AccountError, "Nie masz aktualnych próśb o zmianę adresu email");
            return this.GenUnprocessableProblem();
        }

        var hash = Encoding.UTF8.GetString(SodiumLib.HashPassword(emailChangeInput.Code, _dummySalt));
        var request = requests.FirstOrDefault(req => req.CodeHash == hash);

        if (request != default)
        {
            user.Email = request.Email;
            user.EmailConfirmed = true;

            var result = await _userManager.UpdateAsync(user);

            if (!result.Succeeded)
            {
                ModelState.AddModelError(Constants.AccountError, "Nie udało się zmienić adresu email. Upewnij się, że inne konta nie korzystają z tego adresu email.");
                return this.GenBadRequestProblem();
            }
            
            _logger.LogInformation("User with the ID {UserId} has successfully changed his email", user.Id);

            return Ok();
        }

        ModelState.AddModelError(Constants.InvalidCodeError, Constants.InvalidCodeStatus);
        return this.GenUnprocessableProblem();
    }
}

public class RequestEmailChangeInput
{
    public string Id = Guid.NewGuid().ToString();
    
    [Required] [EmailAddress] 
    public string Email { get; set; }
    
    [Required] 
    public string CaptchaResponse { get; set; }
}

public class ConfirmEmailChangeInput
{
    [Required] 
    public string Code { get; set; }
    
    [Required] 
    public string CaptchaResponse { get; set; }
}