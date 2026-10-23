using UnityEngine;

namespace DigitalLove.Game.SpaceShaping
{
    public class SpaceSourceView : MonoBehaviour
    {
        [SerializeField] private Transform container;

        public void SetLocal(Vector3 position, float scale)
        {
            transform.position = container.TransformPoint(position);
            transform.localScale = new Vector3(scale, scale, scale);
        }
    }
}
