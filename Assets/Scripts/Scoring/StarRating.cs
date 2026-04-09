namespace CoffeeKing.Scoring
{
    public static class StarRating
    {
        public static int FromScore(int score, int maxScore)
        {
            if (maxScore <= 0)
            {
                return 0;
            }

            var percentage = (float)score / maxScore;
            if (percentage >= 0.90f)
            {
                return 3;
            }

            if (percentage >= 0.70f)
            {
                return 2;
            }

            if (percentage >= 0.50f)
            {
                return 1;
            }

            return 0;
        }

        public static string ToDisplayString(int stars)
        {
            switch (stars)
            {
                case 3:
                    return "***";
                case 2:
                    return "**-";
                case 1:
                    return "*--";
                default:
                    return "---";
            }
        }
    }
}
