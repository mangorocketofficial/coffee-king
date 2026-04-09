namespace CoffeeKing.Flow
{
    public enum DrinkFlowState
    {
        WaitingForOrder,
        MoveToGrinder,
        Grinding,
        Tamping,
        PortafilterLocking,
        Extracting,
        CupSetup,
        PourShotToCup,
        PourIngredient,
        Serving,
        Scoring
    }
}
