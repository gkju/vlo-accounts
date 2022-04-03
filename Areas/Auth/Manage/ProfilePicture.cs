using System;
using System.Linq;
using System.Threading.Tasks;
using AccountsData.Data;
using AccountsData.Models.DataModels;
using Amazon.S3;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using VLO_BOARDS.Extensions;

namespace VLO_BOARDS.Areas.Auth.Manage;

[ApiController]
[Area("Auth")]
[Route("api/[area]/[controller]")]
[Authorize]
public class ProfilePicture : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ApplicationDbContext _dbContext;
    private readonly AmazonS3Client _minioClient;
    private readonly MinioConfig _minioConfig;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<ProfilePicture> _logger;

    public ProfilePicture(
        ILogger<ProfilePicture> logger, 
        UserManager<ApplicationUser> userManager, 
        AmazonS3Client minioClient, 
        IServiceProvider serviceProvider, 
        MinioConfig minioConfig, 
        ApplicationDbContext dbContext)
    {
        _logger = logger;
        _userManager = userManager;
        _minioClient = minioClient;
        _serviceProvider = serviceProvider;
        _minioConfig = minioConfig;
        _dbContext = dbContext;
    }

    [HttpGet]
    public async Task<IActionResult> OnGetAsync(string userId)
    {
        var user = await _dbContext.Users
            .Where(u => u.Id == userId)
            .Include(u => u.ProfilePicture)
            .ThenInclude(p => p.Picture)
            .FirstOrDefaultAsync();
        if (user == default)
        {
            ModelState.AddModelError(Constants.IdError, Constants.InvalidIdStatus);
            return this.GenBadRequestProblem();
        }

        if (user.ProfilePicture == null)
        {
            ModelState.AddModelError(Constants.AccountError, Constants.NoProfilePicture);
            return this.GenBadRequestProblem();
        }

        var file = user.ProfilePicture.Picture;
        var fileStream = file.GetSeekableStream(_minioClient);
        return File(fileStream, file.ContentType, true);
    }

    [HttpPost]
    public async Task<IActionResult> OnPostAsync(IFormFile picture)
    {
        var user = await GetUserAsync();
        if (user == default)
        {
            ModelState.AddModelError(Constants.IdError, Constants.InvalidIdStatus);
            return this.GenBadRequestProblem();
        }

        var fileId = await user.UploadFile(picture, _serviceProvider, _minioConfig.BucketName, false, false);
        var file = await _dbContext.Files.FindAsync(fileId);
        
        var profilePicture = new AccountsData.Models.DataModels.ProfilePicture()
        {
            Owner = user,
            Picture = file
        };
        
        if (user.ProfilePicture != null)
        {
            _dbContext.ProfilePictures.Remove(user.ProfilePicture);
        }
        var entry = await _dbContext.ProfilePictures.AddAsync(profilePicture);
        user.ProfilePicture = entry.Entity;
        await _dbContext.SaveChangesAsync();

        return Ok();
    }

    [HttpDelete]
    public async Task<IActionResult> OnDeleteAsync()
    {
        var user = await GetUserAsync();
        if (user == null)
        {
            this.GenInternalError();
        }

        if (user.ProfilePicture == null)
        {
            ModelState.AddModelError(Constants.AccountError, Constants.NoProfilePicture);
            return this.GenBadRequestProblem();
        }

        user.DeleteProfilePicture(_dbContext);
        await _dbContext.SaveChangesAsync();
        
        return Ok();
    }

    private async Task<ApplicationUser> GetUserAsync()
    {
        var userProto = await _userManager.GetUserAsync(User);
        var user = await _dbContext.Users
            .Where(u => u.Id == userProto.Id)
            .Include(u => u.ProfilePicture)
            .ThenInclude(p => p.Picture)
            .FirstOrDefaultAsync();

        return user;
    }
}