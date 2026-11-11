using DigitalLove.XR.GestureArt;
using UnityEngine;

namespace DigitalLove.Game.GesturePrompts
{
    // One prompt's art and timing. The panel is shared; this asset is what differs per gesture.
    [CreateAssetMenu(
        fileName = "GesturePrompt",
        menuName = "DigitalLove/Gesture Prompt")]
    public class GesturePromptContent : ScriptableObject
    {
        [SerializeField] private GestureKind kind;
        [SerializeField] private GestureLoop loop;
        [SerializeField, Min(0.5f)] private float showSecs = 6f;
        [SerializeField] private string captionKey;

        public GestureKind Kind => kind;

        public int FrameCount => loop != null ? loop.FrameCount : 0;

        public float FrameSecs => loop != null ? loop.FrameSecs : 0.12f;

        public float ShowSecs => showSecs;

        public string CaptionKey => captionKey;

        public Sprite FrameAt(int index) => loop != null ? loop.FrameAt(index) : null;
    }
}
