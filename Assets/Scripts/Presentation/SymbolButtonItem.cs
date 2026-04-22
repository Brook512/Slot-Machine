using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Presentation.UI
{
    public class SymbolButtonItem : MonoBehaviour
    {
        [SerializeField] private Button button;
        [SerializeField] private TMP_Text labelText;
        [SerializeField] private Image iconImage;

        private string _symbolId;
        private System.Action<string> _onClick;

        public void Setup(string symbolId, string displayName, Sprite icon, System.Action<string> onClick)
        {
            _symbolId = symbolId;
            _onClick = onClick;

            if (labelText != null)
                labelText.text = string.IsNullOrEmpty(displayName) ? symbolId : displayName;

            if (iconImage != null)
            {
                if (icon != null)
                {
                    iconImage.gameObject.SetActive(true);
                    iconImage.sprite = icon;
                }
                else
                {
                    iconImage.gameObject.SetActive(false);
                }
            }

            if (button != null)
            {
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(HandleClick);
            }
        }

        public void SetInteractable(bool interactable)
        {
            if (button != null)
                button.interactable = interactable;
        }

        private void HandleClick()
        {
            _onClick?.Invoke(_symbolId);
        }
    }
}