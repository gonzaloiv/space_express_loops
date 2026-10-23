using NUnit.Framework;
using UnityEngine;

namespace DigitalLove.Game.Sectors.Tests
{
    public class UniverseBoundsTests
    {
        private const float Tolerance = 0.0001f;

        [Test]
        public void ClampFocus_KeepsWindowInsideField()
        {
            UniverseBounds bounds = new(Vector3.zero, 5f);
            Vector3 clamped = bounds.ClampFocus(new Vector3(20f, -20f, 1f), 1.5f);

            Assert.AreEqual(3.5f, clamped.x, Tolerance);
            Assert.AreEqual(-3.5f, clamped.y, Tolerance);
            Assert.AreEqual(1f, clamped.z, Tolerance);
        }

        [Test]
        public void ClampFocus_KeepsFocusWhenItFits()
        {
            UniverseBounds bounds = new(Vector3.zero, 5f);
            Vector3 focus = new(1f, 2f, -3f);

            Assert.AreEqual(focus, bounds.ClampFocus(focus, 1.5f));
        }

        [Test]
        public void Contains_UsesFieldRadius()
        {
            UniverseBounds bounds = new(Vector3.zero, 2f);

            Assert.IsTrue(bounds.Contains(new Vector3(1f, 1f, 1f)));
            Assert.IsFalse(bounds.Contains(new Vector3(3f, 0f, 0f)));
        }

        [Test]
        public void Moved_ClampsToBounds()
        {
            UniverseBounds bounds = new(Vector3.zero, 4f);
            UniverseFocus focus = new(Vector3.zero, 1f);
            UniverseFocus moved = focus.Moved(new Vector3(100f, 0f, 0f), bounds);

            Assert.AreEqual(3f, moved.Position.x, Tolerance);
            Assert.AreEqual(1f, moved.ViewRadius, Tolerance);
        }
    }
}
