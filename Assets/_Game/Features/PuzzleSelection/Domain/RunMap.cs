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

        public static PuzzleMap Build(IReadOnlyList<PuzzleData> levels, RunProgress progress) =>
            PuzzleMap.FromLevels(levels, progress, PuzzleMapLayout.Galaxy(levels.Count));
    }
}
