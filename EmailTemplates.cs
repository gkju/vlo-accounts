using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Fluid;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Configuration;


namespace VLO_BOARDS
{
    public class EmailTemplates
    {
        public readonly Dictionary<string, string> Templates = new Dictionary<string, string>();
        public readonly IConfiguration _configuration;
        
        public EmailTemplates(IConfiguration configuration)
        {
            _configuration = configuration;
            
            try
            {
                string path = Directory.GetCurrentDirectory();
                List<string> files = new List<string>(Directory.GetFiles(Path.Combine(path, "fluidTemplates")));
                foreach (var filePath in files)
                {
                    string contents = File.ReadAllText(filePath);
                    Templates.Add(Path.GetFileName(filePath), contents);
                }
            }
            catch
            {
                throw new FileLoadException("Failed to load templates");
            }
            
        }

        public async Task<string> RenderFluid(string templateName, Object model)
        {
            var parser = new FluidParser();
            var parserResult = parser.TryParse(Templates[templateName],
                out var template, out var error);
            
            if (!parserResult)
            {
                Console.WriteLine($"Eror: {error}");
                throw new ApplicationException("Error rendering email");
            }

            var renderContext = new TemplateContext(model);

            var renderedTemplate = await template.RenderAsync(renderContext);
                
            var body = PreMailer.Net.PreMailer.MoveCssInline(renderedTemplate).Html;

            return body;
        }

        public Uri GenerateUrl(string route, Dictionary<string, string> qp = null)
        {
            var BaseOrigin = _configuration["BaseOrigin"];
            if (!route.StartsWith("/"))
            {
                route = "/" + route;
            }
            var url = BaseOrigin + route;
            var genUrl = new Uri(QueryHelpers.AddQueryString(url, qp));
            return genUrl;
        }
    }
}