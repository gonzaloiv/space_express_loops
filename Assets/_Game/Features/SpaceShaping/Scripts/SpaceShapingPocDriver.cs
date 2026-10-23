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
        [SerializeField] private Transform container;
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
        private Vector3[] positions;
        private float[] scales;
        private SourceLimits limits;

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
            positions = new Vector3[planets.Length];
            scales = new float[planets.Length];
            limits = new SourceLimits
            {
                MoveRange = moveRange,
                MinScale = minScale,
                MaxScale = maxScale,
                SeparationScale = minSeparationScale
            };
            for (int i = 0; i < planets.Length; i++)
            {
                homePositions[i] = Local(planets[i]);
                homeScales[i] = planets[i].localScale;
            }
        }

        [Button]
        public void Simulate()
        {
            BuildField();
            Vector3 originLocal = Local(origin);
            Vector3 destinationLocal = Local(destination);
            TrajectorySettings settings = new()
            {
                Destination = destinationLocal,
                CaptureRadius = captureRadius,
                StepSecs = stepSecs,
                MaxSteps = maxSteps
            };

            launch.Model = launchModel;
            launch.LaunchSpeed = launchSpeed;
            launch.ThrustAcceleration = thrustAcceleration;
            TrajectoryResult result = launch.Simulate(field, originLocal, destinationLocal, settings, integrator);
            Draw(result);
            UpdateGrid();
            Snapshot();
        }

        private Vector3 Local(Transform source) => container.InverseTransformPoint(source.position);

        private void BuildField()
        {
            field.Clear();
            for (int i = 0; i < planets.Length; i++)
            {
                Transform planet = planets[i];
                float radius = planet.localScale.x * 0.5f;
                field.Add(new SpaceSource(Local(planet), radius, radius * sourceStrengthScale));
            }
        }

        private void Draw(TrajectoryResult result)
        {
            ApplyOutcomeColor(result.Outcome);
            bool arrived = result.Outcome == TrajectoryOutcome.Arrive;
            int count = result.Points.Count;
            pathLine.positionCount = arrived ? count + 1 : count;
            for (int i = 0; i < count; i++)
                pathLine.SetPosition(i, result.Points[i]);
            if (arrived)
                pathLine.SetPosition(count, Local(destination));
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
            if (outcome == TrajectoryOutcome.Arrive)
                return new Color(0.30f, 1f, 0.40f, 1f);
            if (outcome == TrajectoryOutcome.Crash)
                return new Color(1f, 0.28f, 0.24f, 1f);
            return new Color(1f, 0.80f, 0.25f, 1f);
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
                positions[i] = Local(planets[i]);
                scales[i] = planets[i].localScale.x;
            }

            SourceConstraints.Apply(positions, scales, homePositions, limits);
            ApplyConstrained();
        }

        private void ApplyConstrained()
        {
            for (int i = 0; i < planets.Length; i++)
            {
                if ((positions[i] - Local(planets[i])).sqrMagnitude > 0.00000001f)
                    planets[i].position = container.TransformPoint(positions[i]);

                if (!Mathf.Approximately(scales[i], planets[i].localScale.x))
                    planets[i].localScale = new Vector3(scales[i], scales[i], scales[i]);
            }
        }

        [Button]
        public void ResetSources()
        {
            for (int i = 0; i < planets.Length; i++)
            {
                planets[i].position = container.TransformPoint(homePositions[i]);
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
