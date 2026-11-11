using System;
using UnityEngine;

namespace DigitalLove.Game.SpaceShaping
{
    public enum RouteEndpointRole
    {
        Origin,
        Destination,
        Interchange
    }

    // One job's distinct endpoints, derived from the RoutePlan. A role and its route indices come
    // from the plan; Resolve maps each point to the source the session places (separation can push
    // an endpoint off its authored point), so views spawn at the session's position.
    public sealed class RouteEndpoints
    {
        public const int MaxPips = 4;

        private RoutePlan plan = RoutePlan.Empty;
        private Vector3[] points = Array.Empty<Vector3>();
        private RouteEndpointRole[] roles = Array.Empty<RouteEndpointRole>();
        private int[][] routes = Array.Empty<int[]>();
        private int[] routeCounts = Array.Empty<int>();
        private int[] sources = Array.Empty<int>();
        private int count;

        public int Count => count;

        // The primary ship's destination entry, -1 when the plan is empty.
        public int PrimaryDestination { get; private set; } = -1;

        public int PrimaryDestinationSource =>
            PrimaryDestination >= 0 && PrimaryDestination < count
                ? sources[PrimaryDestination]
                : -1;

        public Vector3 PointAt(int entry) => points[entry];

        public RouteEndpointRole RoleAt(int entry) => roles[entry];

        public int SourceAt(int entry) => sources[entry];

        public int RouteCount(int entry) => routeCounts[entry];

        public int RouteAt(int entry, int pip) => routes[entry][pip];

        public Color ColorAt(int entry, int pip) => plan.ColorAt(routes[entry][pip]);

        public int PipCount(int entry) => Mathf.Min(routeCounts[entry], MaxPips);

        public bool IsEndpointSource(int source)
        {
            for (int i = 0; i < count; i++)
            {
                if (sources[i] == source)
                    return true;
            }

            return false;
        }

        public void Build(RoutePlan value)
        {
            plan = value ?? RoutePlan.Empty;
            EnsureCapacity(plan.Count);
            count = 0;
            for (int route = 0; route < plan.Count; route++)
            {
                AddPoint(plan[route].Origin);
                AddPoint(plan[route].Destination);
            }

            for (int entry = 0; entry < count; entry++)
                GatherRoutes(entry);

            PrimaryDestination = plan.Count > 0 ? Find(plan[0].Destination) : -1;
        }

        public void Resolve(SpaceShapingSession session)
        {
            for (int entry = 0; entry < count; entry++)
                sources[entry] = session != null ? session.IndexOf(points[entry]) : -1;
        }

        private void EnsureCapacity(int routeCount)
        {
            int capacity = routeCount * 2;
            if (routes.Length >= capacity && (capacity == 0 || routes[0].Length >= routeCount))
                return;

            points = new Vector3[capacity];
            roles = new RouteEndpointRole[capacity];
            routes = new int[capacity][];
            routeCounts = new int[capacity];
            sources = new int[capacity];
            for (int i = 0; i < capacity; i++)
                routes[i] = new int[routeCount];
        }

        private void AddPoint(Vector3 point)
        {
            if (Find(point) >= 0)
                return;

            points[count] = point;
            sources[count] = -1;
            count++;
        }

        private int Find(Vector3 point)
        {
            for (int i = 0; i < count; i++)
            {
                if (points[i] == point)
                    return i;
            }

            return -1;
        }

        private void GatherRoutes(int entry)
        {
            int used = 0;
            bool starts = false;
            for (int route = 0; route < plan.Count; route++)
            {
                bool origin = plan[route].Origin == points[entry];
                bool destination = plan[route].Destination == points[entry];
                if (!origin && !destination)
                    continue;

                starts |= origin;
                routes[entry][used++] = route;
            }

            routeCounts[entry] = used;
            roles[entry] = Role(used, starts);
        }

        private static RouteEndpointRole Role(int uses, bool starts) =>
            uses > 1 ? RouteEndpointRole.Interchange
            : starts ? RouteEndpointRole.Origin
            : RouteEndpointRole.Destination;
    }
}
