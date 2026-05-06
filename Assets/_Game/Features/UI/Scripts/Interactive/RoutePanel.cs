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

        private int routeEditionCost;

        public Action editButtonClicked = () => { };

        public void Show(Vector3 position)
        {
            transform.position = position;
            gameObject.SetActive(true);
            btnPanel.Show(new Btn().SetText(routeEditionCost.ToString()).SetOnClick(editButtonClicked));
            layoutUpdater.ForceUpdate();
        }

        public void Init(string id, Color color, int routeEditionCost)
        {
            this.routeEditionCost = routeEditionCost;
            foreach (Graphic graphic in graphics)
                graphic.color = color;
            idLabel.text = id.Substring(id.Length - 2, 2);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }

        public void SetEditionButtonActive(bool isActive)
        {
            btnPanel.SetActive(isActive);
        }

        // ! DEBUG

        [Button]
        private void Debug_InvokeEditButtonClicked()
        {
            editButtonClicked();
        }
    }
}
