using System.Collections;
using System.Collections.Generic;
using Framework.Event;
using Framework.Procedure;
using Framework.Runtime;
using Game.Data;
using TMPro;
using UnityEngine;

namespace Presentation.UI
{
    public class WheelView : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private ProcedureManager procedureManager;
        [SerializeField] private Transform itemContainer;
        [SerializeField] private WheelItem itemPrefab;
        [SerializeField] private RectTransform highlightRect;
        [SerializeField] private TMP_Text resultText;

        [Header("Animation")]
        [SerializeField] private float startInterval = 0.06f;
        [SerializeField] private float endInterval = 0.18f;
        [SerializeField] private int extraLoops = 3;
        [SerializeField] private float settleDelay = 0.25f;

        private readonly List<WheelItem> _items = new();
        private GameRuntime _runtime;
        private EventBus _eventBus;
        private Coroutine _spinCoroutine;

        private void Start()
        {
            if (procedureManager == null)
                procedureManager = FindObjectOfType<ProcedureManager>();

            if (procedureManager == null)
            {
                Debug.LogError("[WheelView] ProcedureManager not found.");
                enabled = false;
                return;
            }

            _runtime = procedureManager.Runtime;
            if (_runtime == null || _runtime.EventBus == null)
            {
                Debug.LogError("[WheelView] Runtime or EventBus is null.");
                enabled = false;
                return;
            }

            _eventBus = _runtime.EventBus;
            _eventBus.Subscribe<RuntimeInitializedEvent>(OnRuntimeInitialized);
            _eventBus.Subscribe<SpinCompletedEvent>(OnSpinCompleted);

            BuildWheel();
        }

        private void OnDestroy()
        {
            if (_eventBus == null)
                return;

            _eventBus.Unsubscribe<RuntimeInitializedEvent>(OnRuntimeInitialized);
            _eventBus.Unsubscribe<SpinCompletedEvent>(OnSpinCompleted);
        }

        private void OnRuntimeInitialized(RuntimeInitializedEvent e)
        {
            BuildWheel();
        }

        private void BuildWheel()
        {
            ClearItems();

            if (_runtime == null || _runtime.CurrentLevelData == null || _runtime.CurrentLevelData.spinSlots == null)
                return;

            var slots = _runtime.CurrentLevelData.spinSlots;
            for (int i = 0; i < slots.Length; i++)
            {
                var item = Instantiate(itemPrefab, itemContainer);
                item.Bind(slots[i].symbol);
                item.SetHighlighted(false);
                _items.Add(item);
            }

            SetActiveIndex(0);

            if (resultText != null)
                resultText.text = "Result: -";
        }

        private void ClearItems()
        {
            for (int i = itemContainer.childCount - 1; i >= 0; i--)
            {
                Destroy(itemContainer.GetChild(i).gameObject);
            }

            _items.Clear();
        }

        private void OnSpinCompleted(SpinCompletedEvent e)
        {
            if (_spinCoroutine != null)
                StopCoroutine(_spinCoroutine);

            _spinCoroutine = StartCoroutine(PlaySpinAnimation(e.SectorIndex, e.SymbolId));
        }

        private IEnumerator PlaySpinAnimation(int targetIndex, string symbolId)
        {
            if (_items.Count == 0)
                yield break;

            int count = _items.Count;
            int totalSteps = extraLoops * count + targetIndex;

            for (int step = 0; step <= totalSteps; step++)
            {
                int currentIndex = step % count;
                SetActiveIndex(currentIndex);

                float t = totalSteps <= 0 ? 1f : (float)step / totalSteps;
                float interval = Mathf.Lerp(startInterval, endInterval, t);
                yield return new WaitForSeconds(interval);
            }

            if (resultText != null)
                resultText.text = $"Result: {symbolId}";

            yield return new WaitForSeconds(settleDelay);

            _eventBus.Publish(new WheelSpinAnimationCompletedEvent(targetIndex, symbolId));
            _spinCoroutine = null;
        }

        private void SetActiveIndex(int index)
        {
            for (int i = 0; i < _items.Count; i++)
            {
                bool active = i == index;
                _items[i].SetHighlighted(active);
            }

            if (highlightRect != null && index >= 0 && index < _items.Count)
            {
                RectTransform target = _items[index].GetComponent<RectTransform>();
                if (target != null)
                {
                    highlightRect.position = target.position;
                    highlightRect.sizeDelta = target.sizeDelta;
                }
            }
        }
    }
}