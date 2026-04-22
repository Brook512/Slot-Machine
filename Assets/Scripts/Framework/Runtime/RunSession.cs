/*
思路说明：
1. RunSession 保存的是“一整局游戏级别”的运行数据。
2. 这些数据不会因为单次转盘结算而消失，而是贯穿当前关卡甚至整局流程。
3. 例如当前关卡、当前点数、目标点数、剩余转动次数都属于这一层。
4. 后续如果加入玩家金币、遗物数量、关卡层数等，也可以继续放在这里。
*/

namespace Framework.Runtime
{
    public class RunSession
    {
        public int CurrentLevel;
        public int CurrentPoints;
        public int GoalPoints;
        public int SpinsRemaining;
    }
}