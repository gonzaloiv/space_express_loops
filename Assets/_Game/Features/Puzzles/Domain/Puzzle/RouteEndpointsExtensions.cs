using UnityEngine;

namespace DigitalLove.Game.SpaceShaping
{
    public static class RouteEndpointsExtensions
    {
        public static void Resolve(this RouteEndpoints endpoints, SpaceShapingSession session)
        {
            if (session == null)
            {
                endpoints.ApplySources(NoSource);
                return;
            }

            endpoints.ApplySources(session.IndexOf);
        }

        private static int NoSource(Vector3 point) => -1;
    }
}
