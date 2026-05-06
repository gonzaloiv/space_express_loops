using DigitalLove.Game.Planets;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using DigitalLove.Global;
using UnityEngine.Assertions;

namespace DigitalLove.Game.Spaceships
{
    public class SpaceshipsSpawner : MonoBehaviour
    {
        [SerializeField] private List<SpaceshipBehaviour> spaceships;
        [SerializeField] private ColorIsAvailablePair[] colors;

        private IdCounter idCounter = new();

        public List<SpaceshipBehaviour> All => spaceships;

        public Action<LoopEventArgs> loopCreated = args => { };
        public Action<LoopEventArgs> loopEditionButtonClicked = args => { };
        public Action<LoopCompleteEventArgs> loopComplete = args => { };

        public void SpawnNew(PlanetBaseBehaviour basePlanet)
        {
            SpawnSpaceship(idCounter.NextId, basePlanet);
        }

        private SpaceshipBehaviour SpawnSpaceship(string id, PlanetBaseBehaviour basePlanet)
        {
            SpaceshipBehaviour spaceship = GetOrInstantiate();
            SpaceshipData data = new()
            {
                id = id,
                color = GetRandomAvailableColor()
            };
            spaceship.Spawn(data, basePlanet);
            spaceship.SetOnLoopCreated(OnLoopCreated);
            spaceship.SetOnLoopComplete(OnLoopComplete);
            spaceship.SetOnLoopEditionButtonClicked(OnLoopEditionButtonClicked);
            return spaceship;
        }

        private SpaceshipBehaviour GetOrInstantiate()
        {
            SpaceshipBehaviour spaceship = spaceships.FirstOrDefault(s => !s.IsActive);
            if (spaceship == null)
            {
                spaceship = Instantiate(spaceships[0], transform);
                spaceship.Hide();
                spaceships.Add(spaceship);
            }
            return spaceship;
        }

        private Color GetRandomAvailableColor()
        {
            ColorIsAvailablePair[] availableColors = colors.Where(c => !c.isTaken).ToArray();
            Assert.AreNotEqual(availableColors.Length, 0);
            ColorIsAvailablePair selectedColor = availableColors[UnityEngine.Random.Range(0, availableColors.Length)];
            selectedColor.isTaken = true;
            return selectedColor.color.value;
        }

        private void OnLoopCreated(LoopEventArgs args)
        {
            loopCreated(args);
        }

        private void OnLoopComplete(LoopCompleteEventArgs args)
        {
            loopComplete(args);
        }

        private void OnLoopEditionButtonClicked(LoopEventArgs args)
        {
            loopEditionButtonClicked(args);
        }

        public void SpawnFromLoop(string id, PlanetBaseBehaviour basePlanet, PlanetBehaviour destinationPlanet)
        {
            SpaceshipBehaviour spaceship = SpawnSpaceship(id, basePlanet);
            spaceship.SetRoute(destinationPlanet);
        }

        public void HideAll()
        {
            foreach (SpaceshipBehaviour spaceship in spaceships)
            {
                spaceship.Hide();
            }
        }

        // ! DEBUG

        public SpaceshipBehaviour GetRandom()
        {
            return spaceships[UnityEngine.Random.Range(0, spaceships.Count)];
        }

        public List<SpaceshipBehaviour> GetAll() => spaceships.ToList();
    }

    [Serializable]
    public class ColorIsAvailablePair
    {
        public ColorValue color;
        public bool isTaken;
    }
}