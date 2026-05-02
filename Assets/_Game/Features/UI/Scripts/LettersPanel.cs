using DigitalLove.VFX;
using TMPro;
using UnityEngine;
using DigitalLove.UI.DesignSystem;

namespace DigitalLove.Game.Planets
{
    public class LettersPanel : MonoBehaviour
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

        public void ShowLetters(int letters, int maxLetters)
        {
            gameObject.SetActive(true);
            lettersLabel.text = maxLetters > 0 ? $"{letters} / {maxLetters}" : $"{letters}";
            scalePunch.Animate();
            layoutUpdater.ForceUpdate();
        }
    }
}