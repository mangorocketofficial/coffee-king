using System.Collections.Generic;

namespace CoffeeKing.StageFlow
{
    public enum StageFlowState
    {
        StageIntro,
        Playing,
        StageComplete,
        ResultScreen
    }

    public readonly struct StageResult
    {
        public StageResult(StageData stage, int score, int maxScore, int stars)
        {
            Stage = stage;
            Score = score;
            MaxScore = maxScore;
            Stars = stars;
        }

        public StageData Stage { get; }
        public int Score { get; }
        public int MaxScore { get; }
        public int Stars { get; }
        public bool Passed => Stars > 0;
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
        private readonly IReadOnlyList<StageData> stages;

        public StageManager(IReadOnlyList<StageData> stages)
        {
            this.stages = stages;
            CurrentStageIndex = -1;
        }

        public int CurrentStageIndex { get; private set; }
        public StageData CurrentStage => CurrentStageIndex >= 0 && CurrentStageIndex < stages.Count ? stages[CurrentStageIndex] : null;
        public StageFlowState FlowState { get; private set; }
        public float TimeRemaining { get; private set; }
        public bool HasNextStage => CurrentStageIndex >= 0 && CurrentStageIndex < stages.Count - 1;
        public bool IsFinalStage => CurrentStageIndex == stages.Count - 1;

        public StageData StartFirstStage()
        {
            return StartStage(0);
        }

        public StageData RetryCurrentStage()
        {
            return StartStage(CurrentStageIndex);
        }

        public StageData AdvanceToNextStage()
        {
            return StartStage(CurrentStageIndex + 1);
        }

        public void MarkPlaying()
        {
            FlowState = StageFlowState.Playing;
        }

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

        public StageResult BuildResult(int score, int maxScore, int stars)
        {
            FlowState = StageFlowState.ResultScreen;
            return new StageResult(CurrentStage, score, maxScore, stars);
        }

        private StageData StartStage(int stageIndex)
        {
            CurrentStageIndex = stageIndex;
            FlowState = StageFlowState.StageIntro;
            TimeRemaining = CurrentStage == null ? 0f : CurrentStage.TimeLimitSeconds;
            return CurrentStage;
        }
    }
}
