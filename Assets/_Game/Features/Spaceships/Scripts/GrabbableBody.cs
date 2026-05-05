using DigitalLove.Global;
using DigitalLove.UI.Visuals;
using Oculus.Interaction;
using UnityEngine;

namespace DigitalLove.Game.Spaceships
{
    public class GrabbableBody : MonoBehaviour
    {
        [SerializeField] private Grabbable grabbable;
        [SerializeField] private Renderer grabbableRenderer;

        public void Show()
        {
            grabbable.SetActive(false);
            grabbable.transform.LocalReset();
            grabbableRenderer.gameObject.SetActive(true);
            grabbable.SetActive(true);
        }

        public void Hide()
        {
            grabbableRenderer.gameObject.SetActive(false);
        }
    }
}