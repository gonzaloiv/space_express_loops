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
        [SerializeField] private Sprite[] frames;
        [SerializeField, Min(0.01f)] private float frameSecs = 0.12f;
        [SerializeField, Min(0.5f)] private float showSecs = 6f;
        [SerializeField] private string captionKey;

        public GestureKind Kind => kind;

        public int FrameCount => frames != null ? frames.Length : 0;

        public float FrameSecs => frameSecs;

        public float ShowSecs => showSecs;

        public string CaptionKey => captionKey;

        public Sprite FrameAt(int index)
        {
            if (FrameCount == 0)
                return null;

            return frames[index % FrameCount];
        }
    }
}