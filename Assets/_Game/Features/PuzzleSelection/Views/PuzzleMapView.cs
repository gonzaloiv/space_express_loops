using System;
using DigitalLove.Game.SpaceShaping;
using DigitalLove.Game.SpaceShaping.Cube;
using DigitalLove.Global;
using UnityEngine;

namespace DigitalLove.Game.PuzzleSelection
{
    // The selection map. The selection cube is the puzzle's zone cube: pinch inside it to grow the
    // selected diorama and enter, grab its frame to slide it over the map and snap to another node.
    public class PuzzleMapView : MonoBehaviour
    {
        private const float MoveEpsilonSqr = 0.000001f;

        [SerializeField] private Transform mapRoot;
        [SerializeField] private RunSlotView selectedSlot;
        [SerializeField] private CubeView cube;
        [Tooltip("Edge of the selection cube, in map units.")]
        [SerializeField, Min(0.01f)] private float cubeSize = 0.119f;
        [SerializeField] private MapStarMeter starMeter;
        [SerializeField] private JobCardPanel contractCard;
        [SerializeField] private float viewRadius = 0.5f;
        [Tooltip("Cube scale at which a pinch commits to entering the job.")]
        [SerializeField, Min(1.1f)] private float enterAt = 1.8f;
        [Tooltip("Share of the map that fades out by the time the pinch reaches enterAt; the enter animation fades the rest.")]
        [SerializeField, Range(0f, 1f)] private float pinchFade = 0.6f;
        [SerializeField] private float enterSecs = 0.5f;
        [SerializeField] private float enterScale = 3.6f;
        [Tooltip("How far toward enterAt a pinch may grow a locked node before it refuses.")]
        [SerializeField, Range(0f, 1f)] private float lockedZoomLimit = 0.06f;
        [SerializeField, Min(0.1f)] private float pulseSecs = 2.4f;
        [SerializeField, Range(0f, 1f)] private float pulseDepth = 0.35f;
        [SerializeField, Range(0f, 1f)] private float lockedBrightness = 0.55f;
        [SerializeField] private SelectionCubePalette selectionCubeColors;
        [SerializeField] private ColorValue routeColor;
        [SerializeField, Range(0f, 1f)] private float routeAlpha = 0.35f;
        [SerializeField] private LineRenderer[] routeSegments;
        [Tooltip("Show each preview's zone frames on the map.")]
        [SerializeField] private bool showFrames;
        [SerializeField] private bool showAll;

        private ViewPool<RunSlotView> fields;
        private MapPose pose;
        private MapCube mapCube;
        private MapRouteLine route;
        private PuzzleMap map;
        private MapNodeTint tint;
        private MapSelectionReadout readout;
        private MapFieldRenderer fieldView;
        private MapEnter enter;
        private bool wasScaling;
        private Vector3 lastCubePosition;

        public event Action EnterRequested;
        public event Action EnterStarted;
        public event Action SelectionChanged;

        public Vector3 SelectionCubePosition => mapCube.Position;

        public bool ShowAll => showAll;

        // The selected field's bottom-left-back corner and rotation, in the Origin's space. Read at
        // rest: the console pair frames this, not the live breathing/zoomed diorama.
        public Vector3 FieldCorner
        {
            get
            {
                float half = selectedSlot.FieldHalfMap;
                Vector3 corner = selectedSlot.FieldRotationMap * new Vector3(-half, -half, half);
                return transform.parent.InverseTransformPoint(
                    mapRoot.TransformPoint(selectedSlot.FieldCentreMap + corner));
            }
        }

        public Quaternion FieldRotation =>
            Quaternion.Inverse(transform.parent.rotation)
            * (mapRoot.rotation * selectedSlot.FieldRotationMap);

        private void Awake()
        {
            fields = new ViewPool<RunSlotView>(selectedSlot, mapRoot, "Map Field", null);
            mapCube = new MapCube(cube, cubeSize, enterAt, lockedZoomLimit);
            pose = new MapPose(mapRoot, selectedSlot, cube.transform, fields, viewRadius);
            pose.ShowAll = showAll;
            tint = new MapNodeTint(
                selectionCubeColors, lockedBrightness, pulseSecs, pulseDepth);
            readout = new MapSelectionReadout(contractCard);
            fieldView = new MapFieldRenderer(selectedSlot, fields, tint, readout, showFrames);
            enter = new MapEnter(selectedSlot, enterSecs, enterScale);
        }

