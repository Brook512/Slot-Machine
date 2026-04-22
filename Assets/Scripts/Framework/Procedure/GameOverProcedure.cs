
using UnityEngine;
using Framework.Runtime;

namespace Framework.Procedure
{
    public class GameOverProcedure : ProcedureBase
    {
        public override void OnEnter(GameRuntime runtime)
        {
            Debug.Log("[GameOverProcedure] Game Over.");
        }
    }
}

