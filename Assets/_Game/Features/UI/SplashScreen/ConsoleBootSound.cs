using DigitalLove.Audio;
using UnityEngine;

namespace DigitalLove.Game.UI.SplashScreen
{
    // The console boot's sound: the CRT powering on, typing ticks, a confirm as each setup row gets its
    // OK, and a short glitch as the screen cuts off. Played through the console's system voices.
    public class ConsoleBootSound : MonoBehaviour
    {
        [SerializeField] private ConsoleBootSplashScreen boot;
        [SerializeField] private SynthCuePlayer systemPlayer;
        [SerializeField] private SynthPatch powerOnCue;
        [SerializeField] private SynthPatch typeCue;
        [SerializeField] private SynthPatch confirmCue;
        [SerializeField] private SynthPatch cutOffCue;
        [SerializeField, Min(0f)] private float typeGapSecs = 0.06f;

        private void OnEnable()
        {
            boot.Booted += OnBooted;
            boot.Typed += OnTyped;
            boot.StepConfirmed += OnStepConfirmed;
            boot.CuttingOff += OnCuttingOff;
        }

        private void OnDisable()
        {
            boot.Booted -= OnBooted;
            boot.Typed -= OnTyped;
            boot.StepConfirmed -= OnStepConfirmed;
            boot.CuttingOff -= OnCuttingOff;
        }

        private void OnBooted() => systemPlayer.Play(powerOnCue);

        private void OnTyped() => systemPlayer.PlayTick(typeCue, typeGapSecs);

        private void OnStepConfirmed() => systemPlayer.PlayOver(confirmCue);

        private void OnCuttingOff() => systemPlayer.PlayOver(cutOffCue);
    }
}
