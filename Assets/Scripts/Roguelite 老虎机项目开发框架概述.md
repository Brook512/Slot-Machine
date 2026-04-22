# Roguelite 老虎机项目开发框架概述

## 1. 项目目标

本项目是一个带有 Roguelite 成长循环的老虎机游戏。  
玩家在每一关拥有有限次数的转动机会，在转动前分配点数到不同符号上，转动后根据命中符号和赔率结算分数，最终判断是否达成过关目标并进入下一关。

---

## 2. 框架设计目标

为了避免项目后期出现逻辑混乱、UI 与玩法耦合、规则难扩展等问题，本项目采用分层式框架：

- **Procedure 层**：管理游戏的大流程
- **Runtime 层**：集中保存当前局运行时数据
- **Event 层**：作为系统之间的解耦通信机制
- **RunPhase 层**：管理当前局内部的玩法阶段
- **System 层**：负责执行玩法规则
- **Presentation 层**：负责 UI 和动画表现

---

## 3. 当前第一阶段已建立的骨架

当前第一阶段先搭建以下基础模块：

### 3.1 Procedure
用于管理游戏运行的大流程：

- `BootstrapProcedure`：初始化游戏运行时数据
- `RunProcedure`：进入一局游戏的主流程

后续可以继续扩展：
- `GameOverProcedure`
- `MainMenuProcedure`
- `SettlementProcedure`

### 3.2 Runtime
用于集中保存当前一局游戏的状态：

- `GameRuntime`：总运行时上下文
- `RunSession`：整局级别数据
- `BetRuntime`：当前下注数据
- `RoundRuntime`：当前回合结果数据

### 3.3 EventBus
用于事件订阅和广播，实现逻辑与 UI 的解耦。

当前事件包括：
- `RuntimeInitializedEvent`
- `RunPhaseChangedEvent`
- `PointsChangedEvent`

### 3.4 RunPhase
用于管理一局游戏内部的玩法阶段：

- `PrepareRound`
- `Betting`
- `Spinning`
- `Settling`
- `RoundEnd`
- `LevelEnd`

---

## 4. 当前项目的流程结构

### 外层大流程（Procedure）
`BootstrapProcedure -> RunProcedure`

### 内层玩法流程（RunPhase）
`PrepareRound -> Betting -> Spinning -> Settling -> RoundEnd -> LevelEnd`

这样形成“双层状态控制”：

- Procedure 负责“当前游戏整体在干什么”
- RunPhase 负责“当前一轮玩法进行到哪一步”

---

## 5. 当前代码分层说明

## 5.1 Framework/Procedure
负责管理大流程切换。

- `ProcedureBase.cs`
- `ProcedureManager.cs`
- `BootstrapProcedure.cs`
- `RunProcedure.cs`

## 5.2 Framework/Event
负责统一事件系统。

- `EventBus.cs`

## 5.3 Framework/Runtime
负责统一运行时数据结构。

- `GameRuntime.cs`
- `RunSession.cs`
- `BetRuntime.cs`
- `RoundRuntime.cs`
- `RunPhase.cs`

---

## 6. 下一阶段准备扩展的模块

在当前骨架稳定后，下一步将补充：

### 6.1 System 层
- `BettingSystem`
- `SpinSystem`
- `SettlementSystem`

### 6.2 Data 层
- `SymbolData`
- `LevelData`
- `GameConfig`

### 6.3 Presentation 层
- `HUDView`
- `BetPanelView`
- `WheelView`

---

## 7. 设计原则

本项目后续开发遵循以下原则：

1. **运行时数据集中管理**
   - 所有当前局状态只保存在 Runtime 中

2. **流程与规则分离**
   - Procedure/Phase 负责推进流程
   - System/Rule 负责执行规则

3. **UI 不直接处理核心逻辑**
   - UI 只负责显示和发送输入

4. **通过事件解耦**
   - 系统之间尽量通过 EventBus 通信

5. **先搭骨架，再填玩法**
   - 优先建立可扩展框架，而不是直接堆功能脚本

---

## 8. 当前第一阶段验收标准

完成当前基础骨架后，项目应满足：

- 能在 Unity 场景中挂载 `ProcedureManager`
- 启动后自动进入 `BootstrapProcedure`
- 初始化 `GameRuntime`
- 自动切换到 `RunProcedure`
- `RunProcedure` 能推进到 `Betting` 阶段
- `EventBus` 能正常发布和订阅事件

这代表项目已经具备后续接入玩法系统的基础框架。