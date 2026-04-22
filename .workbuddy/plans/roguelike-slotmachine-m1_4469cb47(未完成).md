---
name: roguelike-slotmachine-m1
overview: "Milestone 1: Roguelike 老虎机最小可玩原型。转盘式轮盘（大富翁风格旋转），ScriptableObject 数据驱动符号系统，Coroutine 角度插值实现旋转动画，状态机管理游戏流程，占位图美术。"
todos:
  - id: setup-folder-structure
    content: "[skill:unity] 创建项目文件夹结构（Scripts/Core、Scripts/SlotMachine、Scripts/Betting、Scripts/Data、Scripts/UI、Prefabs、ScriptableObjects/Symbols）"
    status: pending
  - id: create-symbol-data
    content: "[skill:unity] 创建 SymbolData ScriptableObject 定义（Name、Sprite、Multiplier、Weight 字段），生成 4 个符号资产文件"
    status: pending
  - id: build-wheel-prefab
    content: "[skill:unity] 创建转盘 Prefab（圆形底板 + 12 个符号格子 + 中心指针），在 Unity 中手动布局扇区"
    status: pending
    dependencies:
      - create-symbol-data
  - id: implement-spinning-wheel
    content: "[skill:unity] 实现 SpinningWheel.cs：Coroutine 旋转逻辑、扇区命中检测、OnSpinComplete 事件"
    status: pending
    dependencies:
      - build-wheel-prefab
  - id: implement-betting-system
    content: "[skill:unity] 实现 BettingManager.cs：点数分配/扣除/返还逻辑，BetSlot UI 绑定"
    status: pending
    dependencies:
      - create-symbol-data
  - id: implement-game-manager
    content: "[skill:unity] 实现 GameManager.cs：Singleton、GameState 枚举、状态切换、过关/GameOver 判断"
    status: pending
    dependencies:
      - implement-spinning-wheel
      - implement-betting-system
  - id: create-game-scene
    content: "[skill:unity-scene] 创建 Game.unity 场景：Canvas + 转盘区域（WheelArea）+ 投注区域（BettingPanel）+ HUD（点数、转动次数、过关线）"
    status: pending
    dependencies:
      - implement-game-manager
  - id: setup-ui-elements
    content: "[skill:unity-ui] 在场景中创建所有 UI 组件：SpinButton 按钮、各投注槽（+/-/Label）、结果 Text、过渡提示"
    status: pending
    dependencies:
      - create-game-scene
  - id: setup-prefabs
    content: "[skill:unity-prefab] 将转盘和投注槽保存为 Prefab，完成 Game.unity 场景组装"
    status: pending
    dependencies:
      - setup-ui-elements
  - id: implement-ui-controller
    content: "[skill:unity] 实现 GameUI.cs + UIEvents.cs：订阅事件驱动 UI 更新，转动按钮状态控制"
    status: pending
    dependencies:
      - setup-prefabs
---

## 游戏概念

Roguelike 老虎机游戏，核心循环为：玩家每关拥有 5 次转动机会，转动前将点数分配到不同的轮盘符号上，转动后停在某个符号上，如果玩家在投注阶段押中了该符号，则按该符号的赔率倍数返还点数。5 次用完后判断总点数是否达到过关线，达到则进入下一关（过关线递增，符号池扩大）。

## Milestone 1：最小可玩原型（MVP）

**目标**：完成一个可转动、可投注、可结算的最简版本，建立正反馈循环。

### 核心功能

- **轮盘组件**：大富翁转盘式轮盘，12 个格子围成一圈，点击"转动"后以随机速度旋转并逐渐减速停止，指针指向的格子为中奖结果
- **投注系统**：玩家在转动前将可用点数分配到各个符号（+/- 按钮），总投注不超过剩余点数
- **计分系统**：转动停止后，判断指针符号是否在玩家投注列表中，如果在则按 `投注点数 × 符号赔率` 返还
- **状态机**：Idle（待投注）→ Spinning（转动中）→ Result（显示结果）→ 判断过关 / 继续
- **过关判断**：5 次转动结束后，总点数 ≥ 过关线则过关进入下一关，否则 Game Over
- **占位美术**：使用现有 UI/Sprites/ 中的精灵图作为符号占位，背景使用 background.png

### 轮盘视觉设计

轮盘为圆形转盘，12 个扇形区域各放置一个符号精灵，中心有固定指针。转动时整个转盘以 Transform.Rotate 旋转，使用 Coroutine 控制角度插值实现减速效果。

### 数值设计（MVP）

| 参数 | 值 |
| --- | --- |
| 初始点数 | 100 |
| 过关线 | 150 |
| 每关转动次数 | 5 |
| MVP 符号种类 | 4 种（樱桃、铃铛、星、钻） |
| MVP 赔率 | 樱桃×1.5、铃铛×2、星×3、钻×5 |


### 关键 C# 设计模式（Milestone 1 覆盖）

