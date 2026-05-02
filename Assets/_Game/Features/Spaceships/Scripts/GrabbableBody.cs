using DigitalLove.Global;
using DigitalLove.UI.Visuals;
using Oculus.Interaction;
using UnityEngine;

namespace DigitalLove.Game.Spaceships
{
    public class GrabbableBody : MonoBehaviour
    {
        [SerializeField] private Grabbable grabbable;
        [SerializeField] private RegularScalePunch scalePunch;
        [SerializeField] private Renderer grabbableRenderer;

        private void Awake()
        {
            scalePunch.OnHide();
        }

        public void Show()
        {
            scalePunch.OnShow();
            grabbable.SetActive(false);
            grabbable.transform.LocalReset();
            grabbableRenderer.gameObject.SetActive(true);
            grabbable.SetActive(true);
        }

        public void Hide()
        {
            scalePunch.OnHide();
            grabbableRenderer.gameObject.SetActive(false);
        }
    }
}