using System.Collections;
using DigitalLove.UI.Behaviours;
using UnityEngine;

namespace DigitalLove.Game.UI.Console
{
    // The console's two windows on two authored arms from the grid's bottom-left-back corner. The
    // owner passes the corner and the field's rotation (in this transform's parent space); this lays
    // the arms and windows out and runs the one CRT that boots and cuts the whole pair.
    public class ConsoleBank : MonoBehaviour
    {
        [Tooltip("The card slot's windows; the map and job cards share one place.")]
        [SerializeField] private RectTransform[] cards;
        [SerializeField] private RectTransform jim;
        [SerializeField] private LineRenderer cardArm;
        [SerializeField] private LineRenderer jimArm;
        [SerializeField, Min(0.01f)] private float cardArmLength = 0.24f;
        [SerializeField, Min(0.01f)] private float jimArmLength = 0.28f;
        [SerializeField, Min(0f)] private float verticalGap = 0.02f;
        [SerializeField, Min(0f)] private float bootSecs = 0.18f;
        [SerializeField, Min(0f)] private float cutSecs = 0.18f;
        [SerializeField] private Color armColor = Color.white;

        private Coroutine routine;
        private Vector3 fullScale;

        private void Awake()
        {
            fullScale = transform.localScale;
            cardArm.useWorldSpace = false;
            jimArm.useWorldSpace = false;
            SetArmColor(cardArm);
            SetArmColor(jimArm);
        }

        public void Layout(Vector3 corner, Quaternion rotation)
        {
            Vector3 localCorner = transform.InverseTransformPoint(corner);
            Quaternion localRotation = Quaternion.Inverse(transform.rotation) * rotation;
            Vector3 cardInner = ConsoleArmGeometry.DiagonalEnd(
                localCorner, localRotation, cardArmLength);
            Vector3 jimInner = ConsoleArmGeometry.VerticalEnd(
                localCorner, localRotation, jimArmLength, verticalGap);
            Vector3 jimFoot = ConsoleArmGeometry.VerticalFoot(
                localCorner, localRotation, verticalGap);
            LayArm(cardArm, localCorner, cardInner);
            LayArm(jimArm, jimFoot, jimInner);
            for (int i = 0; i < cards.Length; i++)
                Place(cards[i], cardInner, localRotation);
            Place(jim, jimInner, localRotation);
        }

        public void Show()
        {
            Stop();
            transform.localScale = fullScale;
            gameObject.SetActive(true);
        }

        public void Hide()
        {
            Stop();
            transform.localScale = fullScale;
            gameObject.SetActive(false);
        }

        // Boots the pair from a flat line. Call after Show.
        public void Boot()
        {
            Stop();
            transform.localScale = CrtTransition.Flat(fullScale);
            routine = StartCoroutine(CrtTransition.Boot(transform, fullScale, bootSecs));
        }

        // Collapses the pair to a flat line. The flat scale is left for the next Show/Boot to reset.
        public void CutOff()
        {
            Stop();
            routine = StartCoroutine(CutOffRoutine());
        }

        private IEnumerator CutOffRoutine()
        {
            yield return CrtTransition.CutOff(transform, fullScale, cutSecs);
            routine = null;
        }

        private void OnDisable()
        {
            Stop();
            transform.localScale = fullScale;
        }

        private void Stop()
        {
            if (routine == null)
                return;

            StopCoroutine(routine);
            routine = null;
        }

        private void Place(RectTransform window, Vector3 topInner, Quaternion localRotation)
        {
            Vector2 size = new(
                window.rect.width * window.lossyScale.x,
                window.rect.height * window.lossyScale.y);
            window.localRotation = localRotation;
            window.localPosition = ConsoleArmGeometry.WindowCentre(topInner, localRotation, size);
        }

        private void LayArm(LineRenderer arm, Vector3 start, Vector3 end)
        {
            arm.positionCount = 2;
            arm.SetPosition(0, start);
            arm.SetPosition(1, end);
        }

        private void SetArmColor(LineRenderer arm)
        {
            arm.startColor = armColor;
            arm.endColor = armColor;
        }
    }
}
