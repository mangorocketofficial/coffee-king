using CoffeeKing.Orders;

namespace CoffeeKing.Scoring
{
    public static class ScoreRules
    {
        public const int GrindingPerfectScore = 100;
        public const int GrindingGoodScore = 60;
        public const int GrindingBadScore = 20;
        public const int TampingPerfectScore = 100;
        public const int TampingGoodScore = 60;
        public const int TampingBadScore = 20;
        public const int PortafilterLockPerfectScore = 100;
        public const int PortafilterLockGoodScore = 60;
        public const int PortafilterLockBadScore = 20;
        public const int ExtractionPerfectScore = 100;
        public const int ExtractionGoodScore = 60;
        public const int ExtractionBadScore = 20;
        public const int SteamPerfectScore = 100;
        public const int SteamGoodScore = 60;
        public const int SteamBadScore = 20;
        public const int SpeedFastBonus = 50;
        public const int SpeedGoodBonus = 25;
        public const int TimeoutPenalty = 100;
        public const int WrongDrinkPenalty = 150;
        public const int PerfectDrinkBonus = 75;
        public const int NoMistakeBonus = 200;

        public static int GetSpeedBonus(DrinkRecipe recipe, float durationSeconds)
        {
            var fastThreshold = GetFastSpeedThreshold(recipe);
            var goodThreshold = fastThreshold + 8f;

            if (durationSeconds <= fastThreshold)
            {
                return SpeedFastBonus;
            }

            if (durationSeconds <= goodThreshold)
            {
                return SpeedGoodBonus;
            }

            return 0;
        }

        public static int GetMaximumRecipeScore(DrinkRecipe recipe)
        {
            var score = GrindingPerfectScore +
                        TampingPerfectScore +
                        PortafilterLockPerfectScore +
                        ExtractionPerfectScore;

            if (recipe.HasStep(RecipeStep.SteamMilk))
            {
                score += SteamPerfectScore;
            }

            score += SpeedFastBonus;
            return score;
        }

        private static float GetFastSpeedThreshold(DrinkRecipe recipe)
        {
            var threshold = 30f;

            if (recipe.CupType == CupType.Hot)
            {
                threshold += 4f;
            }

            if (recipe.HasStep(RecipeStep.SteamMilk))
            {
                threshold += 6f;
            }

            return threshold;
        }
    }
}
