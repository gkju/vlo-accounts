using System;
using System.Linq;
using System.Threading.Tasks;
using AccountsData.Data;
using AccountsData.Models.DataModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace VLO_BOARDS.Areas.Auth.Manage;

[ApiController]
[Area("Auth")]
[Route("api/[area]/[controller]")]
[Authorize]
public class ProfileInfo : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<GenerateRecoveryCodes> _logger;
    private readonly ApplicationDbContext _db;
    
    public ProfileInfo(
        UserManager<ApplicationUser> userManager,
        ILogger<GenerateRecoveryCodes> logger,
        ApplicationDbContext db)
    {
        _userManager = userManager;
        _logger = logger;
        _db = db;
    }

    /// <summary>
    /// Get the current user's profile information.
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    [ProducesResponseType(typeof(ApplicationUser), 200)]
    public async Task<IActionResult> OnGetAsync()
    {
        var userP = await _userManager.GetUserAsync(User);
        var user = await _db.Users
            .Include(u => u.FidoCredentials)
            .Where(u => u.Id == userP.Id)
            .FirstOrDefaultAsync();
        foreach (var cred in user.FidoCredentials)
        {
            cred.Owner = null;
        }
        
        if (user.PasswordHash is not null)
        {
            user.PasswordHash = "********";
        }
        else
        {
            user.PasswordHash = "-";
        }
        
        return Ok(user);
    }
}