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

        public void Show()
        {
            gameObject.SetActive(true);
            layoutUpdater.ForceUpdate();
            btnPanel.Show(new Btn().SetOnClick(editButtonClicked));
        }

        public void SetData(string id, Color color)
        {
            foreach (Graphic graphic in graphics)
                graphic.color = color;
            idLabel.text = id.Substring(id.Length - 2, 2);
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
