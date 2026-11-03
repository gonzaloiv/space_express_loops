using System;
using UnityEditor;
using UnityEngine;

namespace DigitalLove.Game.SpaceShaping.Creator
{
    [CreateAssetMenu(fileName = "PlanetNames", menuName = "DigitalLove/Game/Planet Names")]
    public class PlanetNames : ScriptableObject
    {
        private const string AssetPath =
            "Assets/_Game/Features/PuzzleCreator/Data/PlanetNames.asset";

        private static PlanetNames instance;
        public static PlanetNames Instance => instance ??= AssetDatabase.LoadAssetAtPath<PlanetNames>(AssetPath);

        public string[] names = Array.Empty<string>();

        public string NameAt(int index)
        {
            if (names == null || names.Length == 0)
                return string.Empty;

            return names[((index % names.Length) + names.Length) % names.Length];
        }

        public void Assign(PuzzleFieldPlanet[] planets)
        {
            if (planets == null || planets.Length == 0)
                return;

            if (names == null || names.Length == 0)
            {
                Debug.LogWarning($"Planet names: {name} has no names; planets stay unnamed.");
                return;
            }

            int count = Mathf.Min(planets.Length, names.Length);
            for (int i = 0; i < count; i++)
                planets[i].name = names[i];

            if (count < planets.Length)
            {
                Debug.LogError(
                    $"Planet names: {names.Length} name(s) for {planets.Length} planets; "
                    + $"{planets.Length - count} planet(s) left unnamed.");
            }
        }
    }
}
