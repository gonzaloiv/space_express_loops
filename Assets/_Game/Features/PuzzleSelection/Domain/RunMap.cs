using System.Collections.Generic;
using DigitalLove.Casual.Levels;
using DigitalLove.Game.SpaceShaping;

namespace DigitalLove.Game.PuzzleSelection
{
    public static class RunMap
    {
        public static PuzzleData[] ReadLevels(LevelSelector selector)
        {
            int count = selector.TotalLevelsCount;
            PuzzleData[] levels = new PuzzleData[count];
            for (int i = 0; i < count; i++)
                levels[i] = selector.GetLevelByGlobalIndex<PuzzleData>(i);

            return levels;
        }

        public static PuzzleMap Build(LevelSelector selector, RunProgress progress) =>
            Build(ReadLevels(selector), progress);

        // Each chapter is its own system on the map, so an episode reads as a place.
        public static PuzzleMap Build(IReadOnlyList<PuzzleData> levels, RunProgress progress)
        {
            List<string> ids = new();
            List<int> sizes = new();
            GroupChapters(levels, ids, sizes);

            PuzzleMapLayout layout = PuzzleMapLayout.Systems(sizes);
            MapChapter[] chapters = new MapChapter[ids.Count];
            int start = 0;
            for (int i = 0; i < ids.Count; i++)
            {
                chapters[i] = new MapChapter(ids[i], start, sizes[i], layout.Centre(i), layout.Radius(i));
                start += sizes[i];
            }

            return PuzzleMap.FromLevels(levels, progress, layout, chapters);
        }

        // Consecutive jobs with the same chapter id ("02" of "02_03") form one group.
        private static void GroupChapters(
            IReadOnlyList<PuzzleData> levels, List<string> ids, List<int> sizes)
        {
            for (int i = 0; i < levels.Count; i++)
            {
                string id = ChapterOf(levels[i]);
                if (ids.Count > 0 && ids[ids.Count - 1] == id)
                {
                    sizes[sizes.Count - 1]++;
                    continue;
                }

                ids.Add(id);
                sizes.Add(1);
            }
        }

        private static string ChapterOf(PuzzleData level) =>
            level == null || string.IsNullOrEmpty(level.id) ? string.Empty : LevelData.GetChapterId(level.id);
    }
}
