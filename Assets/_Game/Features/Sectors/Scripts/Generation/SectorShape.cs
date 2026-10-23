using DigitalLove.Global;
using UnityEngine;

namespace DigitalLove.Game.Sectors
{
    public static class SectorShape
    {
        private const int ClusterAttempts = 32;
        private const int SeparationPasses = 32;

        public static SectorLayout Generate(SectorShapeData data, SeededRandom random)
        {
            SectorShapeRequest request = Resolve(data, random);
            Quaternion yaw = Quaternion.Euler(0f, random.Range(0f, 360f), 0f);
            SectorPlanet[] planets = new SectorPlanet[request.PlanetCount];
            Fill(request, random, planets);
            ResolveOverlaps(request, planets);
            return new SectorLayout(request.SectorRadius, request.AnchorRadius, yaw, planets);
        }

        private static SectorShapeRequest Resolve(SectorShapeData data, SeededRandom random)
        {
            SectorShapeRequest request = default;
            request.Kind = data.kind;
            request.SectorRadius = random.Range(data.radius);
            request.AnchorRadius = random.Range(data.planetRadius) * random.Range(data.anchorScale);
            request.PlanetCount = Mathf.Max(1, random.RangeInclusive(data.planetCount));
            request.Spread = Mathf.Clamp(data.spread, 0.1f, 1f);
            request.Inclination = Mathf.Max(0f, data.inclination);
            request.MinSeparation = Mathf.Max(1f, data.minSeparation);
            request.PlanetRadius = data.planetRadius;
            request.RingCount = data.ringCount;
            request.ArmCount = data.armCount;
            return request;
        }

        private static void Fill(SectorShapeRequest request, SeededRandom random, SectorPlanet[] planets)
        {
            if (request.Kind == SectorShapeKind.Spiral)
                FillSpiral(request, random, planets);
            else if (request.Kind == SectorShapeKind.Cluster)
                FillCluster(request, random, planets);
            else
                FillRing(request, random, planets);
        }

        private static void FillRing(SectorShapeRequest request, SeededRandom random, SectorPlanet[] planets)
        {
            int rings = Mathf.Clamp(random.RangeInclusive(request.RingCount), 1, planets.Length);
            float phase = random.Range(0f, Mathf.PI * 2f);
            int index = 0;
            for (int ring = 0; ring < rings && index < planets.Length; ring++)
            {
                int inRing = CountInRing(planets.Length, rings, ring);
                float radius = Mathf.Lerp(InnerRadius(request), SafeRadius(request), (ring + 1) / (float)rings);
                for (int i = 0; i < inRing; i++)
                    planets[index++] = Place(request, random, Angle(phase, i, inRing), radius);
            }
        }

        private static void FillSpiral(SectorShapeRequest request, SeededRandom random, SectorPlanet[] planets)
        {
            int arms = Mathf.Clamp(random.RangeInclusive(request.ArmCount), 1, 4);
            float inner = InnerRadius(request);
            float outer = SafeRadius(request);
            float perArm = Mathf.CeilToInt(planets.Length / (float)arms);
            for (int i = 0; i < planets.Length; i++)
            {
                int arm = i % arms;
                float t = (i / arms + 1f) / perArm;
                float radius = Mathf.Lerp(inner, outer, t);
                float angle = arm * Mathf.PI * 2f / arms + t * Mathf.PI * 1.25f;
                planets[i] = Place(request, random, angle, radius);
            }
        }

        private static void FillCluster(SectorShapeRequest request, SeededRandom random, SectorPlanet[] planets)
        {
            for (int i = 0; i < planets.Length; i++)
            {
                float radius = SampleRadius(request, random);
                planets[i] = new SectorPlanet(Vector3.zero, radius);
                planets[i] = new SectorPlanet(SampleClusterPoint(request, random, planets, i), radius);
            }
        }

        private static Vector3 SampleClusterPoint(
            SectorShapeRequest request,
            SeededRandom random,
            SectorPlanet[] planets,
            int index)
        {
            float inner = InnerRadius(request);
            float outer = Mathf.Max(inner, SafeRadius(request));
            for (int attempt = 0; attempt < ClusterAttempts; attempt++)
            {
                Vector3 candidate = SampleAnnulus(request, random, inner, outer);
                if (IsClear(request, planets, index, candidate))
                    return candidate;
            }

            return SampleAnnulus(request, random, inner, outer);
        }

