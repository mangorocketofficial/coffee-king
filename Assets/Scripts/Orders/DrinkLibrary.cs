using System.Collections.Generic;
using CoffeeKing.Core;

namespace CoffeeKing.Orders
{
    public static class DrinkLibrary
    {
        public static IReadOnlyList<DrinkRecipe> CreateHotDrinksSet(GameConfig config)
        {
            return new[]
            {
                new DrinkRecipe(
                    "iced_americano",
                    "Iced Americano",
                    CupType.Plastic,
                    IngredientType.Water,
                    config.CupEspressoColor,
                    config.CupEspressoColor,
                    4500,
                    RecipeStep.MoveToGrinder,
                    RecipeStep.Tamping,
                    RecipeStep.PortafilterLock,
                    RecipeStep.Extraction,
                    RecipeStep.CupSetup,
                    RecipeStep.PourShot,
                    RecipeStep.Ingredient,
                    RecipeStep.Lid,
                    RecipeStep.Serve),
                new DrinkRecipe(
                    "iced_cafe_latte",
                    "Iced Cafe Latte",
                    CupType.Plastic,
                    IngredientType.Milk,
                    config.CupEspressoColor,
                    config.CupLatteColor,
                    5500,
                    RecipeStep.MoveToGrinder,
                    RecipeStep.Tamping,
                    RecipeStep.PortafilterLock,
                    RecipeStep.Extraction,
                    RecipeStep.CupSetup,
                    RecipeStep.PourShot,
                    RecipeStep.Ingredient,
                    RecipeStep.Lid,
                    RecipeStep.Serve),
                new DrinkRecipe(
                    "hot_americano",
                    "Hot Americano",
                    CupType.Hot,
                    IngredientType.Water,
                    config.CupEspressoColor,
                    config.CupEspressoColor,
                    4500,
                    RecipeStep.MoveToGrinder,
                    RecipeStep.Tamping,
                    RecipeStep.PortafilterLock,
                    RecipeStep.Extraction,
                    RecipeStep.CupSetup,
                    RecipeStep.PourShot,
                    RecipeStep.Ingredient,
                    RecipeStep.Lid,
                    RecipeStep.Serve),
                new DrinkRecipe(
                    "hot_cafe_latte",
                    "Hot Cafe Latte",
                    CupType.Hot,
                    IngredientType.Milk,
                    config.CupEspressoColor,
                    config.CupLatteColor,
                    5500,
                    RecipeStep.MoveToGrinder,
                    RecipeStep.Tamping,
                    RecipeStep.PortafilterLock,
                    RecipeStep.Extraction,
                    RecipeStep.SteamMilk,
                    RecipeStep.CupSetup,
                    RecipeStep.PourShot,
                    RecipeStep.Ingredient,
                    RecipeStep.Lid,
                    RecipeStep.Serve)
            };
        }
    }
}
