using CoffeeKing.Orders;
using System.Collections.Generic;

namespace CoffeeKing.Flow
{
    public sealed class DrinkFlowController
    {
        private DrinkFlowState[] currentSequence = { DrinkFlowState.WaitingForOrder };
        private int sequenceIndex;

        public DrinkRecipe CurrentRecipe { get; private set; }
        public DrinkFlowState CurrentState { get; private set; } = DrinkFlowState.WaitingForOrder;

        public void StartDrink(DrinkRecipe recipe)
        {
            CurrentRecipe = recipe;
            currentSequence = BuildSequence(recipe);
            sequenceIndex = 0;
            CurrentState = currentSequence[sequenceIndex];
        }

        public DrinkFlowState Advance()
        {
            if (CurrentRecipe == null)
            {
                CurrentState = DrinkFlowState.WaitingForOrder;
                return CurrentState;
            }

            sequenceIndex++;
            if (sequenceIndex >= currentSequence.Length)
            {
                CurrentState = DrinkFlowState.Scoring;
                return CurrentState;
            }

            CurrentState = currentSequence[sequenceIndex];
            return CurrentState;
        }

        public void Reset()
        {
            CurrentRecipe = null;
            currentSequence = new[] { DrinkFlowState.WaitingForOrder };
            sequenceIndex = 0;
            CurrentState = DrinkFlowState.WaitingForOrder;
        }

        private static DrinkFlowState[] BuildSequence(DrinkRecipe recipe)
        {
            var states = new List<DrinkFlowState>();
            for (var index = 0; index < recipe.Steps.Length; index++)
            {
                switch (recipe.Steps[index])
                {
                    case RecipeStep.MoveToGrinder:
                        states.Add(DrinkFlowState.MoveToGrinder);
                        break;
                    case RecipeStep.Grinding:
                        break;
                    case RecipeStep.Tamping:
                        states.Add(DrinkFlowState.Tamping);
                        break;
                    case RecipeStep.PortafilterLock:
                        states.Add(DrinkFlowState.PortafilterLocking);
                        break;
                    case RecipeStep.Extraction:
                        states.Add(DrinkFlowState.Extracting);
                        break;
                    case RecipeStep.SteamMilk:
                        states.Add(DrinkFlowState.SteamMilk);
                        break;
                    case RecipeStep.CupSetup:
                        states.Add(DrinkFlowState.CupSetup);
                        break;
                    case RecipeStep.PourShot:
                        states.Add(DrinkFlowState.PourShotToCup);
                        break;
                    case RecipeStep.Ingredient:
                        states.Add(DrinkFlowState.PourIngredient);
                        break;
                    case RecipeStep.Lid:
                        states.Add(DrinkFlowState.Lid);
                        break;
                    case RecipeStep.Serve:
                        states.Add(DrinkFlowState.Serving);
                        break;
                }
            }

            states.Add(DrinkFlowState.Scoring);
            return states.ToArray();
        }
    }
}
