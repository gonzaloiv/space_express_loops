using UnityEngine;

namespace DigitalLove.Game.Levels
{
    public class UniverseGhostView : MonoBehaviour
    {
        [SerializeField] private LineRenderer handLine;

        public void ShowHandLine(Vector3 first, Vector3 second)
        {
            gameObject.SetActive(true);
            handLine.useWorldSpace = true;
            handLine.positionCount = 2;
            handLine.SetPosition(0, first);
            handLine.SetPosition(1, second);
            handLine.enabled = true;
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }
    }
}
