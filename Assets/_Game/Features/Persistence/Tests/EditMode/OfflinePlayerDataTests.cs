using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DigitalLove.Casual.Levels;
using DigitalLove.DataAccess;
using NUnit.Framework;

namespace DigitalLove.Game.Persistence.Tests
{
    public class OfflinePlayerDataTests
    {
        private class FakeDataProvider : IDataProvider
        {
            private readonly Dictionary<Type, object> values = new();

            public int PutCount { get; private set; }

            public Task<T> Get<T>() =>
                Task.FromResult(values.TryGetValue(typeof(T), out object value) ? (T)value : default);

            public Task Put<T>(T value)
            {
                values[typeof(T)] = value;
                PutCount++;
                return Task.CompletedTask;
            }

            public T GetValue<T>() => (T)values[typeof(T)];
        }

        [Test]
        public void AddSessionSnapshot_IncrementsAndKeepsUserId()
        {
            PlayerData playerData = new() { userId = "player" };

            playerData.AddSessionSnapshot("fallback");

            Assert.AreEqual("player", playerData.userId);
            Assert.AreEqual(1, playerData.sessionCount);
        }

        [Test]
        public void CompletedJobsCount_CountsOnlyLevelCompleteCookies()
        {
            PlayerData playerData = new();
            playerData.AddCookie(new LevelCompleteCookie("01_01"));
            playerData.AddCookie(new Cookie("gesture_learned_scale"));

            Assert.AreEqual(1, playerData.CompletedJobsCount);
        }

        [Test]
        public void Reconcile_CloudAhead_HoldsItAndDoesNotMirror()
        {
            FakeDataProvider local = new();
            FakeDataProvider cloud = new();
            PlayerData localData = new();
            PlayerData cloudData = new();
            cloudData.AddCookie(new LevelCompleteCookie("01_01"));
            cloudData.AddCookie(new LevelCompleteCookie("01_02"));
            local.Put(localData);
            cloud.Put(cloudData);
            int cloudPuts = cloud.PutCount;

            PlayerDataProvider provider = new(local, cloud);
            provider.ReconcileWithCloudAsync().GetAwaiter().GetResult();
            provider.Put(localData).GetAwaiter().GetResult();

            Assert.AreEqual(cloudPuts, cloud.PutCount);
            PlayerData pending = provider.ApplyPendingCloud();
            Assert.AreEqual(2, pending.CompletedJobsCount);
            Assert.AreEqual(2, local.GetValue<PlayerData>().CompletedJobsCount);
        }

        [Test]
        public void Reconcile_LocalNotBehind_PushesLocalUp()
        {
            FakeDataProvider local = new();
            FakeDataProvider cloud = new();
            PlayerData localData = new();
            localData.AddCookie(new LevelCompleteCookie("01_01"));
            PlayerData cloudData = new();
            cloudData.AddCookie(new LevelCompleteCookie("01_02"));
            local.Put(localData);
            cloud.Put(cloudData);
            int cloudPuts = cloud.PutCount;

            PlayerDataProvider provider = new(local, cloud);
            provider.ReconcileWithCloudAsync().GetAwaiter().GetResult();

            Assert.AreEqual(cloudPuts + 1, cloud.PutCount);
            Assert.AreEqual(1, cloud.GetValue<PlayerData>().CompletedJobsCount);
        }
    }
}
