using DigitalLove.Game.Nodes;
using UnityEngine;

namespace DigitalLove.Game.SpaceShaping
{
    public class SpaceSourceView : MonoBehaviour, IPoolableNode
    {
        public bool IsActive => gameObject.activeSelf;

        public void Initialize() => Hide();

        public void Hide() => gameObject.SetActive(false);

        public void SetLocal(Vector3 position, float scale)
        {
            transform.position = transform.parent.TransformPoint(position);
            transform.localScale = new Vector3(scale, scale, scale);
            gameObject.SetActive(true);
        }
    }
}
