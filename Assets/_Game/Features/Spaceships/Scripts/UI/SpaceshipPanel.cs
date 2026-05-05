using UnityEngine;
using UnityEngine.UI;
using System;

namespace DigitalLove.Game.Spaceships
{
    public class SpaceshipPanel : MonoBehaviour
    {
        [SerializeField] private Button editButton;
        [SerializeField] private Graphic[] graphics;

        public Action editButtonClicked = () => { };

        private void OnEnable() => editButton.onClick.AddListener(OnEditButtonClick);

        private void OnDisable() => editButton.onClick.RemoveListener(OnEditButtonClick);

        private void OnEditButtonClick() => editButtonClicked();

        public void Show()
        {
            gameObject.SetActive(true);
        }

        public void SetColor(Color color)
        {
            foreach (Graphic graphic in graphics)
                graphic.color = color;
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }
    }
}
