using Microsoft.AspNetCore.Mvc.ViewFeatures;
using VLO_BOARDS.Models.DataModels.Helpers;

namespace VLO_BOARDS.Models.DataModels.Implementations.Properties
{
    public class AuthorityProperty : SimpleIntegerProperty
    {
        public AuthorityProperty(int data = 0)
        {
            Data = data;
        }
        
        public override void SetDefaultBannedValue()
        {
            Data = 0;
        }

        public int GetValue()
        {
            return Data;
        }

        public new static readonly string Name = "Authority";
    }
}