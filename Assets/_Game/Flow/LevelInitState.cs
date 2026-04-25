using DigitalLove.FlowControl;
using DigitalLove.Game.Levels;
using UnityEngine;

namespace DigitalLove.Game.Flow
{
    public class LevelInitState : MonoState
    {
        [SerializeField] private LevelContainer levelContainer;

        public override void Enter()
        {
            levelContainer.Setup();
            parent.SetCurrentState<EditionState>();
        }

        public override void Exit()
        {

        }
    }

}
