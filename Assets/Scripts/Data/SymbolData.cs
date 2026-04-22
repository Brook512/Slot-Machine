using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Game.Data
{
    // 这个特性会让你在 Unity 菜单里右键创建这个配置文件
    // 路径：Create -> Game -> Symbol Data
    [CreateAssetMenu(fileName = "SymbolData", menuName = "Game/Symbol Data")]
    public class SymbolData : ScriptableObject
    {
        // 符号唯一ID（代码里用它来匹配，比如 "Cherry"）
        public string symbolId;

        // 给策划/你自己看的显示名字（可选）
        public string displayName;

        // 赔率倍率：例如 2.0 表示押中后返还 bet * 2
        // [Min(0f)] 表示在 Inspector 中不能小于 0
        [Min(0f)] public float multiplier = 1f;

        // 这个符号在 UI 上显示的图标（可选）
        public Sprite icon;
    }
}