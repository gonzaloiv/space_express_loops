using UnityEngine;
using DigitalLove.FlowControl;
using DigitalLove.DataAccess;
using DigitalLove.Game.Levels;
using Reflex.Attributes;
using DigitalLove.Game.Persistence;
using DigitalLove.Casual.Analytics;
using DigitalLove.Game.TTS;
using DigitalLove.Global;

namespace DigitalLove.Game.Flow
{
    public class NewRoundState : MonoState
    {
        [SerializeField] private MonoState editionState;
        [SerializeField] private RoundSelector roundSelector;
        [SerializeField] private LevelContainer levelContainer;
        [SerializeField] private ProgressionEventsHelper progressionEventsHelper;
        [SerializeField] private TTSHelper ttsHelper;
        [SerializeField] private FloatValue gameSpeed;

        [Inject] private MemoryDataClient memoryDataClient;

        public override void Enter()
        {
            GameSnapshot gameSnapshot = memoryDataClient.Get<GameSnapshot>();

            gameSnapshot.IncreaseRoundIndex();
            roundSelector.SetCurrentRound(gameSnapshot.roundIndex);
            levelContainer.SpawnRound(roundSelector.CurrentRound, gameSnapshot);
            levelContainer.PlanetsSpawner.UnlockPlanetStores();

            gameSnapshot.RecalculateLettersRequiredForRound(roundSelector.CurrentRound.lettersIncreaseMultiplier / gameSpeed.value);

            if (roundSelector.CurrentRound.resetsLetters)
                levelContainer.ResetLetters();

            progressionEventsHelper.SendLevelCompleteEvent(roundSelector.CurrentRound.id, score: gameSnapshot.CurrentLetters);
            gameSnapshot.ResetLettersForNewRound();

            parent.SetCurrentState(editionState.RouteId);
        }

        public override void Exit()
        {

        }
    }
}