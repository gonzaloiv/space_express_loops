using DigitalLove.DataAccess;
using DigitalLove.Game.PuzzleSelection;
using DigitalLove.Game.SpaceShaping;
using Reflex.Attributes;
using UnityEngine;

namespace DigitalLove.Game.GesturePrompts
{
    // Decides when a prompt is earned: shows it only while the gesture is unlearned and keeps it
    // beside its cube. The panel owns the timer and closes itself; the recorder persists the learn.
    public class GesturePromptPresenter : MonoBehaviour
    {
        private const int KindCount = 4;

        [SerializeField] private GesturePromptPool pool;
        [SerializeField] private GesturePromptContent scale;
        [SerializeField] private GesturePromptContent enter;
        [SerializeField] private GesturePromptContent move;
        [SerializeField] private GesturePromptContent exit;
        [SerializeField] private SpaceShapingView shapingView;
        [SerializeField] private PuzzleMapView mapView;
        [SerializeField] private Vector3 scaleOffset = new(0.12f, 0.12f, 0f);
        [SerializeField] private Vector3 moveOffset = new(-0.12f, 0.12f, 0f);
        [SerializeField] private Vector3 enterOffset = new(0.12f, 0.12f, 0f);
        [SerializeField] private Vector3 exitOffset = new(0f, 0.22f, 0f);

        [Inject] private MemoryDataClient memoryDataClient;

        public void Show(GestureKind kind)
        {
            if (IsLearned(kind) || pool.IsShowing(kind))
                return;

            pool.Show(Content(kind), Position(kind));
        }

        public void HideAll() => pool.HideAll();

        private void LateUpdate()
        {
            for (int i = 0; i < KindCount; i++)
            {
                GestureKind kind = (GestureKind)i;
                if (pool.IsShowing(kind))
                    pool.Move(kind, Position(kind));
            }
        }

        private bool IsLearned(GestureKind kind)
        {
            PlayerData playerData = memoryDataClient.Get<PlayerData>();
            return playerData != null && GestureLearning.IsLearned(playerData, kind);
        }

        private Vector3 Position(GestureKind kind)
        {
            switch (kind)
            {
                case GestureKind.Scale:
                    return shapingView.CubePosition + scaleOffset;
                case GestureKind.Move:
                    return shapingView.CubePosition + moveOffset;
                case GestureKind.Exit:
                    return shapingView.CubePosition + exitOffset;
                default:
                    return mapView.SelectionCubePosition + enterOffset;
            }
        }

        private GesturePromptContent Content(GestureKind kind)
        {
            switch (kind)
            {
                case GestureKind.Scale:
                    return scale;
                case GestureKind.Enter:
                    return enter;
                case GestureKind.Exit:
                    return exit;
                default:
                    return move;
            }
        }
    }
}