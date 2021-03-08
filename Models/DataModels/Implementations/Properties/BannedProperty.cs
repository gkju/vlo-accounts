using VLO_BOARDS.Models.DataModels.Helpers;

namespace VLO_BOARDS.Models.DataModels.Implementations.Properties
{
    public class BannedProperty : SimpleBoolProperty
    {
        public new static readonly string Name = "Banned";

        public BannedProperty()
        {
            Data = true;
        }
    }
}