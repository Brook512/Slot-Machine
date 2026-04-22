# Slot Machine

Unity Roguelike Slot Machine Game（Unity + C#）

---

## 📁 工程目录规范 (Directory Structure)

```
Assets/Scripts/
├── Data/                    # 存放所有 ScriptableObject 数据配置类
├── Core/                    # 不挂载场景的纯 C# 核心逻辑类
├── Managers/                # 继承 MonoBehaviour 的全局单例管理器
├── Events/                  # 事件总线 EventBus 及事件定义
├── Relics/                  # 所有具体遗物（Buff/道具）逻辑脚本
├── UI/                      # 表现层界面控制脚本
└── (其他模块)/
```

| 文件夹 | 说明 | 依据 |
|--------|------|------|
| `Assets/Scripts/Data/` | 所有继承自 `ScriptableObject` 的数据驱动配置类 | 数据与逻辑分离，支持 Unity Editor 直接配置 |
| `Assets/Scripts/Core/` | 不挂载到场景的纯 C# 核心逻辑（如 `CalculationEngine`、`BoardDataManager`） | 核心算法独立于 Unity 生命周期，便于单元测试和复用 |
| `Assets/Scripts/Managers/` | 继承 `MonoBehaviour` 的全局单例（如 `GameFlowManager`） | 全局状态管理，需要感知 Unity 生命周期（Update/LateUpdate） |
| `Assets/Scripts/Events/` | 事件总线 `EventBus` 及相关事件定义 | 发布-订阅模式解耦模块间通信，避免直接引用 |
| `Assets/Scripts/Relics/` | 所有具体遗物逻辑脚本 | 遗物机制统一入口，方便新增/维护遗物种类 |
| `Assets/Scripts/UI/` | 表现层界面控制脚本 | 界面逻辑与游戏逻辑分离，UI 改动不污染核心 |

---

## 🔧 核心基类与接口设计 (Core Interfaces & Base Classes)

### 1. 数据配置基石 (ScriptableObjects)

通过 Unity Editor 右键菜单 `Create/SlotMachine/` 直接创建配置资源。

| 类名 | 说明 | 依据 |
|------|------|------|
| `SymbolData` | 标志基础属性：`ID`、`名字`、`图标`、`基础倍率` | 老虎机核心玩法的驱动数据，支持配置化管理 |
| `RelicData` | 遗物配置：触发时机枚举 `TriggerTiming` + 效果参数 | 遗物效果多样化需要独立数据载体，与逻辑解耦 |

### 2. 事件总线 (EventBus.cs)

基于字典的发布-订阅模式，解耦所有模块间通信。

```csharp
// 核心 API
EventBus.Subscribe(EventType type, Action<object> listener);   // 订阅
EventBus.Unsubscribe(EventType type, Action<object> listener); // 取消订阅
EventBus.Publish(EventType type, object eventData);            // 发布
```

| 设计要点 | 说明 | 依据 |
|----------|------|------|
| 字典存储 | `Dictionary<EventType, List<Action<object>>>` | O(1) 查找性能，支持多听众 |
| `object` 参数 | 泛型事件数据传递 | 最大灵活性，各模块自行强转类型 |
| 解耦发布/订阅 | 发布者不知道谁在监听 | 新增遗物无需修改现有代码 |

### 3. 遗物基类 (BaseRelic.cs)

所有遗物（Buff / 道具）必须继承此类：

```csharp
public abstract class BaseRelic : MonoBehaviour
{
    protected RelicData Data;

    protected virtual void OnEnable()  => EventBus.Subscribe(Data.TriggerTiming, OnTrigger);
    protected virtual void OnDisable() => EventBus.Unsubscribe(Data.TriggerTiming, OnTrigger);
    public abstract void OnTrigger(object eventData);
}
```

| 生命周期 | 说明 | 依据 |
|----------|------|------|
| `OnEnable` 时 `Subscribe` | 遗物激活即进入监听状态 | 支持运行时动态添加遗物 |
| `OnDisable` 时 `Unsubscribe` | 遗物销毁自动清理订阅 | 防止内存泄漏和野指针回调 |
| `OnTrigger` 重写 | 每个遗物实现自己的修改逻辑 | Open/Closed Principle：对扩展开放，对修改封闭 |

---

## ⚙️ 核心结算管线实现方案 (The Calculation Pipeline)

`CalculationEngine.cs` 严格按以下顺序执行，每一步都通过 `EventBus` 开放扩展点：

```
输入接收
    ↓
获取基础面板 (BoardDataManager)
    ↓
计算基础收益 = Bets[SymbolId] × SymbolData.BaseOdds
    ↓  ← [加算事件] EventBus.Publish(OnCalculateBase, ref baseMoney)
  遗物可修改 baseMoney
    ↓
获取当前乘算因子 currentMultiplier
    ↓  ← [乘算事件] EventBus.Publish(OnCalculateMultiplier, ref currentMultiplier)
  遗物可修改 currentMultiplier
    ↓
finalMoney = baseMoney × currentMultiplier
    ↓
更新 PlayerState 资金
    ↓  ← [结算事件] EventBus.Publish(OnMoneyChanged, finalMoney)
  UI 监听并播放动画
```

| 步骤 | 事件类型 | 遗物作用点 | 依据 |
|------|----------|-----------|------|
| 加算阶段 | `EventType.OnCalculateBase` | 在 `baseMoney` 上累加额外收益 | 加成类遗物（如"每回合+10基础金"） |
| 乘算阶段 | `EventType.OnCalculateMultiplier` | 修改全局乘算因子 | 倍率类遗物（如"所有收益×1.5"） |
| 结算阶段 | `EventType.OnMoneyChanged` | 触发 UI 演出，存档 | 最终金额变化通知 |

> **管线设计依据**：将"计算"拆分为加减算和乘算两个独立扩展点，遗物可以同时作用于两个阶段，组合出丰富效果，同时所有步骤均通过事件总线通知，UI 自动响应，无需在 `CalculationEngine` 中直接引用 UI 代码。

---

## 🏗️ 目录现状 vs 目标结构

| 目标路径 | 当前状态 |
|----------|----------|
| `Assets/Scripts/Data/` | ✅ 已存在：`GameConfig.cs`、`LevelData.cs`、`SymbolData.cs` |
| `Assets/Scripts/Core/` | ⚠️ 需新建：`CalculationEngine.cs`、`BoardDataManager.cs` |
| `Assets/Scripts/Managers/` | ⚠️ 需新建：`GameFlowManager.cs` |
| `Assets/Scripts/Events/` | ✅ 已存在：`EventBus.cs`、`EventBusDebugListener.cs` |
| `Assets/Scripts/Relics/` | ⚠️ 需新建：`BaseRelic.cs` + 具体遗物脚本 |
| `Assets/Scripts/UI/` | ✅ 已存在：`BetButtonBinder.cs`、`SpinButtonBinder.cs`、`RuntimeDebugView.cs` |

---

*Last updated: 2026-04-22*
