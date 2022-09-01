using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AccountsData.Data;
using AccountsData.Models.DataModels;
using Duende.IdentityServer.Services;
using Fido2NetLib;
using Fido2NetLib.Development;
using Fido2NetLib.Objects;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Logging;
using VLO_BOARDS.Extensions;

using CredentialMakeResult = Fido2NetLib.Fido2.CredentialMakeResult;

namespace VLO_BOARDS.Areas.Auth;

[ApiController]
[Area("Auth")]
[Route("api/[area]/[controller]")]
public class FidoController : ControllerBase
{
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEmailSender _emailSender;
        private readonly ILogger<FidoController> _logger;
        private readonly IIdentityServerInteractionService _interaction;
        private readonly EmailTemplates _emailTemplates;
        private readonly IFido2 _fido2;
        private readonly ApplicationDbContext _db;
        private readonly Captcha _captcha;

        public FidoController(
            SignInManager<ApplicationUser> signInManager,
            UserManager<ApplicationUser> userManager,
            ILogger<FidoController> logger,
            IEmailSender emailSender,
            IIdentityServerInteractionService interaction,
            EmailTemplates emailTemplates,
            IFido2 fido2,
            ApplicationDbContext db,
            Captcha captcha)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _logger = logger;
            _emailSender = emailSender;
            _interaction = interaction;
            _emailTemplates = emailTemplates;
            _fido2 = fido2;
            _db = db;
            _captcha = captcha;
        }
        
        [HttpPost]
        [Route("/fidoRegisterUser")]
        [ProducesResponseType(typeof(CredentialCreateOptions), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult> MakeCredentialOptionsRegisterUser(FidoRegisterInput input)
        {
            if (await _captcha.VerifyCaptcha(input.CaptchaResponse) < Captcha.Threshold)
            {
                ModelState.AddModelError(Captcha.ErrorName, Captcha.ErrorStatus);
                return this.GenBadRequestProblem();
            }
            
            try
            {
                var newUser = new ApplicationUser()
                {
                    UserName = input.UserName,
                    Email = input.Email
                };
                var res = await _userManager.CreateAsync(newUser);
                if (!res.Succeeded)
                {
                    foreach (var error in res.Errors)
                    {
                        ModelState.AddModelError(error.Code, error.Description);
                    }

                    return this.GenBadRequestProblem();
                }
                
                var user = await _userManager.FindByNameAsync(input.UserName);
                
                var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));

                var callbackUrl =
                    _emailTemplates.GenerateUrl("ConfirmEmail", new Dictionary<string, string> {{"code", code}, {"userId", user.Id}});

                await _emailSender.SendEmailAsync(
                    input.Email,
                    "Potwierdź swój adres email",
                    await _emailTemplates.RenderFluid("Email.liquid", new {Link = callbackUrl}));

                await _signInManager.SignInAsync(user, new AuthenticationProperties
                {
                    ExpiresUtc = DateTimeOffset.Now + TimeSpan.FromMinutes(10),
                    IsPersistent = false
                });
                
                return Ok(await HandleCreateCredOpts(user, input));
            }
            catch (Exception e)
            {
                ModelState.AddModelError("fido2", FormatException(e));
                return this.GenBadRequestProblem();
            }
        }

        private async Task<CredentialCreateOptions> HandleCreateCredOpts(ApplicationUser user, MakeCredentialsOptionsInput input)
        {
            
            // input username is a weird gimmick of the fido2 library, we wont be using it but for sec reasons it will be accurate
            input.UserName = user.UserName;
            // 2. Get user existing keys by username
            var existingKeys = user.FidoCredentials.Select(x => x.StoredCredential).ToList();
            
            var descriptors = existingKeys.Select(c => c.Descriptor).ToList();

            // 3. Create options
            var authenticatorSelection = new AuthenticatorSelection
            {
                RequireResidentKey = input.RequireResidentKey,
                UserVerification = input.UserVerification.ToEnum<UserVerificationRequirement>()
            };

            if (!string.IsNullOrEmpty(input.AuthType))
                authenticatorSelection.AuthenticatorAttachment = input.AuthType.ToEnum<AuthenticatorAttachment>();

            var exts = new AuthenticationExtensionsClientInputs() 
            { 
                Extensions = true, 
                UserVerificationMethod = true, 
            };

            var options = _fido2.RequestNewCredential(user, descriptors, authenticatorSelection, input.AttType.ToEnum<AttestationConveyancePreference>(), exts);

            // 4. Temporarily store options, session/in-memory cache/redis/db
            user.AttestationOptionsJson = options.ToJson();
            await _userManager.UpdateAsync(user);

            // 5. return options to client
            return options;
        }

        [HttpPost]
        [Route("/makeCredentialOptions")]
        [Authorize]
        [ProducesResponseType(typeof(CredentialCreateOptions), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CredentialCreateOptions), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult> MakeCredentialOptions(MakeCredentialsOptionsInput input)
        {
            try
            {
                var userP = await _userManager.GetUserAsync(User);
                // input username is a weird gimmick of the fido2 library, we wont be using it but for sec reasons it will be accurate
                input.UserName = userP.UserName;
                var user = await _db.Users.Include(u => u.FidoCredentials).ThenInclude(x => x.StoredCredential.Descriptor).FirstOrDefaultAsync(x => x.Id == userP.Id);
                return Ok(await HandleCreateCredOpts(user, input));
            }
            catch (Exception e)
            {
                return BadRequest(new CredentialCreateOptions { Status = "error", ErrorMessage = FormatException(e) });
            }
        }

        [HttpPost]
        [Route("/makeCredential")]
        [Authorize]
        [ProducesResponseType(typeof(CredentialMakeResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(CredentialMakeResult), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult> MakeCredential([FromBody] AuthenticatorAttestationRawResponse attestationResponse, CancellationToken cancellationToken)
        {
            try
            {
                var userP = await _userManager.GetUserAsync(User);
                var user = await _db.Users.Include(u => u.FidoCredentials).ThenInclude(x => x.StoredCredential.Descriptor).FirstAsync(u => u.Id == userP.Id);
                // 1. get the options we sent the client
                var options = CredentialCreateOptions.FromJson(user.AttestationOptionsJson);

                // 2. Create callback so that lib can verify credential id is unique to this user
                IsCredentialIdUniqueToUserAsyncDelegate callback = async (args, cancellationToken) =>
                {
                    var credentials =
                        await _db.FidoCredentials.Include(x => x.StoredCredential.Descriptor).Where(c => c.StoredCredential.Descriptor.Id.SequenceEqual(args.CredentialId)).ToListAsync(cancellationToken);
                    if (credentials.Count > 0)
                        return false;

                    return true;
                };

                // 2. Verify and make the credentials
                var success = await _fido2.MakeNewCredentialAsync(attestationResponse, options, callback, cancellationToken: cancellationToken);

                var cred = new StoredCredentialId
                {
                    Descriptor = new PublicKeyCredentialDescriptor(success.Result.CredentialId),
                    PublicKey = success.Result.PublicKey,
                    UserHandle = success.Result.User.Id,
                    SignatureCounter = success.Result.Counter,
                    CredType = success.Result.CredType,
                    RegDate = DateTime.Now.ToUniversalTime(),
                    AaGuid = success.Result.Aaguid,
                    UserId = Encoding.UTF8.GetBytes(user.Id)
                };

                user.FidoCredentials.Add(new Fido2Pk()
                {
                    Owner = user,
                    StoredCredential = cred
                });
                await _userManager.UpdateAsync(user);

                // 4. return "ok" to the client
                return Ok(success);
            }
            catch (Exception e)
            {
                return BadRequest(new Fido2.CredentialMakeResult(status: "error", errorMessage: FormatException(e), result: null));
            }
        }

        [HttpPost]
        [Route("/assertionOptions")]
        [ProducesResponseType(typeof(AssertionOptions), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(AssertionOptions), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult> AssertionOptionsPost(string username, string userVerification)
        {
            try
            {
                var userName = _userManager.NormalizeName(username);
                var user = await _db.Users.Where(u => u.NormalizedUserName == userName).Include(u => u.FidoCredentials).ThenInclude(x => x.StoredCredential.Descriptor).FirstAsync();

                // 2. Get registered credentials from database
                var existingCredentials = user.FidoCredentials.Select(c => c.StoredCredential.Descriptor).ToList();
                
                var exts = new AuthenticationExtensionsClientInputs()
                { 
                    UserVerificationMethod = true 
                };

                // 3. Create options
                var uv = string.IsNullOrEmpty(userVerification) ? UserVerificationRequirement.Discouraged : userVerification.ToEnum<UserVerificationRequirement>();
                var options = _fido2.GetAssertionOptions(
                    existingCredentials,
                    uv,
                    exts
                );

                // 4. Temporarily store options, session/in-memory cache/redis/db
                user.AssertionOptionsJson = options.ToJson();
                await _userManager.UpdateAsync(user);

                // 5. Return options to client
                return Ok(options);
            }

            catch (Exception e)
            {
                return BadRequest(new AssertionOptions { Status = "error", ErrorMessage = FormatException(e) });
            }
        }

        [HttpPost]
        [Route("/makeAssertion")]
        public async Task<ActionResult> MakeAssertion(AuthenticatorAssertionRawResponse clientResponse, CancellationToken cancellationToken)
        {
            try
            {
                // 2. Get registered credential from database
                var creds =
                    await _db.FidoCredentials.Include(c => c.StoredCredential.Descriptor).Where(c => c.StoredCredential.Descriptor.Id.SequenceEqual(clientResponse.Id)).FirstOrDefaultAsync(cancellationToken);
               
                // 1. Get the assertion options we sent the client
                var userId = creds?.OwnerId;
                var user = await _userManager.FindByIdAsync(userId);
                var options = AssertionOptions.FromJson(user.AssertionOptionsJson);
                
                // 3. Get credential counter from database
                var storedCounter = creds.StoredCredential.SignatureCounter;

                // 4. Create callback to check if userhandle owns the credentialId
                IsUserHandleOwnerOfCredentialIdAsync callback = async (args, cancellationToken) =>
                {
                    var userCur = await _db.Users
                        .Where(u => Encoding.UTF8.GetBytes(u.Id).SequenceEqual(args.UserHandle))
                        .Include(u => u.FidoCredentials)
                        .ThenInclude(x => x.StoredCredential.Descriptor)
                        .FirstAsync(cancellationToken);
                    var storedCreds = userCur.FidoCredentials;
                    return storedCreds.Exists(c => c.StoredCredential.Descriptor.Id.SequenceEqual(args.CredentialId));
                };

                // 5. Make the assertion
                var res = await _fido2.MakeAssertionAsync(clientResponse, options, creds.StoredCredential.PublicKey, storedCounter, callback, cancellationToken: cancellationToken);

                // 6. Store the updated counter
                var cred = await _db.FidoCredentials.Where(c =>
                    c.StoredCredential.Descriptor.Id.SequenceEqual(res.CredentialId)).FirstAsync(cancellationToken);
                cred.StoredCredential.SignatureCounter = res.Counter;
                await _db.SaveChangesAsync(cancellationToken);

                if (!user.EmailConfirmed)
                {
                    throw new Exception("Niepotwierdzony adres e-mail");
                }

                await _signInManager.SignInAsync(user, true);

                // 7. return OK to client
                return Ok(res);
            }
            catch (Exception e)
            {
                return BadRequest(new AssertionVerificationResult { Status = "error", ErrorMessage = FormatException(e) });
            }
        }
        
        private string FormatException(Exception e)
        {
            return string.Format("{0}{1}", e.Message, e.InnerException != null ? " (" + e.InnerException.Message + ")" : "");
        }
}

public class MakeCredentialsOptionsInput
{
    [Required]
    public string UserName { get; set; }
    [Required]
    public string AttType { get; set; }
    public string AuthType { get; set; }
    [Required]
    public bool RequireResidentKey { get; set; }
    [Required]
    public string UserVerification { get; set; }
}

public class FidoRegisterInput : MakeCredentialsOptionsInput
{
    [Required] [EmailAddress]
    public string Email { get; set; }

    [Required]
    public string CaptchaResponse { get; set; }
}