using DigitalLove.Casual.Levels;
using DigitalLove.DataAccess;
using DigitalLove.UI.Behaviours;
using NUnit.Framework;

namespace DigitalLove.Game.Persistence.Tests
{
    public class GestureLearningTests
    {
        [Test]
        public void FreshPlayer_HasNoLearnedGestures()
        {
            PlayerData playerData = new();

            Assert.IsFalse(GestureLearning.IsLearned(playerData, GestureKind.Scale));
            Assert.IsFalse(GestureLearning.IsLearned(playerData, GestureKind.Enter));
            Assert.IsFalse(GestureLearning.IsLearned(playerData, GestureKind.Pan));
            Assert.IsFalse(GestureLearning.IsLearned(playerData, GestureKind.Move));
        }

        [Test]
        public void MarkLearned_AddsOneCookieAndReturnsTrueOnce()
        {
            PlayerData playerData = new();

            Assert.IsTrue(GestureLearning.MarkLearned(playerData, GestureKind.Pan));
            Assert.IsFalse(GestureLearning.MarkLearned(playerData, GestureKind.Pan));

            Assert.IsTrue(GestureLearning.IsLearned(playerData, GestureKind.Pan));
            Assert.AreEqual(1, playerData.cookies.Count);
            Assert.AreEqual("gesture_learned_pan", playerData.cookies[0].id);
        }

        [Test]
        public void MarkLearned_KeysEachGestureSeparately()
        {
            PlayerData playerData = new();

            GestureLearning.MarkLearned(playerData, GestureKind.Scale);
            GestureLearning.MarkLearned(playerData, GestureKind.Move);

            Assert.IsTrue(GestureLearning.IsLearned(playerData, GestureKind.Scale));
            Assert.IsTrue(GestureLearning.IsLearned(playerData, GestureKind.Move));
            Assert.IsFalse(GestureLearning.IsLearned(playerData, GestureKind.Enter));
            Assert.AreEqual(2, playerData.cookies.Count);
        }

        [Test]
        public void MarkLearned_DoesNotCountAsLevelProgress()
        {
            PlayerData playerData = new();

            GestureLearning.MarkLearned(playerData, GestureKind.Scale);

            Assert.AreEqual(0, playerData.GetLevelCompleteCookies().Count);
        }
    }
}
