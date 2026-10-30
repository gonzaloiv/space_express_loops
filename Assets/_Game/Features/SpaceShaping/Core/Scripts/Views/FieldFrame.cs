using UnityEngine;

namespace DigitalLove.Game.SpaceShaping
{
    // The cube the grid and lattice are framed to: every planet, and every run's endpoints.
    public static class FieldFrame
    {
        private const float Padding = 0.02f;

        public static void Measure(SpaceShapingSession session, out Vector3 centre, out float half)
        {
            Vector3 min = session.Origin;
            Vector3 max = session.Origin;
            Encapsulate(ref min, ref max, session.Destination);
            for (int i = 0; i < session.SourceCount; i++)
            {
                Vector3 position = session.Home(i);
                float radius = session.HomeScale(i) * 0.5f;
                Encapsulate(ref min, ref max, position - Vector3.one * radius);
                Encapsulate(ref min, ref max, position + Vector3.one * radius);
            }

            for (int i = 0; i < session.ShipCount; i++)
            {
                Encapsulate(ref min, ref max, session.Ship(i).Origin);
                Encapsulate(ref min, ref max, session.Ship(i).Destination);
            }

            centre = (min + max) * 0.5f;
            Vector3 size = max - min;
            half = Mathf.Max(Mathf.Max(size.x, size.y), size.z) * 0.5f + Padding;
        }

        private static void Encapsulate(ref Vector3 min, ref Vector3 max, Vector3 point)
        {
            min = Vector3.Min(min, point);
            max = Vector3.Max(max, point);
        }
    }
}
