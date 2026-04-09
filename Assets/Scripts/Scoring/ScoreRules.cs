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
        public const int TimeoutPenalty = 100;

        public static int GetSpeedBonus(float durationSeconds)
        {
            return 0;
        }

        public static int GetMaximumRecipeScore(DrinkRecipe recipe)
        {
            return GrindingPerfectScore +
                   TampingPerfectScore +
                   PortafilterLockPerfectScore +
                   ExtractionPerfectScore;
        }
    }
}
