using NUnit.Framework;
using UnityEngine;

namespace DigitalLove.Game.SpaceShaping.Tests.EditMode
{
    public class DustLayoutTests
    {
        [Test]
        public void SameSeed_SameDust()
        {
            DustLayout a = new(64, 7);
            DustLayout b = new(64, 7);

            for (int i = 0; i < a.Count; i++)
            {
                Assert.AreEqual(a.Rest(i), b.Rest(i));
                Assert.AreEqual(a.Drift(i, 3.5f, 0.05f), b.Drift(i, 3.5f, 0.05f));
            }
        }

        [Test]
        public void RestStaysInsideTheReach()
        {
            DustLayout layout = new(256, 11);

            for (int i = 0; i < layout.Count; i++)
            {
                Vector3 rest = layout.Rest(i);
                Assert.LessOrEqual(Mathf.Abs(rest.x), 1f);
                Assert.LessOrEqual(Mathf.Abs(rest.y), 1f);
                Assert.LessOrEqual(Mathf.Abs(rest.z), 1f);
            }
        }

        [Test]
        public void EdgeFade_IsFullInsideAndZeroAtTheEdge()
        {
            DustLayout layout = new(512, 3);

            for (int i = 0; i < layout.Count; i++)
            {
                Vector3 rest = layout.Rest(i);
                float edge = Mathf.Max(Mathf.Abs(rest.x), Mathf.Max(Mathf.Abs(rest.y), Mathf.Abs(rest.z)));
                float fade = layout.EdgeFade(i, 0.6f);
                if (edge <= 0.6f)
                    Assert.AreEqual(1f, fade, 1e-5f);
                Assert.That(fade, Is.InRange(0f, 1f));
            }
        }
    }
}
