/*
思路说明：
1. SpinButtonBinder 用来把 Unity UI Button 的点击事件转成框架事件。
2. 它不直接操作 RunProcedure，也不直接修改 Runtime。
3. 它只负责在按钮点击时发出 SpinRequestEvent。
4. 这样 UI 层与流程层之间仍然保持解耦。
*/

using UnityEngine;
using UnityEngine.UI;
using Framework.Event;
using Framework.Procedure;

namespace Presentation.UI
{
    public class SpinButtonBinder : MonoBehaviour
    {
        [SerializeField] private ProcedureManager procedureManager;
        [SerializeField] private Button spinButton;

        private void Awake()
        {
            if (spinButton == null)
            {
                spinButton = GetComponent<Button>();
            }
        }

        private void Start()
        {
            if (procedureManager == null)
            {
                procedureManager = FindObjectOfType<ProcedureManager>();
            }

            if (procedureManager == null)
            {
                Debug.LogError("[SpinButtonBinder] ProcedureManager not found.");
                enabled = false;
                return;
            }

            if (spinButton == null)
            {
                Debug.LogError("[SpinButtonBinder] Button reference is null.");
                enabled = false;
                return;
            }

            spinButton.onClick.AddListener(OnSpinClicked);
        }

        private void OnDestroy()
        {
            if (spinButton != null)
            {
                spinButton.onClick.RemoveListener(OnSpinClicked);
            }
        }

        private void OnSpinClicked()
        {
            procedureManager.Runtime.EventBus.Publish(new SpinRequestEvent());
            Debug.Log("[SpinButtonBinder] Publish SpinRequestEvent");
        }
    }
}