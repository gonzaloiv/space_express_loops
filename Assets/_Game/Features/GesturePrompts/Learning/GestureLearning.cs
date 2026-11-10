using System;
using DigitalLove.DataAccess;
using UnityEngine;

namespace DigitalLove.Game.GesturePrompts
{
    public static class GestureLearning
    {
        private const string Prefix = "gesture_learned_";

        public static string Key(GestureKind kind) => Prefix + Name(kind);

        public static bool IsLearned(PlayerData playerData, GestureKind kind) =>
            playerData.HasCookie(Key(kind));

        public static bool MarkLearned(PlayerData playerData, GestureKind kind) =>
            playerData.AddCookie(new Cookie(Key(kind)));

        public static async void Save(
            PlayerData playerData, GestureKind kind, IDataProvider dataProvider)
        {
            if (!MarkLearned(playerData, kind))
                return;

            try
            {
                await dataProvider.Put(playerData);
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to save the learned {kind} gesture: {e}");
            }
        }

        private static string Name(GestureKind kind)
        {
            switch (kind)
            {
                case GestureKind.Scale:
                    return "scale";
                case GestureKind.Enter:
                    return "enter";
                case GestureKind.Exit:
                    return "exit";
                case GestureKind.Spend:
                    return "spend";
                case GestureKind.Stars:
                    return "stars";
                default:
                    return "move";
            }
        }
    }
}