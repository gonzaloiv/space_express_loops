using DigitalLove.Casual.Levels;
using DigitalLove.Game.SpaceShaping;
using DigitalLove.Localization;
using UnityEngine;

namespace DigitalLove.Game.PuzzleSelection
{
    // One title per chapter's system on the map ("EPISODE 2 · THE FLEET"), pooled from an authored
    // title under the map root, so it moves with the map and floats over its system's centre.
    public class MapChapterTitles : MonoBehaviour
    {
        private const string TableName = "Levels";
        private const string TallyKey = "map_chapter_tally";

        [Tooltip("Authored title under the map root; copies are pooled from it, one per chapter.")]
        [SerializeField] private MapChapterTitle prototype;
        [Tooltip("Height of the title over its system's centre, in map units; clear of the job tags.")]
        [SerializeField, Min(0f)] private float lift = 0.14f;
        [Tooltip("Height over the system's centre where the leader line ends, in map units.")]
        [SerializeField, Min(0f)] private float anchorHeight = 0.02f;

        private ViewPool<MapChapterTitle> pool;

        private ViewPool<MapChapterTitle> Pool =>
            pool ??= new ViewPool<MapChapterTitle>(
                prototype, prototype.transform.parent, "Chapter Title", null);

        public void Show(PuzzleMap map)
        {
            prototype.Hide();
            int shown = 0;
            for (int i = 0; i < map.Chapters.Count; i++)
            {
                MapChapter chapter = map.Chapters[i];
                string title = LocalizationUtil.GetValue(TableName, ChapterData.TitleKeyFor(chapter.Id));
                if (string.IsNullOrEmpty(title))
                    continue;

                Pool.EnsureSlot(shown).Show(
                    title, Tally(map, chapter),
                    chapter.Centre + Vector3.up * lift, chapter.Centre + Vector3.up * anchorHeight);
                shown++;
            }

            HideFrom(shown);
        }

        public void Hide() => HideFrom(0);

        private static string Tally(PuzzleMap map, MapChapter chapter) =>
            LocalizationUtil.Format(
                TallyKey, map.SolvedIn(chapter).ToString(), chapter.Count.ToString(),
                map.StarsIn(chapter).ToString(), map.MaxStarsIn(chapter).ToString());

        private void HideFrom(int index) => Pool.ForEachFrom(index, title => title.Hide());
    }
}
