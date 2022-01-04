using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net.Mail;
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
using VLO_BOARDS.Auth;

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
    private readonly byte[] _dummySalt = {10, 10, 10};
    
    public RequestEmailChange(UserManager<ApplicationUser> userManager, ApplicationDbContext db, Captcha captcha, IEmailSender emailSender, EmailTemplates emailTemplates)
    {
        _userManager = userManager;
        _db = db;
        _captcha = captcha;
        _emailSender = emailSender;
        _emailTemplates = emailTemplates;
    }

    [HttpPost]
    public async Task<ActionResult> OnPostAsync(RequestEmailChangeInput emailInput)
    {
        /*
        if (await _captcha.VerifyCaptcha(emailInput.CaptchaResponse) < 0.7)
        {
            ModelState.AddModelError(Captcha.ErrorName, "Bad captcha");
            return BadRequest(ModelState);
        }*/

        var user = await _userManager.GetUserAsync(User);
        emailInput.Email = new MailAddress(emailInput.Email).Normalize().ToString();
        if (user.Email == emailInput.Email)
        {
            return UnprocessableEntity("Nie można zmienić maila na ten sam adres mail");
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
        
        return Ok("Success");
    }
    
    [HttpPut]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<ActionResult> OnPutAsync(ConfirmEmailChangeInput emailChangeInput)
    {
        /*if (await _captcha.VerifyCaptcha(emailChangeInput.CaptchaResponse) < 0.7)
        {
            ModelState.AddModelError(Captcha.ErrorName, "Bad captcha");
            return BadRequest(ModelState);
        }*/

        var user = await _userManager.GetUserAsync(User);
        
        var requests = await _db.EmailChangeRequests.Where(req => req.User == user).ToArrayAsync();
        requests = requests.Where(req => req.Date.AddMinutes(30) > DateTime.Now.ToUniversalTime()).ToArray();
        if (!(requests.Length > 0))
        {
            return UnprocessableEntity("Nie masz aktualnych zapytań o zmianę adresu email");
        }

        var hash = Encoding.UTF8.GetString(SodiumLib.HashPassword(emailChangeInput.Code, _dummySalt));
        var request = requests.FirstOrDefault(req => req.CodeHash == hash);
        if (request != default)
        {
            user.Email = request.Email;
            user.EmailConfirmed = true;
            
            await _db.SaveChangesAsync();

            return Ok("Success");
        }

        ModelState.AddModelError("Invalid code", "Invalid code");
        return UnprocessableEntity("Invalid code");
    }
}

public class RequestEmailChangeInput
{
    [Required] [EmailAddress] public string Email { get; set; }
    [Required] public string CaptchaResponse { get; set; }
}

public class ConfirmEmailChangeInput
{
    [Required] public string Code { get; set; }
    [Required] public string CaptchaResponse { get; set; }
}