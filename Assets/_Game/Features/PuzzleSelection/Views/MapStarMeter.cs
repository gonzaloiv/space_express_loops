using DigitalLove.Global;
using DigitalLove.Game.SpaceShaping;
using UnityEngine;

namespace DigitalLove.Game.PuzzleSelection
{
    // The chapter's stars on the map field's base edge: earned from the front on both arms, one tick
    // per job boundary, the rest dim. No preview or last-edit segments here.
    public class MapStarMeter : MonoBehaviour
    {
        [SerializeField] private FieldEdgeMeterView drawing;
        [SerializeField] private RunSlotView field;
        [SerializeField] private ColorValue accent;
        [SerializeField] private ColorValue neutral;
        [SerializeField, Range(0f, 1f)] private float dimAlpha = 0.12f;

        private int ticks;

        public void Show(ChapterStars stars)
        {
            ticks = stars.JobBoundaries;
            float share = stars.Share;
            bool earned = share > 0f;
            Color fill = earned ? accent.value : neutral.value;
            float fillAlpha = earned ? 1f : dimAlpha;
            drawing.SetBands(new MeterBands(
                new MeterBand(0f, fill, fillAlpha),
                new MeterBand(share, fill, fillAlpha),
                new MeterBand(share, neutral.value, dimAlpha),
                new MeterBand(1f, neutral.value, dimAlpha)));
        }

        private void LateUpdate()
        {
            int count = ticks;
            float first = count > 0 ? FieldEdgePath.TickShare(0, count) : 0f;
            float last = count > 0 ? FieldEdgePath.TickShare(count - 1, count) : 0f;
            drawing.Layout(field.FieldCentreMap, field.FieldHalfMap, count, first, last);
            transform.localRotation = field.FieldRotationMap;
        }
    }
}
