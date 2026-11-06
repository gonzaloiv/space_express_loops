using System;
using DigitalLove.Game.SpaceShaping;
using DigitalLove.Global;
using DigitalLove.Input;
using TMPro;
using UnityEngine;

namespace DigitalLove.Game.PuzzleSelection
{
    public class PuzzleMapView : MonoBehaviour
    {
        [SerializeField] private Transform mapRoot;
        [SerializeField] private RunSlotView selectedSlot;
        [SerializeField] private GameObject selectionCube;
        [SerializeField] private Renderer selectionCubeRenderer;
        [SerializeField] private TMP_Text selectionLabel;
        [SerializeField] private JobCardPanel contractCard;
        [SerializeField] private float viewRadius = 0.5f;
        [SerializeField] private float panSecs = 0.25f;
        [SerializeField, Min(0.1f)] private float panPromptSecs = 5f;
        [SerializeField] private float maxZoom = 2.2f;
        [SerializeField] private float overviewScale = 0.55f;
        [SerializeField] private float enterSecs = 0.5f;
        [SerializeField] private float enterScale = 3.6f;
        [SerializeField, Range(0f, 1f)] private float lockedZoomLimit = 0.06f;
        [SerializeField, Min(0.1f)] private float pulseSecs = 2.4f;
        [SerializeField, Range(0f, 1f)] private float pulseDepth = 0.35f;
        [SerializeField, Range(0f, 1f)] private float lockedBrightness = 0.55f;
        [SerializeField] private SelectionCubePalette selectionCubeColors;
        [SerializeField] private ColorValue routeColor;
        [SerializeField, Range(0f, 1f)] private float routeAlpha = 0.35f;
        [SerializeField] private LineRenderer[] routeSegments;
        [SerializeField] private bool showAll;

        private ViewPool<RunSlotView> fields;
        private RunCarouselGesture gesture;
        private MapPose pose;
        private MapZoom zoom;
        private MapOverview overview;
        private MapRouteLine route;
        private PuzzleMap map;
        private MapNodeTint tint;
        private MapSelectionReadout readout;
        private MapFieldRenderer fieldView;
        private bool wasDragging;
        private MapEnter enter;

        public event Action EnterRequested;

        public event Action Panned;

        private MapPanWatch panWatch;

        public MapPanWatch PanWatch
        {
            get
            {
                if (panWatch == null)
                {
                    panWatch = new MapPanWatch(panPromptSecs);
                    panWatch.Panned += OnPanned;
                }

                return panWatch;
            }
        }

        public Vector3 SelectionCubePosition => selectionCube.transform.position;

        private void Update()
        {
            float deltaSecs = Time.unscaledDeltaTime;
            PanWatch.Tick(deltaSecs, map != null);
            pose.Tick(deltaSecs);
            fieldView.TickPulse(deltaSecs);
            if (route != null && map != null)
            {
                route.Update(
                    map, selectedSlot, Focus, fields,
                    selectionCube.transform.position, pose.FadeNear, pose.FadeFar);
            }
        }

        private void Awake()
        {
            fields = new ViewPool<RunSlotView>(selectedSlot, mapRoot, "Map Field", null);
            gesture = new RunCarouselGesture(CarouselGestureSettings.Default);
            pose = new MapPose(
                mapRoot, selectedSlot.transform, selectionCube.transform, fields, viewRadius);
            pose.ShowAll = showAll;
            overview = new MapOverview(mapRoot, pose, overviewScale);
            SelectionCubeLens lens = new(
                selectionCubeRenderer, selectionCube.transform, selectionCubeColors);
            zoom = new MapZoom(selectedSlot, lens, maxZoom);
            tint = new MapNodeTint(
                selectionCubeColors, lockedBrightness, pulseSecs, pulseDepth);
            readout = new MapSelectionReadout(selectionLabel, contractCard);
            fieldView = new MapFieldRenderer(selectedSlot, fields, tint, readout, zoom);
            enter = new MapEnter(zoom, enterSecs, enterScale);
        }

