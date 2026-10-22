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
        [SerializeField] private Vector3 gridMin = new(-7f, -5f, -4f);
        [SerializeField] private Vector3 gridMax = new(7f, 5f, 4f);
        [SerializeField] private int gridCount = 4;
        [SerializeField] private float gridDepthScale = 3.5f;
        [SerializeField] private float gridMaxDisplacement = 3f;

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

        private void Start() => Simulate();

        private void Update()
        {
            if (updateLive && HasMoved())
                Simulate();
        }

        [ContextMenu("Simulate")]
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
                Vector3 node = GridNode(axis, s, i, j, n);
                Vector3 pull = field.Pull(node) * gridDepthScale;
                if (pull.sqrMagnitude > gridMaxDisplacement * gridMaxDisplacement)
                    pull = pull.normalized * gridMaxDisplacement;
                line.SetPosition(s, node + pull);
            }
        }

        private Vector3 GridNode(int axis, int s, int i, int j, int n)
        {
            float a = T(s, n);
            float b = T(i, n);
            float c = T(j, n);
            if (axis == 0)
                return new Vector3(X(a), Y(b), Z(c));
            if (axis == 1)
                return new Vector3(X(b), Y(a), Z(c));
            return new Vector3(X(b), Y(c), Z(a));
        }

        private static float T(int k, int n) => n <= 1 ? 0.5f : (float)k / (n - 1);

        private float X(float t) => Mathf.Lerp(gridMin.x, gridMax.x, t);

        private float Y(float t) => Mathf.Lerp(gridMin.y, gridMax.y, t);

        private float Z(float t) => Mathf.Lerp(gridMin.z, gridMax.z, t);

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
