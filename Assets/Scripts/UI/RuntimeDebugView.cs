/*
思路说明：
1. RuntimeDebugView 是一个最小运行时调试显示组件。
2. 它把当前运行时关键数据直接显示到屏幕上，避免每次都盯着 Console。
3. 它只负责显示，不处理任何玩法逻辑。
4. 当前显示：
   - 当前点数
   - 目标点数
   - 剩余次数
   - 当前阶段
   - 当前总下注
   - 本轮结果
5. 后续正式 HUD 成型后，它可以删除，也可以作为调试界面保留。
*/

using TMPro;
using UnityEngine;
using Framework.Event;
using Framework.Procedure;
using Framework.Runtime;

namespace Presentation.UI
{
    public class RuntimeDebugView : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private ProcedureManager procedureManager;

        [Header("TMP Texts")]
        [SerializeField] private TMP_Text pointsText;
        [SerializeField] private TMP_Text goalText;
        [SerializeField] private TMP_Text spinsText;
        [SerializeField] private TMP_Text phaseText;
        [SerializeField] private TMP_Text totalBetText;
        [SerializeField] private TMP_Text resultText;
        [Header("Stability")]
        [SerializeField] private bool singleTextMode = true;
        [SerializeField] private TMP_Text singleText;
        [SerializeField] private bool useOnGuiFallback = true;

        private EventBus _eventBus;
        private GameRuntime _runtime;
        private bool _isDirty;
        private int _lastDirtyFrame = -1;

        private int _cachedPoints;
        private int _cachedGoal;
        private int _cachedSpins;
        private RunPhase _cachedPhase;
        private int _cachedTotalBet;
        private string _cachedResultSymbol = "-";
        private int _cachedReward;
        private bool _clearedMultiTextOnce;
        private string _summaryText = string.Empty;

