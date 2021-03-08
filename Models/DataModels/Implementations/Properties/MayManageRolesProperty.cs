using VLO_BOARDS.Models.DataModels.Helpers;

namespace VLO_BOARDS.Models.DataModels.Implementations.Properties
{
    public class MayManageRolesProperty : SimpleBoolProperty
    {
        public new static readonly string Name = "ManageRoles";

        public MayManageRolesProperty(bool mayManage = false)
        {
            Data = mayManage;
        }
    }
}