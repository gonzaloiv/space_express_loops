using DigitalLove.Localization;
using DigitalLove.UI.Behaviours;
using DigitalLove.UI.DesignSystem;
using TMPro;
using UnityEngine;

namespace DigitalLove.Game.Flow
{
    public class GameEndPanel : MonoBehaviour
    {
        [SerializeField] private SubtitlesLikeFollow subtitlesLikeFollow;

        [Header("Back To Track Selection")]
        [SerializeField] private int secsToBackToTrackSelection = 5;
        [SerializeField] private TextPanel backToTrackSelectionPanel;
        [SerializeField] private AudioSource countdownAudioSource;

        [Header("New High Score")]
        [SerializeField] private GameObject newHighScorePanel;
        [SerializeField] private TextMeshProUGUI newHighScoreLabel;
        [SerializeField] private TextMeshProUGUI scoreLabel;
        [SerializeField] private AudioSource newHighScoreAudioSource;

        public int SecsToBackToTrackSelection => secsToBackToTrackSelection;

        public void ShowWithNewHighScore(int score)
        {
            Show();
            newHighScorePanel.SetActive(true);
            newHighScoreLabel.text = LocalizationUtil.GetValue("new_high_score");
            scoreLabel.text = $"{score:N0}";
            newHighScoreAudioSource.Play();
        }

        public void Show()
        {
            gameObject.SetActive(true);
            newHighScorePanel.SetActive(false);
            subtitlesLikeFollow.ShowInCameraView();
        }

        public void ShowCountdown(int secondsRemaining)
        {
            backToTrackSelectionPanel.Show(
                new Text(title: LocalizationUtil.GetValue("restarting_in_n_seconds", secondsRemaining)));
        }

        public void PlayCountdownTick() => countdownAudioSource.Play();

        public void Hide()
        {
            gameObject.SetActive(false);
            newHighScorePanel.SetActive(false);
        }
    }
}