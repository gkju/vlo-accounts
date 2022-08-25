using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using Duende.IdentityServer;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using VLO_BOARDS.Auth;
using System.Threading.Tasks;
using AccountsData.Data;
using AccountsData.Models.DataModels;
using Amazon.S3;
using AngleSharp.Html.Dom;
using CanonicalEmails;
using Duende.IdentityServer.Configuration;
using Duende.IdentityServer.EntityFramework.DbContexts;
using IdentityModel;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity.UI.Services;


namespace VLO_BOARDS
{
    public partial class Startup
    {
        public Startup(IConfiguration configuration, IWebHostEnvironment environment)
        {
            Configuration = configuration;
            env = environment;
        }

        public IConfiguration Configuration { get; }
        public IWebHostEnvironment env { get; }

        /// This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // all configuration options except for baseorigin (config)
            string 
                googleClientId = "", 
                googleSecret = "",
                appDbContextNpgsqlConnection = "",
                is4DbContextNpgsqlConnection = "",
                migrationsAssembly = "",
                captchaKey = "",
                captchaPk = "",
                mailKey = "",
                mailUrl = "",
                mailDomain = "",
                minioEndpoint = "",
                minioSecret = "",
                minioAccessToken = "",
                bucketName = "boards",
                videoBucketName = "video",
                clamHost = "",
                clamPort = "",
                serverDomain = "",
                timestampDriftTolerance = "",
                msClientId = "",
                msClientSecret = "",
                twClientId = "",
                twClientSecret = "";
            List<string> corsorigins = new List<string>(), clientOrigins = new List<string>();

            // get configuration options (!add docker secrets for prod)
            {
                googleClientId = Configuration["GoogleAuth:ClientId"];
                googleSecret = Configuration["GoogleAuth:SecretKey"];
                msClientId = Configuration["MicrosoftAuth:ClientId"];
                msClientSecret = Configuration["MicrosoftAuth:SecretKey"];
                twClientId = Configuration["TwitterAuth:ClientId"];
                twClientSecret = Configuration["TwitterAuth:SecretKey"];
                appDbContextNpgsqlConnection = Configuration.GetConnectionString("NPGSQL");
                is4DbContextNpgsqlConnection = Configuration.GetConnectionString("IDENTITYDB");
                migrationsAssembly = typeof(Startup).GetTypeInfo().Assembly.GetName().Name;
                captchaPk = Configuration["CaptchaCredentials:PrivateKey"];
                captchaKey = Configuration["CaptchaCredentials:PublicKey"];
                services.AddDatabaseDeveloperPageExceptionFilter();
                corsorigins = new List<string>() {"http://localhost:3000"};
                
                minioEndpoint = Configuration["minio:endpoint"];
                minioSecret = Configuration["minio:secret"];
                minioAccessToken =  Configuration["minio:access"];
                
                clamHost = Configuration["Clam:Host"];
                clamPort = Configuration["Clam:Port"];
                
                // TODO: remove temporary credentials from mailgun before publishing source code
                mailKey = Configuration["Mailgun:ApiKey"];
                mailDomain = Configuration["Mailgun:MailDomain"];

                serverDomain = Configuration["fido:serverDomain"];
                clientOrigins = Configuration.GetSection("fido:origins").Get<List<string>>();
                timestampDriftTolerance = Configuration["fido:timestampDriftTolerance"];
            }
            
            services.AddTransient(o => new MinioConfig {BucketName = bucketName, VideoBucketName = videoBucketName});
            services.AddTransient(o => new ClamConfig {Host = clamHost, Port = Int32.Parse(clamPort)});
            
            var s3config = new AmazonS3Config()
            {
                AuthenticationRegion = MinioConfig.AuthenticationRegion,
                ServiceURL = minioEndpoint,
                ForcePathStyle = true
            };
            
            services.AddTransient((o) => new AmazonS3Client(minioAccessToken, minioSecret, s3config));

            services.AddScoped(x => new MailgunConfig {ApiKey = mailKey, DomainName = mailDomain});
            services.AddScoped<IEmailSender, EmailSender>();
            services.AddHttpClient();
            services.AddScoped<EmailTemplates>();
            
