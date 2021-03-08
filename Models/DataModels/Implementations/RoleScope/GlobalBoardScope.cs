using System.Collections.Generic;
using VLO_BOARDS.Models.DataModels.Abstracts;

namespace VLO_BOARDS.Models.DataModels.Implementations.RoleScope
{
    public class GlobalBoardScope : Scope
    {
        private void Initialize()
        {
            ParentScopes = new List<Scope> {new GlobalScope()};
        }
        
        public new readonly string Name = "GlobalBoardScope";

        public GlobalBoardScope()
        {
            Initialize();
        }
    }
}