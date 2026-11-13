using System;
using DigitalLove.DataAccess;
using DigitalLove.Game.PuzzleSelection;
using DigitalLove.Game.SpaceShaping;
using Reflex.Attributes;
using UnityEngine;

namespace DigitalLove.Game.GesturePrompts
{
    // Decides when a prompt is earned: shows it only while the gesture is unlearned and keeps it
    // beside its subject. The panel owns the timer and closes itself; the recorder persists the learn.
    public class GesturePromptPresenter : MonoBehaviour
    {
        private static readonly int KindCount = Enum.GetValues(typeof(GestureKind)).Length;

        [SerializeField] private GesturePromptPool pool;
        [SerializeField] private GesturePromptContent scale;
        [SerializeField] private GesturePromptContent enter;
        [SerializeField] private GesturePromptContent move;
        [SerializeField] private GesturePromptContent exit;
        [SerializeField] private GesturePromptContent spend;
        [SerializeField] private GesturePromptContent stars;
        [SerializeField] private GesturePromptContent goal;
        [SerializeField] private SpaceShapingView shapingView;
        [SerializeField] private PuzzleMapView mapView;
        [SerializeField] private Vector3 scaleOffset = new(0.12f, 0.12f, 0f);
        [SerializeField] private Vector3 moveOffset = new(-0.12f, 0.12f, 0f);
        [SerializeField] private Vector3 enterOffset = new(0.12f, 0.12f, 0f);
        [SerializeField] private Vector3 exitOffset = new(0f, 0.22f, 0f);
        [SerializeField] private Vector3 spendOffset = new(-0.14f, 0.14f, 0f);
        [SerializeField] private Vector3 starsOffset = new(-0.14f, 0.02f, 0f);
        [SerializeField] private Vector3 goalOffset = new(0f, 0.14f, 0f);

        [Inject] private MemoryDataClient memoryDataClient;

        private bool spendPending;
        private bool spendRequested;

        public bool SpendShown { get; private set; }

        // The middle of the entropy post on the grid's meter edge, where Spend and Stars point and sit.
        private Vector3 MeterPost =>
            shapingView.transform.TransformPoint(
                shapingView.FieldCentre + FieldFrame.OnMeterEdge(shapingView.FieldHalf, 0f, 0f));

        public void Show(GestureKind kind)
        {
            if (IsLearned(kind))
                return;

            ShowAlways(kind);
        }

        public void ShowAlways(GestureKind kind)
        {
            if (pool.IsShowing(kind))
                return;

            pool.Show(Content(kind), Position(kind), Facing(kind), Subject(kind));
            if (kind == GestureKind.Spend)
                SpendShown = true;
        }

        // Spend waits for the first committed pinch, then for Scale to leave so the two never stack.
        public void RequestSpend()
        {
            if (spendRequested)
                return;

            spendRequested = true;
            spendPending = true;
        }

        public void HideAll()
        {
            spendRequested = false;
            spendPending = false;
            SpendShown = false;
            pool.HideAll();
        }

        private void LateUpdate()
        {
            for (int i = 0; i < KindCount; i++)
            {
                GestureKind kind = (GestureKind)i;
                if (pool.IsShowing(kind))
                    pool.Move(kind, Position(kind), Facing(kind), Subject(kind));
            }

            if (spendPending && !pool.IsShowing(GestureKind.Scale))
            {
                spendPending = false;
                Show(GestureKind.Spend);
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
                    return shapingView.CubePosition + FieldOffset(scaleOffset);
                case GestureKind.Move:
                    return shapingView.CubePosition + FieldOffset(moveOffset);
                case GestureKind.Exit:
                    return shapingView.CubePosition + FieldOffset(exitOffset);
                case GestureKind.Spend:
                    return MeterPost + FieldOffset(spendOffset);
                case GestureKind.Stars:
                    return MeterPost + FieldOffset(starsOffset);
                case GestureKind.Goal:
                    return shapingView.DestinationPosition + FieldOffset(goalOffset);
                default:
                    return mapView.SelectionCubePosition + MapOffset(enterOffset);
            }
        }

        // Spend and Stars stand upright beside the meter post (perpendicular to the floor), turned with
        // the field so they face the seat however the field is yawed.
        private Quaternion? Facing(GestureKind kind)
        {
            if (kind != GestureKind.Spend && kind != GestureKind.Stars)
                return null;

            return shapingView.transform.rotation;
        }

        private Vector3 FieldOffset(Vector3 offset) => shapingView.transform.TransformVector(offset);

        private Vector3 MapOffset(Vector3 offset) => mapView.transform.TransformVector(offset);

        private Vector3? Subject(GestureKind kind)
        {
            switch (kind)
            {
                case GestureKind.Scale:
                case GestureKind.Move:
                    return shapingView.CubePosition;
                case GestureKind.Enter:
                    return mapView.SelectionCubePosition;
                case GestureKind.Spend:
                case GestureKind.Stars:
                    return MeterPost;
                case GestureKind.Goal:
                    return shapingView.DestinationPosition;
                default:
                    return null;
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
                case GestureKind.Spend:
                    return spend;
                case GestureKind.Stars:
                    return stars;
                case GestureKind.Goal:
                    return goal;
                default:
                    return move;
            }
        }
    }
}
