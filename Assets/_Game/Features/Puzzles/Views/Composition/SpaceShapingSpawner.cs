using DigitalLove.Game.Nodes;
using DigitalLove.Game.SpaceShaping.Cube;
using DigitalLove.Global;
using UnityEngine;

namespace DigitalLove.Game.SpaceShaping
{
    // Composition root: owns the authored prototypes and builds the planet, zone, line, and ship
    // collaborators that SpaceShapingView drives. The view never instantiates a prototype itself.
    public class SpaceShapingSpawner : MonoBehaviour
    {
        [SerializeField] private FieldPlanetBehaviour prototype;
        [SerializeField] private FieldPlanetBehaviour sunPrototype;
        [SerializeField] private FieldPlanetBehaviour originPrototype;
        [SerializeField] private FieldPlanetBehaviour destinationPrototype;
        [SerializeField] private Transform sourceParent;
        [SerializeField] private TrajectoryLineView trajectoryPrototype;
        [SerializeField] private ShipTravelView shipPrototype;
        [SerializeField] private float shipStaggerSecs = 0.08f;
        [SerializeField] private Transform shipsParent;
        [SerializeField] private Transform linesParent;
        [SerializeField] private CubeView cubePrototype;
        [SerializeField] private Transform cubeParent;

        [Header("Endpoints")]
        [SerializeField] private ColorValue originColor;
        [SerializeField] private ColorValue destinationColor;

        private FieldPlanetSet planets;
        private ZoneCubePool zones;
        private TrajectoryLinePool lines;
        private TrajectoryLinePool baselines;
        private ShipFlightPool ships;
        private bool initialized;

        public FieldPlanetSet Planets
        {
            get { Initialize(); return planets; }
        }

        public ZoneCubePool Zones
        {
            get { Initialize(); return zones; }
        }

        public TrajectoryLinePool Lines
        {
            get { Initialize(); return lines; }
        }

        public TrajectoryLinePool Baselines
        {
            get { Initialize(); return baselines; }
        }

        public ShipFlightPool Ships
        {
            get { Initialize(); return ships; }
        }

        private void Awake() => Initialize();

        private void Initialize()
        {
            if (initialized)
                return;

            initialized = true;
            FieldPlanetSet.Prototypes planetPrototypes = new(
                prototype, sunPrototype, originPrototype, destinationPrototype);
            FieldPlanetSet.RouteColors routeColors = new(originColor, destinationColor);
            planets = new FieldPlanetSet(planetPrototypes, routeColors);
            planets.Initialize(sourceParent);

            zones = new ZoneCubePool(cubePrototype, cubeParent);
            lines = new TrajectoryLinePool(trajectoryPrototype, linesParent);
            baselines = new TrajectoryLinePool(trajectoryPrototype, linesParent);
            ships = new ShipFlightPool(shipPrototype, shipsParent, shipStaggerSecs);
        }
    }
}
