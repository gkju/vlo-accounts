using System;
using System.Linq;
using System.Net;
using System.Net.Mime;
using System.Threading.Tasks;
using AccountsData.Data;
using AccountsData.Models.DataModels;
using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
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
    [Route("UploadFile")]
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
            string id = await user.UploadFile(file, serviceProvider, minioClient, clamConfig, minioConfig.BucketName, isPublic);
            return Ok(id);
        }
        catch(Exception error)
        {
            Console.WriteLine(error);
            return StatusCode((int) HttpStatusCode.InternalServerError);
        }
        
    }
    
    [HttpGet]
    [Route("GetFile")]
    public async Task<IActionResult> GetFile(string id)
    {
        ApplicationUser user = await userManager.GetUserAsync(User);
        File file = await applicationDbContext.Files.Where(file => file.ObjectId == id).SingleOrDefaultAsync();
        /*if (!file.MayView(user))
        {
            return Unauthorized();
        }*/
        if (file == default(File))
        {
            return NotFound("No file of given id exists");
        }

        using GetObjectResponse response = await minioClient.GetObjectAsync(file.GetFileRequest(minioConfig));
        Response.Headers.Add("Content-Disposition", new ContentDisposition
        {
            FileName = file.FileName,
            Inline = false
        }.ToString());

        return File(response.ResponseStream, file.ContentType, enableRangeProcessing: true);
    }
}