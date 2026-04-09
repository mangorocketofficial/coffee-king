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

        public int StageScore { get; private set; }
        public int CampaignScore { get; private set; }
        public int StageMaxScore { get; private set; }
        public int RoundScore { get; private set; }
        public int ServedCount { get; private set; }
        public int TimedOutCount { get; private set; }

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
            roundBreakdown.Clear();
            stageEvents.Clear();

            for (var index = 0; index < plannedCustomers.Count; index++)
            {
                StageMaxScore += ScoreRules.GetMaximumRecipeScore(plannedCustomers[index].Order);
            }

            stageEvents.Add($"{stage.DisplayName}   Customers {plannedCustomers.Count}");
        }

        public void BeginRound(Customer customer)
        {
            RoundScore = 0;
            roundBreakdown.Clear();
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
            AddScore($"{result.Label}:{result.Grade}", result.Score);
        }

        public int FinalizeServedRound(Customer customer, float orderDurationSeconds)
        {
            var speedBonus = ScoreRules.GetSpeedBonus(orderDurationSeconds);
            if (speedBonus > 0)
            {
                AddScore("Speed", speedBonus);
            }

            ServedCount++;
            stageEvents.Add($"{customer.Order.DisplayName} served   {RoundScore} pts   {orderDurationSeconds:0.0}s");
            return speedBonus;
        }

        public void RegisterTimeout(Customer customer)
        {
            StageScore -= ScoreRules.TimeoutPenalty;
            CampaignScore -= ScoreRules.TimeoutPenalty;
            TimedOutCount++;
            stageEvents.Add($"{customer.Order.DisplayName} timed out   -{ScoreRules.TimeoutPenalty}");
        }
    }
}
