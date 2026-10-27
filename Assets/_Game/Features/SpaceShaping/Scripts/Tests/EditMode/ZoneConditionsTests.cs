using NUnit.Framework;
using UnityEngine;

namespace DigitalLove.Game.SpaceShaping.Tests.EditMode
{
    public class ZoneConditionsTests
    {
        [Test]
        public void From_CarriesThePuzzleFlags()
        {
            PuzzleData puzzle = ScriptableObject.CreateInstance<PuzzleData>();
            puzzle.translatable = true;
            puzzle.resizable = false;

            ZoneConditions conditions = ZoneConditions.From(puzzle);

            Assert.IsTrue(conditions.Translatable);
            Assert.IsFalse(conditions.Resizable);
            Object.DestroyImmediate(puzzle);
        }

        [Test]
        public void ZoneView_ExposesTheLockedGestures()
        {
            GameObject zoneObject = new("Zone");
            SpaceZoneView zone = zoneObject.AddComponent<SpaceZoneView>();
            zone.SetConditions(new ZoneConditions(false, true));

            Assert.IsFalse(zone.CanMove);
            Assert.IsTrue(zone.CanResize);
            Object.DestroyImmediate(zoneObject);
        }
    }
}
