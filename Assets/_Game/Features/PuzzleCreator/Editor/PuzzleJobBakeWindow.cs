using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace DigitalLove.Game.SpaceShaping.Creator.EditorTools
{
    public class PuzzleJobBakeWindow : EditorWindow
    {
        [MenuItem("DigitalLove/Space Shaping/Job Baker")]
        private static void Open() => GetWindow<PuzzleJobBakeWindow>("Job Baker");

        private void OnGUI()
        {
            EditorGUILayout.LabelField("Chapter 01 jobs", EditorStyles.boldLabel);
            IReadOnlyList<PuzzleJobContract> jobs = PuzzleJobContracts.Enabled;
            for (int i = 0; i < jobs.Count; i++)
                DrawJob(jobs[i]);

            EditorGUILayout.Space();
            if (GUILayout.Button("Bake all"))
                PuzzleTestSetBuilder.Build();
        }

        private static void DrawJob(PuzzleJobContract job)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField(
                    $"{job.Id} · {job.Mechanism} · {job.PlanetCount} planets · seed {job.Seed}");
                if (GUILayout.Button("Bake", GUILayout.Width(80f)))
                    PuzzleTestSetBuilder.BuildOne(job);
            }
        }
    }
}
