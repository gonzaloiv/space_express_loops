using DigitalLove.VFX;
using TMPro;
using UnityEngine;
using DigitalLove.UI.DesignSystem;

namespace DigitalLove.Game.UI
{
    public class ResourcePanel : MonoBehaviour
    {
        [SerializeField] private ScalePunch scalePunch;
        [SerializeField] private TextMeshProUGUI lettersLabel;
        [SerializeField] private LayoutUpdater layoutUpdater;

        public void Init(Vector3 position)
        {
            transform.position = position;
            Hide();
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }

        public void ShowLetters(int value, int maxValue)
        {
            gameObject.SetActive(true);
            lettersLabel.text = maxValue > 0 ? $"{value} / {maxValue}" : $"{value}";
            scalePunch.Animate();
            layoutUpdater.ForceUpdate();
        }
    }
}