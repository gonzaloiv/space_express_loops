using System.Collections.Generic;
using DigitalLove.Casual.Levels;
using DigitalLove.DataAccess;
using DigitalLove.Game.SpaceShaping;
using UnityEditor;
using UnityEngine;

namespace DigitalLove.Game.Persistence.Tests
{
    internal sealed class ProgressFixtures
    {
        private readonly List<Object> created = new();

        public LevelSelector Selector(params PuzzleData[] levels)
        {
            ChapterData chapter = ScriptableObject.CreateInstance<ChapterData>();
            chapter.id = "01";
            chapter.levels = levels;
            created.Add(chapter);

            LevelSelector selector = ScriptableObject.CreateInstance<LevelSelector>();
            created.Add(selector);
            SerializedObject serialized = new(selector);
            SerializedProperty chapters = serialized.FindProperty("chapters");
            chapters.arraySize = 1;
            chapters.GetArrayElementAtIndex(0).objectReferenceValue = chapter;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            return selector;
        }

        public PuzzleData Puzzle(string id)
        {
            PuzzleData puzzle = ScriptableObject.CreateInstance<PuzzleData>();
            puzzle.id = id;
            created.Add(puzzle);
            return puzzle;
        }

        public void AddComplete(PlayerData playerData, string levelId, int score)
        {
            Cookie cookie = new LevelCompleteCookie(levelId);
            LevelCompleteCookieMetadata metadata = new() { hasBeenCompleted = true, score = score };
            cookie.SetMetadata(metadata.Serialize<LevelCompleteCookieMetadata>());
            playerData.cookies.Add(cookie);
        }

        public int Score(PlayerData playerData, string levelId) =>
            playerData.GetLevelCompleteCookies().GetByLevelId(levelId).Score;

        public void DestroyAll()
        {
            for (int i = 0; i < created.Count; i++)
                Object.DestroyImmediate(created[i]);

            created.Clear();
        }
    }
}
