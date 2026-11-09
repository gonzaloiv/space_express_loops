using DigitalLove.Global;
using UnityEngine;

namespace DigitalLove.Game.Flow
{
    // App-level pause: the system menu or the headset coming off stops time and audio. Systems that
    // run on unscaled time gate themselves on Time.timeScale.
    public class PauseController : MonoBehaviour
    {
        private readonly AppPause pause = new();

        private void OnApplicationFocus(bool focus)
        {
            if (pause.SetFocus(focus))
                Apply();
        }

        private void OnApplicationPause(bool value)
        {
            if (pause.SetSuspended(value))
                Apply();
        }

        private void OnDisable()
        {
            pause.Clear();
            Apply();
        }

        private void Apply()
        {
            Time.timeScale = pause.IsPaused ? 0f : 1f;
            AudioListener.pause = pause.IsPaused;
        }
    }
}