            services.AddTransient(x => new CaptchaCredentials(captchaPk, captchaKey));
            services.AddTransient<Captcha>();
            
            services.AddScoped<FileInterceptor>();
            
            services.AddDbContext<ApplicationDbContext>((sp, options) =>
            {
                options.AddInterceptors(sp.GetRequiredService<FileInterceptor>());
                options.UseNpgsql(appDbContextNpgsqlConnection, sql => sql.MigrationsAssembly(migrationsAssembly));
            });
            
            services.AddDbContext<ConfigurationDbContext>(options =>
                options.UseNpgsql(is4DbContextNpgsqlConnection,
                    sql => sql.MigrationsAssembly(migrationsAssembly)));
            services.AddDbContext<PersistedGrantDbContext>(options =>
                options.UseNpgsql(is4DbContextNpgsqlConnection,
                    sql => sql.MigrationsAssembly(migrationsAssembly)));
            
            services.AddScoped<IPasswordHasher<ApplicationUser>, Argon2IDHasher<ApplicationUser>>();
            
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo {Title = "ASP.NETCore_suvlo", Version = "v1", Contact = new OpenApiContact()
                {
                    Name = "GJusz",
                    Email = "gkjuszczyk@gmail.com"
                }});
                
                c.CustomSchemaIds(type => type.ToString());
                c.CustomSchemaIds(type => type.FullName.Replace("+", "_"));
                c.EnableAnnotations();

                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                c.IncludeXmlComments(xmlPath);
            });

            services.AddAspNetIdentity();
            services.AddFido2(o =>
            {
                o.ServerDomain = serverDomain;
                o.ServerName = "VLO Accounts";
                o.Origins = new HashSet<string>(clientOrigins);
                o.TimestampDriftTolerance = Int32.Parse(timestampDriftTolerance);
            });

            var is4Builder = services.AddIdentityServer(options =>
                {
                    options.UserInteraction = new UserInteractionOptions()
                    {
                        LogoutUrl = "/Logout",
                        LoginUrl = "/Login",
                        ConsentUrl = "/Consent",
                        DeviceVerificationUrl = "/VerifyDevice",
                        ErrorUrl = "/IS4Error",
                        LoginReturnUrlParameter = "returnUrl"
                    };
                    
                    options.Events.RaiseErrorEvents = true;
                    options.Events.RaiseInformationEvents = true;
                    options.Events.RaiseFailureEvents = true;
                    options.Events.RaiseSuccessEvents = true;
                })
                .AddConfigurationStore(options => options.ConfigureDbContext = b =>
                    b.UseNpgsql(is4DbContextNpgsqlConnection,
                        sql => sql.MigrationsAssembly(migrationsAssembly)))
                .AddOperationalStore(options => options.ConfigureDbContext = b =>
                    b.UseNpgsql(is4DbContextNpgsqlConnection,
                        sql => sql.MigrationsAssembly(migrationsAssembly)))
                .AddAspNetIdentity<ApplicationUser>();
            
            services.AddAutoMapper(typeof(Startup));
            services.AddControllersWithViews();
            
            services.AddAuthentication()
                .AddMicrosoftAccount(options =>
                {
                    options.ClientId = msClientId;
                    options.ClientSecret = msClientSecret;
                    options.SaveTokens = true;
                })
                .AddTwitter(options =>
                {
                    options.ConsumerKey = twClientId;
                    options.ConsumerSecret = twClientSecret;
                    options.SaveTokens = true;
                })
                .AddGoogle(options =>
                {
                    options.ClientId = googleClientId;
                    options.ClientSecret = googleSecret;
                    options.SaveTokens = true;
                    options.AccessType = "offline";
                });
            
            services.AddCors(options =>
            {
                options.AddPolicy(name: "DefaultExternalOrigins",
                    builder =>
                    {
                        builder.WithOrigins(corsorigins.ToArray())
                            .AllowAnyHeader()
                            .AllowAnyMethod()
                            .AllowCredentials();
                    });
            });
        }
    }
}
