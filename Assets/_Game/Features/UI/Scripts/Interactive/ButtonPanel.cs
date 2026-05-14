using UnityEngine;
using DigitalLove.UI.DesignSystem;
using System;
using DigitalLove.Global;

namespace DigitalLove.Game.UI
{
    public class ButtonPanel : MonoBehaviour
    {
        [SerializeField] private BtnPanel btnPanel;
        [SerializeField] private LayoutUpdater layoutUpdater;

        public Action buttonClicked = () => { };

        public void SetPosition(Vector3 position)
        {
            transform.position = position;
        }

        public void Show(int cost)
        {
            gameObject.SetActive(true);
            btnPanel.Show(new Btn().SetText(cost.ToString()).SetOnClick(buttonClicked));
            layoutUpdater.ForceUpdate();
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }

        public void SetActive(bool isActive, int cost)
        {
            if (isActive)
                Show(cost);
            else
                Hide();
        }

        // ! DEBUG

        [Button]
        private void Debug_InvokeButtonClicked()
        {
            buttonClicked();
        }
    }
}
