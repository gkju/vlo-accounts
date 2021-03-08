using System;
using VLO_BOARDS.Models.DataModels.Abstracts;
using VLO_BOARDS.Models.DataModels.Implementations.Properties;

namespace VLO_BOARDS.Models.DataModels.Implementations.Roles
{
    public class BannedRole : Role
    {
        public new string Name = "Banned";

        public override bool UserManageable { get; } = false;

        public void Initialize()
        {
            properties = new DataModels.Properties(new BannedProperty());
        }

        public BannedRole(Scope scope)
        {
            this.scope = scope;
            Initialize();
        }
    }
}