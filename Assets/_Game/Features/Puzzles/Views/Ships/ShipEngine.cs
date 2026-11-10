using DigitalLove.Audio;
using UnityEngine;

namespace DigitalLove.Game.SpaceShaping
{
    // The ship's continuous voice while it flies. Pitch follows the ship's own speed — the distance
    // it covers per second — so a slingshot rises and a drift sits low. The travel view owns start,
    // drive and stop.
    public class ShipEngine : MonoBehaviour
    {
        [SerializeField] private SynthVoice voice;
        [SerializeField] private SynthPatch tone;
        [SerializeField] private int slowNote = 45;
        [SerializeField] private int fastNote = 57;
        [SerializeField, Min(0f)] private float minSpeed = 0.02f;
        [SerializeField, Min(0.01f)] private float maxSpeed = 0.8f;
        [SerializeField, Min(0.001f)] private float smoothSecs = 0.12f;
        [SerializeField, Range(0f, 1f)] private float cutoff = 0.08f;
        [SerializeField, Min(0f)] private float lfoAmp = 0.02f;
        [SerializeField, Min(0.01f)] private float lfoRate = 0.4f;
        [SerializeField, Range(0f, 1f)] private float volume = 0.045f;

        private bool running;
        private float speed;

        public void Begin()
        {
            running = true;
            speed = minSpeed;
            voice.BeginSustain(tone, slowNote,cutoff, lfoAmp, lfoRate);
            voice.SetVolume(volume);
        }

        public void Drive(float value, float deltaSecs)
        {
            if (!running)
                return;

            float blend = 1f - Mathf.Exp(-deltaSecs / Mathf.Max(smoothSecs, 0.0001f));
            speed = Mathf.Lerp(speed, value, blend);
            float t = Mathf.InverseLerp(minSpeed, maxSpeed, speed);
            voice.SetNote(Mathf.Lerp(slowNote, fastNote, t));
        }

        public void Stop()
        {
            running = false;
            voice.EndSustain();
        }
    }
}
