using DigitalLove.Global;
using UnityEngine;

namespace DigitalLove.Game.Sectors
{
    public static class SectorShape
    {
        private const int ClusterAttempts = 32;

        public static SectorLayout Generate(SectorShapeData data, SeededRandom random)
        {
            SectorShapeRequest request = Resolve(data, random);
            Quaternion rotation = request.TiltDegrees > 0f
                ? Quaternion.Euler(
                    random.Range(-request.TiltDegrees, request.TiltDegrees),
                    random.Range(0f, 360f),
                    random.Range(-request.TiltDegrees, request.TiltDegrees))
                : Quaternion.identity;
            SectorPlanet[] planets = new SectorPlanet[request.PlanetCount];
            Fill(request, random, planets);
            SectorSeparation.Resolve(
                planets, request.MinSeparation, InnerRadius(request), SafeRadius(request));
            for (int i = 0; i < planets.Length; i++)
                planets[i] = new SectorPlanet(rotation * planets[i].Position, planets[i].Radius);
            return new SectorLayout(request.SectorRadius, request.AnchorRadius, rotation, planets);
        }

        private static SectorShapeRequest Resolve(SectorShapeData data, SeededRandom random)
        {
            SectorShapeRequest request = default;
            request.Kind = data.kind;
            request.SectorRadius = random.Range(data.radius);
            request.AnchorRadius = data.anchorRadius > 0f
                ? data.anchorRadius
                : random.Range(data.planetRadius) * random.Range(data.anchorScale);
            request.PlanetCount = Mathf.Max(1, random.RangeInclusive(data.planetCount));
            request.Spread = Mathf.Clamp(data.spread, 0.1f, 1f);
            request.Inclination = Mathf.Max(0f, data.inclination);
            request.MinSeparation = Mathf.Max(1f, data.minSeparation);
            request.PlanetRadius = data.planetRadius;
            request.RingCount = data.ringCount;
            request.ArmCount = data.armCount;
            request.TiltDegrees = data.tilt;
            request.BulgeHeight = data.bulge;
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
                float verticalJitter = (ring == 0 && request.BulgeHeight > 0f)
                    ? request.BulgeHeight * radius
                    : Jitter(request);
                for (int i = 0; i < inRing; i++)
                    planets[index++] = Place(request, random, Angle(phase, i, inRing), radius, verticalJitter);
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
                bool bulged = t < 0.5f && request.BulgeHeight > 0f;
                float verticalJitter = bulged ? request.BulgeHeight * radius : Jitter(request);
                planets[i] = Place(request, random, angle, radius, verticalJitter);
            }
        }

        private static void FillCluster(SectorShapeRequest request, SeededRandom random, SectorPlanet[] planets)
        {
            float inner = InnerRadius(request);
            float outer = Mathf.Max(inner, SafeRadius(request));
            bool ellipsoid = request.BulgeHeight > 0f;
            for (int i = 0; i < planets.Length; i++)
            {
                float radius = SampleRadius(request, random);
                Vector3 position = SampleClusterPoint(request, random, planets, i, radius, inner, outer, ellipsoid);
                planets[i] = new SectorPlanet(position, radius);
            }
        }

        private static Vector3 SampleClusterPoint(
            SectorShapeRequest request,
            SeededRandom random,
            SectorPlanet[] planets,
            int index,
            float radius,
            float inner,
            float outer,
            bool ellipsoid)
        {
            for (int attempt = 0; attempt < ClusterAttempts; attempt++)
            {
                Vector3 candidate = ellipsoid
                    ? SampleSphereDirection(random) * random.Range(inner, outer)
                    : SampleAnnulus(request, random, inner, outer);
                if (IsClear(request, planets, index, radius, candidate))
                    return candidate;
            }

            return ellipsoid
                ? SampleSphereDirection(random) * random.Range(inner, outer)
                : SampleAnnulus(request, random, inner, outer);
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

        // Uniform direction on a sphere: gives a cluster comparable vertical and horizontal extent
        // instead of a flattened disc.
        private static Vector3 SampleSphereDirection(SeededRandom random)
        {
            float angle = random.Range(0f, Mathf.PI * 2f);
            float z = random.Range(-1f, 1f);
            float s = Mathf.Sqrt(Mathf.Max(0f, 1f - z * z));
            return new Vector3(Mathf.Cos(angle) * s, z, Mathf.Sin(angle) * s);
        }

        private static bool IsClear(
            SectorShapeRequest request,
            SectorPlanet[] planets,
            int index,
            float radius,
            Vector3 candidate)
        {
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

        private static SectorPlanet Place(
            SectorShapeRequest request,
            SeededRandom random,
            float angle,
            float radius,
            float verticalJitter)
        {
            Vector3 position = new(
                Mathf.Cos(angle) * radius,
                random.Range(-1f, 1f) * verticalJitter,
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
