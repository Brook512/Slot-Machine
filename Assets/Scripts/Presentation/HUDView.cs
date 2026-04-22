using TMPro;
using UnityEngine;
using Framework.Event;
using Framework.Procedure;
using Framework.Runtime;

namespace Presentation.UI
{
    public class HUDView : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private ProcedureManager procedureManager;

        [Header("Texts")]
        [SerializeField] private TMP_Text levelText;
        [SerializeField] private TMP_Text pointsText;
        [SerializeField] private TMP_Text totalBetText;
        [SerializeField] private TMP_Text availablePointsText;
        [SerializeField] private TMP_Text goalText;
        [SerializeField] private TMP_Text spinsRemainingText;
        [SerializeField] private TMP_Text phaseText;
        [SerializeField] private TMP_Text lastResultText;

        private GameRuntime _runtime;
        private EventBus _eventBus;

        private void Start()
        {
            if (procedureManager == null)
                procedureManager = FindObjectOfType<ProcedureManager>();

            if (procedureManager == null)
            {
                Debug.LogError("[HUDView] ProcedureManager not found.");
                enabled = false;
                return;
            }

            _runtime = procedureManager.Runtime;
            if (_runtime == null)
            {
                Debug.LogError("[HUDView] Runtime is null.");
                enabled = false;
                return;
            }

            _eventBus = _runtime.EventBus;
            if (_eventBus == null)
            {
                Debug.LogError("[HUDView] EventBus is null.");
                enabled = false;
                return;
            }

            _eventBus.Subscribe<RuntimeInitializedEvent>(OnRuntimeInitialized);
            _eventBus.Subscribe<RunPhaseChangedEvent>(OnRunPhaseChanged);
            _eventBus.Subscribe<PointsChangedEvent>(OnPointsChanged);
            _eventBus.Subscribe<BetChangedEvent>(OnBetChanged);
            _eventBus.Subscribe<SpinCompletedEvent>(OnSpinCompleted);
            _eventBus.Subscribe<SettlementCompletedEvent>(OnSettlementCompleted);
            _eventBus.Subscribe<SpinsChangedEvent>(OnSpinsChanged);

            RefreshAll();
        }

        private void OnDestroy()
        {
            if (_eventBus == null) return;

            _eventBus.Unsubscribe<RuntimeInitializedEvent>(OnRuntimeInitialized);
            _eventBus.Unsubscribe<RunPhaseChangedEvent>(OnRunPhaseChanged);
            _eventBus.Unsubscribe<PointsChangedEvent>(OnPointsChanged);
            _eventBus.Unsubscribe<BetChangedEvent>(OnBetChanged);
            _eventBus.Unsubscribe<SpinCompletedEvent>(OnSpinCompleted);
            _eventBus.Unsubscribe<SettlementCompletedEvent>(OnSettlementCompleted);
            _eventBus.Unsubscribe<SpinsChangedEvent>(OnSpinsChanged);
        }

        private void OnRuntimeInitialized(RuntimeInitializedEvent e)
        {
            RefreshAll();
        }

        private void OnRunPhaseChanged(RunPhaseChangedEvent e)
        {
            SetText(phaseText, $"Phase: {e.NewPhase}");
            RefreshAvailablePoints();
        }

        private void OnPointsChanged(PointsChangedEvent e)
        {
            SetText(pointsText, $"Points: {e.CurrentPoints}");
            RefreshAvailablePoints();
        }

        private void OnBetChanged(BetChangedEvent e)
        {
            SetText(totalBetText, $"Total Bet: {e.TotalBet}");
            RefreshAvailablePoints();
        }

        private void OnSpinCompleted(SpinCompletedEvent e)
        {
            SetText(lastResultText, $"Last Result: {e.SymbolId} (Sector {e.SectorIndex})");
        }

        private void OnSettlementCompleted(SettlementCompletedEvent e)
        {
            string result = e.IsWin
                ? $"Last Result: {e.SymbolId}  Reward: +{e.Reward}"
                : $"Last Result: {e.SymbolId}  Reward: 0";

            SetText(lastResultText, result);
            SetText(pointsText, $"Points: {e.CurrentPoints}");
            SetText(totalBetText, $"Total Bet: {e.TotalBet}");
            RefreshAvailablePoints();
        }

        private void OnSpinsChanged(SpinsChangedEvent e)
        {
            SetText(spinsRemainingText, $"Spins Left: {e.SpinsRemaining}");
        }

        private void RefreshAll()
        {
            if (_runtime == null) return;
            if (_runtime.RunSession == null) return;
            if (_runtime.BetRuntime == null) return;

            SetText(levelText, $"Level: {_runtime.RunSession.CurrentLevel}");
            SetText(pointsText, $"Points: {_runtime.RunSession.CurrentPoints}");
            SetText(totalBetText, $"Total Bet: {_runtime.BetRuntime.TotalBet}");
            SetText(goalText, $"Goal: {_runtime.RunSession.GoalPoints}");
            SetText(spinsRemainingText, $"Spins Left: {_runtime.RunSession.SpinsRemaining}");
            SetText(phaseText, $"Phase: {_runtime.CurrentRunPhase}");

            if (_runtime.RoundRuntime == null || string.IsNullOrEmpty(_runtime.RoundRuntime.ResultSymbolId))
                SetText(lastResultText, "Last Result: -");
            else
                SetText(lastResultText, $"Last Result: {_runtime.RoundRuntime.ResultSymbolId}");

            RefreshAvailablePoints();
        }

        private void RefreshAvailablePoints()
        {
            if (_runtime == null) return;
            if (_runtime.RunSession == null) return;
            if (_runtime.BetRuntime == null) return;

            int available = _runtime.RunSession.CurrentPoints - _runtime.BetRuntime.TotalBet;
            SetText(availablePointsText, $"Available: {available}");
        }

        private void SetText(TMP_Text text, string value)
        {
            if (text != null)
                text.text = value;
        }
    }
}