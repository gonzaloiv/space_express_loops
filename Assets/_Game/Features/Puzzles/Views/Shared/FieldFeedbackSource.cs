using System;
using UnityEngine;

namespace DigitalLove.Game.SpaceShaping
{
    // The feedback-facing face of the field view. Feedback binds to this instead of the composition
    // root, so the feedback layer never depends on the view that owns it.
    public abstract class FieldFeedbackSource : MonoBehaviour
    {
        public abstract event Action PinchCommitted;
        public abstract event Action<PuzzleReadout> ReadoutChanged;
        public abstract PuzzleReadout Readout { get; protected set; }
        public abstract bool IsScaling { get; }
        public abstract float FieldHalf { get; protected set; }
        public abstract Vector3 FieldCentre { get; protected set; }
    }
}
