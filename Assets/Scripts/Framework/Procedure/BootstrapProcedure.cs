using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/*
思路说明：
1. BootstrapProcedure 负责项目启动后的第一轮初始化。
2. 它的任务不是处理玩法，而是给 GameRuntime 写入一局游戏的初始默认值。
3. 例如：初始关卡、初始点数、初始过关线、初始可转动次数。
4. 初始化完成后，它立即切入 RunProcedure，把控制权交给“真正的一局游戏流程”。
*/


using Framework.Runtime;
using Framework.Event;

namespace Framework.Procedure
{
    public class BootstrapProcedure : ProcedureBase
    {
        private readonly ProcedureManager _procedureManager;

        public BootstrapProcedure(ProcedureManager procedureManager)
        {
            _procedureManager = procedureManager;
        }

        public override void OnEnter(GameRuntime runtime)
        {
            Debug.Log("[BootstrapProcedure] Initializing runtime data...");

            var level = runtime.Config != null ? runtime.Config.GetLevel(0) : null;
            if (level == null)
            {
                Debug.LogError("[BootstrapProcedure] LevelData missing.");
                return;
            }

            runtime.SetLevelData(level);

            runtime.RunSession.CurrentLevel = level.levelIndex;
            runtime.RunSession.CurrentPoints = level.startPoints;
            runtime.RunSession.GoalPoints = level.goalPoints;
            runtime.RunSession.SpinsRemaining = level.spinsPerLevel;
            
            runtime.BetRuntime.Clear();
            runtime.RoundRuntime.Reset();
            runtime.CurrentRunPhase = RunPhase.PrepareRound;

            runtime.EventBus.Publish(new RuntimeInitializedEvent(
                runtime.RunSession.CurrentLevel,
                runtime.RunSession.CurrentPoints,
                runtime.RunSession.GoalPoints,
                runtime.RunSession.SpinsRemaining
            ));

            _procedureManager.ChangeProcedure(new RunProcedure(_procedureManager));
        }
    }
}