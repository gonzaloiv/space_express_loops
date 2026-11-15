using DigitalLove.Game.SpaceShaping;
using DigitalLove.Global;
using UnityEngine;

namespace DigitalLove.Game.PuzzleSelection
{
    // One dotted ring around each chapter's system, just outside its outermost job. Where two
    // systems meet, the part of a ring inside the other is left out, so together they draw one
    // outline pinched between the systems. Each segment fades with distance from the cube like the
    // planets, so the edge shows up as the player explores; the run ending shows it whole.
    public class MapSystemOutlines : MonoBehaviour
    {
        [SerializeField] private PuzzleMapView view;
        [Tooltip("Authored segment under the map root; copies are pooled from it.")]
        [SerializeField] private LineRenderer prototype;
        [SerializeField, Min(8)] private int segmentsPerRing = 48;
        [Tooltip("Gap between a system's outermost job and its ring, in map units.")]
        [SerializeField, Min(0f)] private float margin = 0.04f;
        [SerializeField] private ColorValue color;
        [SerializeField, Range(0f, 1f)] private float alpha = 0.35f;

        private ViewPool<LineRenderer> pool;
        private PuzzleMap shown;
        private int used;

        private ViewPool<LineRenderer> Pool =>
            pool ??= new ViewPool<LineRenderer>(prototype, prototype.transform.parent, "System Outline", null);

        private void LateUpdate()
        {
            PuzzleMap map = view.CurrentMap;
            if (map != shown)
                Build(map);

            if (map != null)
                Fade(view.ShowAll, view.SelectionCubePosition, view.FadeRange);
        }

        private void OnDisable()
        {
            shown = null;
            HideFrom(0);
        }

        private void Build(PuzzleMap map)
        {
            shown = map;
            prototype.enabled = false;
            used = 0;
            if (map != null && map.Chapters.Count > 1)
            {
                SystemRing[] rings = SystemRing.For(map, margin);
                for (int i = 0; i < rings.Length; i++)
                    AddRing(rings, i);
            }

            HideFrom(used);
        }

        private void AddRing(SystemRing[] rings, int index)
        {
            for (int k = 0; k < segmentsPerRing; k++)
            {
                Vector3 from = rings[index].At(k / (float)segmentsPerRing);
                Vector3 to = rings[index].At((k + 1) / (float)segmentsPerRing);
                if (SystemRing.InsideAnother(rings, index, (from + to) * 0.5f))
                    continue;

                LineRenderer segment = Pool.EnsureSlot(used++);
                segment.useWorldSpace = false;
                segment.positionCount = 2;
                segment.SetPosition(0, from);
                segment.SetPosition(1, to);
            }
        }

        // Tinted through the material's base colour, like the route line (the shader ignores vertex
        // colours).
        private void Fade(bool showAll, Vector3 focus, Vector2 nearFar)
        {
            for (int i = 0; i < used; i++)
            {
                LineRenderer segment = Pool.Views[i];
                Vector3 middle = segment.transform.TransformPoint(
                    (segment.GetPosition(0) + segment.GetPosition(1)) * 0.5f);
                float distance = Vector3.Distance(middle, focus);
                float visible = showAll ? 1f : 1f - Mathf.InverseLerp(nearFar.x, nearFar.y, distance);
                segment.enabled = visible > 0f;
                if (segment.enabled)
                    segment.SetBaseColor(color.value.WithAlpha(alpha * visible));
            }
        }

        private void HideFrom(int index) =>
            Pool.ForEachFrom(index, segment => segment.enabled = false);
    }
}
