namespace DigitalLove.Game.Spaceships
{
    public interface ISpaceshipHost
    {
        string Id { get; }
        bool HasRoute { get; }
        LoopEventArgs BuildLoopEventArgs();
        void NotifyLoopChanged();
        void NotifyLoopEditionClicked();
    }
}
