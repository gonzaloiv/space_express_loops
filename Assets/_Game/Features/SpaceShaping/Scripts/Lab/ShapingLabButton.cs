using UnityEngine;
using UnityEngine.UI;

namespace DigitalLove.Game.SpaceShaping
{
    public class ShapingLabButton : MonoBehaviour
    {
        [SerializeField] private Button button;

        private bool pressed;

        public bool ConsumePressed()
        {
            bool value = pressed;
            pressed = false;
            return value;
        }

        private void OnEnable() => button.onClick.AddListener(OnPressed);

        private void OnDisable() => button.onClick.RemoveListener(OnPressed);

        private void OnPressed() => pressed = true;
    }
}
