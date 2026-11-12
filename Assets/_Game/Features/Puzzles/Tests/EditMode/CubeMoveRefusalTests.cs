using DigitalLove.Game.Spatial;
using NUnit.Framework;
using UnityEngine;

namespace DigitalLove.Game.SpaceShaping.Tests.EditMode
{
    public class CubeMoveRefusalTests
    {
        [Test]
        public void Set_KeepsTheLatestAttempt()
        {
            CubeMoveRefusal refusal = default;
            refusal.Set(0, new Vector3(1f, 0f, 0f), Vector3.one * 0.2f);

            Assert.IsTrue(refusal.Blocked);
            Assert.AreEqual(0, refusal.Endpoint);
            Assert.AreEqual(new Vector3(1f, 0f, 0f), refusal.Centre);

            refusal.Set(1, new Vector3(2f, 0f, 0f), Vector3.one * 0.3f);

            Assert.AreEqual(1, refusal.Endpoint);
            Assert.AreEqual(new Vector3(2f, 0f, 0f), refusal.Centre);
            Assert.AreEqual(Vector3.one * 0.3f, refusal.Size);
        }

        [Test]
        public void Clear_EmptiesIt()
        {
            CubeMoveRefusal refusal = default;
            refusal.Set(2, Vector3.zero, Vector3.one);

            refusal.Clear();

            Assert.IsFalse(refusal.Blocked);
        }
    }
}
