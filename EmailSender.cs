using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity.UI.Services;
using RestSharp;
using RestSharp.Authenticators;

namespace VLO_BOARDS;

public class EmailSender : IEmailSender
{
    private readonly MailgunConfig _config;
    
    public EmailSender(MailgunConfig config)
    {
        _config = config;
    }
    
    public Task SendEmailAsync(string email, string subject, string htmlMessage)
    {
        RestClient client = new RestClient ("https://api.mailgun.net/v3");
        client.Authenticator = new HttpBasicAuthenticator ("api",_config.ApiKey);
        RestRequest request = new RestRequest ();
        request.AddParameter ("domain", _config.DomainName, ParameterType.UrlSegment);
        request.Resource = "{domain}/messages";
        request.AddParameter ("from", $"noreply <noreply@{_config.DomainName}>");
        request.AddParameter ("to", email);
        request.AddParameter ("subject", subject);
        request.AddParameter ("html", htmlMessage);
        return client.PostAsync(request);
    }
}

public class MailgunConfig
{
    public string ApiKey { get; set; }
    public string DomainName { get; set; }
}