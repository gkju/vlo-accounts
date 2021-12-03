using System;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Reflection;
using System.Security.Cryptography;
using IdentityServer4;
using IdentityServer4.EntityFramework.DbContexts;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using VLO_BOARDS.Auth;
using System.Linq;
using System.Threading.Tasks;
using AccountsData.Data;
using AccountsData.Models.DataModels;
using IdentityServer4.Configuration;
using IdentityServer4.EntityFramework.Mappers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity.UI.Services;


namespace VLO_BOARDS
{
    public class Startup
    {
        public Startup(IConfiguration configuration, IWebHostEnvironment environment)
        {
            
            Configuration = configuration;
            env = environment;
        }

        public IConfiguration Configuration { get; }
        public IWebHostEnvironment env { get; }

        public class DummySender : IEmailSender
        {
            public Task SendEmailAsync(string email, string subject, string htmlMessage)
            {
                Console.WriteLine(email + subject + htmlMessage);

                return Task.CompletedTask;
            }
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            string googleClientId = "";
            string googleSecret = "";
            string appDbContextNPGSQLConnection = "";
            string is4DbContextNPGSQLConnection = "";
            string migrationsAssembly = "";
            byte[] pemBytes = {};
            string captchaKey = "";
            string captchaPK = "";

            if (env.IsDevelopment())
            {
                googleClientId = Configuration["GoogleAuth:ClientId"];
                googleSecret = Configuration["GoogleAuth:SecretKey"];
                appDbContextNPGSQLConnection = Configuration.GetConnectionString("NPGSQL");
                is4DbContextNPGSQLConnection = Configuration.GetConnectionString("IDENTITYDB");
                migrationsAssembly = typeof(Startup).GetTypeInfo().Assembly.GetName().Name;
                pemBytes = Convert.FromBase64String(@Configuration.GetSection("ISKeys:ECDSAPEM").Get<string>());
                captchaPK = Configuration["CaptchaCredentials:PrivateKey"];
                captchaKey = Configuration["CaptchaCredentials:PublicKey"];
                services.AddDatabaseDeveloperPageExceptionFilter();
                services.AddScoped<IEmailSender, DummySender>();
            }
            
            services.AddHttpClient();
            services.AddScoped<EmailTemplates>();
            
            services.AddTransient<CaptchaCredentials>(x => new CaptchaCredentials(captchaPK, captchaKey));
            services.AddTransient<Captcha>();


            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseNpgsql(appDbContextNPGSQLConnection, sql => sql.MigrationsAssembly(migrationsAssembly)));
            services.AddDbContext<ConfigurationDbContext>(options =>
                options.UseNpgsql(is4DbContextNPGSQLConnection,
                    sql => sql.MigrationsAssembly(migrationsAssembly)));
            services.AddDbContext<PersistedGrantDbContext>(options =>
                options.UseNpgsql(is4DbContextNPGSQLConnection,
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

            services.AddIdentity<ApplicationUser, IdentityRole>(options => options.SignIn.RequireConfirmedAccount = true)
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddErrorDescriber<Internationalization.PolishIdentityErrorDescriber>()
                .AddDefaultTokenProviders();
            
            services.Configure<IdentityOptions>(options =>
            {
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireNonAlphanumeric = true;
                options.Password.RequireUppercase = true;
                options.Password.RequiredLength = 8;
                options.Password.RequiredUniqueChars = 4;
            });

            services.Configure<IdentityOptions>(options =>
            {
                options.User.AllowedUserNameCharacters =
                    "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._+&*() ";
                options.User.RequireUniqueEmail = true;
            });

            services.ConfigureApplicationCookie(options =>
            {
                options.LoginPath = "/Login";
                options.LogoutPath = "/Logout";
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
                    b.UseNpgsql(is4DbContextNPGSQLConnection,
                        sql => sql.MigrationsAssembly(migrationsAssembly)))
                .AddOperationalStore(options => options.ConfigureDbContext = b =>
                    b.UseNpgsql(is4DbContextNPGSQLConnection,
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
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public async void Configure(IApplicationBuilder app, IWebHostEnvironment env, Features features)
        {
            if (env.IsDevelopment())
            {
                app.UseCookiePolicy(new CookiePolicyOptions
                {
                    MinimumSameSitePolicy = SameSiteMode.None
                });
            }
            else
            {
                app.UseCookiePolicy(new CookiePolicyOptions
                {
                    MinimumSameSitePolicy = SameSiteMode.Strict
                });
            }
            
            InitializeDatabase(app, new Config(env));

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseMigrationsEndPoint();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            
            if (features.SwaggerEnabled)
            {
                app.UseSwagger();
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("v1/swagger.json", "VLO Boards API v1");
                });
                app.UseReDoc(c =>
                {
                    c.DocumentTitle = "Dokumentacja API Vlo Boards, dostępna również pod /swagger/";
                });
            }

            app.UseHttpsRedirection();
            app.UseRouting();
            app.UseIdentityServer();
            app.UseAuthorization();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "areas",
                    pattern: "/api/{area:exists}/{controller}/");
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "/api/{controller}/{action=Index}/{id?}");
            });

            if (env.IsDevelopment())
            {
                app.UseSpa(spa => spa.UseProxyToSpaDevelopmentServer("http://localhost:3000"));
            }
        }

        private void InitializeDatabase(IApplicationBuilder app, Config config)
        {
            using (var serviceScope = app.ApplicationServices.GetService<IServiceScopeFactory>
                ().CreateScope())
            {
                serviceScope.ServiceProvider.GetRequiredService<PersistedGrantDbContext>().
                    Database.Migrate();
                var context = serviceScope.ServiceProvider.GetRequiredService
                    <ConfigurationDbContext>();
                context.Database.Migrate();
                if (!context.Clients.Any())
                {
                    foreach (var client in config.Clients)
                    {
                        context.Clients.Add(client.ToEntity());
                    }
                    context.SaveChanges();
                }

                if (!context.ApiResources.Any())
                {
                    foreach (var resource in config.ApiResources)
                    {
                        context.ApiResources.Add(resource.ToEntity());
                    }

                    context.SaveChanges();
                }
                if (!context.IdentityResources.Any())
                {
                    foreach (var resource in config.IdentityResources)
                    {
                        context.IdentityResources.Add(resource.ToEntity());
                    }
                    context.SaveChanges();
                }
                if (!context.ApiScopes.Any())
                {
                    foreach (var resource in config.ApiScopes)
                    {
                        context.ApiScopes.Add(resource.ToEntity());
                    }
                    context.SaveChanges();
                }
            }
        }


    }
}
