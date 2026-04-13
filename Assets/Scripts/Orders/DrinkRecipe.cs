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

    public enum CupType
    {
        Plastic,
        Hot
    }

    public sealed class DrinkRecipe
    {
        public DrinkRecipe(
            string id,
            string displayName,
            CupType cupType,
            IngredientType ingredientType,
            Color baseCupColor,
            Color finalCupColor,
            int priceWon,
            params RecipeStep[] steps)
        {
            Id = id;
            DisplayName = displayName;
            CupType = cupType;
            IngredientType = ingredientType;
            BaseCupColor = baseCupColor;
            FinalCupColor = finalCupColor;
            PriceWon = priceWon;
            Steps = steps;
        }

        public string Id { get; }
        public string DisplayName { get; }
        public CupType CupType { get; }
        public IngredientType IngredientType { get; }
        public Color BaseCupColor { get; }
        public Color FinalCupColor { get; }
        public int PriceWon { get; }
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
