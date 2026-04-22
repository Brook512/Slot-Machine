/*
思路说明：
1. BetButtonBinder 用来把某个下注按钮与一个固定下注请求绑定。
2. 例如一个按钮绑定 Cherry +10，另一个按钮绑定 Star +10。
3. 它只负责发布 BetRequestEvent，不直接改运行时数据。
4. 这样做可以先用最小 UI 验证完整下注流程，后面再扩展成正式下注面板。
*/

using UnityEngine;
using UnityEngine.UI;
using Framework.Event;
using Framework.Procedure;

namespace Presentation.UI
{
    public class BetButtonBinder : MonoBehaviour
    {
        [SerializeField] private ProcedureManager procedureManager;
        [SerializeField] private Button targetButton;

        [Header("Bet Config")]
        [SerializeField] private string symbolId = "Cherry";
        [SerializeField] private int amount = 10;

        private void Awake()
        {
            if (targetButton == null)
            {
                targetButton = GetComponent<Button>();
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
                Debug.LogError("[BetButtonBinder] ProcedureManager not found.");
                enabled = false;
                return;
            }

            if (targetButton == null)
            {
                Debug.LogError("[BetButtonBinder] Button reference is null.");
                enabled = false;
                return;
            }

            targetButton.onClick.AddListener(OnBetClicked);
        }

        private void OnDestroy()
        {
            if (targetButton != null)
            {
                targetButton.onClick.RemoveListener(OnBetClicked);
            }
        }

        private void OnBetClicked()
        {
            procedureManager.Runtime.EventBus.Publish(new BetRequestEvent(symbolId, amount));
            Debug.Log($"[BetButtonBinder] Publish BetRequestEvent => {symbolId}, +{amount}");
        }
    }
}