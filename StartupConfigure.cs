using System.Collections.Generic;
using System.Threading.Tasks;
using AccountsData.Models.DataModels;
using Amazon.S3;
using CanonicalEmails;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace VLO_BOARDS
{
    public partial class Startup
    {
        /// This method configures the app
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, AmazonS3Client minioClient, MinioConfig minioConfig)
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
            
            IS4Utils.InitializeDatabase(app, new Config(env));
            EnsureBucketsExits(minioClient, minioConfig).Wait();
            

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseMigrationsEndPoint();
                
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
            
            app.UseHttpsRedirection();
            app.UseRouting();
            app.UseIdentityServer();
            app.UseAuthorization();
            app.UseCors("DefaultExternalOrigins");
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
                app.UseSpa(spa =>
                {
                    spa.UseProxyToSpaDevelopmentServer("http://localhost:3000");
                });
            }
        }
        
        private async Task EnsureBucketsExits(AmazonS3Client client, MinioConfig minioConfig)
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