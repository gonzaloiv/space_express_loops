using DigitalLove.Global;
using Oculus.Interaction;
using UnityEngine;
using DigitalLove.VFX;

namespace DigitalLove.Game.Spaceships
{
    public class GrabbableBody : MonoBehaviour
    {
        [SerializeField] private Grabbable grabbable;
        [SerializeField] private Renderer grabbableRenderer;
        [SerializeField] private ConstantRotation constantRotation;

        public void Show()
        {
            grabbable.SetActive(false);
            grabbable.transform.LocalReset();
            grabbableRenderer.gameObject.SetActive(true);
            grabbable.SetActive(true);
            constantRotation.IsEnabled = true;
        }

        public void Hide()
        {
            grabbableRenderer.gameObject.SetActive(false);
            constantRotation.IsEnabled = false;
        }
    }
}