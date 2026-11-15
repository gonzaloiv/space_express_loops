using TMPro;
using UnityEngine;

namespace DigitalLove.Game.PuzzleSelection
{
    // One chapter's title on the map: a small world-space canvas under the map root, floating over its
    // system's centre with a leader line down to it.
    public class MapChapterTitle : MonoBehaviour
    {
        [SerializeField] private TMP_Text text;
        [Tooltip("The system's progress under its title: jobs solved and stars earned.")]
        [SerializeField] private TMP_Text tally;
        [SerializeField] private LineRenderer leader;

        // The title turns on Y to face the player, so a leader straight down to the anchor stays put.
        public void Show(string title, string progress, Vector3 mapPosition, Vector3 mapAnchor)
        {
            text.text = title;
            tally.text = progress;
            transform.localPosition = mapPosition;
            leader.positionCount = 2;
            leader.SetPosition(0, Vector3.zero);
            leader.SetPosition(1, transform.InverseTransformPoint(transform.parent.TransformPoint(mapAnchor)));
            gameObject.SetActive(true);
        }

        public void Hide() => gameObject.SetActive(false);
    }
}
