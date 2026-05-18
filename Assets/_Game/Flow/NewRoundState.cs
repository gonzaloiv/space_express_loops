using UnityEngine;
using DigitalLove.FlowControl;
using DigitalLove.DataAccess;
using DigitalLove.Game.Levels;
using Reflex.Attributes;
using DigitalLove.Game.Persistence;
using DigitalLove.Casual.Analytics;
using DigitalLove.Game.TTS;

namespace DigitalLove.Game.Flow
{
    public class NewRoundState : MonoState
    {
        [SerializeField] private MonoState editionState;
        [SerializeField] private RoundSelector roundSelector;
        [SerializeField] private LevelContainer levelContainer;
        [SerializeField] private ProgressionEventsHelper progressionEventsHelper;
        [SerializeField] private TTSHelper ttsHelper;

        [Inject] private MemoryDataClient memoryDataClient;

        public override void Enter()
        {
            GameSnapshot gameSnapshot = memoryDataClient.Get<GameSnapshot>();

            gameSnapshot.IncreaseRoundIndex();
            roundSelector.SetCurrentRound(gameSnapshot.roundIndex);
            levelContainer.SpawnRound(roundSelector.CurrentRound, gameSnapshot);
            gameSnapshot.RecalculateLettersRequiredForRound(roundSelector.CurrentRound.lettersIncreaseMultiplier);

            progressionEventsHelper.SendLevelCompleteEvent(roundSelector.CurrentRound.id, score: gameSnapshot.CurrentLetters);

            SayRoundIntro();
        }

        private void SayRoundIntro()
        {
            ttsHelper.SetInFrontOfCameraOrDefault(false);
            if (ttsHelper.HasIntro(roundSelector.CurrentRound))
            {
                ttsHelper.SayRoundIntro(roundSelector.CurrentRound, ToEditionState);
            }
            else
            {
                ttsHelper.SayRandomTipIfAvailable(ToEditionState);
            }
        }

        private void ToEditionState()
        {
            parent.SetCurrentState(editionState.RouteId);
        }

        public override void Exit()
        {

        }
    }
}