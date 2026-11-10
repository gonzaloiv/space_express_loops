using DigitalLove.Audio;
using UnityEngine;

namespace DigitalLove.Game.SpaceShaping.Cube
{
    // The cube's voice: a live stretch tone whose pitch and filter follow the zone's scale, with a
    // grab tick and a falling release. The zone does not know the entropy budget, so strain is
    // pushed in from the readout.
    public class CubeVoice : MonoBehaviour
    {
        [SerializeField] private CubeView zone;
        [SerializeField] private SynthVoice voice;
        [SerializeField] private SynthPatch tone;

        [Header("Stretch")]
        [SerializeField] private int baseNote = 52;
        [SerializeField] private float semitonesPerOctave = 12f;
        [SerializeField] private float minScale = 0.6f;
        [SerializeField] private float maxScale = 1.8f;
        [SerializeField, Range(0f, 1f)] private float cutoffShrunk = 0.05f;
        [SerializeField, Range(0f, 1f)] private float cutoffGrown = 0.2f;

        [Header("Gesture")]
        [SerializeField, Min(0.001f)] private float grabTickSecs = 0.09f;
        [SerializeField] private float grabTickSemitones = 5f;
        [SerializeField] private float releaseDropSemitones = 3f;

        [Header("Strain")]
        [SerializeField, Range(0f, 1f)] private float strainLfoAmp = 0.08f;
        [SerializeField, Min(0.01f)] private float strainLfoRate = 3.5f;
        [SerializeField, Min(0.01f)] private float restingLfoRate = 1.2f;
        [SerializeField, Min(0f)] private float strainWobbleCents = 8f;

        private float wobblePhase;
        private bool wasScaling;
        private float strain;
        private float grabStart;
        private float releaseStart;
        private float releaseUntil;
        private float releaseNote;
        private float lastNote;

        internal void SetStrain(float value) => strain = Mathf.Clamp01(value);

        private void OnDisable()
        {
            wasScaling = false;
            releaseUntil = 0f;
        }

        private void LateUpdate()
        {
            bool scaling = zone.IsScaling;
            if (scaling && !wasScaling)
                Begin();
            else if (scaling)
                Drive();
            else if (wasScaling)
                BeginRelease();

            if (!scaling && releaseUntil > 0f)
                UpdateRelease();

            wasScaling = scaling;
        }

        private void Begin()
        {
            releaseUntil = 0f;
            grabStart = Time.unscaledTime;
            lastNote = StretchNote() + grabTickSemitones;
            wobblePhase = 0f;
            voice.BeginSustain(tone, lastNote, Cutoff(), StrainLfoAmp(), StrainLfoRate());
        }

        private void Drive()
        {
            lastNote = StretchNote() + GrabTickOffset();
            voice.DriveSustain(lastNote + StrainWobble(), Cutoff(), StrainLfoAmp(), StrainLfoRate());
        }

        private void BeginRelease()
        {
            releaseNote = lastNote;
            releaseStart = Time.unscaledTime;
            releaseUntil = releaseStart + tone.ReleaseSecs;
            voice.EndSustain();
        }

        private void UpdateRelease()
        {
            float elapsed = Time.unscaledTime - releaseStart;
            float drop = releaseDropSemitones * Mathf.Clamp01(elapsed / tone.ReleaseSecs);
            voice.SetNote(releaseNote - drop);

            if (Time.unscaledTime >= releaseUntil)
                releaseUntil = 0f;
        }

        private float StretchNote() =>
            baseNote + Mathf.Log(zone.Scale, 2f) * semitonesPerOctave;

        private float Cutoff() =>
            Mathf.Lerp(cutoffShrunk, cutoffGrown, Mathf.InverseLerp(minScale, maxScale, zone.Scale));

        private float StrainLfoAmp() => strain * strainLfoAmp;

        private float StrainLfoRate() => Mathf.Lerp(restingLfoRate, strainLfoRate, strain);

        // A pitch wobble that grows with strain, so a stretch near the budget sounds strained.
        private float StrainWobble()
        {
            wobblePhase = Mathf.Repeat(
                wobblePhase + Mathf.PI * 2f * StrainLfoRate() * Time.deltaTime, Mathf.PI * 2f);
            return Mathf.Sin(wobblePhase) * strainWobbleCents * 0.01f * strain;
        }

        private float GrabTickOffset() =>
            grabTickSemitones * Mathf.Clamp01(1f - (Time.unscaledTime - grabStart) / grabTickSecs);
    }
}
