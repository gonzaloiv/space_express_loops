using UnityEngine;

namespace DigitalLove.Game.Universe
{
    public class UniverseColorScope : MonoBehaviour
    {
        [SerializeField] private UniverseView universeView;
        [SerializeField, Min(0f)] private float softnessMeters = 0.03f;
        [SerializeField, Range(0f, 1f)] private float maxStrength = 1f;

        private static readonly int CenterId = Shader.PropertyToID("_ColorScopeCenter");
        private static readonly int HalfSizeId = Shader.PropertyToID("_ColorScopeHalfSize");
        private static readonly int RotationId = Shader.PropertyToID("_ColorScopeRotation");
        private static readonly int SoftnessId = Shader.PropertyToID("_ColorScopeSoftness");
        private static readonly int StrengthId = Shader.PropertyToID("_ColorScopeStrength");

        private void LateUpdate()
        {
            float strength = universeView.State.HasRoomAnchor
                ? universeView.BubbleBlend * maxStrength
                : 0f;

            Shader.SetGlobalVector(CenterId, universeView.FrameCenterWorld);
            Shader.SetGlobalVector(HalfSizeId, Vector3.one * universeView.ContentRadiusWorld);
            Shader.SetGlobalVector(RotationId, ToVector(universeView.FrameRotation));
            Shader.SetGlobalFloat(SoftnessId, softnessMeters);
            Shader.SetGlobalFloat(StrengthId, strength);
        }

        private void OnDisable() => Shader.SetGlobalFloat(StrengthId, 0f);

        private static Vector4 ToVector(Quaternion rotation) =>
            new(rotation.x, rotation.y, rotation.z, rotation.w);
    }
}
