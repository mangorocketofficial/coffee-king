using System.Collections.Generic;
using CoffeeKing.CustomerLogic;
using CoffeeKing.StageFlow;

namespace CoffeeKing.Scoring
{
    public readonly struct MechanicScoreResult
    {
        public MechanicScoreResult(string label, QualityGrade grade, int score, float measuredValue)
        {
            Label = label;
            Grade = grade;
            Score = score;
            MeasuredValue = measuredValue;
        }

        public string Label { get; }
        public QualityGrade Grade { get; }
        public int Score { get; }
        public float MeasuredValue { get; }
    }

    public enum QualityGrade
    {
        Perfect,
        Good,
        Bad
    }

    public sealed class ScoreManager
    {
        private readonly List<string> roundBreakdown = new List<string>();
        private readonly List<string> stageEvents = new List<string>();
        private readonly List<QualityGrade> roundGrades = new List<QualityGrade>();

        public int StageScore { get; private set; }
        public int CampaignScore { get; private set; }
        public int StageMaxScore { get; private set; }
        public int RoundScore { get; private set; }
        public int ServedCount { get; private set; }
        public int TimedOutCount { get; private set; }
        public bool HasAnyBadGrade { get; private set; }
        public int NoMistakeBonusAwarded { get; private set; }
        public int DailyEarnings { get; private set; }

        public string Breakdown => roundBreakdown.Count == 0 ? "Waiting..." : string.Join(" | ", roundBreakdown);
        public string StageSummary => stageEvents.Count == 0 ? "No results yet." : string.Join("\n", stageEvents);
        public float StagePercentage => StageMaxScore <= 0 ? 0f : (float)StageScore / StageMaxScore;

        public void BeginStage(StageData stage, IReadOnlyList<Customer> plannedCustomers)
        {
            StageScore = 0;
            RoundScore = 0;
            ServedCount = 0;
            TimedOutCount = 0;
            StageMaxScore = 0;
            HasAnyBadGrade = false;
            NoMistakeBonusAwarded = 0;
            DailyEarnings = 0;
            roundBreakdown.Clear();
            stageEvents.Clear();
            roundGrades.Clear();

            for (var index = 0; index < plannedCustomers.Count; index++)
            {
                StageMaxScore += ScoreRules.GetMaximumRecipeScore(plannedCustomers[index].Order);
            }

            StageMaxScore += ScoreRules.NoMistakeBonus;

            stageEvents.Add($"{stage.DisplayName}   Customers {plannedCustomers.Count}");
        }

        public void BeginRound(Customer customer)
        {
            RoundScore = 0;
            roundBreakdown.Clear();
            roundGrades.Clear();
            roundBreakdown.Add(customer.Order.DisplayName);
        }

        public void AddScore(string label, int score)
        {
            RoundScore += score;
            StageScore += score;
            CampaignScore += score;
            roundBreakdown.Add($"{label}+{score}");
        }

        public void AddResult(MechanicScoreResult result)
        {
            roundGrades.Add(result.Grade);
            if (result.Grade == QualityGrade.Bad)
            {
                HasAnyBadGrade = true;
            }

            AddScore($"{result.Label}:{result.Grade}", result.Score);
        }

        public int FinalizeServedRound(Customer customer, float orderDurationSeconds)
        {
            var speedBonus = ScoreRules.GetSpeedBonus(customer.Order, orderDurationSeconds);
            if (speedBonus > 0)
            {
                AddScore("Speed", speedBonus);
            }

            var perfectBonus = 0;
            if (roundGrades.Count > 0 && IsAllPerfect())
            {
                perfectBonus = ScoreRules.PerfectDrinkBonus;
                AddScore("AllPerfect", perfectBonus);
            }

            ServedCount++;
            DailyEarnings += customer.Order.PriceWon;
            var suffix = perfectBonus > 0 ? "   ALL PERFECT" : string.Empty;
            stageEvents.Add($"{customer.Order.DisplayName} served   {RoundScore} pts   {FormatWon(customer.Order.PriceWon)}   {orderDurationSeconds:0.0}s{suffix}");
            return speedBonus;
        }

        public void RegisterWrongDrink(Customer customer)
        {
            var pointsToRemove = RoundScore;
            StageScore -= pointsToRemove;
            CampaignScore -= pointsToRemove;
            RoundScore = 0;
            roundBreakdown.Clear();
            roundBreakdown.Add(customer.Order.DisplayName);

            StageScore -= ScoreRules.WrongDrinkPenalty;
            CampaignScore -= ScoreRules.WrongDrinkPenalty;
            RoundScore = -ScoreRules.WrongDrinkPenalty;
            roundBreakdown.Add($"WrongDrink-{ScoreRules.WrongDrinkPenalty}");

            ServedCount++;
            // No earnings for wrong drink
            stageEvents.Add($"{customer.Order.DisplayName} WRONG DRINK   -{ScoreRules.WrongDrinkPenalty}");
        }

        public void FinalizeNoMistakeBonus()
        {
            if (!HasAnyBadGrade && ServedCount > 0)
            {
                NoMistakeBonusAwarded = ScoreRules.NoMistakeBonus;
                StageScore += ScoreRules.NoMistakeBonus;
                CampaignScore += ScoreRules.NoMistakeBonus;
                stageEvents.Add($"No Mistake Bonus   +{ScoreRules.NoMistakeBonus}");
            }
        }

        public void RegisterTimeout(Customer customer, string reason = "timed out")
        {
            StageScore -= ScoreRules.TimeoutPenalty;
            CampaignScore -= ScoreRules.TimeoutPenalty;
            TimedOutCount++;
            // No earnings for timed out customer
            stageEvents.Add($"{customer.Order.DisplayName} {reason}   -{ScoreRules.TimeoutPenalty}");
        }

        public static string FormatWon(int amount)
        {
            return $"{amount:N0}\uc6d0";
        }

        public static string FormatWon(long amount)
        {
            return $"{amount:N0}\uc6d0";
        }

        private bool IsAllPerfect()
        {
            for (var index = 0; index < roundGrades.Count; index++)
            {
                if (roundGrades[index] != QualityGrade.Perfect)
                {
                    return false;
                }
            }

            return true;
        }
    }
}
