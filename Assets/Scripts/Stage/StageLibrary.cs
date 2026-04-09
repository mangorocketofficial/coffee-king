using System.Collections.Generic;
using System.Linq;
using CoffeeKing.Orders;

namespace CoffeeKing.StageFlow
{
    public static class StageLibrary
    {
        public static IReadOnlyList<StageData> Create(IReadOnlyList<DrinkRecipe> recipes)
        {
            var recipeMap = recipes.ToDictionary(recipe => recipe.Id, recipe => recipe);

            return new[]
            {
                new StageData(1, 3, 1, 75f, 8f, 180f, new[] { recipeMap["iced_americano"] }),
                new StageData(2, 5, 1, 65f, 7f, 210f, new[] { recipeMap["iced_americano"] }),
                new StageData(3, 7, 2, 65f, 6f, 240f, new[] { recipeMap["iced_americano"] }),
                new StageData(4, 10, 2, 55f, 5f, 270f, new[] { recipeMap["iced_americano"] }),
                new StageData(5, 12, 3, 50f, 4.5f, 300f, new[] { recipeMap["iced_americano"] })
            };
        }
    }
}
