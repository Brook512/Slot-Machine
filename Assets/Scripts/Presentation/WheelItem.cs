using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Game.Data;

namespace Presentation.UI
{
    public class WheelItem : MonoBehaviour
    {
        [SerializeField] private Image iconImage;
        [SerializeField] private GameObject highlightObject;

        public void Bind(SymbolData data)
        {
            if (data == null)
            {
                if (iconImage != null) iconImage.sprite = null;
                return;
            }

            if (iconImage != null)
            {
                iconImage.sprite = data.icon;
                iconImage.enabled = data.icon != null;
            }
        }

        public void SetHighlighted(bool highlighted)
        {
            if (highlightObject != null)
                highlightObject.SetActive(highlighted);
        }
    }
}