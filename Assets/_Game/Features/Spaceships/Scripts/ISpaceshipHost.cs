using DigitalLove.Game.Planets;

namespace DigitalLove.Game.Spaceships
{
    public interface ISpaceshipHost
    {
        string Id { get; }
        string ColorCode { get; }
        string HubId { get; }
        HubBehaviour Hub { get; }
        LoopEventArgs BuildLoopEventArgs();
        void NotifyLoopChanged();
        void NotifyLoopEditionClicked();
        void MoveToHub();
    }
}
