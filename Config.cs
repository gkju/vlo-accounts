using System.Collections.Generic;
using IdentityServer4;
using IdentityServer4.Models;
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
                Name = "VLO_BOARDS_API",
                DisplayName = "VLO Boards API",
                AllowedAccessTokenSigningAlgorithms = new List<string> {"ES512"},
                Scopes = new List<string> {"VLO_BOARDS"},
                UserClaims = new List<string> {"name", "email", "openid", "profile"},
                ApiSecrets = new List<Secret> {new Secret("SECRET1")}
            }
        };
        public IEnumerable<ApiScope> ApiScopes => new List<ApiScope> { new ApiScope("VLO_BOARDS", "VLO Boards api scope") };
        
        public IEnumerable<Client> Clients =>
            new List<Client>
            {
                new Client
                {
                    Enabled = _env.IsDevelopment(),
                    ClientId = "VLO_BOARDS",
                    ClientName = "VLO_BOARDS",
                    AllowedGrantTypes = GrantTypes.Code,
                    RedirectUris = { "https://localhost:5001/login-callback" },
                    PostLogoutRedirectUris = { "https://localhost:5001/logout-callback" },
                        AllowedScopes = new List<string>
                        {
                        IdentityServerConstants.StandardScopes.OpenId,
                        IdentityServerConstants.StandardScopes.Profile,
                        "VLO_BOARDS"
                    },
                        RequirePkce = true,
                        RequireClientSecret = false,
                        ClientSecrets = new List<Secret> {},
                    AllowedIdentityTokenSigningAlgorithms = new List<string> {"ES512"},
                    AllowedCorsOrigins = new List<string>() {"https://localhost:5001", "http://localhost:5001"}
                },
                new Client
                {
                    Enabled = _env.IsDevelopment(),
                    ClientId = "VLO_BOARDS2",
                    ClientName = "VLO_BOARDS2",
                    AllowedGrantTypes = GrantTypes.Code,
                    RedirectUris = { "https://localhost:3000/authentication/login-callback" },
                    PostLogoutRedirectUris = { "https://localhost:3000/authentication/logout-callback" },
                    AllowedScopes = new List<string>
                    {
                        IdentityServerConstants.StandardScopes.OpenId,
                        IdentityServerConstants.StandardScopes.Profile,
                        "VLO_BOARDS"
                    },
                    RequirePkce = true,
                    RequireClientSecret = false,
                    ClientSecrets = new List<Secret> {},
                    AllowedIdentityTokenSigningAlgorithms = new List<string> {"ES512"},
                    AllowedCorsOrigins = new List<string>() {"https://localhost:3000", "http://localhost:3000"}
                }
            };
    }
}