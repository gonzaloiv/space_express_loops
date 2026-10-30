using System;
using System.Collections.Generic;
using DigitalLove.Game.Nodes;
using DigitalLove.Game.SpaceShaping.Cube;
using UnityEngine;

namespace DigitalLove.Game.SpaceShaping
{
    // Owns the authored prototypes and the pooled views that SpaceShapingView presents. The view
    // drives these instances; it never instantiates a prototype itself.
    public class SpaceShapingSpawner : MonoBehaviour
    {
        [SerializeField] private FieldPlanetBehaviour prototype;
        [SerializeField] private FieldPlanetBehaviour originPrototype;
        [SerializeField] private FieldPlanetBehaviour destinationPrototype;
        [SerializeField] private Transform sourceParent;
        [SerializeField] private TrajectoryLineView trajectoryPrototype;
        [SerializeField] private ShipTravelView shipPrototype;
        [SerializeField] private Transform shipsParent;
        [SerializeField] private Transform linesParent;
        [SerializeField] private SpaceZoneView cubePrototype;
        [SerializeField] private Transform cubeParent;

        private readonly List<FieldPlanetBehaviour> pool = new();
        private readonly List<SpaceZoneView> cubes = new();
        private TrajectoryLinePool lines;
        private ShipFlightPool ships;
        private FieldPlanetBehaviour originPlanet;
        private FieldPlanetBehaviour destinationPlanet;

        public FieldPlanetBehaviour Origin => originPlanet;
        public FieldPlanetBehaviour Destination => destinationPlanet;
        public TrajectoryLinePool Lines => lines;
        public ShipFlightPool Ships => ships;
        public IReadOnlyList<SpaceZoneView> Cubes => cubes;

        public event Action<ZoneRefusal> CubeRefused;
        public event Action ShipsCompleted;

        private void Awake()
        {
            originPlanet = Instantiate(originPrototype, sourceParent);
            originPlanet.Initialize();
            destinationPlanet = Instantiate(destinationPrototype, sourceParent);
            destinationPlanet.Initialize();
            lines = new TrajectoryLinePool(trajectoryPrototype, linesParent);
            ships = new ShipFlightPool(shipPrototype, shipsParent);
            ships.Completed += OnShipsCompleted;
        }

        public void SpawnPlanets(SpaceShapingSession session)
        {
            int count = session.SourceCount;
            int origin = session.IndexOf(session.Origin);
            int destination = session.IndexOf(session.Destination);
            int slot = 0;
            for (int i = 0; i < count; i++)
            {
                if (i == origin || i == destination)
                    continue;

                Source(slot++).Spawn(session.GetPosition(i), session.GetScale(i), i);
            }

            for (int i = slot; i < pool.Count; i++)
                pool[i].Hide();

            Spawn(originPlanet, session, origin);
            Spawn(destinationPlanet, session, destination);
        }

        public SpaceZoneView Cube(int index)
        {
            while (index >= cubes.Count)
                cubes.Add(Create());

            return cubes[index];
        }

        public void HideCubesFrom(int index)
        {
            for (int i = index; i < cubes.Count; i++)
                cubes[i].gameObject.SetActive(false);
        }

        private static void Spawn(FieldPlanetBehaviour planet, SpaceShapingSession session, int index)
        {
            if (index >= 0)
                planet.Spawn(session.GetPosition(index), session.GetScale(index), index);
            else
                planet.Hide();
        }

        private FieldPlanetBehaviour Source(int index)
        {
            if (pool.Count == 0)
                pool.AcquireInactive(prototype, sourceParent, created => created.Initialize());

            return pool.EnsureSlot(index, sourceParent, created => created.Initialize());
        }

        private SpaceZoneView Create()
        {
            SpaceZoneView cube = Instantiate(cubePrototype, cubeParent);
            cube.Refused += OnCubeRefused;
            return cube;
        }

        private void OnCubeRefused(ZoneRefusal reason) => CubeRefused?.Invoke(reason);

        private void OnShipsCompleted() => ShipsCompleted?.Invoke();
    }
}
