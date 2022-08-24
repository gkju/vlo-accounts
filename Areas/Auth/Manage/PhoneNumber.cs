using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AccountsData.Data;
using AccountsData.Models.DataModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using VLO_BOARDS.Extensions;

namespace VLO_BOARDS.Areas.Auth.Manage;

[ApiController]
[Area("Auth")]
[Route("api/[area]/[controller]")]
[Authorize]
public class PhoneNumber : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ApplicationDbContext _db;
    private readonly Captcha _captcha;
    private readonly ILogger<RequestEmailChange> _logger;
    private readonly byte[] _dummySalt = {10, 10, 10};

    public PhoneNumber(
        UserManager<ApplicationUser> userManager, 
        ApplicationDbContext db, 
        Captcha captcha,
        ILogger<RequestEmailChange> logger)
    {
        _userManager = userManager;
        _db = db;
        _captcha = captcha;
        _logger = logger;
    }

    /// <summary>
    /// Sets a phone number for the current user.
    /// </summary>
    /// <param name="phoneNumber"> Phone number </param>
    /// <returns></returns>
    [HttpPut]
    public async Task<ActionResult> OnPutAsync(string phoneNumber)
    {
        var regex = new Regex("^\\+?\\d{1,4}?[-.\\s]?\\(?\\d{1,3}?\\)?[-.\\s]?\\d{1,4}[-.\\s]?\\d{1,4}[-.\\s]?\\d{1,9}$");
        phoneNumber = phoneNumber.Replace("-", "").Replace(" ", "").Replace("(", "").Replace(")", "");
        if(!regex.IsMatch(phoneNumber))
        {
            ModelState.AddModelError(Constants.PhoneNumberError, Constants.InvalidPhoneNumberStatus);
            return this.GenBadRequestProblem();
        }
        
        var user = await _userManager.GetUserAsync(User);
        user.PhoneNumber = phoneNumber;
        
        var result = await _userManager.UpdateAsync(user);

        if (!result.Succeeded)
        {
            ModelState.AddModelError(Constants.AccountError, "Nie udało się zmienić numeru telefonu");
            return this.GenBadRequestProblem();
        }
            
        _logger.LogInformation("User with the ID {UserId} set a phone number", user.Id);

        return Ok();
    }
}