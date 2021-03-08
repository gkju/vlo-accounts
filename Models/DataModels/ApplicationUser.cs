using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Microsoft.AspNetCore.Identity;
using VLO_BOARDS.Models.DataModels.Abstracts;
using VLO_BOARDS.Models.DataModels.Implementations.Properties;
using VLO_BOARDS.Models.DataModels.Implementations.RoleScope;
using VLO_BOARDS.Models.DataModels.RoleProperties;

namespace VLO_BOARDS.Models.DataModels
{
    public class ApplicationUser : IdentityUser
    {

        private List<Role> Roles { get; set; } = new List<Role>();

        public List<Role> GetRoles(Scope scope)
        {
            List<Scope> scopes = scope.GetAllParentScopesIncludingSelf();
            List<Role> scopedRoles = new List<Role> (Roles);
            scopedRoles = scopedRoles.Where(role => scopes.Contains(role.scope)).ToList();

            return scopedRoles;
        }

        public void AddRole(Role role)
        {
            if (Roles.Contains(role))
            {
                Role ogRole = Roles.FirstOrDefault(x => x == role);
                foreach (var property in role.properties)
                {
                    ogRole.properties.InsertOrMerge(property);
                }
            }
            else
            {
                Roles.Add(role);
            }
        }

        public void RemoveRoles(Role role)
        {
            if (Roles.Contains(role))
            {
                Roles.RemoveAll(x => x == role);
            }
        }

        public Properties GetProperties(Scope scope)
        {
            List<Role> recievedRoles = new List<Role>(GetRoles(scope));
            Properties properties = new Properties(recievedRoles);
            return properties;
        }

        public int GetAuthority(Scope scope)
        {
            try
            {
                return ((AuthorityProperty) GetProperties(scope)[AuthorityProperty.Name]).GetValue();
            }
            catch
            {
                return 0;
            }
        }

        public bool MayManageRole(Role role)
        {
            //use != true for better readability
            if (role.UserManageable != true)
            {
                return false;
            }

            try
            {
                var properties = GetProperties(role.scope);

                if ((MayManageRolesProperty) properties[MayManageRolesProperty.Name] != true)
                {
                    return false;
                }

                if ((AuthorityProperty) properties[AuthorityProperty.Name] <=
                    (AuthorityProperty) role.properties[AuthorityProperty.Name])
                {
                    return false;
                }

                return true;
            }
            catch
            {
                return false;
            }
            
        }
    }
}