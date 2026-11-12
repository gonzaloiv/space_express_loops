using DigitalLove.Audio;
using DigitalLove.Game.UI.Console;
using UnityEngine;

namespace DigitalLove.Game.UI.SplashScreen
{
    // The console screen's sound: the CRT powering on, typing ticks, a confirm as each boot setup row
    // gets its OK, and a short glitch as the screen cuts off. Played through the console's system voices,
    // for the boot and for every other message the screen plays.
    public class ConsoleBootSound : MonoBehaviour
    {
        [SerializeField] private ConsoleScreen screen;
        [SerializeField] private ConsoleBootSplashScreen boot;
        [SerializeField] private SynthCuePlayer systemPlayer;
        [SerializeField] private SynthPatch powerOnCue;
        [SerializeField] private SynthPatch typeCue;
        [SerializeField] private SynthPatch confirmCue;
        [SerializeField] private SynthPatch cutOffCue;
        [SerializeField, Min(0f)] private float typeGapSecs = 0.06f;

        private void OnEnable()
        {
            screen.Booted += OnBooted;
            screen.Typed += OnTyped;
            boot.StepConfirmed += OnStepConfirmed;
            screen.CuttingOff += OnCuttingOff;
        }

        private void OnDisable()
        {
            screen.Booted -= OnBooted;
            screen.Typed -= OnTyped;
            boot.StepConfirmed -= OnStepConfirmed;
            screen.CuttingOff -= OnCuttingOff;
        }

        private void OnBooted() => systemPlayer.Play(powerOnCue);

        private void OnTyped() => systemPlayer.PlayTick(typeCue, typeGapSecs);

        private void OnStepConfirmed() => systemPlayer.PlayOver(confirmCue);

        private void OnCuttingOff() => systemPlayer.PlayOver(cutOffCue);
    }
}
