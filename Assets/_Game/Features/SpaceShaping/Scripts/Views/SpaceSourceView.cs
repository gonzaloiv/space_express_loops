using UnityEngine;

namespace DigitalLove.Game.SpaceShaping
{
    public class SpaceSourceView : MonoBehaviour
    {
        public void SetLocal(Vector3 position, float scale)
        {
            transform.position = transform.parent.TransformPoint(position);
            transform.localScale = new Vector3(scale, scale, scale);
        }
    }
}
