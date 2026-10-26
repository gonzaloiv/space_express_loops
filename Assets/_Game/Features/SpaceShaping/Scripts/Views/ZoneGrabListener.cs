using Oculus.Interaction;
using UnityEngine;

namespace DigitalLove.Game.SpaceShaping
{
    public class ZoneGrabListener : MonoBehaviour
    {
        [SerializeField] private Grabbable grabbable;
        [SerializeField] private SpaceZoneView zone;
        [SerializeField] private Renderer frameRenderer;
        [SerializeField] private Renderer fillRenderer;

        [Header("Frame - the region")]
        [SerializeField] private Color frameIdle = new(0.9f, 0.93f, 1f, 0.7f);
        [SerializeField] private Color frameHover = new(0.65f, 0.95f, 1f, 1f);
        [SerializeField] private Color frameMove = new(0.5f, 0.9f, 1f, 1f);
        [SerializeField] private Color frameResize = new(1f, 0.55f, 0.15f, 1f);
        [SerializeField] private float frameGain = 1.5f;
        [SerializeField] private float frameGrowRatio = 2f;

        [Header("Fill - the scaled space")]
        [SerializeField] private Color fillIdle = new(0.55f, 0.65f, 0.95f, 0.1f);
        [SerializeField] private Color fillHover = new(0.7f, 0.82f, 1f, 0.22f);
        [SerializeField] private Color fillShrink = new(0.2f, 0.6f, 1f, 0.28f);
        [SerializeField] private Color fillMagnify = new(1f, 0.45f, 0.2f, 0.28f);
        [SerializeField] private SpaceShapingSettings settings;
        [SerializeField] private float scaleGain = 2f;
        [SerializeField] private float scaleSmoothSecs = 0.08f;

        private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
        private static readonly int ColorId = Shader.PropertyToID("_Color");

        private MaterialPropertyBlock frameBlock;
        private MaterialPropertyBlock fillBlock;

        private bool classified;
        private bool frameGrab;
        private int lastCount;
        private float startHandDistance;
        private float startScale;
        private Vector3 startSize;
        private Vector3 grabOffset;
        private Vector3 restZoneSize;
        private Vector3 restGrabScale;

        private bool hovering;
        private Vector3 hoverWorld;

        private void Awake()
        {
            restZoneSize = zone.Size;
            restGrabScale = transform.localScale;
        }

        private void OnEnable() => grabbable.WhenPointerEventRaised += OnPointerEvent;

        private void OnDisable() => grabbable.WhenPointerEventRaised -= OnPointerEvent;

        private void OnPointerEvent(PointerEvent evt)
        {
            switch (evt.Type)
            {
                case PointerEventType.Hover:
                case PointerEventType.Move:
                    hoverWorld = evt.Pose.position;
                    hovering = true;
                    break;
                case PointerEventType.Unhover:
                case PointerEventType.Cancel:
                    hovering = false;
                    break;
            }
        }

        private void Update()
        {
            int count = grabbable.SelectingPointsCount;
            if (count == 0)
            {
                zone.SetScaling(false);
                ResetGesture();
                ApplyFeedback();
                return;
            }

            if (!classified)
                BeginGesture();

            if (count >= 2 && lastCount < 2)
            {
                startHandDistance = HandDistance();
                startScale = zone.Scale;
                startSize = zone.Size;
                frameGrab = !AllInside();
            }

            if (count == 1 && lastCount != 1)
                grabOffset = zone.Centre - HandLocal();

            if (count == 1)
                Move();
            else if (frameGrab)
                ResizeFrame();
            else
                ScaleUniverse();

            zone.SetScaling(count >= 2 && !frameGrab);
            ApplyFeedback();
            lastCount = count;
        }

        private void BeginGesture()
        {
            classified = true;
            restZoneSize = zone.Size;
            restGrabScale = transform.localScale;
            frameGrab = !InsideContent(HandWorld());
            grabOffset = zone.Centre - HandLocal();
        }

        private void ResetGesture()
        {
            classified = false;
            lastCount = 0;
        }

        private void Move()
        {
            zone.SetFrame(HandLocal() + grabOffset, zone.Size);
            SyncGrab();
        }

