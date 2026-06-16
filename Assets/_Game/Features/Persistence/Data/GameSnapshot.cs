using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using DigitalLove.Game.Nodes;
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
        [JsonIgnore] public bool HasAnyLoopWithDestinations => loops != null && loops.Exists(l => l.HasDestinations);
        [JsonIgnore] public bool IsCurrentRoundLetterGoalMet => store != null && store.letters >= lettersRequiredForRoundCompletion;

        public GameSnapshot()
        {
            roundIndex = 0;
            EnsureInitialized();
        }

        [OnDeserialized]
        internal void OnDeserialized(StreamingContext context) => EnsureInitialized();

        private void EnsureInitialized()
        {
            planets ??= new();
            hubs ??= new();
            loops ??= new();
            store ??= new();

            foreach (LoopData loop in loops)
                loop.destinationIds ??= new();
        }

        public static GameSnapshot FromCookieMetadata(string metadata) =>
            string.IsNullOrEmpty(metadata)
                ? new GameSnapshot()
                : JsonConvert.DeserializeObject<GameSnapshot>(metadata) ?? new GameSnapshot();

        public void EnsureLettersRequiredForRound(float lettersIncreaseMultiplier)
        {
            if (lettersRequiredForRoundCompletion <= 0)
                RecalculateLettersRequiredForRound(lettersIncreaseMultiplier);
        }

        public void SetOnUpdated(Action onUpdated) => this.onUpdated = onUpdated;

        private void NotifyUpdated() => onUpdated?.Invoke();

        /// <summary>
        /// Sets <see cref="lettersRequiredForRoundCompletion"/> from current planet and loop counts.
        /// Call only when a round begins (first spawn after <c>GameStartState</c>, or after <c>SpawnRound</c> in new-round flow) so the target stays fixed until the next round.
        /// </summary>
        public void RecalculateLettersRequiredForRound(float lettersIncreaseMultiplier)
        {
            float raw = BaseLettersPerRoundCompletion * planets.Count * loops.Count * lettersIncreaseMultiplier;
            lettersRequiredForRoundCompletion = Mathf.Max(1, Mathf.RoundToInt(raw));
            NotifyUpdated();
        }

        public void IncreaseRoundIndex()
        {
            roundIndex++;
            NotifyUpdated();
        }

        public void ResetLettersForNewRound()
        {
            store.ResetLetters();
            NotifyUpdated();
        }

        public void AddPlanets(List<PlanetData> toAdd)
        {
            planets.AddRange(toAdd);
            NotifyUpdated();
        }

        public void AddHub(HubData toAdd)
        {
            HubData existing = hubs.FirstOrDefault(h => string.Equals(h.id, toAdd.id));
            if (existing != null)
                hubs.Remove(existing);
            hubs.Add(toAdd);
            NotifyUpdated();
        }

        public HubData GetHubById(string hubId) =>
            string.IsNullOrEmpty(hubId) ? null : hubs.FirstOrDefault(h => string.Equals(h.id, hubId));

        public void SaveLoop(LoopData loopData)
        {
            LoopData existing = loops.FirstOrDefault(l => string.Equals(l.spaceshipId, loopData.spaceshipId));
            if (existing != null)
                existing.CopyFrom(loopData);
            else
                loops.Add(LoopData.From(loopData));

            NotifyUpdated();
        }

        public void IncreaseLettersAndMoney(int lettersValue, int moneyValue)
        {
            store.IncreaseLettersAndMoney(lettersValue, moneyValue);
            NotifyUpdated();
        }

        public void SpendMoney(int value)
        {
            store.SpendMoney(value);
            NotifyUpdated();
        }

        public void RemoveLoopBySpaceshipId(string spaceshipId, int cost)
        {
            LoopData toRemove = loops.FirstOrDefault(l => string.Equals(l.spaceshipId, spaceshipId));
            if (toRemove != null)
                loops.Remove(toRemove);
            store.SpendMoney(cost);
            NotifyUpdated();
        }
    }
}