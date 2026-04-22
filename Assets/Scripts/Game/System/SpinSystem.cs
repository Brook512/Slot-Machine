/*
思路说明：
1. SpinSystem 负责生成一次转盘结果。
2. 当前第二步先不接真实 WheelView 动画，只做“逻辑结果”的生成。
3. 它的职责包括：
   - 随机得到中奖扇区 index
   - 将扇区 index 映射成符号 id
   - 写入 RoundRuntime
   - 广播转盘完成事件
4. 后续真正接 UI 动画时，这里仍然负责逻辑结果生成，动画只属于表现层。
*/

using Framework.Event;
using Framework.Runtime;
using Game.Data;
using UnityEngine;
namespace Game.Systems
{
    public class SpinSystem
    {
        public void StartSpin(GameRuntime runtime)
        {
            if (runtime == null)
                return;

            LevelData level = runtime.CurrentLevelData;
            if (level == null || level.spinSlots == null || level.spinSlots.Length == 0)
            {
                Debug.LogError("[SpinSystem] LevelData or spinSlots is missing.");
                return;
            }

            // 1) 先计算总权重
            int totalWeight = 0;
            for (int i = 0; i < level.spinSlots.Length; i++)
            {
                totalWeight += Mathf.Max(1, level.spinSlots[i].weight);
            }

            // 2) 随机一个 [0, totalWeight) 的整数
            int roll = Random.Range(0, totalWeight);

            // 3) 按权重区间命中一个槽位
            int sectorIndex = 0;
            int acc = 0;
            for (int i = 0; i < level.spinSlots.Length; i++)
            {
                acc += Mathf.Max(1, level.spinSlots[i].weight);
                if (roll < acc)
                {
                    sectorIndex = i;
                    break;
                }
            }

            SymbolData symbolData = level.spinSlots[sectorIndex].symbol;
            string symbolId = symbolData != null ? symbolData.symbolId : "-";

            runtime.RoundRuntime.ResultSectorIndex = sectorIndex;
            runtime.RoundRuntime.ResultSymbolId = symbolId;

            Debug.Log($"[SpinSystem] Spin result => Sector={sectorIndex}, Symbol={symbolId}");
            runtime.EventBus.Publish(new SpinCompletedEvent(sectorIndex, symbolId));
        }
    }
}