using UnityEngine;

namespace DigitalLove.Game.PuzzleSelection
{
    // A chapter's system outline on the map, in map units: a flat circle round the system's centre,
    // just outside its outermost job.
    public readonly struct SystemRing
    {
        public readonly Vector3 Centre;
        public readonly float Radius;

        public SystemRing(Vector3 centre, float radius)
        {
            Centre = centre;
            Radius = radius;
        }

        // t runs 0..1 once round the ring.
        public Vector3 At(float t)
        {
            float angle = t * Mathf.PI * 2f;
            return Centre + new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * Radius;
        }

        public bool Contains(Vector3 point)
        {
            Vector3 flat = point - Centre;
            flat.y = 0f;
            return flat.sqrMagnitude < Radius * Radius;
        }

        public static SystemRing[] For(PuzzleMap map, float margin)
        {
            SystemRing[] rings = new SystemRing[map.Chapters.Count];
            for (int i = 0; i < rings.Length; i++)
            {
                MapChapter chapter = map.Chapters[i];
                rings[i] = new SystemRing(chapter.Centre, Reach(map, chapter) + margin);
            }

            return rings;
        }

        public static bool InsideAnother(SystemRing[] rings, int own, Vector3 point)
        {
            for (int i = 0; i < rings.Length; i++)
            {
                if (i != own && rings[i].Contains(point))
                    return true;
            }

            return false;
        }

        private static float Reach(PuzzleMap map, MapChapter chapter)
        {
            float reach = 0f;
            for (int i = chapter.Start; i < chapter.Start + chapter.Count && i < map.Count; i++)
            {
                Vector3 flat = map.Position(i) - chapter.Centre;
                flat.y = 0f;
                reach = Mathf.Max(reach, flat.magnitude);
            }

            return reach;
        }
    }
}
