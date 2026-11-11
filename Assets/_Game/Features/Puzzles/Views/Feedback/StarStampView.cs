using System;
using System.Collections;
using DigitalLove.Global;
using UnityEngine;
using UnityEngine.UI;

namespace DigitalLove.Game.SpaceShaping
{
    // Types the ECO star stamp above the destination: one icon per step, with a per-star event the
    // cue player can tick. The icon count is fixed; the grade decides how many are filled.
    public class StarStampView : MonoBehaviour
    {
        private const float EmptyStarAlpha = 0.18f;

        [SerializeField] private RawImage[] stars;
        [SerializeField] private float starStepSecs = 0.25f;
        [SerializeField] private ColorValue starColor;

        private Coroutine typing;
        private Camera view;

        public event Action<int> StarStamped;

        private Camera View => view != null ? view : view = Camera.main;

        private Color FilledStar => starColor.value;

        private Color EmptyStar => starColor.value.WithAlpha(EmptyStarAlpha);

        public float Play(int grade, Vector3 worldPosition)
        {
            Stop();
            transform.position = worldPosition;
            gameObject.SetActive(true);
            int filledCount = Mathf.Clamp(grade, 0, stars.Length);
            for (int i = 0; i < stars.Length; i++)
            {
                stars[i].enabled = false;
                stars[i].color = i < filledCount ? FilledStar : EmptyStar;
            }

            typing = StartCoroutine(Type());
            return Mathf.Max(starStepSecs, 0f) * stars.Length;
        }

        public void ShowStatic(int grade)
        {
            StopTyping();
            gameObject.SetActive(true);
            int filledCount = Mathf.Clamp(grade, 0, stars.Length);
            for (int i = 0; i < stars.Length; i++)
            {
                stars[i].enabled = true;
                stars[i].color = i < filledCount ? FilledStar : EmptyStar;
            }
        }

        public void Hide() => gameObject.SetActive(false);

        public void Stop()
        {
            StopTyping();
            gameObject.SetActive(false);
        }

        private IEnumerator Type()
        {
            for (int i = 0; i < stars.Length; i++)
            {
                stars[i].enabled = true;
                StarStamped?.Invoke(i);
                yield return new WaitForSeconds(starStepSecs);
            }

            typing = null;
        }

        private void LateUpdate()
        {
            Camera camera = View;
            if (camera == null)
                return;

            Vector3 forward = transform.position - camera.transform.position;
            if (forward.sqrMagnitude < 0.000001f)
                return;

            transform.rotation = Quaternion.LookRotation(forward, Vector3.up);
        }

        private void StopTyping()
        {
            if (typing == null)
                return;

            StopCoroutine(typing);
            typing = null;
        }

        private void OnDisable() => StopTyping();
    }
}
