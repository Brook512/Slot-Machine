using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
思路说明：
1. ProcedureBase 是所有“大流程状态”的统一基类。
2. 这里的 Procedure 指的是游戏整体流程，例如启动、进入一局、游戏结束等。
3. 它不负责具体玩法规则，只负责定义统一的进入、更新、退出接口。
4. 后续所有具体 Procedure（如 BootstrapProcedure、RunProcedure）都继承它。
*/

using Framework.Runtime;

namespace Framework.Procedure
{
    public abstract class ProcedureBase
    {
        public virtual void OnEnter(GameRuntime runtime) { }

        public virtual void OnUpdate(GameRuntime runtime, float deltaTime) { }

        public virtual void OnExit(GameRuntime runtime) { }
    }
}