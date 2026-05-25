using UnityEngine;
using DigitalLove.TTS;
using System;
using DigitalLove.Game.Levels;
using DigitalLove.Global;
using DigitalLove.DataAccess;
using Reflex.Attributes;
using DigitalLove.UI.Behaviours;

namespace DigitalLove.Game.TTS
{
    public class TTSHelper : MonoBehaviour
    {
        [SerializeField] private MarkdownTTSVoiceWrapper ttsVoiceWrapper;
        [SerializeField] private SubtitlesLikeFollow subtitlesLikeFollow;
        [SerializeField] private Transform defaultPoint;

        [Inject] private MemoryDataClient memoryDataClient;
        [Inject] private UnityPlayerDataClient unityPlayerDataClient;

        public void Say(string key, Action onComplete)
        {
            ttsVoiceWrapper.Speak(key, onComplete);
            SaveCookie(key);
        }

        public void SayRoundIntro(RoundData roundData, Action onComplete)
        {
            Say(roundData.IntroKey, onComplete);
        }

        private async void SaveCookie(string key)
        {
            PlayerData playerData = memoryDataClient.Get<PlayerData>();
            Cookie cookie = playerData.GetOrCreateCookie(key);
            playerData.AddCookie(cookie);
            await unityPlayerDataClient.Put(playerData);
        }

        public void SetInFrontOfCameraOrDefault(bool isInFrontOfCamera)
        {
            if (isInFrontOfCamera)
            {
                subtitlesLikeFollow.ShowInCameraView();
                subtitlesLikeFollow.SetIsFollowing(true);
            }
            else
            {
                transform.SetWorldPose(defaultPoint.ToWorldPose());
                subtitlesLikeFollow.SetIsFollowing(false);
            }
        }
    }
}