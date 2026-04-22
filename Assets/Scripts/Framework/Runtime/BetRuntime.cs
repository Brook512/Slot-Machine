/*
思路说明：
1. BetRuntime 用来保存“当前下注阶段”的数据。
2. 它只关心当前这一轮玩家分别在什么符号上压了多少点数。
3. 它不直接负责判断是否合法，也不负责最终奖励计算，那是后续 BettingSystem 和 SettlementSystem 的职责。
4. 它的职责只是作为运行时下注数据容器，并提供基础增删清空方法。
*/

using System.Collections.Generic;

namespace Framework.Runtime
{
    public class BetRuntime
    {
        public Dictionary<string, int> Bets { get; } = new Dictionary<string, int>();

        public int TotalBet { get; private set; }

        public void AddBet(string symbolId, int amount)
        {
            if (string.IsNullOrEmpty(symbolId) || amount <= 0)
                return;

            if (Bets.ContainsKey(symbolId))
            {
                Bets[symbolId] += amount;
            }
            else
            {
                Bets[symbolId] = amount;
            }

            TotalBet += amount;
        }

        public void RemoveBet(string symbolId, int amount)
        {
            if (string.IsNullOrEmpty(symbolId) || amount <= 0)
                return;

            if (!Bets.ContainsKey(symbolId))
                return;

            int actualRemove = amount;
            if (Bets[symbolId] < amount)
            {
                actualRemove = Bets[symbolId];
            }

            Bets[symbolId] -= actualRemove;
            TotalBet -= actualRemove;

            if (Bets[symbolId] <= 0)
            {
                Bets.Remove(symbolId);
            }
        }

        public void Clear()
        {
            Bets.Clear();
            TotalBet = 0;
        }
    }
}