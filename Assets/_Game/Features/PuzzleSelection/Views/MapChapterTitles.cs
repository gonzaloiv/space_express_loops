using DigitalLove.Casual.Levels;
using DigitalLove.Game.SpaceShaping;
using DigitalLove.Localization;
using UnityEngine;

namespace DigitalLove.Game.PuzzleSelection
{
    // One title per chapter's system on the map ("EPISODE 2 · THE FLEET"), pooled from an authored
    // title under the map root, so it moves with the map and sits beside its cluster.
    public class MapChapterTitles : MonoBehaviour
    {
        private const string TableName = "Levels";

        [Tooltip("Authored title under the map root; copies are pooled from it, one per chapter.")]
        [SerializeField] private MapChapterTitle prototype;
        [Tooltip("Where the title sits from its system's centre, in map units: the direction is " +
                 "scaled by the system's radius plus the outset.")]
        [SerializeField] private Vector3 direction = Vector3.forward;
        [SerializeField, Min(0f)] private float outset = 0.04f;
        [SerializeField, Min(0f)] private float height = 0.02f;

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

                Pool.EnsureSlot(shown).Show(title, Position(chapter));
                shown++;
            }

            HideFrom(shown);
        }

        public void Hide() => HideFrom(0);

        private Vector3 Position(MapChapter chapter) =>
            chapter.Centre + direction.normalized * (chapter.Radius + outset) + Vector3.up * height;

        private void HideFrom(int index) => Pool.ForEachFrom(index, title => title.Hide());
    }
}
