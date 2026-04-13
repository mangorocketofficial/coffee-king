using System.Collections.Generic;
using CoffeeKing.Orders;

namespace CoffeeKing.StageFlow
{
    public sealed class StageData
    {
        public StageData(
            int number,
            int customerCount,
            int maxSimultaneousCustomers,
            float patienceSeconds,
            float spawnIntervalSeconds,
            float timeLimitSeconds,
            IReadOnlyList<DrinkRecipe> allowedRecipes)
        {
            Number = number;
            CustomerCount = customerCount;
            MaxSimultaneousCustomers = maxSimultaneousCustomers;
            PatienceSeconds = patienceSeconds;
            SpawnIntervalSeconds = spawnIntervalSeconds;
            TimeLimitSeconds = timeLimitSeconds;
            AllowedRecipes = allowedRecipes;
        }

        public int Number { get; }
        public int CustomerCount { get; }
        public int MaxSimultaneousCustomers { get; }
        public float PatienceSeconds { get; }
        public float SpawnIntervalSeconds { get; }
        public float TimeLimitSeconds { get; }
        public IReadOnlyList<DrinkRecipe> AllowedRecipes { get; }

        public string DisplayName => $"Day {Number}";
    }
}
