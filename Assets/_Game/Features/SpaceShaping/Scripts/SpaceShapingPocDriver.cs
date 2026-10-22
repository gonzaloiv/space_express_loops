using DigitalLove.Global;
using UnityEngine;

namespace DigitalLove.Game.SpaceShaping
{
    public class SpaceShapingPocDriver : MonoBehaviour
    {
        [SerializeField] private Transform origin;
        [SerializeField] private Transform destination;
        [SerializeField] private Transform[] planets;
        [SerializeField] private LineRenderer pathLine;
        [SerializeField] private LaunchModel launchModel = LaunchModel.Coast;
        [SerializeField] private float launchSpeed = 1f;
        [SerializeField] private float thrustAcceleration = 1f;
        [SerializeField] private float sourceStrengthScale = 1f;
        [SerializeField] private float captureRadius = 0.6f;
        [SerializeField] private float stepSecs = 1f / 120f;
        [SerializeField] private int maxSteps = 1200;
        [SerializeField] private bool updateLive = true;
        [SerializeField] private LineRenderer[] gridLines;
        [SerializeField] private Vector3 gridMin = new(-6f, -4f, -3f);
        [SerializeField] private Vector3 gridMax = new(6f, 4f, 3f);
        [SerializeField] private int gridCount = 4;
        [SerializeField] private float gridDepthScale = 1.5f;
        [SerializeField] private float gridMaxDisplacement = 0.8f;
        [SerializeField] private float moveRange = 2.5f;
        [SerializeField] private float minSeparationScale = 1.2f;
        [SerializeField] private float minScale = 1.2f;
        [SerializeField] private float maxScale = 3.5f;

        private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
        private static readonly int ColorId = Shader.PropertyToID("_Color");

        private readonly SpacetimeField field = new();
        private readonly TrajectoryIntegrator integrator = new();
        private readonly ShipLaunch launch = new();

        private MaterialPropertyBlock propertyBlock;

        private Vector3 lastOrigin;
        private Vector3 lastDestination;
        private Vector3[] lastPlanetPositions;
        private float[] lastPlanetScales;
        private bool hasSnapshot;

        private Vector3[] homePositions;
        private Vector3[] homeScales;

        private void Start()
        {
            CaptureHomes();
            Simulate();
        }

        private void Update()
        {
            if (updateLive && HasMoved())
            {
                ConstrainSources();
                Simulate();
            }
        }

        private void CaptureHomes()
        {
            homePositions = new Vector3[planets.Length];
            homeScales = new Vector3[planets.Length];
            for (int i = 0; i < planets.Length; i++)
            {
                homePositions[i] = planets[i].position;
                homeScales[i] = planets[i].localScale;
            }
        }

        [Button]
        public void Simulate()
        {
            BuildField();
            TrajectorySettings settings = new()
            {
                Destination = destination.position,
                CaptureRadius = captureRadius,
                StepSecs = stepSecs,
                MaxSteps = maxSteps
            };

            launch.Model = launchModel;
            launch.LaunchSpeed = launchSpeed;
            launch.ThrustAcceleration = thrustAcceleration;
            TrajectoryResult result = launch.Simulate(field, origin.position, destination.position, settings, integrator);
            Draw(result);
            UpdateGrid();
            Snapshot();
        }

        private void BuildField()
        {
            field.Clear();
            for (int i = 0; i < planets.Length; i++)
            {
                Transform planet = planets[i];
                float radius = planet.localScale.x * 0.5f;
                field.Add(new SpaceSource(planet.position, radius, radius * sourceStrengthScale));
            }
        }

        private void Draw(TrajectoryResult result)
        {
            ApplyOutcomeColor(result.Outcome);
            pathLine.positionCount = result.Points.Count;
            for (int i = 0; i < result.Points.Count; i++)
                pathLine.SetPosition(i, result.Points[i]);
        }

