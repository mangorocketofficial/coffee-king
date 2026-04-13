using System;
using System.Collections.Generic;
using System.Linq;
using CoffeeKing.Orders;

namespace CoffeeKing.StageFlow
{
    public static class StageLibrary
    {
        public static StageData CreateDay(int dayNumber, IReadOnlyList<DrinkRecipe> recipes)
        {
            var recipeMap = recipes.ToDictionary(recipe => recipe.Id, recipe => recipe);
            var allowedRecipes = GetAllowedRecipes(dayNumber, recipeMap);
            var customerCount = GetCustomerCount(dayNumber);
            var maxSimultaneous = GetMaxSimultaneous(dayNumber);
            var patience = GetPatience(dayNumber);
            var spawnInterval = GetSpawnInterval(dayNumber);
            var timeLimit = GetTimeLimit(dayNumber, customerCount);

            return new StageData(dayNumber, customerCount, maxSimultaneous, patience, spawnInterval, timeLimit, allowedRecipes);
        }

        private static IReadOnlyList<DrinkRecipe> GetAllowedRecipes(int day, Dictionary<string, DrinkRecipe> recipeMap)
        {
            var allowed = new List<DrinkRecipe>();

            // Day 1: Iced Americano only
            allowed.Add(recipeMap["iced_americano"]);

            // Day 2+: add Iced Latte
            if (day >= 2)
            {
                allowed.Add(recipeMap["iced_cafe_latte"]);
            }

            // Day 4+: add Hot Americano
            if (day >= 4)
            {
                allowed.Add(recipeMap["hot_americano"]);
            }

            // Day 5+: add Hot Latte
            if (day >= 5)
            {
                allowed.Add(recipeMap["hot_cafe_latte"]);
            }

            return allowed;
        }

        private static int GetCustomerCount(int day)
        {
            // Day 1: 3, Day 2: 4, Day 3: 5, Day 4+: 3 + day
            return 2 + day;
        }

        private static int GetMaxSimultaneous(int day)
        {
            if (day <= 2)
            {
                return 1;
            }

            return 2;
        }

        private static float GetPatience(int day)
        {
            // Day 1: 75s, decrease by 5s per day, cap at 40s
            var patience = 80f - (day * 5f);
            return Math.Max(40f, patience);
        }

        private static float GetSpawnInterval(int day)
        {
            // Day 1: 8s, decrease by 0.5s per day, cap at 3s
            var interval = 8.5f - (day * 0.5f);
            return Math.Max(3f, interval);
        }

        private static float GetTimeLimit(int day, int customerCount)
        {
            // Base time scales with customer count and day
            // Day 1: 180s, then roughly 45s per customer with slight reduction per day
            var baseTimePerCustomer = Math.Max(30f, 50f - (day * 2f));
            var timeLimit = customerCount * baseTimePerCustomer;
            return Math.Max(120f, timeLimit);
        }
    }
}
