using System;
using UnityEngine;

namespace Game.Data
{
    // 关卡配置：每一关的初始资源和转盘规则都放这里
    [CreateAssetMenu(fileName = "LevelData", menuName = "Game/Level Data")]
    public class LevelData : ScriptableObject
    {
        [Header("Runtime Init")]
        // 当前关卡编号（第几关）
        public int levelIndex = 1;

        // 进入本关时的初始点数
        public int startPoints = 100;

        // 过关目标点数
        public int goalPoints = 150;

        // 本关可旋转次数
        public int spinsPerLevel = 5;

        [Header("Spin Table")]
        // 转盘“池子”：每个槽位对应一个符号 + 一个权重
        // 权重越大，被抽中的概率越高
        public SpinSlot[] spinSlots;

        [Serializable]
        public struct SpinSlot
        {
            // 这个槽位对应哪个符号（引用 SymbolData）
            public SymbolData symbol;

            // 权重，最小为 1，避免出现 0 权重导致逻辑麻烦
            [Min(1)] public int weight;
        }
    }
}
