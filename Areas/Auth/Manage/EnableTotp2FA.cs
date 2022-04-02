using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
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
public class EnableTotp2FA : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly UrlEncoder _urlEncoder;
    private readonly ILogger<EnableTotp2FA> _logger;
    
    private const string AuthenticatorUriFormat = "otpauth://totp/{0}:{1}?secret={2}&issuer={0}&digits=6";

    public EnableTotp2FA(
        UserManager<ApplicationUser> userManager, 
        UrlEncoder urlEncoder, 
        ILogger<EnableTotp2FA> logger)
    {
        _userManager = userManager;
        _urlEncoder = urlEncoder;
        _logger = logger;
    }

    private class KeyQrUriTuple
    {
        public string SharedKey { get; set; }
        public string AuthenticatorUri { get; set; }
    }

    /// <summary>
    /// Initiates the request to add TOTP to the account by returning the appropriate TOTP uri
    /// </summary>
    /// <returns> The shared key and uri for TOTP </returns>
    [HttpGet]
    public async Task<IActionResult> GetAsync()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return this.GenInternalError();
        }

        var tuple = await GetSharedKeyAndQrCodeUriAsync(user);
        
        _logger.LogInformation("User with ID '{UserId}' has requested a shared totp key.", user.Id);

        return Ok(tuple);
    }

    /// <summary>
    /// Enables 2FA TOTP based on the previous uri request
    /// </summary>
    /// <param name="code"></param>
    /// <returns></returns>
    [HttpPost]
    public async Task<ActionResult<string[]>> OnPostAsync(string code)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return this.GenInternalError();
        }
        
        var verificationCode = code.Replace(" ", string.Empty).Replace("-", string.Empty);
        
        var is2FATokenValid = await _userManager.VerifyTwoFactorTokenAsync(
            user, _userManager.Options.Tokens.AuthenticatorTokenProvider, verificationCode);
        
        if (!is2FATokenValid)
        {
            ModelState.AddModelError(Constants.TwoFaError, Constants.InvalidCodeError);
            return this.GenBadRequestProblem();
        }
        
        await _userManager.SetTwoFactorEnabledAsync(user, true);
        
        if (await _userManager.CountRecoveryCodesAsync(user) == 0)
        {
            var recoveryCodes = await _userManager.GenerateNewTwoFactorRecoveryCodesAsync(user, 10);

            return Ok(recoveryCodes.ToList());
        }
        
        _logger.LogInformation("User with ID '{UserId}' has enabled 2FA with totp.", user.Id);

        return Ok();
    }
    
    private async Task<KeyQrUriTuple> GetSharedKeyAndQrCodeUriAsync(ApplicationUser user)
    {
        // Load the authenticator key & QR code URI
        var unformattedKey = await _userManager.GetAuthenticatorKeyAsync(user);
        if (string.IsNullOrEmpty(unformattedKey))
        {
            await _userManager.ResetAuthenticatorKeyAsync(user);
            unformattedKey = await _userManager.GetAuthenticatorKeyAsync(user);
        }

        var email = await _userManager.GetEmailAsync(user);
        
        var tuple = new KeyQrUriTuple
        {
            SharedKey = FormatKey(unformattedKey),
            AuthenticatorUri = GenerateQrCodeUri(email, unformattedKey)
        };

        return tuple;
    }

    private string FormatKey(string unformattedKey)
    {
        var result = new StringBuilder();
        int currentPosition = 0;
        while (currentPosition + 4 < unformattedKey.Length)
        {
            result.Append(unformattedKey.Substring(currentPosition, 4)).Append(" ");
            currentPosition += 4;
        }
        if (currentPosition < unformattedKey.Length)
        {
            result.Append(unformattedKey.Substring(currentPosition));
        }

        return result.ToString().ToLowerInvariant();
    }

    private string GenerateQrCodeUri(string email, string unformattedKey)
    {
        return string.Format(
            AuthenticatorUriFormat,
            _urlEncoder.Encode("VLO Accounts"),
            _urlEncoder.Encode(email),
            unformattedKey);
    }
}