using System;
using System.Collections.Generic;
using System.Linq;
using DigitalLove.Game.Planets;
using DigitalLove.Game.Spaceships;
using Newtonsoft.Json;

namespace DigitalLove.Game.Persistence
{
    [Serializable]
    public class GameSnapshot
    {
        public static string CookieKey => typeof(GameSnapshot).Name;

        public int roundIndex;
        public List<PlanetData> planets;
        public List<LoopData> loops;
        public Store store;

        private Action onUpdated;

        [JsonIgnore] public int CurrentLetters => store.letters;
        [JsonIgnore] public bool HasPlanets => planets != null && planets.Count > 0;
        [JsonIgnore] public bool HasLoops => loops != null && loops.Count > 0;

        public GameSnapshot()
        {
            roundIndex = 0;
            planets = new();
            loops = new();
            store = new();
        }

        public void IncreaseRoundIndex()
        {
            roundIndex++;
            onUpdated?.Invoke();
        }

        public void SetPlanets(List<PlanetData> toSet)
        {
            planets = toSet;
        }

        public void AddPlanets(List<PlanetData> toAdd)
        {
            planets.AddRange(toAdd);
            onUpdated?.Invoke();
        }

        public void AddLoop(LoopData toAdd)
        {
            if (loops.Count > 0)
            {
                LoopData toRemove = loops.FirstOrDefault(l => string.Equals(l.spaceshipId, toAdd.spaceshipId));
                if (toRemove != null)
                    loops.Remove(toRemove);
            }
            loops.Add(toAdd);
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

        public void RemoveLoopBySpaceshipId(string spaceshipId)
        {
            LoopData toRemove = loops.FirstOrDefault(l => string.Equals(l.spaceshipId, spaceshipId));
            if (toRemove != null)
                loops.Remove(toRemove);
            onUpdated?.Invoke();
        }
    }
}