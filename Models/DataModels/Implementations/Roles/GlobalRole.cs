using System;
using VLO_BOARDS.Models.DataModels.Abstracts;
using VLO_BOARDS.Models.DataModels.Implementations.Properties;
using VLO_BOARDS.Models.DataModels.Implementations.RoleScope;
using VLO_BOARDS.Models.DataModels.RoleProperties;

namespace VLO_BOARDS.Models.DataModels.Implementations.Roles
{
    public class GlobalRole : Role
    {
        public string Name { get; set; }
        public GlobalRole(DataModels.Properties properties, ApplicationUser author, string Name)
        {
            scope = new GlobalScope();
            this.Name = Name;
            Id = Guid.NewGuid();
            int authorAuthority = author.GetAuthority(scope);
            this.properties = new GenericRoleProperties(properties, authorAuthority);
        }
    }
}