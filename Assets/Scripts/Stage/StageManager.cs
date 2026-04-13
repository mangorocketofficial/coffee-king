using System.Collections.Generic;
using CoffeeKing.Orders;

namespace CoffeeKing.StageFlow
{
    public enum StageFlowState
    {
        StageIntro,
        Playing,
        StageComplete,
        ResultScreen
    }

    public enum StageEndReason
    {
        ClearedOrders,
        TimeExpired
    }

    public readonly struct StageResult
    {
        public StageResult(StageData stage, int score, int maxScore, int stars, StageEndReason endReason, int dailyEarnings, long totalEarnings)
        {
            Stage = stage;
            Score = score;
            MaxScore = maxScore;
            Stars = stars;
            EndReason = endReason;
            DailyEarnings = dailyEarnings;
            TotalEarnings = totalEarnings;
        }

        public StageData Stage { get; }
        public int Score { get; }
        public int MaxScore { get; }
        public int Stars { get; }
        public StageEndReason EndReason { get; }
        public int DailyEarnings { get; }
        public long TotalEarnings { get; }
        public bool Passed => true; // Days always pass - no fail state
        public float Percentage
        {
            get
            {
                if (MaxScore <= 0)
                {
                    return 0f;
                }

                var percentage = (float)Score / MaxScore;
                return percentage < 0f ? 0f : percentage;
            }
        }
    }

    public sealed class StageManager
    {
        private readonly IReadOnlyList<DrinkRecipe> recipes;

        public StageManager(IReadOnlyList<DrinkRecipe> recipes)
        {
            this.recipes = recipes;
            CurrentDayNumber = 0;
        }

        public int CurrentDayNumber { get; private set; }
        public StageData CurrentStage { get; private set; }
        public StageFlowState FlowState { get; private set; }
        public float TimeRemaining { get; private set; }

        public StageData StartDay(int dayNumber)
        {
            CurrentDayNumber = dayNumber;
            CurrentStage = StageLibrary.CreateDay(dayNumber, recipes);
            FlowState = StageFlowState.StageIntro;
            TimeRemaining = CurrentStage.TimeLimitSeconds;
            return CurrentStage;
        }

        public StageData StartNextDay()
        {
            return StartDay(CurrentDayNumber + 1);
        }

        public StageData RetryCurrentDay()
        {
            return StartDay(CurrentDayNumber);
        }

        public void MarkPlaying()
        {
            FlowState = StageFlowState.Playing;
        }

        public bool HasTimeExpired => FlowState == StageFlowState.Playing && TimeRemaining <= 0f;

        public void Tick(float deltaTime)
        {
            if (FlowState != StageFlowState.Playing || CurrentStage == null)
            {
                return;
            }

            TimeRemaining = TimeRemaining - deltaTime;
            if (TimeRemaining < 0f)
            {
                TimeRemaining = 0f;
            }
        }

        public void MarkStageComplete()
        {
            FlowState = StageFlowState.StageComplete;
        }

        public StageResult BuildResult(int score, int maxScore, int stars, StageEndReason endReason, int dailyEarnings, long totalEarnings)
        {
            FlowState = StageFlowState.ResultScreen;
            return new StageResult(CurrentStage, score, maxScore, stars, endReason, dailyEarnings, totalEarnings);
        }
    }
}
