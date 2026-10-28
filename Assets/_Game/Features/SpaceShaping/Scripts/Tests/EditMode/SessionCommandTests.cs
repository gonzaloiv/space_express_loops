using NUnit.Framework;
using UnityEngine;

namespace DigitalLove.Game.SpaceShaping.Tests.EditMode
{
    public class SessionCommandTests
    {
        [Test]
        public void Rescale_ReplacesTheZoneInsteadOfCompounding()
        {
            SpaceShapingSession session = new(TestSetups.Content());
            session.ApplyZone(new SpaceZone(Vector3.zero, new Vector3(6f, 6f, 6f), 2f, 0f));
            session.ApplyZone(new SpaceZone(Vector3.zero, new Vector3(6f, 6f, 6f), 0.5f, 0f));

            Assert.AreEqual(-1f, session.GetPosition(0).x, 0.0001f);
            Assert.AreEqual(1f, session.GetScale(0), 0.0001f);
            Assert.AreEqual(2, session.CommandCount);
            Assert.AreEqual(1, session.Zones.Count);
        }

        [Test]
        public void Undo_RemovesLastCommandAndRestoresContent()
        {
            SpaceShapingSession session = new(TestSetups.Content());
            session.ApplyZone(new SpaceZone(Vector3.zero, new Vector3(6f, 6f, 6f), 2f, 0f));
            Assert.AreEqual(4f, session.GetScale(0), 0.0001f);

            Assert.IsTrue(session.Undo());
            Assert.AreEqual(2f, session.GetScale(0), 0.0001f);
            Assert.AreEqual(0, session.CommandCount);
            Assert.IsFalse(session.Undo());
        }

        [Test]
        public void Gesture_PreviewChargesNetScaleDelta()
        {
            SpaceShapingSession session = new(TestSetups.Content());
            session.PreviewZone(new SpaceZone(Vector3.zero, new Vector3(6f, 6f, 6f), 1f, 0f));
            session.PreviewZone(new SpaceZone(Vector3.zero, new Vector3(6f, 6f, 6f), 1.5f, 0f));
            session.PreviewZone(new SpaceZone(Vector3.zero, new Vector3(6f, 6f, 6f), 2f, 0f));

            Assert.AreEqual(1f, session.EntropySpent, 0.0001f);

            session.CommitPreview();
            Assert.AreEqual(1, session.CommandCount);
            Assert.AreEqual(1f, session.EntropySpent, 0.0001f);
        }

        [Test]
        public void Gesture_ReturningToReferenceRefundsThePreview()
        {
            SpaceShapingSession session = new(TestSetups.Content());
            session.PreviewZone(new SpaceZone(Vector3.zero, new Vector3(6f, 6f, 6f), 1.5f, 0f));
            Assert.AreEqual(0.5f, session.EntropySpent, 0.0001f);

            session.PreviewZone(new SpaceZone(Vector3.zero, new Vector3(6f, 6f, 6f), 1f, 0f));
            Assert.AreEqual(0f, session.EntropySpent, 0.0001f);
        }

        [Test]
        public void Gesture_ReturningToCommittedScaleCostsNothing()
        {
            SpaceShapingSession session = new(TestSetups.Content());
            session.ApplyZone(new SpaceZone(Vector3.zero, new Vector3(6f, 6f, 6f), 1.5f, 0f));

            session.PreviewZone(new SpaceZone(Vector3.zero, new Vector3(6f, 6f, 6f), 1.2f, 0f));
            Assert.AreEqual(0.8f, session.EntropySpent, 0.0001f);

            session.PreviewZone(new SpaceZone(Vector3.zero, new Vector3(6f, 6f, 6f), 1.5f, 0f));
            Assert.AreEqual(0.5f, session.EntropySpent, 0.0001f);
        }

        [Test]
        public void Undo_RefundsEntropySpent()
        {
            SpaceShapingSession session = new(TestSetups.Content());
            session.ApplyZone(new SpaceZone(Vector3.zero, new Vector3(6f, 6f, 6f), 2f, 0f));
            Assert.AreEqual(1f, session.EntropySpent, 0.0001f);

            session.Undo();
            Assert.AreEqual(0f, session.EntropySpent, 0.0001f);
        }
    }
}
