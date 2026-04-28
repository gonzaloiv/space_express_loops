using DigitalLove.VFX;
using TMPro;
using UnityEngine;

namespace DigitalLove.Game.Planets
{
    public class LettersPanel : MonoBehaviour
    {
        [SerializeField] private ScalePunch scalePunch;
        [SerializeField] private TextMeshProUGUI lettersLabel;

        public void Init(Vector3 position)
        {
            transform.position = position;
            lettersLabel.text = "0";
        }

        public void Show(int letters)
        {
            gameObject.SetActive(true);
            lettersLabel.text = letters.ToString();
            scalePunch.Animate();
        }
    }
}
