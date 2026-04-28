using System;
using System.Collections.Generic;
using System.Linq;
using DigitalLove.Game.Planets;
using DigitalLove.Game.Spaceships;
using DigitalLove.Levels;
using Newtonsoft.Json;

namespace DigitalLove.Game.Levels
{
    [Serializable]
    public class GameSnapshot
    {
        public static string CookieKey => typeof(GameSnapshot).Name;

        public List<PlanetData> planets;
        public List<LoopData> loops;
        public Store store;

        private Action onUpdated;

        [JsonIgnore] public int CurrentLetters => store.letters;
        [JsonIgnore] public bool HasPlanets => planets != null && planets.Count > 0;
        [JsonIgnore] public bool HasLoops => loops != null && loops.Count > 0;

        public GameSnapshot()
        {
            planets = new();
            loops = new();
            store = new();
        }

        public void SetPlanets(List<PlanetData> planets)
        {
            this.planets = planets;
        }

        public void AddLoop(LoopData loopData)
        {
            if (loops.Count > 0)
            {
                LoopData toRemove = loops.FirstOrDefault(l => string.Equals(l.spaceshipId, loopData.spaceshipId));
                if (toRemove != null)
                    loops.Remove(toRemove);
            }
            loops.Add(loopData);
            onUpdated?.Invoke();
        }

        public void IncreaseLetters(int value)
        {
            store.IncreaseLetters(value);
            onUpdated?.Invoke();
        }

        public void SetOnUpdated(Action onUpdated)
        {
            this.onUpdated = onUpdated;
        }
    }
}