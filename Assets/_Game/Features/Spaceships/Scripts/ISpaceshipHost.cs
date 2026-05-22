namespace DigitalLove.Game.Spaceships
{
    public interface ISpaceshipHost
    {
        string Id { get; }
        LoopEventArgs BuildLoopEventArgs();
        void NotifyLoopChanged();
        void NotifyLoopEditionClicked();
    }
}
