/*
思路说明：
1. RunProcedure 负责驱动“一整局游戏中的当前玩法流程”。
2. 第三步开始，不再自动下注和自动 Spin，而是正式等待 UI 输入。
3. 在 Betting 阶段：
   - 玩家可以通过事件请求下注
   - 玩家点击 Spin 后发送 SpinRequestEvent
   - RunProcedure 收到请求后才推进到 Spinning
4. Procedure 只负责流程推进，不直接做下注规则、转盘计算和结算规则。
5. 这些具体逻辑继续交给 BettingSystem、SpinSystem、SettlementSystem。
*/

using UnityEngine;
using Framework.Runtime;
using Framework.Event;
using Game.Systems;

namespace Framework.Procedure
{
    public class RunProcedure : ProcedureBase
    {
        private readonly ProcedureManager _procedureManager;

        private readonly BettingSystem _bettingSystem = new();
        private readonly SpinSystem _spinSystem = new();
        private readonly SettlementSystem _settlementSystem = new();
        private bool _spinAnimationCompleted;

        private bool _phaseEntered;
        private bool _eventsBound;

        public RunProcedure(ProcedureManager procedureManager)
        {
            _procedureManager = procedureManager;
        }

        public override void OnEnter(GameRuntime runtime)
        {
            Debug.Log("[RunProcedure] Enter gameplay procedure.");

            BindEvents(runtime);
            EnterPhase(runtime, RunPhase.PrepareRound);
        }

        public override void OnUpdate(GameRuntime runtime, float deltaTime)
        {
            switch (runtime.CurrentRunPhase)
            {
                case RunPhase.PrepareRound:
                    UpdatePrepareRound(runtime);
                    break;

                case RunPhase.Betting:
                    UpdateBetting(runtime);
                    break;

                case RunPhase.Spinning:
                    UpdateSpinning(runtime);
                    break;

                case RunPhase.Settling:
                    UpdateSettling(runtime);
                    break;

                case RunPhase.RoundEnd:
                    UpdateRoundEnd(runtime);
                    break;

                case RunPhase.LevelEnd:
                    UpdateLevelEnd(runtime);
                    break;
            }
        }

        public override void OnExit(GameRuntime runtime)
        {
            UnbindEvents(runtime);

            Debug.Log("[RunProcedure] Exit gameplay procedure.");
        }

        private void BindEvents(GameRuntime runtime)
        {
            if (_eventsBound)
                return;
            runtime.EventBus.Subscribe<WheelSpinAnimationCompletedEvent>(OnWheelSpinAnimationCompleted);
            runtime.EventBus.Subscribe<BetRequestEvent>(OnBetRequested);
            runtime.EventBus.Subscribe<SpinRequestEvent>(OnSpinRequested);
            _eventsBound = true;
        }

        private void UnbindEvents(GameRuntime runtime)
        {
            if (!_eventsBound || runtime == null || runtime.EventBus == null)
                return;
            runtime.EventBus.Unsubscribe<WheelSpinAnimationCompletedEvent>(OnWheelSpinAnimationCompleted);
            runtime.EventBus.Unsubscribe<BetRequestEvent>(OnBetRequested);
            runtime.EventBus.Unsubscribe<SpinRequestEvent>(OnSpinRequested);
            _eventsBound = false;
        }

        private void UpdatePrepareRound(GameRuntime runtime)
        {
            if (_phaseEntered)
                return;

            _phaseEntered = true;

            _bettingSystem.ClearBets(runtime);
            runtime.RoundRuntime.Reset();
            runtime.PendingSpinRequest = false;

            Debug.Log("[RunProcedure] PrepareRound complete.");
            EnterPhase(runtime, RunPhase.Betting);
        }

