using System.Collections.Generic;
using Duende.IdentityServer;
using Duende.IdentityServer.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Writers;

namespace VLO_BOARDS
{
    public class Config
    {

        private readonly IWebHostEnvironment _env;
        public Config(IWebHostEnvironment env)
        {
            _env = env;
        }
        
        public IEnumerable<IdentityResource> IdentityResources => new List<IdentityResource> { new IdentityResources.OpenId(),  new IdentityResources.Profile(), };

        public IEnumerable<ApiResource> ApiResources => new List<ApiResource>()
        {
            //for prod retrieve secret in a safe manner
            new ApiResource()
            {
                Enabled = _env.IsDevelopment(),
                Name = "main",
                DisplayName = "VLO main API",
                AllowedAccessTokenSigningAlgorithms = new List<string> {"RS256"},
                Scopes = new List<string> {"main.general"},
                UserClaims = new List<string> {"name", "email", "openid", "profile"},
                ApiSecrets = new List<Secret> {new Secret("SECRET1")}
            }
        };
        public IEnumerable<ApiScope> ApiScopes => new List<ApiScope>
        {
            new ApiScope("main.general", "VLO general scope")
        };
        
        public IEnumerable<Client> Clients => new List<Client>
        {
            new Client
            {
                Enabled = _env.IsDevelopment(),
                ClientId = "VLO_BOARDS_DEV",
                ClientName = "VLO_BOARDS_DEV",
                AllowedGrantTypes = GrantTypes.Code,
                RedirectUris = { "http://localhost:3000/login-callback", "https://localhost:3000/login-callback" },
                PostLogoutRedirectUris = { "http://localhost:3000/logout-callback", "https://localhost:3000/logout-callback" },
                    AllowedScopes = new List<string>
                    {
                    IdentityServerConstants.StandardScopes.OpenId,
                    IdentityServerConstants.StandardScopes.Profile,
                    "main.general"
                },
                RequirePkce = true,
                RequireClientSecret = false,
                ClientSecrets = new List<Secret> {},
                AllowedIdentityTokenSigningAlgorithms = new List<string> {"RS256"},
                AllowedCorsOrigins = new List<string>() {"http://localhost:3000", "https://localhost:3000"}
            },
            new Client
            {
                Enabled = true,
                ClientId = "VLO_BOARDS",
                ClientName = "VLO_BOARDS",
                AllowedGrantTypes = GrantTypes.Code,
                RedirectUris = { "https://accounts.suvlo.pl/login-callback" },
                PostLogoutRedirectUris = { "https://accounts.suvlo.pl/logout-callback" },
                AllowedScopes = new List<string>
                {
                    IdentityServerConstants.StandardScopes.OpenId,
                    IdentityServerConstants.StandardScopes.Profile,
                    "main.general"
                },
                RequirePkce = true,
                RequireClientSecret = false,
                ClientSecrets = new List<Secret> {},
                AllowedIdentityTokenSigningAlgorithms = new List<string> {"RS256"},
                AllowedCorsOrigins = new List<string>() {"https://accounts.suvlo.pl"}
            },
            new Client
            {
                Enabled = _env.IsDevelopment(),
                ClientId = "VLO_MAIN_DEV",
                ClientName = "VLO_MAIN_DEV",
                AllowedGrantTypes = GrantTypes.Code,
                RedirectUris = { "http://localhost:3001/login-callback", "https://localhost:3001/login-callback" },
                PostLogoutRedirectUris = { "http://localhost:3001/logout-callback", "https://localhost:3001/login-callback" },
                AllowedScopes = new List<string>
                {
                    IdentityServerConstants.StandardScopes.OpenId,
                    IdentityServerConstants.StandardScopes.Profile,
                    "main.general"
                },
                RequirePkce = true,
                RequireClientSecret = false,
                ClientSecrets = new List<Secret> {},
                AllowedIdentityTokenSigningAlgorithms = new List<string> {"RS256"},
                AllowedCorsOrigins = new List<string>() {"http://localhost:3001", "https://localhost:3001"}
            },
            new Client
            {
                Enabled = true,
                ClientId = "VLO_MAIN",
                ClientName = "VLO_MAIN",
                AllowedGrantTypes = GrantTypes.Code,
                RedirectUris = { "https://suvlo.pl/login-callback" },
                PostLogoutRedirectUris = { "https://suvlo.pl/logout-callback" },
                AllowedScopes = new List<string>
                {
                    IdentityServerConstants.StandardScopes.OpenId,
                    IdentityServerConstants.StandardScopes.Profile,
                    "main.general"
                },
                RequirePkce = true,
                RequireClientSecret = false,
                ClientSecrets = new List<Secret> {},
                AllowedIdentityTokenSigningAlgorithms = new List<string> {"RS256"},
                AllowedCorsOrigins = new List<string>() {"https://suvlo.pl"}
            },
        };
    }
}