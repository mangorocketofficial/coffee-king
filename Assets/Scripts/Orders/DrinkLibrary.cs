using System.Collections.Generic;
using CoffeeKing.Core;

namespace CoffeeKing.Orders
{
    public static class DrinkLibrary
    {
        public static IReadOnlyList<DrinkRecipe> CreatePhaseOneDemo(GameConfig config)
        {
            return new[]
            {
                new DrinkRecipe(
                    "americano",
                    "Americano",
                    config.CupEspressoColor,
                    config.CupEspressoColor,
                    RecipeStep.Portafilter,
                    RecipeStep.Extraction,
                    RecipeStep.Serve),
                new DrinkRecipe(
                    "caffe_latte",
                    "Caffe Latte",
                    config.CupEspressoColor,
                    config.CupLatteColor,
                    RecipeStep.Portafilter,
                    RecipeStep.Extraction,
                    RecipeStep.SteamMilk,
                    RecipeStep.Serve),
                new DrinkRecipe(
                    "vanilla_latte",
                    "Vanilla Latte",
                    config.CupEspressoColor,
                    config.CupVanillaColor,
                    RecipeStep.Portafilter,
                    RecipeStep.Extraction,
                    RecipeStep.Syrup,
                    RecipeStep.SteamMilk,
                    RecipeStep.Serve)
            };
        }
    }
}
