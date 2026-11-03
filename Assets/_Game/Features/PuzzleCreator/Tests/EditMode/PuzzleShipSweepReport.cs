namespace DigitalLove.Game.SpaceShaping.Creator
{
    public struct PuzzleShipSweepReport
    {
        public int Fields;
        public int Baked;
        public int FieldsWithTwo;
        public int FieldsWithTwoFixed;
        public int BestCount;
        public int BestFixedCount;
        public float BestMargin;

        public override string ToString() =>
            $"Multi-ship sweep: fields={Fields} baked={Baked} "
            + $"fields>=2={FieldsWithTwo} fields>=2(field-driven)={FieldsWithTwoFixed} "
            + $"best={BestCount} best(field-driven)={BestFixedCount} bestMargin={BestMargin:0.0000}";
    }
}
