using DigitalLove.Casual.Levels;
using DigitalLove.Game.SpaceShaping;
using DigitalLove.Localization;
using TMPro;
using UnityEngine;

namespace DigitalLove.Game.PuzzleSelection
{
    // One title per chapter's system on the map ("EPISODE 2 · THE FLEET"), pooled from an authored
    // label under the map root, so it moves with the map and sits beside its cluster.
    public class MapChapterTitles : MonoBehaviour
    {
        private const string TableName = "Levels";

        [Tooltip("Authored title under the map root; copies are pooled from it, one per chapter.")]
        [SerializeField] private TMP_Text prototype;
        [Tooltip("Where the title sits from its system's centre, in map units: the direction is " +
                 "scaled by the system's radius plus the outset.")]
        [SerializeField] private Vector3 direction = Vector3.forward;
        [SerializeField, Min(0f)] private float outset = 0.04f;
        [SerializeField, Min(0f)] private float height = 0.02f;

        private ViewPool<TMP_Text> pool;

        private ViewPool<TMP_Text> Pool =>
            pool ??= new ViewPool<TMP_Text>(prototype, prototype.transform.parent, "Chapter Title", null);

        public void Show(PuzzleMap map)
        {
            prototype.gameObject.SetActive(false);
            int shown = 0;
            for (int i = 0; i < map.Chapters.Count; i++)
            {
                MapChapter chapter = map.Chapters[i];
                string title = LocalizationUtil.GetValue(TableName, ChapterData.TitleKeyFor(chapter.Id));
                if (string.IsNullOrEmpty(title))
                    continue;

                Place(Pool.EnsureSlot(shown), chapter, title);
                shown++;
            }

            HideFrom(shown);
        }

        public void Hide() => HideFrom(0);

        private void Place(TMP_Text label, MapChapter chapter, string title)
        {
            label.text = title;
            label.transform.localPosition = chapter.Centre
                + direction.normalized * (chapter.Radius + outset)
                + Vector3.up * height;
            label.gameObject.SetActive(true);
        }

        private void HideFrom(int index) =>
            Pool.ForEachFrom(index, label => label.gameObject.SetActive(false));
    }
}
