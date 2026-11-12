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

        public void Show(int jobNumber, bool showStars, int earned, Color color)
        {
            gameObject.SetActive(true);
            number.text = jobNumber.ToString();
            number.color = color;
            if (showStars)
                stars.ShowStatic(earned);
            else
                stars.Hide();
        }

        public void SetNumberColor(Color color) => number.color = color;

        public void SetAlpha(float alpha) => group.alpha = Mathf.Clamp01(alpha);

        public void SetVisible(bool visible) => gameObject.SetActive(visible);
    }
}
