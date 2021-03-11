using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Reflection;
using System.Security.Cryptography;
using AutoMapper.Features;
using IdentityModel;
using IdentityServer4;
using IdentityServer4.EntityFramework.DbContexts;
using IdentityServer4.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Components.Server.Circuits;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.SpaServices.ReactDevelopmentServer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using RazorLight;
using VLO_BOARDS.Auth;
using VLO_BOARDS.Data;
using VLO_BOARDS.Models;
using VLO_BOARDS.Models.DataModels;
using System.Linq;
using IdentityServer4.Configuration;
using IdentityServer4.EntityFramework.DbContexts;
using IdentityServer4.EntityFramework.Mappers;
using Microsoft.AspNetCore.Http;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using VLO_BOARDS.Migrations;
using Client = IdentityServer4.EntityFramework.Entities.Client;


namespace VLO_BOARDS
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            var migrationsAssembly = typeof(Startup).GetTypeInfo().Assembly.GetName().Name;
            
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseNpgsql(Configuration.GetConnectionString("NPGSQL")));
            services.AddDbContext<ConfigurationDbContext>(options =>
                options.UseNpgsql(Configuration.GetConnectionString("IDENTITYDB"),
                    sql => sql.MigrationsAssembly(migrationsAssembly)));
            services.AddDbContext<PersistedGrantDbContext>(options =>
                options.UseNpgsql(Configuration.GetConnectionString("IDENTITYDB"),
                    sql => sql.MigrationsAssembly(migrationsAssembly)));
            
            services.AddScoped<IPasswordHasher<ApplicationUser>, Argon2IDHasher<ApplicationUser>>();
            
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo {Title = "ASP.NETCore_suvlo", Version = "v1", Contact = new OpenApiContact()
                {
                    Name = "GJusz",
                    Email = "gkjuszczyk@gmail.com"
                }});

                c.EnableAnnotations();

                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                c.IncludeXmlComments(xmlPath);
            });

            services.AddTransient<Features>(x => new Features
                {SwaggerEnabled = Configuration.GetValue<bool>("Features:Swagger")});
            
            
            services.AddDatabaseDeveloperPageExceptionFilter();

            services.AddIdentity<ApplicationUser, IdentityRole>(options => options.SignIn.RequireConfirmedAccount = true)
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            var is4Builder = services.AddIdentityServer(options =>
                {
                    options.UserInteraction = new UserInteractionOptions()
                    {
                        LogoutUrl = "/Identity/account/logout",
                        LoginUrl = "/Identity/account/login",
                        LoginReturnUrlParameter = "returnUrl"
                    };
                    
                    options.Events.RaiseErrorEvents = true;
                    options.Events.RaiseInformationEvents = true;
                    options.Events.RaiseFailureEvents = true;
                    options.Events.RaiseSuccessEvents = true;
                })
                .AddConfigurationStore(options => options.ConfigureDbContext = b =>
                    b.UseNpgsql(Configuration.GetConnectionString("IDENTITYDB"),
                        sql => sql.MigrationsAssembly(migrationsAssembly)))
                .AddOperationalStore(options => options.ConfigureDbContext = b =>
                    b.UseNpgsql(Configuration.GetConnectionString("IDENTITYDB"),
                        sql => sql.MigrationsAssembly(migrationsAssembly)))
                .AddAspNetIdentity<ApplicationUser>();
                


            var pemBytes = Convert.FromBase64String(@Configuration.GetSection("ISKeys:ECDSAPEM").Get<string>());
            var ecdsa = ECDsa.Create();
            ecdsa.ImportECPrivateKey(pemBytes, out _);
            var ecdsaKey = new ECDsaSecurityKey(ecdsa) {KeyId = "secp521r1key"};

            is4Builder.AddSigningCredential(ecdsaKey, IdentityServerConstants.ECDsaSigningAlgorithm.ES512);

            services.AddControllersWithViews();
            services.AddRazorPages();

            // In production, the React files will be served from this directory
            services.AddSpaStaticFiles(configuration =>
            {
                configuration.RootPath = "ClientApp/build";
            });

            //razor light instead of razor since it's built for rendering loose cshtml files + the whole app should not be dependent on any razor engine besides razor light so razor can be dumped all together in the near future
            services.AddSingleton<RazorLightEngine>(x =>
            {
                string razorTemplateDir = Configuration.GetSection("RazorLightTemplateDirs").Get<string>();
                var templateEngine = new RazorLightEngineBuilder()
                    .UseFileSystemProject(razorTemplateDir)
                    .UseMemoryCachingProvider()
                    .Build();
                return templateEngine;
            });
            
            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();
            services.AddAuthentication();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, Features features)
        {
            app.UseCookiePolicy(new CookiePolicyOptions { MinimumSameSitePolicy = SameSiteMode.Strict });
            InitializeDatabase(app);

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
            app.UseStaticFiles();
            app.UseSpaStaticFiles();
            app.UseRouting();
            app.UseIdentityServer();
            app.UseAuthorization();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "areas",
                    pattern: "{area:exists}/{controller}/");
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller}/{action=Index}/{id?}");
                endpoints.MapRazorPages();
            });

            app.UseSpa(spa =>
            {
                spa.Options.SourcePath = "ClientApp";

                if (env.IsDevelopment())
                {
                    spa.UseReactDevelopmentServer(npmScript: "start");
                }
            });
        }

        private void InitializeDatabase(IApplicationBuilder app)
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
                    foreach (var client in Config.Clients)
                    {
                        context.Clients.Add(client.ToEntity());
                    }
                    context.SaveChanges();
                }
                if (!context.IdentityResources.Any())
                {
                    foreach (var resource in Config.IdentityResources)
                    {
                        context.IdentityResources.Add(resource.ToEntity());
                    }
                    context.SaveChanges();
                }
                if (!context.ApiScopes.Any())
                {
                    foreach (var resource in Config.ApiScopes)
                    {
                        context.ApiScopes.Add(resource.ToEntity());
                    }
                    context.SaveChanges();
                }
            }
        }


    }
}