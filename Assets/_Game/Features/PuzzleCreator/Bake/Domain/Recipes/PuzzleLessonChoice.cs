namespace DigitalLove.Game.SpaceShaping.Creator
{
    public static class PuzzleLessonChoice
    {
        public static IPuzzleLesson For(PuzzleBrief brief)
        {
            if (brief.IsMulti)
            {
                if (brief.Relation == PuzzleShipRelation.FanOut)
                    return brief.Mechanism == PuzzleMechanism.Curve
                        ? new PuzzleFanOutCurveLesson()
                        : null;
                if (brief.Mechanism == PuzzleMechanism.Clear)
                    return new PuzzleFleetClearLesson();
                if (brief.Mechanism == PuzzleMechanism.Curve)
                    return new PuzzleFleetCurveLesson();
                return null;
            }

            if (brief.Mechanism == PuzzleMechanism.Clear)
                return new PuzzleClearLesson();
            if (brief.Mechanism == PuzzleMechanism.Curve)
                return new PuzzleCurveLesson();
            return null;
        }
    }
}
