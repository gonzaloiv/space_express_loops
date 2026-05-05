using UnityEngine;
using UnityEngine.UI;
using System;
using TMPro;
using DigitalLove.UI.DesignSystem;
using DigitalLove.Global;

namespace DigitalLove.Game.Spaceships
{
    public class SpaceshipPanel : MonoBehaviour
    {
        [SerializeField] private Graphic[] graphics;
        [SerializeField] private TextMeshProUGUI idLabel;
        [SerializeField] private BtnPanel btnPanel;
        [SerializeField] private LayoutUpdater layoutUpdater;

        private int routeEditionCost;

        public Action editButtonClicked = () => { };

        public void Show()
        {
            gameObject.SetActive(true);
            btnPanel.Show(new Btn().SetText(routeEditionCost.ToString()).SetOnClick(editButtonClicked));
            layoutUpdater.ForceUpdate();
        }

        public void Init(SpaceshipData data, int routeEditionCost)
        {
            this.routeEditionCost = routeEditionCost;
            foreach (Graphic graphic in graphics)
                graphic.color = data.color;
            idLabel.text = data.id.Substring(data.id.Length - 2, 2);
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
