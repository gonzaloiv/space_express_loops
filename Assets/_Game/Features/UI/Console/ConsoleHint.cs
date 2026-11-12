using TMPro;
using UnityEngine;

namespace DigitalLove.Game.UI.Console
{
    // Authored console lines that a waiting check asks the boot to type, in turn and looped, while it
    // is due ("> HAND TRACKING NEEDED", ...). The check only raises Due; the boot does the typing.
    public class ConsoleHint : MonoBehaviour
    {
        [SerializeField] private TMP_Text[] lines;

        public bool Due { get; set; }
        public int Count => lines.Length;

        public TMP_Text Line(int index) => lines[index];

        public void Hide()
        {
            foreach (TMP_Text line in lines)
                line.gameObject.SetActive(false);
        }
    }
}
