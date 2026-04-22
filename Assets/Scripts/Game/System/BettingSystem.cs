/*
思路说明：
1. BettingSystem 负责处理当前回合的下注行为。
2. 它的职责是修改 BetRuntime，而不是负责 UI 表现。
3. 当前第二步先做最小版本：支持下注、清空下注、读取下注总额。
4. 合法性规则目前先做最基本校验：下注不能超过当前可用点数。
5. 后续如果加入遗物、特殊加成、锁定投注位等规则，可以继续扩展这里。
*/

using UnityEngine;
using Framework.Runtime;
using Framework.Event;

namespace Game.Systems
{
    public class BettingSystem
    {
        public bool PlaceBet(GameRuntime runtime, string symbolId, int amount)
        {
            if (runtime == null || string.IsNullOrEmpty(symbolId) || amount <= 0)
                return false;

            int currentPoints = runtime.RunSession.CurrentPoints;
            int totalBet = runtime.BetRuntime.TotalBet;

            if (totalBet + amount > currentPoints)
            {
                Debug.LogWarning($"[BettingSystem] Bet failed. Not enough points. symbol={symbolId}, amount={amount}");
                return false;
            }

            runtime.BetRuntime.AddBet(symbolId, amount);

            Debug.Log($"[BettingSystem] PlaceBet => {symbolId} +{amount}, TotalBet={runtime.BetRuntime.TotalBet}");
            runtime.EventBus.Publish(new BetChangedEvent(runtime.BetRuntime.TotalBet));

            return true;
        }

        public bool RemoveBet(GameRuntime runtime, string symbolId, int amount)
        {
            if (runtime == null || string.IsNullOrEmpty(symbolId) || amount <= 0)
                return false;

            runtime.BetRuntime.RemoveBet(symbolId, amount);

            Debug.Log($"[BettingSystem] RemoveBet => {symbolId} -{amount}, TotalBet={runtime.BetRuntime.TotalBet}");
            runtime.EventBus.Publish(new BetChangedEvent(runtime.BetRuntime.TotalBet));

            return true;
        }

        public void ClearBets(GameRuntime runtime)
        {
            if (runtime == null)
                return;

            runtime.BetRuntime.Clear();

            Debug.Log("[BettingSystem] ClearBets");
            runtime.EventBus.Publish(new BetChangedEvent(runtime.BetRuntime.TotalBet));
        }
    }
}