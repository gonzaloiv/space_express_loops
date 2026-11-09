using System.Collections;
using UnityEngine;

namespace DigitalLove.Game.SpaceShaping
{
    // The Wrap straining at a beat (a job opens, fails or lands): the field stutters in a few short
    // bursts, jumping by a centimetre or so while the lattice tears into bands. Each burst stays under
    // ~0.1 s for comfort; never during the solve.
    public class FieldGlitch : MonoBehaviour
    {
        private static readonly int GlitchId = Shader.PropertyToID("_WrapGlitch");

        [SerializeField] private Transform target;
        [SerializeField, Min(1)] private int bursts = 3;
        [SerializeField, Min(0f)] private float burstSecs = 0.08f;
        [SerializeField, Min(0f)] private float gapSecs = 0.07f;
        [Tooltip("Largest jump of the field, in metres.")]
        [SerializeField, Min(0f)] private float shake = 0.004f;
        [Tooltip("How hard the lattice tears, drops bands and flares during a burst (0 = not at all, 1 = full).")]
        [SerializeField, Range(0f, 1f)] private float intensity = 0.3f;

        private Coroutine routine;
        private Vector3 home;

        public float Play()
        {
            Stop();
            home = target.localPosition;
            routine = StartCoroutine(Run());
            return bursts * (Mathf.Max(burstSecs, 0f) + Mathf.Max(gapSecs, 0f));
        }

        private IEnumerator Run()
        {
            for (int i = 0; i < bursts; i++)
            {
                yield return Burst();
                Settle();
                yield return new WaitForSecondsRealtime(gapSecs);
            }

            Stop();
        }

        private IEnumerator Burst()
        {
            Shader.SetGlobalFloat(GlitchId, intensity);
            float elapsed = 0f;
            while (elapsed < burstSecs)
            {
                target.localPosition = home + Random.insideUnitSphere * shake;
                elapsed += Time.unscaledDeltaTime;
                yield return null;
            }
        }

        private void Settle()
        {
            target.localPosition = home;
            Shader.SetGlobalFloat(GlitchId, 0f);
        }

        private void Stop()
        {
            if (routine != null)
            {
                StopCoroutine(routine);
                routine = null;
                Settle();
            }

            Shader.SetGlobalFloat(GlitchId, 0f);
        }

        private void OnDisable() => Stop();
    }
}
