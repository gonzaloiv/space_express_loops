using NUnit.Framework;
using UnityEngine;

namespace DigitalLove.Game.SpaceShaping.Creator.Tests.EditMode
{
    public class PuzzleShipRelationTests
    {
        private static readonly Vector3 Origin = Vector3.zero;
        private static readonly Vector3 OtherOrigin = Vector3.up;
        private static readonly Vector3 Destination = Vector3.right;
        private static readonly Vector3 OtherDestination = Vector3.forward;

        [Test]
        public void FanOut_SharesAnOriginWithDifferentDestinations()
        {
            Assert.IsTrue(PuzzleMultiShipBake.Allowed(
                PuzzleShipRelation.FanOut, Origin, Destination, Origin, OtherDestination));
            Assert.IsFalse(PuzzleMultiShipBake.Allowed(
                PuzzleShipRelation.FanOut, Origin, Destination, Origin, Destination));
            Assert.IsFalse(PuzzleMultiShipBake.Allowed(
                PuzzleShipRelation.FanOut, Origin, Destination, OtherOrigin, OtherDestination));
        }

        [Test]
        public void Converge_SharesADestinationWithDifferentOrigins()
        {
            Assert.IsTrue(PuzzleMultiShipBake.Allowed(
                PuzzleShipRelation.Converge, Origin, Destination, OtherOrigin, Destination));
            Assert.IsFalse(PuzzleMultiShipBake.Allowed(
                PuzzleShipRelation.Converge, Origin, Destination, Origin, Destination));
            Assert.IsFalse(PuzzleMultiShipBake.Allowed(
                PuzzleShipRelation.Converge, Origin, Destination, OtherOrigin, OtherDestination));
        }

        [Test]
        public void Separate_SharesNoPlanet()
        {
            Assert.IsTrue(PuzzleMultiShipBake.Allowed(
                PuzzleShipRelation.Separate, Origin, Destination, OtherOrigin, OtherDestination));
            Assert.IsFalse(PuzzleMultiShipBake.Allowed(
                PuzzleShipRelation.Separate, Origin, Destination, Origin, OtherDestination));
        }

        [Test]
        public void Brief_CarriesTheRelation()
        {
            PuzzleSeed seed = ScriptableObject.CreateInstance<PuzzleSeed>();
            SpaceShapingSettings settings = ScriptableObject.CreateInstance<SpaceShapingSettings>();
            seed.ships = 2;
            seed.relation = PuzzleShipRelation.FanOut;

            PuzzleBrief brief = PuzzleBrief.From(seed, settings);

            Assert.AreEqual(PuzzleShipRelation.FanOut, brief.Relation);
            Object.DestroyImmediate(seed);
            Object.DestroyImmediate(settings);
        }
    }
}
