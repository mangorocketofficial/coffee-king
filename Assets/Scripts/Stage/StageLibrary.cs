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
                new StageData(1, 3, 1, 45f, 8f, 120f, new[] { recipeMap["americano"] }),
                new StageData(2, 5, 1, 35f, 7f, 150f, new[] { recipeMap["americano"], recipeMap["caffe_latte"] }),
                new StageData(3, 7, 2, 35f, 6f, 180f, new[] { recipeMap["americano"], recipeMap["caffe_latte"], recipeMap["vanilla_latte"] }),
                new StageData(4, 10, 2, 27f, 5f, 200f, new[] { recipeMap["americano"], recipeMap["caffe_latte"], recipeMap["vanilla_latte"] }),
                new StageData(5, 12, 3, 22f, 4.5f, 220f, new[] { recipeMap["americano"], recipeMap["caffe_latte"], recipeMap["vanilla_latte"] })
            };
        }
    }
}
