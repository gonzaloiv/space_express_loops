using DigitalLove.Audio;
using UnityEngine;

namespace DigitalLove.Game.SpaceShaping
{
    // The field's quiet presence: a low sustained voice whose filter and slow LFO open with strain and
    // dip while a pinch is held. Tuned on the prefab; the view only pushes strain and pinch state.
    public class FieldHum : MonoBehaviour
    {
        [SerializeField] private SynthVoice voice;
        [SerializeField] private SynthPatch tone;
        [SerializeField] private int note = 45;
        [SerializeField, Range(0f, 1f)] private float cutoffResting = 0.05f;
        [SerializeField, Range(0f, 1f)] private float cutoffStrain = 0.14f;
        [SerializeField, Min(0f)] private float lfoAmpResting = 0.03f;
        [SerializeField, Min(0f)] private float lfoAmpStrain = 0.1f;
        [SerializeField, Min(0.01f)] private float lfoRateResting = 0.18f;
        [SerializeField, Min(0.01f)] private float lfoRateStrain = 0.55f;
        [SerializeField, Range(0f, 1f)] private float dip = 0.35f;
        [SerializeField, Min(0.01f)] private float dipSecs = 0.12f;
        [SerializeField, Range(0f, 1f)] private float volume = 0.05f;

        private float strain;
        private float pinch;
        private bool pinching;
        private bool running;

        private void OnEnable() => Begin();

        private void OnDisable() => Stop();

        private void Update()
        {
            if (!running)
                return;

            float target = pinching ? 1f : 0f;
            pinch = Mathf.MoveTowards(pinch, target, Time.unscaledDeltaTime / dipSecs);
            voice.SetVolume(volume * Mathf.Lerp(1f, 1f - dip, pinch));
        }

        public void SetStrain(float value)
        {
            strain = Mathf.Clamp01(value);
            if (running)
                Drive();
        }

        public void SetPinching(bool value) => pinching = value;

        private void Begin()
        {
            running = true;
            voice.BeginSustain(tone, note,cutoffResting, lfoAmpResting, lfoRateResting);
            Drive();
            voice.SetVolume(volume * Mathf.Lerp(1f, 1f - dip, pinch));
        }

        private void Drive() =>
            voice.DriveSustain(
                note,
                Mathf.Lerp(cutoffResting, cutoffStrain, strain),
                Mathf.Lerp(lfoAmpResting, lfoAmpStrain, strain),
                Mathf.Lerp(lfoRateResting, lfoRateStrain, strain));

        private void Stop()
        {
            running = false;
            pinch = 0f;
            voice.EndSustain();
        }
    }
}
