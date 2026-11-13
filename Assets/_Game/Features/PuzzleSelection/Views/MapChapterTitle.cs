using TMPro;
using UnityEngine;

namespace DigitalLove.Game.PuzzleSelection
{
    // One chapter's title on the map: a small world-space canvas under the map root, placed in map
    // units beside its system.
    public class MapChapterTitle : MonoBehaviour
    {
        [SerializeField] private TMP_Text text;

        public void Show(string title, Vector3 mapPosition)
        {
            text.text = title;
            transform.localPosition = mapPosition;
            gameObject.SetActive(true);
        }

        public void Hide() => gameObject.SetActive(false);
    }
}
