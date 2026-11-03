using UnityEngine;

public class PointIndicatorView : MonoBehaviour
{
    [SerializeField] private Transform indicator;
    [SerializeField] private MeshRenderer rend;
    [Tooltip("Gap above the planet surface, as a fraction of the planet radius.")]
    [SerializeField] private float yOffset = 0.1f;

    private void LateUpdate()
    {
        float radius = rend.localBounds.extents.y * rend.transform.localScale.y;
        indicator.localPosition = new Vector3(0f, radius * (1f + yOffset), 0f);
    }
}
