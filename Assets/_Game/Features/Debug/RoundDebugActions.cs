using DigitalLove.FlowControl;
using DigitalLove.Global;
using Reflex.Attributes;
using UnityEngine;

namespace DigitalLove.Game.DebugActions
{
    public class RoundDebugActions : MonoBehaviour
    {
        [SerializeField] private MonoState editionState;
        [SerializeField] private MonoState newRoundState;

        [Inject] private StateMachine stateMachine;

        #region Debug

        [Button]
        public void Debug_CompleteCurrentRound()
        {
            if (newRoundState == null)
            {
                Debug.LogWarning("RoundDebug: newRoundState is not assigned.");
                return;
            }

            if (stateMachine == null)
            {
                Debug.LogWarning("RoundDebug: StateMachine is not available.");
                return;
            }

            if (editionState != null && !stateMachine.IsCurrentState(editionState.RouteId))
            {
                Debug.LogWarning(
                    $"RoundDebug: Only available during {editionState.RouteId}; current: {stateMachine.CurrentRoute}");
                return;
            }

            stateMachine.SetCurrentState(newRoundState.RouteId);
        }

        #endregion
    }
}
