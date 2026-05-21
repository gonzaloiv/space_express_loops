using System;

namespace DigitalLove.Game.Spaceships
{
    public readonly struct LoopHandlers
    {
        public Action<LoopEventArgs> Changed { get; }
        public Action<LoopEventArgs> EditionClicked { get; }
        public Action<LoopCompleteEventArgs> Complete { get; }

        public LoopHandlers(
            Action<LoopEventArgs> changed = null,
            Action<LoopEventArgs> editionClicked = null,
            Action<LoopCompleteEventArgs> complete = null)
        {
            Changed = changed;
            EditionClicked = editionClicked;
            Complete = complete;
        }
    }
}
