/*
思路说明：
1. SettlementSystem 负责根据本轮下注和转盘结果计算奖励。
2. 当前第二步先使用内置倍率表，而不是 ScriptableObject 配置。
3. 结算流程为：
   - 先扣掉本轮总下注
   - 再根据命中符号查找对应下注
   - 如果命中则按 倍率 返还奖励
   - 更新 RunSession.CurrentPoints
4. 这里是规则执行层，UI 只需要订阅结算结果事件，不应直接参与计算。
*/
using UnityEngine;
using Framework.Runtime;
using Framework.Event;
using Game.Data;

namespace Game.Systems
{
    public class SettlementSystem
    {
        public void Settle(GameRuntime runtime)
        {
            if (runtime == null)
                return;

            int totalBet = runtime.BetRuntime.TotalBet;
            string resultSymbolId = runtime.RoundRuntime.ResultSymbolId;

            // 先扣总下注
            runtime.RunSession.CurrentPoints -= totalBet;

            int reward = 0;
            bool isWin = false;

            // 从配置里找命中符号的倍率
            SymbolData symbolData = runtime.Config != null
                ? runtime.Config.FindSymbol(resultSymbolId)
                : null;

            if (symbolData != null &&
                runtime.BetRuntime.Bets.TryGetValue(resultSymbolId, out int hitBet))
            {
                reward = Mathf.FloorToInt(hitBet * symbolData.multiplier);
                isWin = reward > 0;
            }

            runtime.RoundRuntime.RewardPoints = reward;
            runtime.RoundRuntime.IsWin = isWin;

            // 加回奖励
            runtime.RunSession.CurrentPoints += reward;

            Debug.Log($"[SettlementSystem] Settle => TotalBet={totalBet}, Hit={resultSymbolId}, Reward={reward}, CurrentPoints={runtime.RunSession.CurrentPoints}");

            runtime.EventBus.Publish(new PointsChangedEvent(runtime.RunSession.CurrentPoints));
            runtime.EventBus.Publish(new SettlementCompletedEvent(
                resultSymbolId,
                totalBet,
                reward,
                runtime.RunSession.CurrentPoints,
                isWin
            ));
        }
    }
}