        private void OnPanned() => Panned?.Invoke();

        public void Show(PuzzleMap map)
        {
            this.map = map;
            gameObject.SetActive(true);

            wasDragging = false;
            PanWatch.Reset();
            enter.Reset();
            overview.Reset(showAll);
            route = new MapRouteLine(routeSegments, routeColor, routeAlpha, map.Count);
            pose.ShowAll = showAll;
            tint.Reset();
            RenderFields();
            pose.SetPose(SunFocus, Focus);
            selectionCube.SetActive(true);
        }

        public bool ShowAll => showAll;

        public void SimulateSwipe(int delta)
        {
            if (map == null)
                return;

            if (map.SelectNext(delta))
                RenderFields();
            AnimateTo(Focus);
        }

        public void SetShowAll(bool value)
        {
            showAll = value;
            pose.ShowAll = value;
            pose.ApplyFade();
        }

        public void ApplyGesture(TwoHandGestureState state)
        {
            if (map == null)
                return;

            PanWatch.SetGestureActive(state.IsActive);

            if (enter.IsActive)
            {
                if (!state.IsActive)
                    enter.Cancel(map.Selected.State);
                else if (enter.Tick(map.Selected.State, Time.unscaledDeltaTime))
                    EnterRequested?.Invoke();
                return;
            }

            CarouselGestureOutcome outcome = gesture.Update(state);
            overview.Apply(outcome.OverviewProgress, showAll, SunFocus, Focus);
            float zoomProgress = map.CanEnter
                ? outcome.ZoomProgress
                : Mathf.Min(outcome.ZoomProgress, lockedZoomLimit);
            zoom.Update(state.IsActive, map.Selected.State, zoomProgress);

            if (outcome.Enter)
            {
                StartEnter(outcome);
                return;
            }

            if (outcome.IsDragging)
            {
                ApplyDrag(outcome);
                return;
            }

            SnapAfterDrag();
        }

        private void StartEnter(CarouselGestureOutcome outcome)
        {
            if (!map.CanEnter)
            {
                Refuse();
                return;
            }

            enter.Start(Mathf.Lerp(1f, maxZoom, outcome.ZoomProgress));
            pose.StopPan();
        }

        private void ApplyDrag(CarouselGestureOutcome outcome)
        {
            if (!wasDragging)
                pose.BeginDrag();

            pose.ApplyDrag(outcome.DragOffset);
            fieldView.PreviewNearest(map, mapRoot, selectionCube.transform.position);
            wasDragging = true;
        }

        private void SnapAfterDrag()
        {
            if (!wasDragging)
                return;

            wasDragging = false;
            int nearest = MapSnap.Nearest(
                map, mapRoot, selectionCube.transform.position);
            if (nearest >= 0 && nearest != map.SelectedIndex && map.Select(nearest))
            {
                RenderFields();
                PanWatch.NotifyPanned();
            }
            AnimateTo(Focus);
        }

        public void Refuse()
        {
            gesture.Reset();
            wasDragging = false;
            enter.Reset();
            overview.Reset(showAll);
            readout.Show(map.Selected);
            zoom.Reset(map.Selected.State);
            selectedSlot.PlayRefusal();
        }

        public void Hide()
        {
            pose.StopPan();
            PanWatch.Reset();
            wasDragging = false;
            enter.Reset();
            overview.Reset(showAll);
            route = null;
            gesture.Reset();
            map = null;
            readout.Reset();
            zoom.Clear();
            selectionCube.SetActive(false);
            fieldView.Hide();
            gameObject.SetActive(false);
        }

        private Vector3 Focus => map.Position(map.SelectedIndex);

        private Vector3 SunFocus => Focus + selectedSlot.SunLocalPosition;

        private void RenderFields()
        {
            fieldView.Render(map);
            pose.ApplyFade();
        }

        private void AnimateTo(Vector3 focus)
        {
            zoom.Reset(map.Selected.State);
            pose.BeginPan(SunFocus, Focus, panSecs);
        }
    }
}
