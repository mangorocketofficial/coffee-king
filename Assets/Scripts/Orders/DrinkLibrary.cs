using System.Collections.Generic;
using CoffeeKing.Core;

namespace CoffeeKing.Orders
{
    public static class DrinkLibrary
    {
        public static IReadOnlyList<DrinkRecipe> CreateRebootPhaseOne(GameConfig config)
        {
            return new[]
            {
                new DrinkRecipe(
                    "iced_americano",
                    "Iced Americano",
                    IngredientType.Water,
                    config.CupEspressoColor,
                    config.CupEspressoColor,
                    RecipeStep.MoveToGrinder,
                    RecipeStep.Grinding,
                    RecipeStep.Tamping,
                    RecipeStep.PortafilterLock,
                    RecipeStep.Extraction,
                    RecipeStep.CupSetup,
                    RecipeStep.PourShot,
                    RecipeStep.Ingredient,
                    RecipeStep.Serve),
            };
        }
    }
}
