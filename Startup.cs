using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Security.Cryptography;
using IdentityServer4;
using IdentityServer4.EntityFramework.DbContexts;
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
using CanonicalEmails;
using IdentityServer4.Configuration;
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
                MailKey = "",
                MailUrl = "",
                MailDoname = "";
            List<string> corsorigins = new List<string>();
            byte[] pemBytes = {};

            if (env.IsDevelopment())
            {
                googleClientId = Configuration["GoogleAuth:ClientId"];
                googleSecret = Configuration["GoogleAuth:SecretKey"];
                appDbContextNpgsqlConnection = Configuration.GetConnectionString("NPGSQL");
                is4DbContextNpgsqlConnection = Configuration.GetConnectionString("IDENTITYDB");
                migrationsAssembly = typeof(Startup).GetTypeInfo().Assembly.GetName().Name;
                pemBytes = Convert.FromBase64String(@Configuration.GetSection("ISKeys:ECDSAPEM").Get<string>());
                captchaPk = Configuration["CaptchaCredentials:PrivateKey"];
                captchaKey = Configuration["CaptchaCredentials:PublicKey"];
                services.AddDatabaseDeveloperPageExceptionFilter();
                corsorigins = new List<string>() {"http://localhost:3000"};
                // TODO: remove temporary credentials from mailgun before publishing source code
                MailKey = Configuration["Mailgun:ApiKey"];
                MailDoname = Configuration["Mailgun:MailDoname"];
            }

            services.AddScoped(x => new MailgunConfig {ApiKey = MailKey, DomainName = MailDoname});
            services.AddScoped<IEmailSender, EmailSender>();
            services.AddHttpClient();
            services.AddScoped<EmailTemplates>();
            
            services.AddTransient(x => new CaptchaCredentials(captchaPk, captchaKey));
            services.AddTransient<Captcha>();
            
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseNpgsql(appDbContextNpgsqlConnection, sql => sql.MigrationsAssembly(migrationsAssembly)));
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

                c.EnableAnnotations();

                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                c.IncludeXmlComments(xmlPath);
            });

            services.AddTransient<Features>(x => new Features
                {SwaggerEnabled = Configuration.GetValue<bool>("Features:Swagger")});

            services.AddAspNetIdentity();

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
            
            var ecdsa = ECDsa.Create();
            ecdsa.ImportECPrivateKey(pemBytes, out _);
            var ecdsaKey = new ECDsaSecurityKey(ecdsa) {KeyId = "secp521r1key"};

            is4Builder.AddSigningCredential(ecdsaKey, IdentityServerConstants.ECDsaSigningAlgorithm.ES512);

            services.AddControllersWithViews();
            
            services.AddAuthentication()
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
                            .AllowAnyMethod();
                    });
            });
        }
    }
}
