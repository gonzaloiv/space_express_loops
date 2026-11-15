using DigitalLove.Game.SpaceShaping;
using TMPro;
using UnityEngine;

namespace DigitalLove.Game.PuzzleSelection
{
    // One job's tag on the map: its number and its stars on a single panel over its sun. Its slot
    // fades it with distance from the map cube, the way the planets fade, so sliding the cube
    // reveals the jobs around it before one is picked.
    public class MapJobTag : MonoBehaviour
    {
        [SerializeField] private TMP_Text number;
        [SerializeField] private StarStampView stars;
        [SerializeField] private CanvasGroup group;
        [Tooltip("The number's x beside its stars; with no stars it sits at the tag's centre.")]
        [SerializeField] private float numberBesideStarsX = -75f;

        public void Show(int jobNumber, bool showStars, int earned, Color color)
        {
            gameObject.SetActive(true);
            number.text = jobNumber.ToString();
            number.color = color;
            if (showStars)
                stars.ShowStatic(earned);
            else
                stars.Hide();

            RectTransform rect = number.rectTransform;
            rect.anchoredPosition = new Vector2(showStars ? numberBesideStarsX : 0f, rect.anchoredPosition.y);
        }

        public void SetNumberColor(Color color) => number.color = color;

        public void SetAlpha(float alpha) => group.alpha = Mathf.Clamp01(alpha);

        public void SetVisible(bool visible) => gameObject.SetActive(visible);
    }
}