        private void ScaleUniverse()
        {
            if (startHandDistance < 0.0001f)
                return;

            float ratio = HandDistance() / startHandDistance;
            float target = Mathf.Clamp(
                startScale * Mathf.Pow(ratio, scaleGain), settings.minScaleFactor, settings.maxScaleFactor);
            float blend = 1f - Mathf.Exp(-Time.deltaTime / Mathf.Max(scaleSmoothSecs, 0.001f));
            zone.SetScale(Mathf.Lerp(zone.Scale, target, blend));
        }

        private void ResizeFrame()
        {
            if (startHandDistance < 0.0001f)
                return;

            float ratio = HandDistance() / startHandDistance;
            float factor = Mathf.Clamp(Mathf.Pow(ratio, frameGain), 1f, frameGrowRatio);
            zone.SetFrame(zone.Centre, startSize * factor);
            SyncGrab();
        }

        private void SyncGrab()
        {
            transform.localPosition = zone.Centre;
            Vector3 size = zone.Size;
            transform.localScale = new Vector3(
                restGrabScale.x * SafeRatio(size.x, restZoneSize.x),
                restGrabScale.y * SafeRatio(size.y, restZoneSize.y),
                restGrabScale.z * SafeRatio(size.z, restZoneSize.z));
        }

        private static float SafeRatio(float value, float reference) =>
            reference > 0.0001f ? value / reference : 1f;

        private void ApplyFeedback()
        {
            if (grabbable.SelectingPointsCount == 0)
            {
                ApplyIdleFeedback();
                return;
            }

            bool scaling = grabbable.SelectingPointsCount >= 2 && !frameGrab;
            Color frameColor = scaling ? frameIdle : FrameActiveColor();
            SetColor(frameRenderer, ref frameBlock, frameColor);
            SetColor(fillRenderer, ref fillBlock, scaling ? ScaleColor() : fillIdle);
        }

        private void ApplyIdleFeedback()
        {
            bool hoverInside = hovering && InsideContent(hoverWorld);
            SetColor(frameRenderer, ref frameBlock, hovering && !hoverInside ? frameHover : frameIdle);
            SetColor(fillRenderer, ref fillBlock, hovering && hoverInside ? fillHover : fillIdle);
        }

        private Color FrameActiveColor() =>
            grabbable.SelectingPointsCount >= 2 ? frameResize : frameMove;

        private Color ScaleColor() =>
            Color.Lerp(fillShrink, fillMagnify, Mathf.InverseLerp(
                settings.minScaleFactor, settings.maxScaleFactor, zone.Scale));

        private void SetColor(Renderer target, ref MaterialPropertyBlock block, Color color)
        {
            if (target == null)
                return;

            block ??= new MaterialPropertyBlock();
            target.GetPropertyBlock(block);
            block.SetColor(BaseColorId, color);
            block.SetColor(ColorId, color);
            target.SetPropertyBlock(block);
        }

        private bool InsideContent(Vector3 world)
        {
            Vector3 local = ToParentLocal(world) - zone.Centre;
            Vector3 half = zone.ContentHalfExtents;
            return Mathf.Abs(local.x) <= half.x
                && Mathf.Abs(local.y) <= half.y
                && Mathf.Abs(local.z) <= half.z;
        }

        private bool AllInside()
        {
            for (int i = 0; i < grabbable.GrabPoints.Count; i++)
            {
                if (!InsideContent(grabbable.GrabPoints[i].position))
                    return false;
            }

            return true;
        }

        private Vector3 HandLocal() => ToParentLocal(HandWorld());

        private Vector3 ToParentLocal(Vector3 world)
        {
            Transform parent = zone.transform.parent;
            return parent != null ? parent.InverseTransformPoint(world) : world;
        }

        private Vector3 HandWorld() =>
            grabbable.GrabPoints.Count > 0 ? grabbable.GrabPoints[0].position : zone.transform.position;

        private float HandDistance()
        {
            if (grabbable.GrabPoints.Count < 2)
                return 0f;

            return Vector3.Distance(grabbable.GrabPoints[0].position, grabbable.GrabPoints[1].position);
        }
    }
}
