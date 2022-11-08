using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using AccountsData.Models.DataModels;
using Amazon.S3;
using CanonicalEmails;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.CookiePolicy;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Net.Http.Headers;
using SameSiteMode = Microsoft.AspNetCore.Http.SameSiteMode;

namespace VLO_BOARDS
{
    public partial class Startup
    {
        /// This method configures the app
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, AmazonS3Client minioClient, MinioConfig minioConfig)
        {
            var forwardOptions = new ForwardedHeadersOptions
            {
                ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto | ForwardedHeaders.XForwardedHost,
                // Needed because of mixing http and https.
                RequireHeaderSymmetry = false,
            };

            // Accept X-Forwarded-* headers from all sources.
            forwardOptions.KnownNetworks.Clear();
            forwardOptions.KnownProxies.Clear();

            app.UseForwardedHeaders(forwardOptions);
            
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
                    Secure = CookieSecurePolicy.Always,
                    HttpOnly = HttpOnlyPolicy.Always,
                    MinimumSameSitePolicy = SameSiteMode.Lax
                });
            }
            
            IS4Utils.InitializeDatabase(app, new Config(env));
            EnsureBucketsExits(minioClient, minioConfig).Wait();

            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("v1/swagger.json", "VLO Boards API v1");
            });
            app.UseReDoc(c =>
            {
                c.DocumentTitle = "Dokumentacja API Vlo Boards, dostępna również pod /swagger/";
            });
            
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

            Normalizer.ConfigureDefaults(new NormalizerSettings
            {
                RemoveDots = true,
                RemoveTags = true,
                LowerCase = true,
                NormalizeHost = true
            });

            app.UseRouting();
            app.UseIdentityServer();
            app.UseAuthorization();
            app.UseCors("DefaultExternalOrigins");
            app.UseStaticFiles(new StaticFileOptions
            {
                OnPrepareResponse = ctx =>
                {
                    
                    if (ctx.Context.Request.Path.StartsWithSegments("/static"))
                    {
                        var headers = ctx.Context.Response.GetTypedHeaders();
                        headers.CacheControl = new CacheControlHeaderValue
                        {
                            Public = true,
                            MaxAge = TimeSpan.FromDays(365)
                        };
                    }
                    else
                    {
                        var headers = ctx.Context.Response.GetTypedHeaders();
                        headers.CacheControl = new CacheControlHeaderValue
                        {
                            Public = true,
                            MaxAge = TimeSpan.FromDays(0),
                            NoCache = true,
                            NoStore = true
                        };
                    }
                }
            });
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "areas",
                    pattern: "/api/{area:exists}/{controller}/");
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "/api/{controller}/{action=Index}/{id?}");
                if (!env.IsDevelopment())
                {
                    endpoints.MapFallbackToFile("index.html");
                }
            });

            if (env.IsDevelopment())
            {
                app.UseSpa(a =>
                {
                    a.UseProxyToSpaDevelopmentServer("http://localhost:3000");
                });
            }
            else
            {
                
                app.UseSentryTracing();
            }
        }
        
        public static async Task EnsureBucketsExits(AmazonS3Client client, MinioConfig minioConfig)
        {
            var buckets = await client.ListBucketsAsync();
            var bucketNames = new List<string>();

            foreach (var bucket in buckets.Buckets)
            {
                bucketNames.Add(bucket.BucketName);
            }

            if (!bucketNames.Contains(minioConfig.BucketName))
            {
                await client.PutBucketAsync(minioConfig.BucketName);
            }
            
            if (!bucketNames.Contains(minioConfig.VideoBucketName))
            {
                await client.PutBucketAsync(minioConfig.VideoBucketName);
            }
        }
    }
}