- `MonoBehaviour` + `Coroutine` 生命周期
- `ScriptableObject` 数据定义
- 简单状态机（switch/enum）
- `event` / `Action` 观察者模式（UI 与逻辑解耦）

## 技术栈

- **引擎**：Unity 2022.3.61（Tunai Gi 内核，兼容标准 Unity 工作流）
- **渲染**：URP（Universal Render Pipeline），2D 项目配置
- **UI**：UGUI + TextMeshPro（已安装）
- **动画**：原生 Coroutine（转动动画）+ DOTween（UI 动画，后续安装）
- **数据**：ScriptableObject（符号/关卡/遗物配置）
- **音频**：Unity AudioSource（内置）

## 架构设计

```
Assets/
├── Scripts/
│   ├── Core/
│   │   ├── GameManager.cs          # [NEW] 全局状态机 + Singleton
│   │   ├── GameState.cs            # [NEW] 状态枚举定义
│   │   └── EventBus.cs             # [NEW] C# event/Action 事件总线
│   ├── SlotMachine/
│   │   ├── SpinningWheel.cs        # [NEW] 转盘旋转逻辑（核心）
│   │   ├── WheelSymbol.cs          # [NEW] 转盘格子组件
│   │   └── SpinResult.cs           # [NEW] 转动结果数据结构
│   ├── Betting/
│   │   ├── BettingManager.cs       # [NEW] 点数分配逻辑
│   │   └── PointCalculator.cs      # [NEW] 计分/赔率计算
│   ├── Data/
│   │   └── SymbolData.cs           # [NEW] 符号 ScriptableObject
│   └── UI/
│       ├── GameUI.cs               # [NEW] UI 状态驱动
│       └── UIEvents.cs             # [NEW] UI 事件定义
├── Prefabs/
│   ├── Wheel.prefab                # [NEW] 转盘预制体
│   └── BetSlotItem.prefab          # [NEW] 投注槽预制体
├── ScriptableObjects/
│   ├── Symbols/
│   │   ├── Cherry.asset            # [NEW] 樱桃符号配置
│   │   ├── Bell.asset              # [NEW] 铃铛符号配置
│   │   ├── Star.asset              # [NEW] 星符号配置
│   │   └── Diamond.asset           # [NEW] 钻符号配置
├── Scenes/
│   └── Game.unity                  # [MODIFY] 游戏主场景
```

## 关键技术决策

### 转盘旋转实现

转盘旋转使用 Coroutine + `Mathf.Lerp` 模拟物理减速：

1. 玩家点击转动 → 随机生成 `targetAngle = currentAngle + 720 + Random.Range(0, 360)`
2. Coroutine 中每帧 `currentAngle = Mathf.Lerp(currentAngle, targetAngle, 1 - elapsedRatio)`
3. `elapsedRatio` 从 0 增长到 1，实现先快后慢的减速感
4. 停止时计算指针（指针固定在顶部 0 度）指向的扇区 index
5. 每个扇区对应一个 SymbolData，决定中奖结果

### 符号扇区分配

- 12 个扇区，4 种符号，每种出现 3 次（概率均等）
- 转盘停止角度计算：`sectorIndex = ((360 - normalizedAngle) % 360 / 30)`（每扇区 30 度）

### 事件驱动解耦

GameManager 作为事件发布者，UI/BettingManager 作为订阅者：

- `event Action<SpinResult> OnSpinComplete`
- `event Action<int> OnPointsChanged`
- `event Action<GameState> OnStateChanged`

这样 UI 逻辑和游戏逻辑完全分离，符合 MVC-like 设计。

## DOTween 安装

Milestone 1 完成后，安装 DOTween（免费版）用于后续 UI 动画打磨：

- Unity Asset Store 搜索 "DOTween"（免费）
- 或通过 `Window > Package Manager > Add package from git URL` 安装

## Agent Extensions

### Skill

- **unity**
- 用途：创建 Unity 项目文件夹结构、GameObject、场景配置
- 预期结果：在 `Assets/Scripts/` 下创建所有目录结构，在 Unity Editor 中可见

- **unity-scene**
- 用途：创建 Game.unity 场景，设置 Main Camera 和 Canvas
- 预期结果：Game.unity 场景创建完成，可直接在 Unity Editor 中打开

- **unity-component**
- 用途：为转盘、投注槽等 GameObject 添加和配置组件（Image、SpriteRenderer、Button 等）
- 预期结果：各 Prefab 和场景对象组件配置正确

- **unity-ui**
- 用途：创建 Canvas、转盘 UI 区域、投注面板、HUD 文字等 UGUI 元素
- 预期结果：游戏 UI 层级清晰，所有 TextMeshPro 文字显示正常

- **unity-prefab**
- 用途：将转盘、投注槽等创建为 Prefab 方便复用
- 预期结果：Prefab 保存到 Assets/Prefabs/ 目录，可在场景中实例化