using DigitalLove.Game.SpaceShaping;
using UnityEngine;

namespace DigitalLove.Game.GesturePrompts
{
    // Spawns prompt panels from the one authored prototype, so every gesture shares the same art
    // pipeline and two prompts can be on screen at once.
    public class GesturePromptPool : MonoBehaviour
    {
        [SerializeField] private GesturePromptPanel prototype;
        [SerializeField] private Transform promptsParent;

        private ViewPool<GesturePromptPanel> pool;

        private ViewPool<GesturePromptPanel> Pool =>
            pool ??= new ViewPool<GesturePromptPanel>(
                prototype, promptsParent, "Gesture Prompt", null);

        public void Show(GesturePromptContent content, Vector3 position) =>
            Take(content.Kind).Show(content, position);

        public void HideAll()
        {
            ViewPool<GesturePromptPanel> views = Pool;
            for (int i = 0; i < views.Views.Count; i++)
                views.Views[i].Hide();
        }

        public bool IsShowing(GestureKind kind)
        {
            ViewPool<GesturePromptPanel> views = Pool;
            for (int i = 0; i < views.Views.Count; i++)
            {
                if (views.Views[i].Kind == kind && views.Views[i].IsShowing)
                    return true;
            }

            return false;
        }

        public void Move(GestureKind kind, Vector3 position)
        {
            ViewPool<GesturePromptPanel> views = Pool;
            for (int i = 0; i < views.Views.Count; i++)
            {
                if (views.Views[i].Kind == kind)
                    views.Views[i].transform.position = position;
            }
        }

        private GesturePromptPanel Take(GestureKind kind)
        {
            ViewPool<GesturePromptPanel> views = Pool;
            for (int i = 0; i < views.Views.Count; i++)
            {
                if (views.Views[i].Kind == kind)
                    return views.Views[i];
            }

            for (int i = 0; i < views.Views.Count; i++)
            {
                if (views.Views[i].IsFree)
                    return views.Views[i];
            }

            return views.EnsureSlot(views.Views.Count);
        }
    }
}