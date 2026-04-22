using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Framework.Event;
using Framework.Procedure;
using Framework.Runtime;
using Game.Data;

namespace Presentation.UI
{
    public class BetPanelView : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private ProcedureManager procedureManager;

        [Header("UI References")]
        [SerializeField] private Transform symbolButtonRoot;
        [SerializeField] private SymbolButtonItem symbolButtonPrefab;
        [SerializeField] private Button spinButton;

        [Header("Bet Config")]
        [SerializeField] private int defaultBetAmount = 10;

        private GameRuntime _runtime;
        private EventBus _eventBus;
        private readonly List<SymbolButtonItem> _items = new();

        private void Start()
        {
            InitializeReferences();
            InitializeEvents();
            InitializeButtons();

            BuildSymbolButtons();
            RefreshInteractable();
        }

        private void OnDestroy()
        {
            if (_eventBus == null) return;

            _eventBus.Unsubscribe<RuntimeInitializedEvent>(OnRuntimeInitialized);
            _eventBus.Unsubscribe<RunPhaseChangedEvent>(OnRunPhaseChanged);
            _eventBus.Unsubscribe<SpinsChangedEvent>(OnSpinsChanged);
        }

        private void InitializeReferences()
        {
            if (procedureManager == null)
                procedureManager = FindObjectOfType<ProcedureManager>();

            if (procedureManager == null)
            {
                Debug.LogError("[BetPanelView] ProcedureManager not found.");
                enabled = false;
                return;
            }

            if (symbolButtonRoot == null)
            {
                Debug.LogError("[BetPanelView] SymbolButtonRoot is not assigned.");
                enabled = false;
                return;
            }

            if (symbolButtonPrefab == null)
            {
                Debug.LogError("[BetPanelView] SymbolButtonPrefab is not assigned.");
                enabled = false;
                return;
            }

            if (spinButton == null)
            {
                Debug.LogError("[BetPanelView] SpinButton is not assigned.");
                enabled = false;
                return;
            }

            _runtime = procedureManager.Runtime;
            if (_runtime == null)
            {
                Debug.LogError("[BetPanelView] Runtime is null.");
                enabled = false;
                return;
            }

            _eventBus = _runtime.EventBus;
            if (_eventBus == null)
            {
                Debug.LogError("[BetPanelView] EventBus is null.");
                enabled = false;
                return;
            }
        }

        private void InitializeEvents()
        {
            if (!enabled) return;

            _eventBus.Subscribe<RuntimeInitializedEvent>(OnRuntimeInitialized);
            _eventBus.Subscribe<RunPhaseChangedEvent>(OnRunPhaseChanged);
            _eventBus.Subscribe<SpinsChangedEvent>(OnSpinsChanged);
        }

        private void InitializeButtons()
        {
            if (!enabled) return;

            spinButton.onClick.RemoveAllListeners();
            spinButton.onClick.AddListener(HandleSpinClicked);
        }

        private void OnRuntimeInitialized(RuntimeInitializedEvent e)
        {
            BuildSymbolButtons();
            RefreshInteractable();
        }

        private void OnRunPhaseChanged(RunPhaseChangedEvent e)
        {
            RefreshInteractable();
        }

        private void OnSpinsChanged(SpinsChangedEvent e)
        {
            RefreshInteractable();
        }

        private void BuildSymbolButtons()
        {
            ClearButtons();

            if (_runtime == null || _runtime.CurrentLevelData == null)
            {
                Debug.LogWarning("[BetPanelView] CurrentLevelData is null.");
                return;
            }

            var slots = _runtime.CurrentLevelData.spinSlots;
            if (slots == null || slots.Length == 0)
            {
                Debug.LogWarning("[BetPanelView] spinSlots is empty.");
                return;
            }

            for (int i = 0; i < slots.Length; i++)
            {
                SymbolData symbol = slots[i].symbol;
                if (symbol == null || string.IsNullOrEmpty(symbol.symbolId))
                    continue;

                SymbolButtonItem item = Instantiate(symbolButtonPrefab, symbolButtonRoot);

                RectTransform itemRect = item.GetComponent<RectTransform>();
                if (itemRect != null)
                {
                    itemRect.localScale = Vector3.one;
                    itemRect.anchoredPosition3D = Vector3.zero;
                }

                item.Setup(
                    symbol.symbolId,
                    symbol.symbolId,
                    symbol.icon,
                    HandleSymbolClicked
                );

                _items.Add(item);
            }

            if (symbolButtonRoot is RectTransform rootRect)
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(rootRect);
            }
        }

        private void ClearButtons()
        {
            for (int i = 0; i < _items.Count; i++)
            {
                if (_items[i] != null)
                    Destroy(_items[i].gameObject);
            }

            _items.Clear();
        }

        private void HandleSymbolClicked(string symbolId)
        {
            if (_runtime == null || _eventBus == null)
                return;

            if (_runtime.CurrentRunPhase != RunPhase.Betting)
                return;

            if (_runtime.CurrentLevelData == null)
                return;

            _eventBus.Publish(new BetRequestEvent(symbolId, defaultBetAmount));
        }

        private void HandleSpinClicked()
        {
            if (_runtime == null || _eventBus == null)
                return;

            if (_runtime.CurrentRunPhase != RunPhase.Betting)
                return;

            if (_runtime.RunSession == null)
                return;

            if (_runtime.RunSession.SpinsRemaining <= 0)
                return;

            _eventBus.Publish(new SpinRequestEvent());
        }

        private void RefreshInteractable()
        {
            bool canBet = _runtime != null
                          && _runtime.CurrentRunPhase == RunPhase.Betting
                          && _runtime.CurrentLevelData != null
                          && _runtime.CurrentLevelData.spinSlots != null
                          && _runtime.CurrentLevelData.spinSlots.Length > 0;

            bool canSpin = canBet
                           && _runtime.RunSession != null
                           && _runtime.RunSession.SpinsRemaining > 0;

            for (int i = 0; i < _items.Count; i++)
            {
                if (_items[i] != null)
                    _items[i].SetInteractable(canBet);
            }

            if (spinButton != null)
                spinButton.interactable = canSpin;
        }
    }
}