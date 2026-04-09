using UnityEngine;

namespace CoffeeKing.Orders
{
    public enum RecipeStep
    {
        MoveToGrinder,
        Grinding,
        Tamping,
        PortafilterLock,
        Extraction,
        CupSetup,
        PourShot,
        Ingredient,
        Lid,
        Serve,
        SteamMilk,
        Syrup
    }

    public enum IngredientType
    {
        Water,
        Milk
    }

    public sealed class DrinkRecipe
    {
        public DrinkRecipe(
            string id,
            string displayName,
            IngredientType ingredientType,
            Color baseCupColor,
            Color finalCupColor,
            params RecipeStep[] steps)
        {
            Id = id;
            DisplayName = displayName;
            IngredientType = ingredientType;
            BaseCupColor = baseCupColor;
            FinalCupColor = finalCupColor;
            Steps = steps;
        }

        public string Id { get; }
        public string DisplayName { get; }
        public IngredientType IngredientType { get; }
        public Color BaseCupColor { get; }
        public Color FinalCupColor { get; }
        public RecipeStep[] Steps { get; }

        public bool HasStep(RecipeStep step)
        {
            for (var index = 0; index < Steps.Length; index++)
            {
                if (Steps[index] == step)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
