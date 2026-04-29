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

        private int maxLetters;

        public void Init(Vector3 position, int maxLetters)
        {
            transform.position = position;
            this.maxLetters = maxLetters;
            Show(0);
        }

        public void SetMaxLetters(int maxLetters)
        {
            this.maxLetters = maxLetters;
        }

        public void Show(int letters)
        {
            gameObject.SetActive(true);
            lettersLabel.text = maxLetters > 0 ? $"{letters} / {maxLetters}" : $"{letters}";
            scalePunch.Animate();
            layoutUpdater.ForceUpdate();
        }
    }
}