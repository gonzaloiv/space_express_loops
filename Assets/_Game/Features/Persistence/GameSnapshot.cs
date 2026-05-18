using System;
using System.Collections.Generic;
using System.Linq;
using DigitalLove.Game.Planets;
using DigitalLove.Game.Spaceships;
using Newtonsoft.Json;
using UnityEngine;

namespace DigitalLove.Game.Persistence
{
    [Serializable]
    public class GameSnapshot
    {
        public const int BaseLettersPerRoundCompletion = 15;

        [JsonIgnore] public static string CookieKey => typeof(GameSnapshot).Name;

        public int roundIndex;
        public List<PlanetData> planets;
        public List<HubData> hubs;
        public List<LoopData> loops;
        public Store store;
        public int lettersRequiredForRoundCompletion;

        [JsonIgnore] private Action onUpdated;

        [JsonIgnore] public int CurrentLetters => store.letters;
        [JsonIgnore] public bool HasPlanets => planets != null && planets.Count > 0;
        [JsonIgnore] public bool HasHubs => hubs != null && hubs.Count > 0;
        [JsonIgnore] public bool HasLoops => loops != null && loops.Count > 0;
        [JsonIgnore] public bool IsCurrentRoundLetterGoalMet => store != null && store.letters >= lettersRequiredForRoundCompletion;

        public GameSnapshot()
        {
            roundIndex = 0;
            planets = new();
            hubs = new();
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

        /// <summary>
        /// Sets <see cref="lettersRequiredForRoundCompletion"/> from current planet and loop counts.
        /// Call only when a round begins (first spawn after <c>GameStartState</c>, or after <c>SpawnRound</c> in new-round flow) so the target stays fixed until the next round.
        /// </summary>
        public void RecalculateLettersRequiredForRound(float lettersIncreaseMultiplier)
        {
            int planetCount = planets?.Count ?? 0;
            int routeCount = loops?.Count ?? 0;
            float raw = BaseLettersPerRoundCompletion * planetCount * routeCount * lettersIncreaseMultiplier;
            lettersRequiredForRoundCompletion = Mathf.Max(1, Mathf.RoundToInt(raw));
            onUpdated?.Invoke();
        }

        // ? Updates

        public void IncreaseRoundIndex()
        {
            roundIndex++;
            onUpdated?.Invoke();
        }

        public void ResetLettersForNewRound()
        {
            store ??= new Store();
            store.ResetLetters();
            onUpdated?.Invoke();
        }

        public void AddPlanets(List<PlanetData> toAdd)
        {
            planets.AddRange(toAdd);
            onUpdated?.Invoke();
        }

        public void AddHub(HubData toAdd)
        {
            hubs ??= new();
            HubData existing = hubs.FirstOrDefault(h => string.Equals(h.id, toAdd.id));
            if (existing != null)
                hubs.Remove(existing);
            hubs.Add(toAdd);
            onUpdated?.Invoke();
        }

        public HubData GetHubById(string hubId)
        {
            if (hubs == null || string.IsNullOrEmpty(hubId))
                return null;
            return hubs.FirstOrDefault(h => string.Equals(h.id, hubId));
        }

        public void AddLoop(LoopData toAdd)
        {
            LoopData toRemove = loops.FirstOrDefault(l => string.Equals(l.spaceshipId, toAdd.spaceshipId));
            if (toRemove != null)
                loops.Remove(toRemove);
            loops.Add(toAdd);
            onUpdated?.Invoke();
        }

        public void ClearLoopDestinations(string spaceshipId)
        {
            LoopData loop = loops.FirstOrDefault(l => string.Equals(l.spaceshipId, spaceshipId));
            if (loop == null)
                return;

            loop.destinationIds?.Clear();
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