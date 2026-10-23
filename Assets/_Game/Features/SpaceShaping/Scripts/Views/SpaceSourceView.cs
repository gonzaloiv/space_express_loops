using Oculus.Interaction;
using UnityEngine;

namespace DigitalLove.Game.SpaceShaping
{
    public class SpaceSourceView : MonoBehaviour
    {
        [SerializeField] private Transform container;
        [SerializeField] private Transform rangeSphere;
        [SerializeField] private Grabbable grabbable;

        private bool rangeShown;

        public Vector3 LocalPosition => container.InverseTransformPoint(transform.position);

        public float LocalScale => transform.localScale.x;

        public void SetLocal(Vector3 position, float scale)
        {
            transform.position = container.TransformPoint(position);
            transform.localScale = new Vector3(scale, scale, scale);
        }

        public void ShowRange(Vector3 home, float range)
        {
            rangeSphere.position = container.TransformPoint(home);
            rangeSphere.localScale = Vector3.one * (2f * range);
        }

        private void OnEnable() => rangeShown = rangeSphere.gameObject.activeSelf;

        private void Update()
        {
            bool show = grabbable.SelectingPointsCount > 0;
            if (show == rangeShown)
                return;

            rangeShown = show;
            rangeSphere.gameObject.SetActive(show);
        }
    }
}
