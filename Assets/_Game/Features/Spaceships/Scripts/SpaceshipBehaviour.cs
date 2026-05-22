using System.Collections.Generic;
using DigitalLove.FlowControl;
using DigitalLove.Game.Nodes;
using DigitalLove.Global;
using UnityEngine;

namespace DigitalLove.Game.Spaceships
{
    [RequireComponent(typeof(SpaceshipRefs))]
    public class SpaceshipBehaviour : MonoBehaviour, ISpaceshipHost
    {
        [SerializeField] private SpaceshipRefs refs;

        private StateMachine stateMachine;
        private SpaceshipRouteSession session;
        private SpaceshipIdleState idleState;
        private SpaceshipSelectingState selectingState;
        private SpaceshipRunningState runningState;

        private SpaceshipData data;
        private bool isInitialized;
        private LoopHandlers handlers;

        public bool IsInitialized => isInitialized;
        public DestinationSelector DestinationSelector => refs.destinationSelector;

        public string Id => data.id;
        public string HubId => data.hubId;
        public string ColorCode => data.colorCode;
        public HubBehaviour Hub => refs.destinationSelector.Hub;
        public bool IsActive => gameObject.activeInHierarchy;
        public bool HasRoute => isInitialized && session.HasDestinations &&
            (stateMachine.IsCurrentState<SpaceshipRunningState>() || stateMachine.IsCurrentState<SpaceshipSelectingState>());

        private void Awake()
        {
            if (refs == null)
                refs = GetComponent<SpaceshipRefs>();
        }

        public void Initialize()
        {
            if (isInitialized)
                return;

            isInitialized = true;
            session = new SpaceshipRouteSession(refs, this);
            session.ResetVisuals();

            stateMachine = new StateMachine();
            SpaceshipDestinationSelection destinationSelection = new SpaceshipDestinationSelection(refs, session);
            idleState = new SpaceshipIdleState(stateMachine, refs);
            selectingState = new SpaceshipSelectingState(stateMachine, refs, destinationSelection, this);
            selectingState.Init();
            runningState = new SpaceshipRunningState(stateMachine, refs, session, destinationSelection, this);
            stateMachine.Register(new IState[] { idleState, selectingState, runningState });
            stateMachine.SetCurrentState<SpaceshipIdleState>();
            ApplyLoopHandlers();
        }

        public void Configure(LoopHandlers loopHandlers)
        {
            handlers = loopHandlers;
            ApplyLoopHandlers();
        }

        private void ApplyLoopHandlers()
        {
            if (session != null)
                session.SetOnLoopComplete(handlers.Complete);
        }

        public void StartSelectingDestination() => stateMachine.SetCurrentState<SpaceshipSelectingState>();

        public void NotifyLoopChanged() => handlers.Changed?.Invoke(BuildLoopEventArgs());

        public void NotifyLoopEditionClicked()
        {
            MoveToHub();
            handlers.EditionClicked?.Invoke(BuildLoopEventArgs());
        }

        public void MoveToHub()
        {
            refs.grabbableWrapper.SetWorldPosition(Hub.SpawnPose.position);
            refs.grabZone.LookAtStationCenter(Hub.Position);
        }

        public LoopEventArgs BuildLoopEventArgs() => new()
        {
            spaceshipId = Id,
            destinationIds = session.GetDestinationIds(),
            colorCode = ColorCode,
            hubId = HubId
        };

        public void Spawn(SpaceshipData data, Color color, HubBehaviour hub)
        {
            this.data = data;

            refs.grabbableWrapper.SetWorldPosition(hub.SpawnPose.position);
            hub.ApplyRouteColor(color);
            refs.destinationSelector.Init(hub, color);
            session.SetRouteColor(color);
            refs.grabZone.SetColor(color);
            session.ClearDestinations();

            MoveToHub();

            this.SetActive(true);
            refs.routePanel.Show(Id, color, hub.transform.position);
        }

        public void Hide()
        {
            if (!isInitialized)
            {
                this.SetActive(false);
                return;
            }

            session.ClearDestinations();
            refs.routePanel.Hide();
            this.SetActive(false);
        }

        public void SetRoute(IReadOnlyList<PlanetBehaviour> destinations)
        {
            if (!isInitialized)
                Initialize();

            session.SetDestinations(destinations);
            stateMachine.SetCurrentState<SpaceshipRunningState>();
        }

        #region Debug

        public void Debug_SimulateGrabSelect()
        {
            if (!isInitialized)
                Initialize();

            if (stateMachine.IsCurrentState<SpaceshipIdleState>())
                idleState.Debug_SimulateGrabSelect();
            else if (stateMachine.IsCurrentState<SpaceshipSelectingState>())
                selectingState.Debug_SimulateGrabSelect();
            else if (stateMachine.IsCurrentState<SpaceshipRunningState>())
                runningState.Debug_SimulateGrabSelect();
            else
                Debug.LogWarning($"EditorDebug: Grab select ignored in state {stateMachine.CurrentRoute}.");
        }

        public void Debug_SimulateGrabRelease()
        {
            if (!isInitialized)
                return;

            if (stateMachine.IsCurrentState<SpaceshipSelectingState>())
                selectingState.Debug_SimulateGrabRelease();
            else if (stateMachine.IsCurrentState<SpaceshipRunningState>())
                runningState.Debug_SimulateGrabRelease();
            else
                Debug.LogWarning($"EditorDebug: Grab release ignored in state {stateMachine.CurrentRoute}.");
        }

        public void Debug_ConfirmDestination()
        {
            if (stateMachine.IsCurrentState<SpaceshipSelectingState>())
                selectingState.Debug_ConfirmDestination();
            else if (stateMachine.IsCurrentState<SpaceshipRunningState>())
                runningState.Debug_ConfirmDestination();
        }

        public void Debug_InvokeOnLoopEditionButtonClicked() => runningState.Debug_InvokeOnLoopEditionButtonClicked();

        public List<string> Debug_GetDestinationIds() => session.GetDestinationIds();

        #endregion
    }
}
