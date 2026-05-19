using System;
using System.Collections;
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

        public Action countdownComplete = () => { };

        public void ShowWithNewHighScore(int score)
        {
            Show();
            newHighScorePanel.SetActive(true);
            newHighScoreLabel.text = LocalizationUtil.GetValue("new_high_score");
            scoreLabel.text = score.ToString();
            newHighScoreAudioSource.Play();
        }

        public void Show()
        {
            gameObject.SetActive(true);
            subtitlesLikeFollow.ShowInCameraView();
            ShowBackToTrackSelectionPanel();
        }

        private void ShowBackToTrackSelectionPanel()
        {
            int countdown = secsToBackToTrackSelection;
            IEnumerator CountdownRoutine()
            {
                while (countdown > 0)
                {
                    countdownAudioSource.Play();
                    backToTrackSelectionPanel.Show(new Text(title: LocalizationUtil.GetValue("restarting_in_n_seconds", countdown)));
                    yield return new WaitForSeconds(1);
                    countdown--;
                }
                countdownComplete.Invoke();
            }
            StartCoroutine(CountdownRoutine());
        }

        public void Hide()
        {
            gameObject.SetActive(false);
            newHighScorePanel.gameObject.SetActive(false);
        }
    }
}