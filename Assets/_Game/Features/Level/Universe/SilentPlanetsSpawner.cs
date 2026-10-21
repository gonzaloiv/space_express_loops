using System.Collections.Generic;
using DigitalLove.Game.Nodes;
using DigitalLove.Global;
using UnityEngine;

namespace DigitalLove.Game.Levels
{
    public class SilentPlanetsSpawner : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private MrukRoomLocalPlacement roomPlacement;
        [SerializeField] private HubsSpawner hubsSpawner;
        [SerializeField] private UniverseView universeView;
        [SerializeField] private SilentPlanetBehaviour prototype;

        [Header("Budget")]
        [SerializeField, Min(0)] private int planetCount = 24;
        [SerializeField, Min(0)] private int planetCap = 40;
        [SerializeField, Min(1)] private int clusterCount = 3;
        [SerializeField, Min(0f)] private float clusterRadius = 1.2f;
        [SerializeField, Min(0f)] private float clusterRingRadius = 1f;
        [SerializeField, Min(0f)] private float clusterRingJitter = 0.5f;
        [SerializeField, Min(0f)] private float minSeparation = 0.25f;
        [SerializeField, Min(0f)] private float spacingMultiplier = 1.25f;
        [SerializeField, Min(0f)] private float anchorClearance = 0.6f;
        [SerializeField, Min(0f)] private float minRadius = 0.08f;
        [SerializeField, Min(0f)] private float maxRadius = 0.14f;
        [SerializeField, Min(0f)] private float verticalJitter = 0.35f;
        [SerializeField, Min(1)] private int placementAttempts = 32;

        [Header("Room view")]
        [SerializeField, Min(0f)] private float roomVisibleRadius = 3f;

        private readonly List<SilentPlanetBehaviour> planets = new();
        private bool hasSpawned;
        private bool isBubbleView;

        private void OnEnable() => SetSubscriptions(true);
        private void OnDisable() => SetSubscriptions(false);

        public void ResetPool()
        {
            HideAll();
            InitializePool();
        }

        public void SpawnIfNeeded(int seed)
        {
            SetBubbleView(universeView.IsMini);
            if (hasSpawned)
                return;

            hasSpawned = true;
            SeededRandom random = new(seed);
            Vector3 anchorLocal = GetAnchorLocal();
            int count = Mathf.Clamp(planetCount, 0, Mathf.Max(0, planetCap));
            Vector3[] centers = BuildClusterCenters(random, anchorLocal, count);
            for (int i = 0; i < count; i++)
                TrySpawnPlanet(random, centers[i % centers.Length], anchorLocal);
        }

        private void SetBubbleView(bool bubble)
        {
            isBubbleView = bubble;
            if (bubble)
                ShowAll();
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

        private void TrySpawnPlanet(SeededRandom random, Vector3 center, Vector3 anchorLocal)
        {
            for (int i = 0; i < placementAttempts; i++)
            {
                float radius = random.Range(minRadius, maxRadius);
                Vector3 candidate = center + random.InsideCircleXZ(clusterRadius);
                candidate.y = anchorLocal.y + random.Range(-verticalJitter, verticalJitter);
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
            Vector3 flat = candidate - anchorLocal;
            flat.y = 0f;
            float clearance = anchorClearance + radius;
            if (flat.sqrMagnitude < clearance * clearance)
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

        private Vector3[] BuildClusterCenters(SeededRandom random, Vector3 anchorLocal, int count)
        {
            if (count <= 0)
                return System.Array.Empty<Vector3>();

            int centerCount = Mathf.Clamp(clusterCount, 1, Mathf.Max(1, count));
            Vector3[] centers = new Vector3[centerCount];
            for (int i = 0; i < centerCount; i++)
            {
                float angle = (Mathf.PI * 2f * i / centerCount) + random.Range(-0.35f, 0.35f);
                float distance = clusterRingRadius + random.Range(0f, clusterRingJitter);
                centers[i] = anchorLocal + new Vector3(
                    Mathf.Cos(angle) * distance,
                    0f,
                    Mathf.Sin(angle) * distance);
            }

            return centers;
        }

        private Vector3 GetAnchorLocal()
        {
            HubBehaviour hub = hubsSpawner.GetFirstActive();
            if (hub != null)
                return transform.InverseTransformPoint(hub.Position);

            Vector3 roomCenterWorld = roomPlacement.transform.TransformPoint(roomPlacement.RoomCenterLocalPosition);
            return transform.InverseTransformPoint(roomCenterWorld);
        }

        private SilentPlanetBehaviour AcquirePlanet()
        {
            for (int i = 0; i < planets.Count; i++)
            {
                SilentPlanetBehaviour planet = planets[i];
                if (planet != null && !planet.IsActive)
                    return planet;
            }

            SilentPlanetBehaviour created = Instantiate(prototype, transform);
            created.Initialize();
            created.Hide();
            planets.Add(created);
            return created;
        }

        private void InitializePool()
        {
            for (int i = 0; i < planets.Count; i++)
            {
                if (planets[i] != null)
                    planets[i].Initialize();
            }
        }

        private void HideAll()
        {
            for (int i = 0; i < planets.Count; i++)
            {
                SilentPlanetBehaviour planet = planets[i];
                if (planet == null)
                    continue;

                if (planet.IsActive)
                    roomPlacement.Unregister(planet.LocalPosition, planet.Radius);
                planet.Hide();
            }

            hasSpawned = false;
        }

        private void ShowAll()
        {
            for (int i = 0; i < planets.Count; i++)
            {
                if (planets[i] != null && planets[i].IsActive)
                    planets[i].SetVisible(true);
            }
        }

        private void SetSubscriptions(bool subscribe)
        {
            if (subscribe)
            {
                universeView.TransitionStarted += OnTransitionStarted;
                universeView.TransitionCompleted += OnTransitionCompleted;
                SetBubbleView(universeView.IsMini);
                return;
            }

            universeView.TransitionStarted -= OnTransitionStarted;
            universeView.TransitionCompleted -= OnTransitionCompleted;
        }

        private void OnTransitionStarted(bool toMini)
        {
            if (toMini)
                SetBubbleView(true);
        }

        private void OnTransitionCompleted(bool toMini) => SetBubbleView(toMini);
    }
}
