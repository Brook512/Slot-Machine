/*
思路说明：
1. RunPhase 定义的是“一局游戏内部”的玩法阶段。
2. 它和 Procedure 不是同一个层级：Procedure 管大流程，RunPhase 管当前玩法推进到哪一步。
3. 当前先定义好完整阶段，哪怕部分阶段还没有写具体系统，也先把骨架留出来。
4. 后续 RunProcedure 会根据这个枚举逐步驱动 Betting、Spinning、Settling 等系统。
*/

namespace Framework.Runtime
{
    public enum RunPhase
    {
        PrepareRound,
        Betting,
        Spinning,
        Settling,
        RoundEnd,
        LevelEnd
    }
}