using NUnit.Framework;
using UnityEngine;

namespace DigitalLove.Game.SpaceShaping.Creator.Tests.EditMode
{
    public class PuzzleBriefTests
    {
        private SpaceShapingSettings settings;

        [SetUp]
        public void SetUp()
        {
            settings = ScriptableObject.CreateInstance<SpaceShapingSettings>();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(settings);
        }

        [Test]
        public void Brief_CarriesTheSeedConditions()
        {
            PuzzleSeed authored = ScriptableObject.CreateInstance<PuzzleSeed>();
            authored.translatable = true;
            authored.zones = 2;
            authored.resizable = true;

            PuzzleBrief mapped = PuzzleBrief.From(authored, settings);

            Assert.AreEqual(settings.endpointClearance, mapped.EndpointClearance, 1e-4f);
            Assert.IsTrue(mapped.Translatable);
            Assert.AreEqual(2, mapped.Zones);
            Assert.AreEqual(1, mapped.MinRoutes);
            Assert.IsTrue(mapped.Resizable);
            Object.DestroyImmediate(authored);
        }

        [Test]
        public void Brief_ResolvesMinimumRoutes()
        {
            Assert.AreEqual(1, MinRoutes(false, 1, 0));
            Assert.AreEqual(2, MinRoutes(true, 1, 0));
            Assert.AreEqual(1, MinRoutes(true, 2, 0));
            Assert.AreEqual(2, MinRoutes(true, 2, 2));
            Assert.AreEqual(1, MinRoutes(false, 2, 0));
        }

        private int MinRoutes(bool translatable, int zones, int minRoutes)
        {
            PuzzleSeed seed = ScriptableObject.CreateInstance<PuzzleSeed>();
            seed.translatable = translatable;
            seed.zones = zones;
            seed.minRoutes = minRoutes;
            int resolved = PuzzleBrief.From(seed, settings).MinRoutes;
            Object.DestroyImmediate(seed);
            return resolved;
        }
    }
}