        private void ApplyOutcomeColor(TrajectoryOutcome outcome)
        {
            propertyBlock ??= new MaterialPropertyBlock();
            pathLine.GetPropertyBlock(propertyBlock);
            Color color = ColorFor(outcome);
            propertyBlock.SetColor(BaseColorId, color);
            propertyBlock.SetColor(ColorId, color);
            pathLine.SetPropertyBlock(propertyBlock);
        }

        private static Color ColorFor(TrajectoryOutcome outcome)
        {
            switch (outcome)
            {
                case TrajectoryOutcome.Arrive:
                    return new Color(0.30f, 1f, 0.40f, 1f);
                case TrajectoryOutcome.Crash:
                    return new Color(1f, 0.28f, 0.24f, 1f);
                default:
                    return new Color(1f, 0.80f, 0.25f, 1f);
            }
        }

        private void UpdateGrid()
        {
            int n = Mathf.Max(2, gridCount);
            int perAxis = n * n;
            for (int index = 0; index < gridLines.Length; index++)
                DeformGridLine(gridLines[index], index / perAxis, index % perAxis, n);
        }

        private void DeformGridLine(LineRenderer line, int axis, int rem, int n)
        {
            int i = rem / n;
            int j = rem % n;
            line.positionCount = n;
            for (int s = 0; s < n; s++)
            {
                Vector3 node = LatticeGrid.Node(axis, s, i, j, n, gridMin, gridMax);
                line.SetPosition(s, LatticeGrid.Warp(node, field, gridDepthScale, gridMaxDisplacement));
            }
        }

        private void ConstrainSources()
        {
            for (int i = 0; i < planets.Length; i++)
            {
                Vector3 offset = planets[i].position - homePositions[i];
                if (offset.magnitude > moveRange)
                    planets[i].position = homePositions[i] + offset.normalized * moveRange;

                float scale = Mathf.Clamp(planets[i].localScale.x, minScale, maxScale);
                planets[i].localScale = new Vector3(scale, scale, scale);
            }

            ResolveOverlaps();
        }

        private void ResolveOverlaps()
        {
            for (int i = 0; i < planets.Length; i++)
            {
                for (int j = i + 1; j < planets.Length; j++)
                {
                    Vector3 delta = planets[j].position - planets[i].position;
                    float distance = delta.magnitude;
                    float minDistance = (planets[i].localScale.x + planets[j].localScale.x) * 0.5f * minSeparationScale;
                    if (distance >= minDistance || distance < 0.0001f)
                        continue;

                    Vector3 push = delta.normalized * ((minDistance - distance) * 0.5f);
                    planets[i].position -= push;
                    planets[j].position += push;
                }
            }
        }

        [Button]
        public void ResetSources()
        {
            for (int i = 0; i < planets.Length; i++)
            {
                planets[i].position = homePositions[i];
                planets[i].localScale = homeScales[i];
            }

            Simulate();
        }

        private bool HasMoved()
        {
            if (!hasSnapshot)
                return true;

            if (origin.position != lastOrigin || destination.position != lastDestination)
                return true;

            if (lastPlanetPositions == null || lastPlanetPositions.Length != planets.Length)
                return true;

            for (int i = 0; i < planets.Length; i++)
            {
                if (planets[i].position != lastPlanetPositions[i] || planets[i].localScale.x != lastPlanetScales[i])
                    return true;
            }

            return false;
        }

        private void Snapshot()
        {
            lastOrigin = origin.position;
            lastDestination = destination.position;
            if (lastPlanetPositions == null || lastPlanetPositions.Length != planets.Length)
            {
                lastPlanetPositions = new Vector3[planets.Length];
                lastPlanetScales = new float[planets.Length];
            }

            for (int i = 0; i < planets.Length; i++)
            {
                lastPlanetPositions[i] = planets[i].position;
                lastPlanetScales[i] = planets[i].localScale.x;
            }

            hasSnapshot = true;
        }
    }
}
