using DigitalLove.Localization;
using DigitalLove.UI.Behaviours;
using DigitalLove.UI.DesignSystem;
using UnityEngine;

namespace DigitalLove.Game.Placement
{
    // The boot's hand-tracking notice. It is a card, not a launch-failure panel: there is no button,
    // because the setting lives in the system menu and the gate resumes on its own.
    public class HandTrackingRequiredPanel : MonoBehaviour
    {
        [SerializeField] private CardPanel cardPanel;
        [SerializeField] private Canvas canvas;
        [SerializeField] private SubtitlesLikeFollow follow;

        public void Show()
        {
            canvas.gameObject.SetActive(true);
            follow.ShowInCameraView();
            cardPanel.Show(new Card().SetText(new Text(
                title: LocalizationUtil.GetValue("hand_tracking_required_title"),
                body: LocalizationUtil.GetValue("hand_tracking_required_body"))));
        }

        public void Hide()
        {
            cardPanel.Hide(true);
            canvas.gameObject.SetActive(false);
        }
    }
}