        private static Vector3 SampleAnnulus(
            SectorShapeRequest request,
            SeededRandom random,
            float inner,
            float outer)
        {
            float angle = random.Range(0f, Mathf.PI * 2f);
            float radius = random.Range(inner, outer);
            float jitter = Jitter(request);
            return new Vector3(
                Mathf.Cos(angle) * radius,
                random.Range(-1f, 1f) * jitter,
                Mathf.Sin(angle) * radius);
        }

        private static bool IsClear(
            SectorShapeRequest request,
            SectorPlanet[] planets,
            int index,
            Vector3 candidate)
        {
            float radius = planets[index].Radius;
            if (candidate.magnitude < InnerRadius(request))
                return false;

            for (int i = 0; i < index; i++)
            {
                float required = request.MinSeparation * (radius + planets[i].Radius);
                if ((planets[i].Position - candidate).sqrMagnitude < required * required)
                    return false;
            }

            return true;
        }

        private static void ResolveOverlaps(SectorShapeRequest request, SectorPlanet[] planets)
        {
            for (int pass = 0; pass < SeparationPasses; pass++)
            {
                bool moved = false;
                for (int i = 0; i < planets.Length; i++)
                {
                    for (int j = i + 1; j < planets.Length; j++)
                        moved |= PushApart(planets, i, j, request.MinSeparation);
                }

                if (!moved)
                    return;

                for (int i = 0; i < planets.Length; i++)
                    planets[i] = Constrain(request, planets[i]);
            }
        }

        private static bool PushApart(SectorPlanet[] planets, int i, int j, float minSeparation)
        {
            Vector3 delta = planets[j].Position - planets[i].Position;
            float required = minSeparation * (planets[i].Radius + planets[j].Radius);
            float distance = delta.magnitude;
            if (distance >= required)
                return false;

            Vector3 direction = distance > 0.0001f ? delta / distance : Vector3.right;
            float push = (required - distance) * 0.5f;
            planets[i] = new SectorPlanet(planets[i].Position - direction * push, planets[i].Radius);
            planets[j] = new SectorPlanet(planets[j].Position + direction * push, planets[j].Radius);
            return true;
        }

        private static SectorPlanet Constrain(SectorShapeRequest request, SectorPlanet planet)
        {
            float inner = InnerRadius(request);
            float outer = Mathf.Max(inner, SafeRadius(request));
            float length = planet.Position.magnitude;
            if (length <= 0.0001f)
                return new SectorPlanet(new Vector3(inner, 0f, 0f), planet.Radius);

            float clamped = Mathf.Clamp(length, inner, outer);
            return new SectorPlanet(planet.Position * (clamped / length), planet.Radius);
        }

        private static SectorPlanet Place(
            SectorShapeRequest request,
            SeededRandom random,
            float angle,
            float radius)
        {
            float jitter = Jitter(request);
            Vector3 position = new(
                Mathf.Cos(angle) * radius,
                random.Range(-1f, 1f) * jitter,
                Mathf.Sin(angle) * radius);
            return new SectorPlanet(position, SampleRadius(request, random));
        }

        private static float InnerRadius(SectorShapeRequest request) =>
            request.AnchorRadius + request.PlanetRadius.max * request.MinSeparation;

        private static float SafeRadius(SectorShapeRequest request) =>
            Mathf.Max(InnerRadius(request), request.OuterRadius - Jitter(request));

        private static float Jitter(SectorShapeRequest request) =>
            request.Inclination * request.SectorRadius;

        private static float Angle(float phase, int index, int count) =>
            phase + Mathf.PI * 2f * index / Mathf.Max(1, count);

        private static int CountInRing(int total, int rings, int ring) =>
            total / rings + (ring < total % rings ? 1 : 0);

        private static float SampleRadius(SectorShapeRequest request, SeededRandom random) =>
            random.Range(request.PlanetRadius);
    }
}
