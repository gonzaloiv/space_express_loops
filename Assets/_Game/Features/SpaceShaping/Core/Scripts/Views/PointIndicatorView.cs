using UnityEngine;

public class PointIndicatorView : MonoBehaviour
{
    [SerializeField] private Transform indicator;
    [SerializeField] private MeshRenderer rend;
    [SerializeField] private float yOffset = 0.1f;

    private void OnEnable()
    {
        indicator.localPosition = new Vector3(0, rend.bounds.size.y * rend.transform.lossyScale.y +
            rend.transform.lossyScale.y * yOffset, 0);
    }
}