        private void OnEnable() => mapCube.Moved += OnCubeMoved;

        private void OnDisable() => mapCube.Moved -= OnCubeMoved;

        private void Update()
        {
            if (map == null || Time.timeScale <= 0f)
                return;

            float deltaSecs = Time.unscaledDeltaTime;
            fieldView.TickPulse(deltaSecs);
            TrackCube(deltaSecs);
            if (map == null)
                return; // entering a job hid the map mid-frame

            route.Tick(
                map, selectedSlot, fields,
                new MapFade(mapCube.Position, pose.FadeNear, pose.FadeFar));
        }

        public void Show(PuzzleMap map)
        {
            this.map = map;
            gameObject.SetActive(true);

            wasScaling = false;
            enter.Reset();
            route = new MapRouteLine(routeSegments, routeColor, routeAlpha, map.Count);
            pose.ShowAll = showAll;
            tint.Reset();
            fieldView.Render(map);
            starMeter.Show(ChapterStars.From(map));
            pose.SetPose(Focus);
            mapCube.Show(SunFocus, map.CanEnter);
            lastCubePosition = mapCube.Position;
            pose.SetReach(1f);
        }

        public void SimulateSwipe(int delta)
        {
            if (map == null)
                return;

            if (map.SelectNext(delta))
                RenderFields();
            SnapCube();
        }

        public void SetShowAll(bool value)
        {
            showAll = value;
            pose.ShowAll = value;
            pose.ApplyFade();
        }

        public void Refuse()
        {
            enter.Reset();
            readout.Show(map.Selected);
            selectedSlot.PlayRefusal();
            mapCube.Refuse();
        }

        public void Hide()
        {
            enter.Reset();
            wasScaling = false;
            route = null;
            map = null;
            readout.Reset();
            selectedSlot.SetZoom(1f);
            mapCube.Hide();
            fieldView.Hide();
            gameObject.SetActive(false);
        }

        private void TrackCube(float deltaSecs)
        {
            if (enter.IsActive)
            {
                bool entered = enter.Tick(deltaSecs);
                pose.SetReach(Mathf.Lerp(1f - pinchFade, 0f, enter.Progress));
                if (entered)
                    EnterRequested?.Invoke();
                return;
            }

            if (mapCube.IsScaling)
            {
                TrackScale();
                return;
            }

            if (wasScaling)
                ReleaseScale();

            TrackMove();
        }

        private void TrackScale()
        {
            wasScaling = true;
            selectedSlot.SetZoom(mapCube.Scale);
            pose.SetReach(1f - pinchFade * Mathf.InverseLerp(1f, enterAt, mapCube.Scale));
            if (map.CanEnter && mapCube.ReachedEnter)
                StartEnter();
        }

        private void StartEnter()
        {
            wasScaling = false;
            float scale = mapCube.Scale;
            mapCube.Lock();
            enter.Start(scale);
            EnterStarted?.Invoke();
        }

        private void ReleaseScale()
        {
            wasScaling = false;
            bool refused = !map.CanEnter && mapCube.TriedToScale;
            mapCube.Release();
            selectedSlot.SetZoom(1f);
            pose.SetReach(1f);
            if (refused)
                Refuse();
        }

        // While the frame is dragged, the nearest node previews and the cull follows the cube.
        private void TrackMove()
        {
            Vector3 position = mapCube.Position;
            if ((position - lastCubePosition).sqrMagnitude < MoveEpsilonSqr)
                return;

            lastCubePosition = position;
            fieldView.PreviewNearest(map, mapRoot, position);
            pose.ApplyFade();
        }

        private void OnCubeMoved()
        {
            if (map == null)
                return;

            int nearest = MapSnap.Nearest(map, mapRoot, mapCube.Position);
            if (nearest >= 0 && nearest != map.SelectedIndex && map.Select(nearest))
            {
                RenderFields();
                SelectionChanged?.Invoke();
            }
            else
                readout.Show(map.Selected);

            SnapCube();
        }

        private void SnapCube()
        {
            mapCube.SnapTo(SunFocus);
            lastCubePosition = mapCube.Position;
            pose.ApplyFade();
        }

        private Vector3 Focus => map.Position(map.SelectedIndex);

        private Vector3 SunFocus => selectedSlot.SunMapPosition;

        private void RenderFields()
        {
            fieldView.Render(map);
            starMeter.Show(ChapterStars.From(map));
            pose.PlaceSelected(Focus);
            mapCube.SetEnterable(map.CanEnter);
        }
    }
}
