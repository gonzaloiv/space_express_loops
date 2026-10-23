using UnityEngine;

namespace DigitalLove.Game.SpaceShaping
{
    public class SpaceSourceView : MonoBehaviour
    {
        [SerializeField] private Transform container;

        public Vector3 LocalPosition => container.InverseTransformPoint(transform.position);

        public float LocalScale => transform.localScale.x;

        public void SetLocal(Vector3 position, float scale)
        {
            transform.position = container.TransformPoint(position);
            transform.localScale = new Vector3(scale, scale, scale);
        }
    }
}
