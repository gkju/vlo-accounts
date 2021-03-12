using System.Collections.Generic;
using IdentityServer4;
using IdentityServer4.Models;
using Microsoft.OpenApi.Writers;

namespace VLO_BOARDS
{
    public static class Config
    {
        
        public static IEnumerable<IdentityResource> IdentityResources => new List<IdentityResource> { new IdentityResources.OpenId(),  new IdentityResources.Profile(), };

        public static IEnumerable<ApiResource> ApiResources => new List<ApiResource>()
        {
            new ApiResource()
            {
                Enabled = true,
                Name = "VLO_BOARDS_API",
                DisplayName = "VLO Boards API",
                AllowedAccessTokenSigningAlgorithms = new List<string> {"ES512"},
                Scopes = new List<string> {"VLO_BOARDS"},
                UserClaims = new List<string> {"name", "email", "openid", "profile"},
                ApiSecrets = new List<Secret> {new Secret("SECRET1")}
            }
        };
        public static IEnumerable<ApiScope> ApiScopes => new List<ApiScope> { new ApiScope("VLO_BOARDS", "VLO Boards api scope") };
        
        public static IEnumerable<Client> Clients =>
            new List<Client>
            {
                new Client
                {
                    Enabled = true,
                    ClientId = "VLO_BOARDS",
                    ClientName = "VLO_BOARDS",
                    AllowedGrantTypes = GrantTypes.Code,
                    RedirectUris = { "https://localhost:44328/authentication/login-callback" },
                    PostLogoutRedirectUris = { "https://localhost:44328/authentication/logout-callback" },
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
                    AllowedCorsOrigins = new List<string>() {"https://localhost:44328", "http://localhost:44328"}
                },
                new Client
                {
                    Enabled = true,
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