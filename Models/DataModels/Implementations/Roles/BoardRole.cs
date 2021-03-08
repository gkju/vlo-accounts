using System;
using VLO_BOARDS.Models.DataModels;
using VLO_BOARDS.Models.DataModels.Abstracts;
using VLO_BOARDS.Models.DataModels.Implementations.Properties;
using VLO_BOARDS.Models.DataModels.Implementations.RoleScope;
using VLO_BOARDS.Models.DataModels.RoleProperties;


namespace VLO_BOARDS.Models.DataModels.Implementations.Roles
{

        public class BoardRole : Role
        {
            public string Name { get; set; }
            public BoardRole(DataModels.Properties properties, Board board, ApplicationUser author, string roleName)
            {
                Name = roleName;
                scope = new BoardScope(board);
                Id = Guid.NewGuid();
                int authorAuthority = author.GetAuthority(scope);
                this.properties = new GenericRoleProperties(properties, authorAuthority);
            }
            
        }
    
}