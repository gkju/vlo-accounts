using System;
using System.Linq;
using System.Net;
using System.Net.Mime;
using System.Threading.Tasks;
using AccountsData.Data;
using AccountsData.Models.DataModels;
using Amazon.S3;
using Amazon.S3.Model;
using Cppl.Utilities.AWS;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Build.Framework;
using Microsoft.EntityFrameworkCore;
using VLO_BOARDS.Extensions;

namespace VLO_BOARDS.Areas.Files;

[ApiController]
[Area("Files")]
[Route("api/[area]/[controller]")]
[Authorize]
public class FilesController : ControllerBase
{
    private readonly UserManager<ApplicationUser> userManager;
    private readonly AmazonS3Client minioClient;
    private readonly ApplicationDbContext applicationDbContext;
    private readonly MinioConfig minioConfig;
    private readonly ClamConfig clamConfig;
    private readonly IServiceProvider serviceProvider;

    public FilesController(UserManager<ApplicationUser> userManager, IServiceProvider serviceProvider, ApplicationDbContext dbContext, AmazonS3Client minioClient, MinioConfig minioConfig, ClamConfig clamConfig)
    {
        this.userManager = userManager;
        this.applicationDbContext = dbContext;
        this.minioClient = minioClient;
        this.minioConfig = minioConfig;
        this.serviceProvider = serviceProvider;
        this.clamConfig = clamConfig;
    }

    [HttpPost]
    [Route("File")]
    [ProducesResponseType(typeof(string), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> OnPost(IFormFile file, bool isPublic)
    {
        if (file is null)
        {
            ModelState.AddModelError("File", "No file");
            return this.GenBadRequestProblem();
        }

        try
        {
            ApplicationUser user = await userManager.GetUserAsync(User);
            string id = await user.UploadFile(file, serviceProvider, minioConfig.BucketName, isPublic);
            return Ok(id);
        }
        catch(Exception error)
        {
            Console.WriteLine(error);
            return StatusCode((int) HttpStatusCode.InternalServerError);
        }
        
    }
    
    [HttpGet]
    [Route("File")]
    [ProducesResponseType(typeof(string), 200)]
    [ProducesResponseType(500)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetFile(string id)
    {
        ApplicationUser user = await userManager.GetUserAsync(User);
        File file = await applicationDbContext.Files.Where(file => file.ObjectId == id).FirstOrDefaultAsync();
        if (file == default(File))
        {
            return NotFound("No file of given id exists");
        }
        if (!file.MayView(user))
        {
            return Unauthorized();
        }

        return Ok(file.GetSignedUrl(minioClient));
    }

    [HttpDelete]
    [Route("File")]
    [ProducesResponseType(typeof(string), 200)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> DeleteFile(string id)
    {
        var user = await userManager.GetUserAsync(User);
        await user.DeleteFile(id, serviceProvider);

        return Ok(id);
    }
}