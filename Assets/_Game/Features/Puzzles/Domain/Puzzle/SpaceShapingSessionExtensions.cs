using UnityEngine;

namespace DigitalLove.Game.SpaceShaping
{
    public static class SpaceShapingSessionExtensions
    {
        // How far a ship's run passes from being caught: 0 once it reaches the destination's surface
        // (or its capture radius, if wider), otherwise its closest approach beyond it (field metres).
        public static float MissGap(this SpaceShapingSession session, int ship, TrajectoryResult result)
        {
            ShipSpec spec = session.Ship(ship);
            float catchRadius = Mathf.Max(spec.CaptureRadius, session.RadiusAt(spec.Destination));
            return Mathf.Max(result.ClosestApproach(spec.Destination) - catchRadius, 0f);
        }
    }
}
