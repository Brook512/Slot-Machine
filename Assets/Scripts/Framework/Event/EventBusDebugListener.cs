/*
思路说明：
1. 这是一个纯调试脚本，用来验证 EventBus 是否正常工作。
2. 它不属于正式游戏逻辑，只负责订阅关键事件并输出日志。
3. 当前第二步很适合用它观察：阶段切换、下注变化、转盘结果、结算结果、剩余次数变化。
4. 等正式 UI 完成后，它可以保留作为调试工具，也可以关闭。
*/

using UnityEngine;
using Framework.Event;
using Framework.Procedure;

public class EventBusDebugListener : MonoBehaviour
{
    [SerializeField] private ProcedureManager procedureManager;

    private void Start()
    {
        if (procedureManager == null)
        {
            procedureManager = FindObjectOfType<ProcedureManager>();
        }

        if (procedureManager == null)
        {
            Debug.LogError("[EventBusDebugListener] ProcedureManager not found.");
            return;
        }

        var eventBus = procedureManager.Runtime.EventBus;

        eventBus.Subscribe<RuntimeInitializedEvent>(OnRuntimeInitialized);
        eventBus.Subscribe<RunPhaseChangedEvent>(OnRunPhaseChanged);
        eventBus.Subscribe<BetChangedEvent>(OnBetChanged);
        eventBus.Subscribe<SpinCompletedEvent>(OnSpinCompleted);
        eventBus.Subscribe<SettlementCompletedEvent>(OnSettlementCompleted);
        eventBus.Subscribe<PointsChangedEvent>(OnPointsChanged);
        eventBus.Subscribe<SpinsChangedEvent>(OnSpinsChanged);
    }

    private void OnDestroy()
    {
        if (procedureManager == null || procedureManager.Runtime == null)
            return;

        var eventBus = procedureManager.Runtime.EventBus;

        eventBus.Unsubscribe<RuntimeInitializedEvent>(OnRuntimeInitialized);
        eventBus.Unsubscribe<RunPhaseChangedEvent>(OnRunPhaseChanged);
        eventBus.Unsubscribe<BetChangedEvent>(OnBetChanged);
        eventBus.Unsubscribe<SpinCompletedEvent>(OnSpinCompleted);
        eventBus.Unsubscribe<SettlementCompletedEvent>(OnSettlementCompleted);
        eventBus.Unsubscribe<PointsChangedEvent>(OnPointsChanged);
        eventBus.Unsubscribe<SpinsChangedEvent>(OnSpinsChanged);
    }

    private void OnRuntimeInitialized(RuntimeInitializedEvent e)
    {
        Debug.Log($"[DebugEvent] Init => Level={e.Level}, Points={e.Points}, Goal={e.GoalPoints}, Spins={e.SpinsRemaining}");
    }

    private void OnRunPhaseChanged(RunPhaseChangedEvent e)
    {
        Debug.Log($"[DebugEvent] Phase => {e.NewPhase}");
    }

    private void OnBetChanged(BetChangedEvent e)
    {
        Debug.Log($"[DebugEvent] TotalBet => {e.TotalBet}");
    }

    private void OnSpinCompleted(SpinCompletedEvent e)
    {
        Debug.Log($"[DebugEvent] Spin => Sector={e.SectorIndex}, Symbol={e.SymbolId}");
    }

    private void OnSettlementCompleted(SettlementCompletedEvent e)
    {
        Debug.Log($"[DebugEvent] Settle => Symbol={e.SymbolId}, TotalBet={e.TotalBet}, Reward={e.Reward}, CurrentPoints={e.CurrentPoints}, IsWin={e.IsWin}");
    }

    private void OnPointsChanged(PointsChangedEvent e)
    {
        Debug.Log($"[DebugEvent] Points => {e.CurrentPoints}");
    }

    private void OnSpinsChanged(SpinsChangedEvent e)
    {
        Debug.Log($"[DebugEvent] SpinsRemaining => {e.SpinsRemaining}");
    }
}