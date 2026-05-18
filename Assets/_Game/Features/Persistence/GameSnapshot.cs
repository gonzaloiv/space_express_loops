using System;
using System.Collections.Generic;
using System.Linq;
using DigitalLove.Game.Planets;
using DigitalLove.Game.Spaceships;
using Newtonsoft.Json;
using DigitalLove.Global;

namespace DigitalLove.Game.Persistence
{
    [Serializable]
    public class GameSnapshot
    {
        [JsonIgnore] public static string CookieKey => typeof(GameSnapshot).Name;

        public int roundIndex;
        public List<PlanetData> planets;
        public List<LoopData> loops;
        public Store store;

        [JsonIgnore] private Action onUpdated;

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

        public void SetPlanets(List<PlanetData> toSet)
        {
            planets = toSet;
        }

        public void SetOnUpdated(Action onUpdated)
        {
            this.onUpdated = onUpdated;
        }

        // ? Updates

        public void IncreaseRoundIndex()
        {
            roundIndex++;
            onUpdated?.Invoke();
        }

        public void AddPlanets(List<PlanetData> toAdd)
        {
            planets.AddRange(toAdd);
            onUpdated?.Invoke();
        }

        public void AddLoop(LoopData toAdd)
        {
            LoopData toRemove = loops.FirstOrDefault(l => string.Equals(l.spaceshipId, toAdd.spaceshipId));
            if (toRemove != null)
                loops.Remove(toRemove);
            loops.Add(toAdd);
            onUpdated?.Invoke();
        }

        public void ClearLoopDestination(string spaceshipId, int cost)
        {
            LoopData loop = loops.FirstOrDefault(l => string.Equals(l.spaceshipId, spaceshipId));
            if (loop == null)
                return;

            loop.destinationId = null;
            store.SpendMoney(cost);
            onUpdated?.Invoke();
        }

        public void IncreaseLettersAndMoney(int lettersValue, int moneyValue)
        {
            store.IncreaseLettersAndMoney(lettersValue, moneyValue);
            onUpdated?.Invoke();
        }

        public void SpendMoney(int value)
        {
            store.SpendMoney(value);
            onUpdated?.Invoke();
        }

        public void RemoveLoopBySpaceshipId(string spaceshipId, int cost)
        {
            LoopData toRemove = loops.FirstOrDefault(l => string.Equals(l.spaceshipId, spaceshipId));
            if (toRemove != null)
                loops.Remove(toRemove);
            store.SpendMoney(cost);
            onUpdated?.Invoke();
        }

    }
}