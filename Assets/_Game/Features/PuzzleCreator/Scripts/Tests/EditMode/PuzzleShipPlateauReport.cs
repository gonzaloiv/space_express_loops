namespace DigitalLove.Game.SpaceShaping.Creator
{
    public struct PuzzleShipPlateauReport
    {
        public int Fields;
        public int Baked;
        public int FieldsWithPlateau;
        public float SumWidth;
        public float MinWidth;
        public float BestWidth;
        public float Range;

        public float MeanWidth => Baked > 0 ? SumWidth / Baked : 0f;

        public override string ToString() =>
            $"Plateau sweep: fields={Fields} baked={Baked} withPlateau={FieldsWithPlateau} "
            + $"width min={MinWidth:0.00} mean={MeanWidth:0.00} best={BestWidth:0.00} of range {Range:0.00}";
    }
}
