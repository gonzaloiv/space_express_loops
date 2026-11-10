using UnityEngine;

namespace DigitalLove.Game.SpaceShaping
{
    // The cube the grid and lattice are framed to: every planet, and every run's endpoints.
    public static class FieldFrame
    {
        private const float Padding = 0.02f;

        public static void Measure(SpaceShapingSession session, out Vector3 centre, out float half)
        {
            BoundsBuilder builder = new();
            builder.AddPoint(session.Origin);
            builder.AddPoint(session.Destination);
            for (int i = 0; i < session.SourceCount; i++)
                builder.AddSphere(session.Home(i), session.HomeScale(i) * 0.5f);

            for (int i = 0; i < session.ShipCount; i++)
            {
                builder.AddPoint(session.Ship(i).Origin);
                builder.AddPoint(session.Ship(i).Destination);
            }

            builder.TryGet(out centre, out half);
            half += Padding;
        }
    }
}
