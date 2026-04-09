using CoffeeKing.Orders;

namespace CoffeeKing.Scoring
{
    public static class ScoreRules
    {
        public const int PortafilterLockScore = 40;
        public const int ExtractionPerfectScore = 100;
        public const int ExtractionGoodScore = 60;
        public const int ExtractionBadScore = 20;
        public const int SteamPerfectScore = 100;
        public const int SteamGoodScore = 60;
        public const int SteamBadScore = 20;
        public const int SyrupScore = 20;
        public const int ServeScore = 30;
        public const int SpeedBonusFast = 30;
        public const int SpeedBonusMedium = 15;
        public const int TimeoutPenalty = 50;

        public static int GetSpeedBonus(float durationSeconds)
        {
            if (durationSeconds <= 15f)
            {
                return SpeedBonusFast;
            }

            if (durationSeconds <= 25f)
            {
                return SpeedBonusMedium;
            }

            return 0;
        }

        public static int GetMaximumRecipeScore(DrinkRecipe recipe)
        {
            var total = PortafilterLockScore + ExtractionPerfectScore + ServeScore + SpeedBonusFast;
            if (recipe.HasStep(RecipeStep.Syrup))
            {
                total += SyrupScore;
            }

            if (recipe.HasStep(RecipeStep.SteamMilk))
            {
                total += SteamPerfectScore;
            }

            return total;
        }
    }
}
