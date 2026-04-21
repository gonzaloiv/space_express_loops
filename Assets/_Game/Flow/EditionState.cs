using DigitalLove.FlowControl;
using DigitalLove.Game.Levels;
using DigitalLove.Game.Planets;
using UnityEngine;

namespace DigitalLove.Game.Flow
{
    public class EditionState : MonoState
    {
        [SerializeField] private LevelSetup levelSetup;
        [SerializeField] private PlanetsSpawner planetsSpawner;

        public override void Enter()
        {
            RoundData roundData = levelSetup.GetCurrent();
            planetsSpawner.Setup(roundData.planets);
        }

        public override void Exit()
        {

        }
    }
}