        // Unity 生命周期：组件启用后的初始化入口。
        // 这里做三件事：找依赖、订阅事件、把当前运行时数据先显示一次。
        private void Start()
        {
            if (procedureManager == null)
            {
                procedureManager = FindObjectOfType<ProcedureManager>();
            }

            if (procedureManager == null)
            {
                Debug.LogError("[RuntimeDebugView] ProcedureManager not found.");
                enabled = false;
                return;
            }

            _runtime = procedureManager.Runtime;
            if (_runtime == null)
            {
                Debug.LogError("[RuntimeDebugView] GameRuntime is null.");
                enabled = false;
                return;
            }

            _eventBus = _runtime.EventBus;
            if (_eventBus == null)
            {
                Debug.LogError("[RuntimeDebugView] EventBus is null.");
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

            if (singleText == null)
            {
                singleText = resultText != null ? resultText : pointsText;
            }

            if (useOnGuiFallback)
            {
                HideTmpDebugTexts();
            }

            ConfigureDebugTexts();
            RefreshFromRuntime();
            ApplyView(force: true);
        }

        // Unity 生命周期：对象销毁时取消订阅，避免空引用和重复回调。
        private void OnDestroy()
        {
            if (_eventBus == null)
                return;

            _eventBus.Unsubscribe<RuntimeInitializedEvent>(OnRuntimeInitialized);
            _eventBus.Unsubscribe<RunPhaseChangedEvent>(OnRunPhaseChanged);
            _eventBus.Unsubscribe<PointsChangedEvent>(OnPointsChanged);
            _eventBus.Unsubscribe<BetChangedEvent>(OnBetChanged);
            _eventBus.Unsubscribe<SpinCompletedEvent>(OnSpinCompleted);
            _eventBus.Unsubscribe<SettlementCompletedEvent>(OnSettlementCompleted);
            _eventBus.Unsubscribe<SpinsChangedEvent>(OnSpinsChanged);
        }

        // Unity 生命周期：每帧执行。
        // 这里刻意把刷新延后到“数据变脏后的下一帧”，避免与当前帧的 Canvas 预渲染流程重入。
        private void Update()
        {
            if (!_isDirty)
                return;

            if (Time.frameCount <= _lastDirtyFrame)
                return;

            ApplyView();
        }

        // 事件回调：运行时初始化后，更新基础显示缓存。
        private void OnRuntimeInitialized(RuntimeInitializedEvent e)
        {
            _cachedPoints = e.Points;
            _cachedGoal = e.GoalPoints;
            _cachedSpins = e.SpinsRemaining;
            MarkDirty();
        }

        // 事件回调：流程阶段变化（如 Betting/Spinning）时更新阶段缓存。
        private void OnRunPhaseChanged(RunPhaseChangedEvent e)
        {
            _cachedPhase = e.NewPhase;
            MarkDirty();
        }

        // 事件回调：点数变化时更新点数缓存。
        private void OnPointsChanged(PointsChangedEvent e)
        {
            _cachedPoints = e.CurrentPoints;
            MarkDirty();
        }

        // 事件回调：总下注变化时更新下注缓存。
        private void OnBetChanged(BetChangedEvent e)
        {
            _cachedTotalBet = e.TotalBet;
            MarkDirty();
        }

        // 事件回调：转轮结束时先记录符号和奖励缓存。
        // 奖励可能会在结算后再次更新，所以这里先取当前 runtime 的值。
        private void OnSpinCompleted(SpinCompletedEvent e)
        {
            _cachedResultSymbol = string.IsNullOrEmpty(e.SymbolId) ? "-" : e.SymbolId;
            _cachedReward = _runtime != null ? _runtime.RoundRuntime.RewardPoints : 0;
            MarkDirty();
        }

        // 事件回调：结算完成后用最终奖励与点数覆盖缓存。
        private void OnSettlementCompleted(SettlementCompletedEvent e)
        {
            _cachedResultSymbol = string.IsNullOrEmpty(e.SymbolId) ? "-" : e.SymbolId;
            _cachedReward = e.Reward;
            _cachedPoints = e.CurrentPoints;
            MarkDirty();
        }

        // 事件回调：剩余次数变化时更新缓存。
        private void OnSpinsChanged(SpinsChangedEvent e)
        {
            _cachedSpins = e.SpinsRemaining;
            MarkDirty();
        }

        // 从运行时对象一次性读取当前状态到本地缓存。
        // 用于启动时首次填充，防止界面一开始是空值。
        private void RefreshFromRuntime()
        {
            if (_runtime == null)
                return;

            _cachedPoints = _runtime.RunSession.CurrentPoints;
            _cachedGoal = _runtime.RunSession.GoalPoints;
            _cachedSpins = _runtime.RunSession.SpinsRemaining;
            _cachedPhase = _runtime.CurrentRunPhase;
            _cachedTotalBet = _runtime.BetRuntime.TotalBet;
            _cachedResultSymbol = string.IsNullOrEmpty(_runtime.RoundRuntime.ResultSymbolId)
                ? "-"
                : _runtime.RoundRuntime.ResultSymbolId;
            _cachedReward = _runtime.RoundRuntime.RewardPoints;
            MarkDirty();
        }

        // 把缓存内容真正写入 UI 文本。
        // force=true 用在首帧强制刷新；普通情况下会走变更检查，减少不必要重建。
        private void ApplyView(bool force = false)
        {
            _summaryText = $"Points: {_cachedPoints}\n" +
                           $"Goal: {_cachedGoal}\n" +
                           $"Spins: {_cachedSpins}\n" +
                           $"Phase: {_cachedPhase}\n" +
                           $"Total Bet: {_cachedTotalBet}\n" +
                           $"Result: {_cachedResultSymbol} / Reward: {_cachedReward}";

            if (useOnGuiFallback)
            {
                _isDirty = false;
                return;
            }

            if (singleTextMode)
            {
                SetText(singleText, _summaryText, force);

                if (!_clearedMultiTextOnce)
                {
                    // Clear the other TMP fields once so only one TMP keeps parsing.
                    if (pointsText != singleText) SetText(pointsText, string.Empty, true);
                    if (goalText != singleText) SetText(goalText, string.Empty, true);
                    if (spinsText != singleText) SetText(spinsText, string.Empty, true);
                    if (phaseText != singleText) SetText(phaseText, string.Empty, true);
                    if (totalBetText != singleText) SetText(totalBetText, string.Empty, true);
                    if (resultText != singleText) SetText(resultText, string.Empty, true);
                    _clearedMultiTextOnce = true;
                }

                _isDirty = false;
                return;
            }

            SetText(pointsText, $"Points: {_cachedPoints}", force);
            SetText(goalText, $"Goal: {_cachedGoal}", force);
            SetText(spinsText, $"Spins: {_cachedSpins}", force);
            SetText(phaseText, $"Phase: {_cachedPhase}", force);
            SetText(totalBetText, $"Total Bet: {_cachedTotalBet}", force);
            SetText(resultText, $"Result: {_cachedResultSymbol} / Reward: {_cachedReward}", force);
            _isDirty = false;
        }

        // 调试文本不需要富文本，关闭后可减少 TMP 解析路径里的额外工作。
        private void ConfigureDebugTexts()
        {
            SetRichText(pointsText, false);
            SetRichText(goalText, false);
            SetRichText(spinsText, false);
            SetRichText(phaseText, false);
            SetRichText(totalBetText, false);
            SetRichText(resultText, false);
        }

        private static void SetRichText(TMP_Text target, bool enabled)
        {
            if (target == null)
                return;

            target.richText = enabled;
        }

        private void MarkDirty()
        {
            _isDirty = true;
            _lastDirtyFrame = Time.frameCount;
        }

        private void HideTmpDebugTexts()
        {
            HideText(pointsText);
            HideText(goalText);
            HideText(spinsText);
            HideText(phaseText);
            HideText(totalBetText);
            HideText(resultText);
        }

        private static void HideText(TMP_Text text)
        {
            if (text == null)
                return;

            text.gameObject.SetActive(false);
        }

        // 安全设置 TMP 文本：
        // 1) 目标为空直接返回。
        // 2) 文本没变化且非强制刷新时不写入，避免额外的 TMP 解析与分配。
        private void SetText(TMP_Text target, string value, bool force)
        {
            if (target == null)
                return;

            if (!force && target.text == value)
                return;

            target.text = value;
        }

        private void OnGUI()
        {
            if (!useOnGuiFallback || !Application.isPlaying)
                return;

            if (string.IsNullOrEmpty(_summaryText))
                return;

            const int x = 20;
            const int y = 20;
            const int width = 360;
            const int height = 180;
            GUI.Label(new Rect(x, y, width, height), _summaryText);
        }
    }
}
