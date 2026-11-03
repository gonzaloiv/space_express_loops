using DigitalLove.Game.SpaceShaping.Cube;
using NUnit.Framework;
using UnityEngine;

namespace DigitalLove.Game.SpaceShaping.Tests.EditMode
{
    public class SessionRescaleTests
    {
        private static SpaceShapingSetup Setup() => new()
        {
            Origin = new Vector3(-4f, 0f, 0f),
            Destination = new Vector3(4f, 0f, 0f),
            MaxSteps = 4000,
            LaunchSpeed = 1f,
            Sim = new PuzzleSimSettings
            {
                CaptureRadius = 0.6f,
                StepSecs = 1f / 120f,
                SourceStrengthScale = 2f,
                SeparationScale = 1.2f
            },
            Sources = new[]
            {
                new SourceSpec { Home = new Vector3(-2f, 0f, 0f), InitialScale = 2f, MinScale = 1f, MaxScale = 4f }
            }
        };

        private static SpaceZone Covering(float scale) =>
            new(Vector3.zero, new Vector3(6f, 6f, 6f), scale, 0f);

        [Test]
        public void ScalingBackToPristine_RestoresContent()
        {
            SpaceShapingSession session = new(Setup());
            session.ApplyZone(Covering(2f));
            Assert.AreEqual(4f, session.GetScale(0), 0.0001f);

            session.ApplyZone(Covering(1f));

            Assert.AreEqual(new Vector3(-2f, 0f, 0f), session.GetPosition(0));
            Assert.AreEqual(2f, session.GetScale(0), 0.0001f);
            Assert.AreEqual(1, session.Zones.Count);
            Assert.AreEqual(2, session.CommandCount);
        }

        [Test]
        public void PreviewingBackToPristine_RestoresContentBeforeCommit()
        {
            SpaceShapingSession session = new(Setup());
            session.ApplyZone(Covering(2f));

            session.PreviewZone(Covering(1f));

            Assert.AreEqual(new Vector3(-2f, 0f, 0f), session.GetPosition(0));
            Assert.AreEqual(2f, session.GetScale(0), 0.0001f);
            Assert.AreEqual(1, session.Zones.Count);

            session.CommitPreview();
            Assert.AreEqual(new Vector3(-2f, 0f, 0f), session.GetPosition(0));
            Assert.AreEqual(2f, session.GetScale(0), 0.0001f);
        }
    }
}
