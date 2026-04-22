/*
思路说明：
1. GameRuntime 是当前游戏运行时的总上下文。
2. 第三步开始，除了保存运行时数据，还要保存“本轮是否收到了 UI 输入请求”。
3. 这样 RunProcedure 就可以在 Betting 阶段真正等待玩家输入，而不是自动推进。
4. 这些输入标记属于运行时状态，因此也应该集中收纳在这里。
*/

using Framework.Event;
using Game.Data;

namespace Framework.Runtime
{
    public class GameRuntime
    {
        public RunSession RunSession { get; private set; }
        public BetRuntime BetRuntime { get; private set; }
        public RoundRuntime RoundRuntime { get; private set; }
        public EventBus EventBus { get; private set; }

        public RunPhase CurrentRunPhase { get; set; }

        public bool PendingSpinRequest { get; set; }

        public GameConfig Config { get; private set; }
        public LevelData CurrentLevelData { get; private set; }

        public void BindConfig(GameConfig config)
        {
            Config = config;
        }

        public void SetLevelData(LevelData levelData)
        {
            CurrentLevelData = levelData;
        }
        public void Initialize()
        {
            RunSession = new RunSession();
            BetRuntime = new BetRuntime();
            RoundRuntime = new RoundRuntime();
            EventBus = new EventBus();

            CurrentRunPhase = RunPhase.PrepareRound;
            PendingSpinRequest = false;
        }
    }
}