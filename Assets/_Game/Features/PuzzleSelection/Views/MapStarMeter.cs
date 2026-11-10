using DigitalLove.Global;
using DigitalLove.Game.SpaceShaping;
using UnityEngine;

namespace DigitalLove.Game.PuzzleSelection
{
    // The chapter's stars on the map field's base edge: earned from the front on both arms, one tick
    // per graded job, the rest dim. No preview or last-edit segments here.
    public class MapStarMeter : MonoBehaviour
    {
        [SerializeField] private FieldEdgeMeterView drawing;
        [SerializeField] private RunSlotView field;
        [SerializeField] private ColorValue accent;
        [SerializeField] private ColorValue neutral;
        [SerializeField, Range(0f, 1f)] private float dimAlpha = 0.12f;

        private readonly GradientColorKey[] colorKeys = new GradientColorKey[4];
        private readonly GradientAlphaKey[] alphaKeys = new GradientAlphaKey[4];
        private Gradient gradient;
        private int ticks;

        private Gradient Gradient => gradient ??= new Gradient { mode = GradientMode.Fixed };

        public void Show(ChapterStars stars)
        {
            ticks = stars.GradedJobs;
            float share = stars.Share;
            Color earned = accent.value;
            Color rest = neutral.value;
            Color fill = share > 0f ? earned : rest;
            SetKey(0, 0f, fill, share > 0f ? 1f : dimAlpha);
            SetKey(1, share, fill, share > 0f ? 1f : dimAlpha);
            SetKey(2, share, rest, dimAlpha);
            SetKey(3, 1f, rest, dimAlpha);
            Gradient.SetKeys(colorKeys, alphaKeys);
            drawing.SetGradient(Gradient);
        }

        private void LateUpdate()
        {
            drawing.Layout(field.FieldCentreMap, field.FieldHalfMap, ticks);
            transform.localRotation = field.FieldRotationMap;
        }

        private void SetKey(int index, float time, Color color, float alpha)
        {
            colorKeys[index] = new GradientColorKey(color, time);
            alphaKeys[index] = new GradientAlphaKey(alpha, time);
        }
    }
}