        private void UpdateBetting(GameRuntime runtime)
        {
            if (!_phaseEntered)
            {
                _phaseEntered = true;
                Debug.Log("[RunProcedure] Betting phase start. Waiting for player input...");
            }

            if (!runtime.PendingSpinRequest)
                return;

            if (runtime.BetRuntime.TotalBet <= 0)
            {
                Debug.LogWarning("[RunProcedure] Spin requested but total bet is 0. Ignored.");
                runtime.PendingSpinRequest = false;
                return;
            }

            runtime.PendingSpinRequest = false;
            EnterPhase(runtime, RunPhase.Spinning);
        }

        private void UpdateSpinning(GameRuntime runtime)
        {
            if (!_phaseEntered)
            {
                _phaseEntered = true;
                _spinAnimationCompleted = false;

                Debug.Log("[RunProcedure] Spinning phase start.");
                _spinSystem.StartSpin(runtime); // 这里只生成逻辑结果，并广播 SpinCompletedEvent
                return;
            }

            if (!_spinAnimationCompleted)
                return;

            EnterPhase(runtime, RunPhase.Settling);
        }
        private void UpdateSettling(GameRuntime runtime)
        {
            if (_phaseEntered)
                return;

            _phaseEntered = true;

            Debug.Log("[RunProcedure] Settling phase start.");
            _settlementSystem.Settle(runtime);

            EnterPhase(runtime, RunPhase.RoundEnd);
        }

        private void UpdateRoundEnd(GameRuntime runtime)
        {
            if (_phaseEntered)
                return;

            _phaseEntered = true;

            runtime.RunSession.SpinsRemaining--;
            runtime.EventBus.Publish(new SpinsChangedEvent(runtime.RunSession.SpinsRemaining));

            Debug.Log($"[RunProcedure] RoundEnd => SpinsRemaining={runtime.RunSession.SpinsRemaining}");

            if (runtime.RunSession.SpinsRemaining > 0)
            {
                EnterPhase(runtime, RunPhase.PrepareRound);
            }
            else
            {
                EnterPhase(runtime, RunPhase.LevelEnd);
            }
        }

        private void UpdateLevelEnd(GameRuntime runtime)
        {
            if (_phaseEntered)
                return;

            _phaseEntered = true;

            Debug.Log($"[RunProcedure] LevelEnd => CurrentPoints={runtime.RunSession.CurrentPoints}, Goal={runtime.RunSession.GoalPoints}");
        }

        private void EnterPhase(GameRuntime runtime, RunPhase nextPhase)
        {
            runtime.CurrentRunPhase = nextPhase;
            _phaseEntered = false;

            Debug.Log($"[RunProcedure] Change RunPhase => {nextPhase}");
            runtime.EventBus.Publish(new RunPhaseChangedEvent(nextPhase));
        }

        private void OnBetRequested(BetRequestEvent e)
        {
            var runtime = _procedureManager.Runtime;
            if (runtime == null)
                return;

            if (runtime.CurrentRunPhase != RunPhase.Betting)
            {
                Debug.LogWarning("[RunProcedure] Bet request ignored. Current phase is not Betting.");
                return;
            }

            _bettingSystem.PlaceBet(runtime, e.SymbolId, e.Amount);
        }

        private void OnSpinRequested(SpinRequestEvent e)
        {
            var runtime = _procedureManager.Runtime;
            if (runtime == null)
                return;

            if (runtime.CurrentRunPhase != RunPhase.Betting)
            {
                Debug.LogWarning("[RunProcedure] Spin request ignored. Current phase is not Betting.");
                return;
            }

            runtime.PendingSpinRequest = true;
            Debug.Log("[RunProcedure] Spin request received.");
        }
        private void OnWheelSpinAnimationCompleted(WheelSpinAnimationCompletedEvent e)
        {
            var runtime = _procedureManager.Runtime;
            if (runtime == null)
                return;

            if (runtime.CurrentRunPhase != RunPhase.Spinning)
                return;

            _spinAnimationCompleted = true;
            Debug.Log("[RunProcedure] Wheel animation completed.");
        }
    }
}