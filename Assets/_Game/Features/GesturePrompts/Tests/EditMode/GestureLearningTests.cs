using DigitalLove.Casual.Levels;
using DigitalLove.DataAccess;
using NUnit.Framework;

namespace DigitalLove.Game.GesturePrompts.Tests
{
    public class GestureLearningTests
    {
        [Test]
        public void FreshPlayer_HasNoLearnedGestures()
        {
            PlayerData playerData = new();

            Assert.IsFalse(GestureLearning.IsLearned(playerData, GestureKind.Scale));
            Assert.IsFalse(GestureLearning.IsLearned(playerData, GestureKind.Enter));
            Assert.IsFalse(GestureLearning.IsLearned(playerData, GestureKind.Move));
            Assert.IsFalse(GestureLearning.IsLearned(playerData, GestureKind.Exit));
        }

        [Test]
        public void Exit_HasItsOwnKey()
        {
            Assert.AreEqual("gesture_learned_exit", GestureLearning.Key(GestureKind.Exit));
        }

        [Test]
        public void MarkLearned_AddsOneCookieAndReturnsTrueOnce()
        {
            PlayerData playerData = new();

            Assert.IsTrue(GestureLearning.MarkLearned(playerData, GestureKind.Enter));
            Assert.IsFalse(GestureLearning.MarkLearned(playerData, GestureKind.Enter));

            Assert.IsTrue(GestureLearning.IsLearned(playerData, GestureKind.Enter));
            Assert.AreEqual(1, playerData.cookies.Count);
            Assert.AreEqual("gesture_learned_enter", playerData.cookies[0].id);
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
