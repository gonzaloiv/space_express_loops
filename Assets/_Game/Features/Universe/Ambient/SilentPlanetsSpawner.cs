using System.Collections.Generic;
using DigitalLove.Game.Nodes;
using DigitalLove.Global;
using UnityEngine;

namespace DigitalLove.Game.SilentPlanets
{
    public class SilentPlanetsSpawner : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private MrukRoomLocalPlacement roomPlacement;
        [SerializeField] private HubsSpawner hubsSpawner;
        [SerializeField] private SilentPlanetBehaviour prototype;

        [Header("Budget")]
        [SerializeField, Min(0)] private int planetCount = 36;
        [SerializeField, Min(0)] private int planetCap = 48;
        [SerializeField, Min(1)] private int clusterCount = 4;
        [SerializeField, Min(0f)] private float clusterRadius = 0.7f;
        [SerializeField, Min(0f)] private float clusterRingRadius = 0.35f;
        [SerializeField, Min(0f)] private float clusterRingJitter = 0.25f;
        [SerializeField, Min(0f)] private float verticalMargin;
        [SerializeField, Min(0f)] private float minSeparation = 0.22f;
        [SerializeField, Min(0f)] private float spacingMultiplier = 1.15f;
        [SerializeField, Min(0f)] private float anchorClearance = 0.3f;
        [SerializeField, Min(0f)] private float minRadius = 0.08f;
        [SerializeField, Min(0f)] private float maxRadius = 0.14f;
        [SerializeField, Min(1)] private int placementAttempts = 64;

        [Header("Room view")]
        [SerializeField, Min(0f)] private float roomVisibleRadius = 3f;

        private readonly List<SilentPlanetBehaviour> planets = new();
        private bool hasSpawned;
        private bool isBubbleView;

        public void ResetPool()
        {
            HideAll();
            InitializePool();
        }

        public void SetBubbleView(bool bubble)
        {
            isBubbleView = bubble;
            if (bubble)
                ShowAll();
        }

        public void SpawnIfNeeded(int seed)
        {
            if (hasSpawned)
                return;

            hasSpawned = true;
            SeededRandom random = new(seed);
            Vector3 anchorLocal = GetAnchorLocal();
            roomPlacement.TryGetHeightRange(maxRadius, verticalMargin, out float lowestY, out float highestY);
            int count = Mathf.Clamp(planetCount, 0, Mathf.Max(0, planetCap));
            SilentPlanetLayout layout = new(
                random,
                anchorLocal,
                new Vector2(lowestY, highestY),
                clusterRingRadius,
                clusterRingJitter);
            Vector3[] centers = layout.BuildClusterCenters(count, clusterCount);
            for (int i = 0; i < count; i++)
                TrySpawnPlanet(random, centers[i % centers.Length], anchorLocal, layout);
        }

        private void LateUpdate()
        {
            if (!hasSpawned || isBubbleView)
            {
                ShowAll();
                return;
            }

            UpdateRoomVisibility();
        }

        private void UpdateRoomVisibility()
        {
            Camera camera = Camera.main;
            if (camera == null)
            {
                ShowAll();
                return;
            }

            Vector3 eye = camera.transform.position;
            float sqrRadius = roomVisibleRadius * roomVisibleRadius;
            for (int i = 0; i < planets.Count; i++)
            {
                SilentPlanetBehaviour planet = planets[i];
                if (planet == null || !planet.IsActive)
                    continue;

                planet.SetVisible((planet.Position - eye).sqrMagnitude <= sqrRadius);
            }
        }

        private void TrySpawnPlanet(
            SeededRandom random,
            Vector3 center,
            Vector3 anchorLocal,
            SilentPlanetLayout layout)
        {
            for (int i = 0; i < placementAttempts; i++)
            {
                float radius = random.Range(minRadius, maxRadius);
                Vector3 candidate = center + random.InsideCircleXZ(clusterRadius);
                candidate.y = layout.NextY();
                if (!IsAcceptable(candidate, radius, anchorLocal))
                    continue;

                SilentPlanetBehaviour planet = AcquirePlanet();
                planet.Spawn(candidate, radius, random.NextInt());
                roomPlacement.Register(candidate, radius, false);
                return;
            }
        }

        private bool IsAcceptable(Vector3 candidate, float radius, Vector3 anchorLocal)
        {
            float clearance = anchorClearance + radius;
            if ((candidate - anchorLocal).sqrMagnitude < clearance * clearance)
                return false;

            if (!roomPlacement.IsValidLocalPosition(candidate, radius))
                return false;

            for (int i = 0; i < planets.Count; i++)
            {
                SilentPlanetBehaviour other = planets[i];
                if (other == null || !other.IsActive)
                    continue;

                float separation = Mathf.Max(minSeparation, (radius + other.Radius) * spacingMultiplier);
                if ((other.LocalPosition - candidate).sqrMagnitude < separation * separation)
                    return false;
            }

            return true;
        }

        private Vector3 GetAnchorLocal()
        {
            HubBehaviour hub = hubsSpawner.GetFirstActive();
            if (hub != null)
                return transform.InverseTransformPoint(hub.Position);

            Vector3 roomCenterWorld = roomPlacement.transform.TransformPoint(roomPlacement.RoomCenterLocalPosition);
            return transform.InverseTransformPoint(roomCenterWorld);
        }

        private SilentPlanetBehaviour AcquirePlanet() =>
            planets.AcquireInactive(prototype, transform, created =>
            {
                created.Initialize();
                created.Hide();
            });

        private void InitializePool() => planets.ForEachInPool(planet => planet.Initialize());

        private void HideAll()
        {
            planets.ForEachInPool(planet =>
            {
                if (planet.IsActive)
                    roomPlacement.Unregister(planet.LocalPosition, planet.Radius);
                planet.Hide();
            });

            hasSpawned = false;
        }

        private void ShowAll() => planets.ForEachInPool(planet =>
        {
            if (planet.IsActive)
                planet.SetVisible(true);
        });
    }
}
