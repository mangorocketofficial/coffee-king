using CoffeeKing.Orders;

namespace CoffeeKing.Flow
{
    public sealed class DrinkFlowController
    {
        private static readonly DrinkFlowState[] AmericanoSequence =
        {
            DrinkFlowState.MoveToGrinder,
            DrinkFlowState.Tamping,
            DrinkFlowState.PortafilterLocking,
            DrinkFlowState.Extracting,
            DrinkFlowState.CupSetup,
            DrinkFlowState.PourShotToCup,
            DrinkFlowState.PourIngredient,
            DrinkFlowState.Serving,
            DrinkFlowState.Scoring
        };

        private int sequenceIndex;

        public DrinkRecipe CurrentRecipe { get; private set; }
        public DrinkFlowState CurrentState { get; private set; } = DrinkFlowState.WaitingForOrder;

        public void StartDrink(DrinkRecipe recipe)
        {
            CurrentRecipe = recipe;
            sequenceIndex = 0;
            CurrentState = AmericanoSequence[sequenceIndex];
        }

        public DrinkFlowState Advance()
        {
            if (CurrentRecipe == null)
            {
                CurrentState = DrinkFlowState.WaitingForOrder;
                return CurrentState;
            }

            sequenceIndex++;
            if (sequenceIndex >= AmericanoSequence.Length)
            {
                CurrentState = DrinkFlowState.Scoring;
                return CurrentState;
            }

            CurrentState = AmericanoSequence[sequenceIndex];
            return CurrentState;
        }

        public void Reset()
        {
            CurrentRecipe = null;
            sequenceIndex = 0;
            CurrentState = DrinkFlowState.WaitingForOrder;
        }
    }
}
