using UnityEngine;
using UnityEngine.UI;
using System;
using TMPro;
using DigitalLove.UI.DesignSystem;
using DigitalLove.Global;

namespace DigitalLove.Game.UI
{
    public class RoutePanel : MonoBehaviour
    {
        [SerializeField] private Graphic[] graphics;
        [SerializeField] private TextMeshProUGUI idLabel;
        [SerializeField] private BtnPanel btnPanel;
        [SerializeField] private LayoutUpdater layoutUpdater;

        public Action editButtonClicked = () => { };

        public void SetPosition(Vector3 position)
        {
            transform.position = position;
        }

        public void Show(string id, Color color)
        {
            gameObject.SetActive(true);
            layoutUpdater.ForceUpdate();
            SetButtonActive(false);
            foreach (Graphic graphic in graphics)
                graphic.color = color;
            idLabel.text = $"ROUTE {id.Substring(id.Length - 2, 2)}";
        }

        public void SetButtonActive(bool isActive)
        {
            if (isActive)
                btnPanel.Show(new Btn().SetOnClick(editButtonClicked));
            else
                btnPanel.Hide();
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }

        // ! DEBUG

        [Button]
        private void Debug_InvokeEditButtonClicked()
        {
            editButtonClicked();
        }
    }
}
