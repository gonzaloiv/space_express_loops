using NUnit.Framework;
using UnityEngine;

namespace DigitalLove.Game.SpaceShaping.Tests.EditMode
{
    public class PuzzleContractKeysTests
    {
        [Test]
        public void Keys_DeriveFromTheLevelId()
        {
            PuzzleData puzzle = ScriptableObject.CreateInstance<PuzzleData>();
            puzzle.id = "01_02";

            Assert.AreEqual("level_01_02_init_client", puzzle.ClientKey);
            Assert.AreEqual("level_01_02_init_cargo", puzzle.CargoKey);
            Assert.AreEqual("level_01_02_init_manual_entry", puzzle.ManualKey);

            Object.DestroyImmediate(puzzle);
        }
    }
}
