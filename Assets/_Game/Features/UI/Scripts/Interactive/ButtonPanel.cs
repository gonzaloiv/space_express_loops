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
        [SerializeField] private IntValue cost;

        public Action buttonClicked = () => { };

        public void SetPosition(Vector3 position)
        {
            transform.position = position;
        }

        public void Show()
        {
            gameObject.SetActive(true);
            btnPanel.Show(new Btn().SetText(cost.ToString()).SetOnClick(buttonClicked));
            layoutUpdater.ForceUpdate();
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }

        public void SetActive(bool isActive)
        {
            if (isActive)
            {
                Show();
            }
            else
            {
                Hide();
            }
        }

        // ! DEBUG

        [Button]
        private void Debug_InvokeButtonClicked()
        {
            buttonClicked();
        }
    }
}
