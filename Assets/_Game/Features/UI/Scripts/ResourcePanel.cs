using DigitalLove.VFX;
using TMPro;
using UnityEngine;
using DigitalLove.UI.DesignSystem;
using DigitalLove.Global;
using UnityEngine.UI;

namespace DigitalLove.Game.UI
{
    public class ResourcePanel : MonoBehaviour
    {
        [SerializeField] private ScalePunch scalePunch;
        [SerializeField] private TextMeshProUGUI lettersLabel;
        [SerializeField] private LayoutUpdater layoutUpdater;
        [SerializeField] private ColorValue maxColorValue;
        [SerializeField] private Image backgroundImage;

        private Color initialColor;

        private void Awake()
        {
            if (backgroundImage != null)
                initialColor = backgroundImage.color;
        }

        public void Init(Vector3 position)
        {
            transform.position = position;
            Hide();
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }

        public void ShowLetters(int value, int maxValue, bool showMaxValue = true)
        {
            gameObject.SetActive(true);
            lettersLabel.text = maxValue > 0 ? $"{value} / {maxValue}" : $"{value}";
            scalePunch.Animate();
            layoutUpdater.ForceUpdate();
            if (backgroundImage != null)
                backgroundImage.color = showMaxValue ? maxColorValue.value : initialColor;
        }
    }
}