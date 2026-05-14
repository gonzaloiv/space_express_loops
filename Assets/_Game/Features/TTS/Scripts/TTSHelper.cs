using UnityEngine;
using DigitalLove.TTS;
using System;
using DigitalLove.Game.Levels;
using DigitalLove.Global;
using DigitalLove.DataAccess;
using System.Collections.Generic;
using System.Linq;
using Reflex.Attributes;
using DigitalLove.UI.Behaviours;

namespace DigitalLove.Game.TTS
{
    public class TTSHelper : MonoBehaviour
    {
        [SerializeField] private MarkdownTTSVoiceWrapper ttsVoiceWrapper;
        [SerializeField] private StringValue[] tips;
        [SerializeField] private SubtitlesLikeFollow subtitlesLikeFollow;
        [SerializeField] private Transform defaultPoint;

        [Inject] private MemoryDataClient memoryDataClient;
        [Inject] private UnityPlayerDataClient unityPlayerDataClient;

        public void Say(string key, Action onComplete)
        {
            ttsVoiceWrapper.Speak(key, onComplete);
            SaveCookie(key);
        }

        public bool HasIntro(RoundData roundData)
        {
            Entry[] entries = ttsVoiceWrapper.GetEntriesByKey(roundData.IntroKey);
            return entries.HasAnyValid();
        }

        public void SayRoundIntro(RoundData roundData, Action onComplete)
        {
            Say(roundData.IntroKey, onComplete);
        }

        public void SayRandomTipIfAvailable(Action onComplete)
        {
            PlayerData playerData = memoryDataClient.Get<PlayerData>();
            List<StringValue> availableTips = tips.Where(m => !playerData.HasCookie(m.value)).ToList();
            if (availableTips.Count == 0)
            {
                onComplete();
                return;
            }
            else
            {
                StringValue tip = availableTips[UnityEngine.Random.Range(0, availableTips.Count)];
                Say(tip.value, onComplete);
            }
        }

        private async void SaveCookie(string key)
        {
            PlayerData playerData = memoryDataClient.Get<PlayerData>();
            Cookie cookie = playerData.GetOrCreateCookie(key);
            playerData.AddCookie(cookie);
            await unityPlayerDataClient.Put(playerData);
        }

        public void SayAfter(float secs, string key, Action onComplete)
        {
            this.InvokeDelayed(secs, () => Say(key, onComplete));
        }

        public void SetInFrontOfCameraOrDefault(bool isInFrontOfCamera)
        {
            if (isInFrontOfCamera)
            {
                subtitlesLikeFollow.ShowInCameraView();
            }
            else
            {
                transform.SetWorldPose(defaultPoint.ToWorldPose());
            }
        }
    }
}