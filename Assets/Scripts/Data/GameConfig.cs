using UnityEngine;

namespace Game.Data
{
    // 总配置：统一管理所有关卡
    [CreateAssetMenu(fileName = "GameConfig", menuName = "Game/Game Config")]
    public class GameConfig : ScriptableObject
    {
        // 所有关卡数据（按顺序放）
        public LevelData[] levels;

        // 根据索引拿关卡，自动防越界
        public LevelData GetLevel(int index)
        {
            // 没配关卡就返回 null，调用方要判空
            if (levels == null || levels.Length == 0) return null;

            // 防止 index 超范围，自动夹到合法范围
            int safe = Mathf.Clamp(index, 0, levels.Length - 1);
            return levels[safe];
        }

        // 用 symbolId 查符号配置（从所有关卡的 spinSlots 里找）
        public SymbolData FindSymbol(string symbolId)
        {
            if (string.IsNullOrEmpty(symbolId)) return null;
            if (levels == null || levels.Length == 0) return null;

            for (int i = 0; i < levels.Length; i++)
            {
                LevelData level = levels[i];
                if (level == null || level.spinSlots == null) continue;

                for (int j = 0; j < level.spinSlots.Length; j++)
                {
                    SymbolData s = level.spinSlots[j].symbol;
                    if (s != null && s.symbolId == symbolId)
                        return s;
                }
            }

            // 没找到就返回 null
            return null;
        }
    }
}
