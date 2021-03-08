using System;
using System.Collections.Generic;
using VLO_BOARDS.Models.DataModels.Abstracts;

namespace VLO_BOARDS.Models.DataModels.Implementations.RoleScope
{
    public class BoardScope : Scope
    {
        private void Initialize()
        {
            ParentScopes = new List<Scope> {new GlobalBoardScope()};
        }
        
        public new readonly string Name = "BoardScope";
        public string SubName { get; set; }
        public BoardScope(Board board)
        {
            Initialize();
            
            SubName = board.Name;
        }
        
    }
}