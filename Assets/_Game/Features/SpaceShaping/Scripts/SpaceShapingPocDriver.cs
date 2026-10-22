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
