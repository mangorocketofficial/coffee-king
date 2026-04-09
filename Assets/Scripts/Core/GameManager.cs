using System.Collections;
using CoffeeKing.CustomerLogic;
using CoffeeKing.GameInput;
using CoffeeKing.Mechanics;
using CoffeeKing.Orders;
using CoffeeKing.Scoring;
using CoffeeKing.StageFlow;
using CoffeeKing.UI;
using CoffeeKing.Util;
using CoffeeKing.View;
using UnityEngine;

namespace CoffeeKing.Core
{
    public enum GameState
    {
        Bootstrapping,
        TitleScreen,
        WaitingForOrder,
        PortafilterStep,
        ExtractionStep,
        SyrupStep,
        SteamStep,
        ServingStep,
        StageResult
    }

    public sealed class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        public GameState State { get; private set; }

        private GameConfig config;
        private GrayboxSceneContext sceneContext;
        private GestureDetector gestureDetector;
        private PortafilterMechanic portafilterMechanic;
        private ExtractionMechanic extractionMechanic;
        private SyrupMechanic syrupMechanic;
        private SteamMilkMechanic steamMilkMechanic;
        private ServingMechanic servingMechanic;
        private ScoreManager scoreManager;
        private CustomerSpawner customerSpawner;
        private ResultScreenView resultScreenView;
        private StageManager stageManager;
        private UIContext uiContext;
        private AudioManager audioManager;

        private Customer currentCustomer;
        private DrinkRecipe currentRecipe;
        private Coroutine introCoroutine;
        private Coroutine tutorialHideCoroutine;
        private Coroutine rotateHintCoroutine;
        private float roundStartTime;
        private bool initialized;
        private bool dragHintShown;
        private bool rotateHintShown;
        private bool gaugeHintShown;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
            SetState(GameState.Bootstrapping);
        }

        private void Start()
        {
            InitializeIfNeeded();
        }

        private void Update()
        {
            if (!initialized)
            {
                return;
            }

            if (stageManager.CurrentStage != null)
            {
                customerSpawner.Tick(Time.deltaTime);
            }

            if (stageManager.FlowState == StageFlowState.Playing)
            {
                stageManager.Tick(Time.deltaTime);

                if (currentCustomer == null && customerSpawner.TryClaimNextCustomer(out var nextCustomer))
                {
                    BeginOrder(nextCustomer);
                }

                if (currentCustomer == null && customerSpawner.IsFinished)
                {
                    CompleteStage();
                }
            }

            RefreshHud();
        }

        private void OnDestroy()
        {
            if (portafilterMechanic != null)
            {
                portafilterMechanic.Locked -= HandlePortafilterLocked;
            }

            if (extractionMechanic != null)
            {
                extractionMechanic.Completed -= HandleExtractionCompleted;
            }

            if (syrupMechanic != null)
            {
                syrupMechanic.Completed -= HandleSyrupCompleted;
            }

            if (steamMilkMechanic != null)
            {
                steamMilkMechanic.Completed -= HandleSteamCompleted;
            }

            if (servingMechanic != null)
            {
                servingMechanic.Served -= HandleServed;
            }

            if (customerSpawner != null)
            {
                customerSpawner.CustomerTimedOut -= HandleCustomerTimedOut;
                customerSpawner.DestroyViews();
            }

            if (resultScreenView != null)
            {
                resultScreenView.NextRequested -= HandleNextStageRequested;
                resultScreenView.RetryRequested -= HandleRetryRequested;
                resultScreenView.Dispose();
            }

            if (uiContext != null)
            {
                uiContext.TitleScreenView.StartRequested -= HandleTitleStartRequested;
            }

            if (introCoroutine != null)
            {
                StopCoroutine(introCoroutine);
            }

            if (tutorialHideCoroutine != null)
            {
                StopCoroutine(tutorialHideCoroutine);
            }

            if (rotateHintCoroutine != null)
            {
                StopCoroutine(rotateHintCoroutine);
            }

            if (Instance == this)
            {
                Instance = null;
            }

            if (config != null)
            {
                Destroy(config);
            }
        }

        private void InitializeIfNeeded()
        {
            if (initialized)
            {
                return;
            }

            config = GameConfig.CreateRuntimeDefault();
            sceneContext = new GameSceneBuilder().Build(config);
            scoreManager = new ScoreManager();
            var recipes = DrinkLibrary.CreatePhaseOneDemo(config);
            stageManager = new StageManager(StageLibrary.Create(recipes));

            gestureDetector = CreateSubsystem<GestureDetector>("[GestureDetector]");
            audioManager = CreateSubsystem<AudioManager>("[AudioManager]");
            portafilterMechanic = CreateSubsystem<PortafilterMechanic>("[PortafilterMechanic]");
            extractionMechanic = CreateSubsystem<ExtractionMechanic>("[ExtractionMechanic]");
            syrupMechanic = CreateSubsystem<SyrupMechanic>("[SyrupMechanic]");
            steamMilkMechanic = CreateSubsystem<SteamMilkMechanic>("[SteamMilkMechanic]");
            servingMechanic = CreateSubsystem<ServingMechanic>("[ServingMechanic]");

            portafilterMechanic.Initialize(config, gestureDetector, sceneContext);
            extractionMechanic.Initialize(config, gestureDetector, sceneContext);
            syrupMechanic.Initialize(gestureDetector, sceneContext);
            steamMilkMechanic.Initialize(config, gestureDetector, sceneContext);
            servingMechanic.Initialize(config, gestureDetector, sceneContext);

            portafilterMechanic.Locked += HandlePortafilterLocked;
            extractionMechanic.Completed += HandleExtractionCompleted;
            syrupMechanic.Completed += HandleSyrupCompleted;
            steamMilkMechanic.Completed += HandleSteamCompleted;
            servingMechanic.Served += HandleServed;

            customerSpawner = new CustomerSpawner();
            customerSpawner.Initialize(config, sceneContext);
            customerSpawner.CustomerTimedOut += HandleCustomerTimedOut;

            resultScreenView = ResultScreenView.Create(sceneContext.OverlayLayer, gestureDetector);
            resultScreenView.NextRequested += HandleNextStageRequested;
            resultScreenView.RetryRequested += HandleRetryRequested;

            uiContext = UIBuilder.Build(transform);
            uiContext.TitleScreenView.StartRequested += HandleTitleStartRequested;

            initialized = true;
            ShowTitleScreen();
        }

        private void ShowTitleScreen()
        {
            CancelCurrentOrder();
            resultScreenView.Hide();
            uiContext.HUDView.SetVisible(false);
            uiContext.TutorialOverlay.Hide();
            uiContext.TitleScreenView.Show("Programmatic graybox build");

            sceneContext.SetInstruction("Coffee Meister");
            sceneContext.SetStatus("Press start");
            sceneContext.SetFeedback("Title screen ready", ColorPalette.HighlightGood);
            sceneContext.SetActiveOrder(string.Empty);
            sceneContext.SetQueueSummary(string.Empty);
            sceneContext.SetScore(string.Empty);
            sceneContext.SetRound(string.Empty);

            currentCustomer = null;
            currentRecipe = null;
            SetState(GameState.TitleScreen);
        }

        private void HandleTitleStartRequested()
        {
            audioManager.PlayUiClick();
            uiContext.TitleScreenView.Hide();
            StartStage(stageManager.StartFirstStage());
        }

        private void StartStage(StageData stage)
        {
            if (introCoroutine != null)
            {
                StopCoroutine(introCoroutine);
            }

            CancelTutorial();
            CancelCurrentOrder();
            resultScreenView.Hide();
            uiContext.HUDView.SetVisible(true);
            uiContext.TitleScreenView.Hide();

            currentCustomer = null;
            currentRecipe = null;

            customerSpawner.BeginStage(stage, System.Environment.TickCount ^ stage.Number);
            scoreManager.BeginStage(stage, customerSpawner.PlannedCustomers);

            sceneContext.SetRound($"{stage.DisplayName}   Customers {stage.CustomerCount}   Cap {stage.MaxSimultaneousCustomers}");
            sceneContext.SetInstruction(stage.DisplayName);
            sceneContext.SetStatus("Stage intro");
            sceneContext.SetFeedback($"Get ready for {stage.DisplayName}", ColorPalette.HighlightGood);
            sceneContext.SetActiveOrder("No active order");
            sceneContext.SetQueueSummary("Queue closed during intro");
            RefreshHud();

            introCoroutine = StartCoroutine(RunStageIntro(stage));
        }

        private IEnumerator RunStageIntro(StageData stage)
        {
            yield return new WaitForSeconds(2f);
            introCoroutine = null;

            stageManager.MarkPlaying();
            customerSpawner.StartSpawning();
            sceneContext.SetInstruction("Serve customers before their patience runs out.");
            sceneContext.SetStatus("Queue open");
            sceneContext.SetFeedback(string.Empty, config.SecondaryTextColor);
            SetState(GameState.WaitingForOrder);
        }

        private void BeginOrder(Customer customer)
        {
            currentCustomer = customer;
            currentRecipe = customer.Order;
            roundStartTime = Time.time;

            scoreManager.BeginRound(customer);
            ResetDrinkState();
            portafilterMechanic.BeginRound();

            sceneContext.SetActiveOrder($"Now making\nCustomer {customer.SequenceNumber}\n{currentRecipe.DisplayName}");
            sceneContext.SetInstruction("Drag the portafilter into the machine slot and rotate to lock it.");
            sceneContext.SetStatus("Active ticket claimed from queue.");
            sceneContext.SetFeedback(string.Empty, config.SecondaryTextColor);
            SetState(GameState.PortafilterStep);

            if (ShouldShowStageOneTutorial() && !dragHintShown)
            {
                dragHintShown = true;
                ShowTutorialHint(
                    "Stage 1 Hint",
                    "Drag the portafilter to the machine.",
                    new Vector2(-360f, 220f),
                    new Vector2(-430f, 40f),
                    2.4f);
            }

            if (ShouldShowStageOneTutorial() && !rotateHintShown)
            {
                if (rotateHintCoroutine != null)
                {
                    StopCoroutine(rotateHintCoroutine);
                }

                rotateHintCoroutine = StartCoroutine(ShowRotateHintAfterDelay(1.4f));
            }
        }

        private IEnumerator ShowRotateHintAfterDelay(float delay)
        {
            yield return new WaitForSeconds(delay);
            rotateHintCoroutine = null;

            if (!ShouldShowStageOneTutorial() || rotateHintShown || currentCustomer == null)
            {
                yield break;
            }

            rotateHintShown = true;
            ShowTutorialHint(
                "Stage 1 Hint",
                "Rotate to lock the handle in place.",
                new Vector2(-140f, 220f),
                new Vector2(0f, 30f),
                2.6f);
        }

        private void HandlePortafilterLocked()
        {
            if (currentCustomer == null || State != GameState.PortafilterStep)
            {
                return;
            }

            audioManager.PlaySnap();
            scoreManager.AddScore("Lock", ScoreRules.PortafilterLockScore);
            sceneContext.SetFeedback("Portafilter locked", ColorPalette.HighlightGood);
            sceneContext.SetInstruction("Tap the brew button to start extraction. Tap again in the green zone.");
            sceneContext.SetStatus("Extraction target: 65 to 72");
            extractionMechanic.BeginStep();
            SetState(GameState.ExtractionStep);

            if (ShouldShowStageOneTutorial() && !gaugeHintShown)
            {
                gaugeHintShown = true;
                ShowTutorialHint(
                    "Stage 1 Hint",
                    "Tap in the green zone for the best score.",
                    new Vector2(0f, 220f),
                    new Vector2(0f, 120f),
                    2.8f);
            }
        }

        private void HandleExtractionCompleted(MechanicScoreResult result)
        {
            if (currentCustomer == null || State != GameState.ExtractionStep)
            {
                return;
            }

            audioManager.PlayGauge(result.Grade);
            scoreManager.AddResult(result);
            SetCupStateAfterExtraction();
            sceneContext.SetFeedback(
                $"{FormatGrade(result.Grade)} extraction {result.MeasuredValue:0.0}",
                GradeToColor(result.Grade));

            if (currentRecipe.HasStep(RecipeStep.Syrup))
            {
                sceneContext.SetInstruction("Tap the syrup bottle to add vanilla syrup.");
                sceneContext.SetStatus("Syrup step");
                syrupMechanic.BeginStep();
                SetState(GameState.SyrupStep);
                return;
            }

            if (currentRecipe.HasStep(RecipeStep.SteamMilk))
            {
                BeginSteamStep();
                return;
            }

            BeginServingStep();
        }

        private void HandleSyrupCompleted()
        {
            if (currentCustomer == null || State != GameState.SyrupStep)
            {
                return;
            }

            scoreManager.AddScore("Syrup", ScoreRules.SyrupScore);
            sceneContext.SetCupVisual(SpriteAssetNames.CupVanilla, config.CupSize, config.CupVanillaColor);
            sceneContext.SetFeedback("Vanilla syrup added", config.CupVanillaColor);

            if (currentRecipe.HasStep(RecipeStep.SteamMilk))
            {
                BeginSteamStep();
                return;
            }

            BeginServingStep();
        }

        private void BeginSteamStep()
        {
            sceneContext.SetInstruction("Drag the steam wand into the pitcher, move it up or down, then tap to stop.");
            sceneContext.SetStatus("Milk target: 60C to 70C. Over 75C burns it.");
            steamMilkMechanic.BeginStep();
            SetState(GameState.SteamStep);
        }

        private void HandleSteamCompleted(MechanicScoreResult result)
        {
            if (currentCustomer == null || State != GameState.SteamStep)
            {
                return;
            }

            audioManager.PlayGauge(result.Grade);
            scoreManager.AddResult(result);
            SetCupStateForFinalDrink();
            sceneContext.SetFeedback(
                $"{FormatGrade(result.Grade)} steam {result.MeasuredValue:0.0}C",
                GradeToColor(result.Grade));

            BeginServingStep();
        }

        private void BeginServingStep()
        {
            SetCupStateForFinalDrink();
            sceneContext.SetInstruction("Drag the completed drink into the serving zone.");
            sceneContext.SetStatus("Serve the active customer.");
            servingMechanic.BeginStep();
            SetState(GameState.ServingStep);
        }

        private void HandleServed()
        {
            if (currentCustomer == null || State != GameState.ServingStep)
            {
                return;
            }

            audioManager.PlayServe();
            scoreManager.AddScore("Serve", ScoreRules.ServeScore);
            var orderDuration = Time.time - roundStartTime;
            var speedBonus = scoreManager.FinalizeServedRound(currentCustomer, orderDuration);
            customerSpawner.MarkServed(currentCustomer);

            sceneContext.SetFeedback(
                speedBonus > 0 ? $"{currentRecipe.DisplayName} served  speed +{speedBonus}" : $"{currentRecipe.DisplayName} served",
                ColorPalette.HighlightGood);
            sceneContext.SetStatus("Looking for next ticket.");

            currentCustomer = null;
            currentRecipe = null;
            SetState(GameState.WaitingForOrder);
        }

        private void HandleCustomerTimedOut(Customer customer)
        {
            scoreManager.RegisterTimeout(customer);
            sceneContext.SetFeedback($"Customer {customer.SequenceNumber} timed out", ColorPalette.HighlightBad);

            if (currentCustomer == customer)
            {
                CancelCurrentOrder();
                currentCustomer = null;
                currentRecipe = null;
                SetState(GameState.WaitingForOrder);
            }
        }

        private void CompleteStage()
        {
            stageManager.MarkStageComplete();
            CancelCurrentOrder();
            CancelTutorial();

            var stars = StarRating.FromScore(scoreManager.StageScore, scoreManager.StageMaxScore);
            var result = stageManager.BuildResult(scoreManager.StageScore, scoreManager.StageMaxScore, stars);

            audioManager.PlayStageComplete();
            uiContext.HUDView.SetVisible(false);
            sceneContext.SetInstruction("Stage result");
            sceneContext.SetStatus("Select next stage or retry.");
            sceneContext.SetFeedback(result.Passed ? "Stage clear" : "Stage failed", result.Passed ? ColorPalette.HighlightGood : ColorPalette.HighlightBad);
            sceneContext.SetActiveOrder("Result screen open");

            var allowAdvance = result.Passed && (stageManager.HasNextStage || stageManager.IsFinalStage);
            var summary =
                $"Served {scoreManager.ServedCount}   Timed Out {scoreManager.TimedOutCount}\n" +
                scoreManager.StageSummary;

            resultScreenView.Show(result, summary, allowAdvance);
            SetState(GameState.StageResult);
        }

        private void HandleNextStageRequested()
        {
            audioManager.PlayUiClick();

            if (stageManager.IsFinalStage)
            {
                StartStage(stageManager.StartFirstStage());
                return;
            }

            if (stageManager.HasNextStage)
            {
                StartStage(stageManager.AdvanceToNextStage());
            }
        }

        private void HandleRetryRequested()
        {
            audioManager.PlayUiClick();
            StartStage(stageManager.RetryCurrentStage());
        }

        private void ResetDrinkState()
        {
            extractionMechanic.CancelStep();
            syrupMechanic.CancelStep();
            steamMilkMechanic.CancelStep();
            servingMechanic.CancelStep();
            portafilterMechanic.CancelStep();

            sceneContext.CupRoot.position = sceneContext.CupAnchorPosition;
            sceneContext.SetCupVisual(SpriteAssetNames.CupEmpty, config.CupSize, config.CupEmptyColor);
        }

        private void CancelCurrentOrder()
        {
            extractionMechanic.CancelStep();
            syrupMechanic.CancelStep();
            steamMilkMechanic.CancelStep();
            servingMechanic.CancelStep();
            portafilterMechanic.CancelStep();

            if (currentCustomer != null)
            {
                customerSpawner.ReleaseClaim(currentCustomer);
            }

            sceneContext.CupRoot.position = sceneContext.CupAnchorPosition;
            sceneContext.SetCupVisual(SpriteAssetNames.CupEmpty, config.CupSize, config.CupEmptyColor);
            sceneContext.SetActiveOrder("No active order");
        }

        private void SetCupStateAfterExtraction()
        {
            sceneContext.SetCupVisual(SpriteAssetNames.CupAmericano, config.CupSize, currentRecipe.BaseCupColor);
        }

        private void SetCupStateForFinalDrink()
        {
            var assetName = currentRecipe.HasStep(RecipeStep.Syrup)
                ? SpriteAssetNames.CupVanilla
                : currentRecipe.HasStep(RecipeStep.SteamMilk)
                    ? SpriteAssetNames.CupLatte
                    : SpriteAssetNames.CupAmericano;

            sceneContext.SetCupVisual(assetName, config.CupSize, currentRecipe.FinalCupColor);
        }

        private void RefreshHud()
        {
            if (!initialized || stageManager.CurrentStage == null)
            {
                return;
            }

            var hudVisible = State != GameState.TitleScreen && State != GameState.StageResult;
            uiContext.HUDView.SetVisible(hudVisible);
            uiContext.HUDView.SetScore(
                $"Score {scoreManager.StageScore}/{scoreManager.StageMaxScore}\n" +
                $"Served {scoreManager.ServedCount}   Timeout {scoreManager.TimedOutCount}");
            uiContext.HUDView.SetStage($"{stageManager.CurrentStage.DisplayName}   {stageManager.FlowState}");
            uiContext.HUDView.SetTimer(stageManager.TimeRemaining);
            uiContext.HUDView.SetOrderList(customerSpawner.GetQueueSummary());
            uiContext.HUDView.SetProgress((scoreManager.ServedCount + scoreManager.TimedOutCount) / (float)stageManager.CurrentStage.CustomerCount);

            sceneContext.SetQueueSummary(customerSpawner.GetQueueSummary());
            sceneContext.SetScore(
                $"Stage Score {scoreManager.StageScore}/{scoreManager.StageMaxScore}\n" +
                $"Served {scoreManager.ServedCount}   Timeout {scoreManager.TimedOutCount}\n" +
                scoreManager.Breakdown);
            sceneContext.SetRound($"{stageManager.CurrentStage.DisplayName}   {stageManager.FlowState}");
        }

        private bool ShouldShowStageOneTutorial()
        {
            return stageManager.CurrentStage != null &&
                   stageManager.CurrentStage.Number == 1 &&
                   !(dragHintShown && rotateHintShown && gaugeHintShown);
        }

        private void ShowTutorialHint(string title, string body, Vector2 panelOffset, Vector2 arrowOffset, float durationSeconds)
        {
            if (tutorialHideCoroutine != null)
            {
                StopCoroutine(tutorialHideCoroutine);
            }

            uiContext.TutorialOverlay.Show(title, body, panelOffset, arrowOffset);
            tutorialHideCoroutine = StartCoroutine(HideTutorialAfterDelay(durationSeconds));
        }

        private IEnumerator HideTutorialAfterDelay(float delay)
        {
            yield return new WaitForSeconds(delay);
            tutorialHideCoroutine = null;
            uiContext.TutorialOverlay.Hide();
        }

        private void CancelTutorial()
        {
            if (tutorialHideCoroutine != null)
            {
                StopCoroutine(tutorialHideCoroutine);
                tutorialHideCoroutine = null;
            }

            if (rotateHintCoroutine != null)
            {
                StopCoroutine(rotateHintCoroutine);
                rotateHintCoroutine = null;
            }

            uiContext?.TutorialOverlay.Hide();
        }

        private static string FormatGrade(QualityGrade grade)
        {
            switch (grade)
            {
                case QualityGrade.Perfect:
                    return "Perfect";
                case QualityGrade.Good:
                    return "Good";
                default:
                    return "Bad";
            }
        }

        private static Color GradeToColor(QualityGrade grade)
        {
            switch (grade)
            {
                case QualityGrade.Perfect:
                case QualityGrade.Good:
                    return ColorPalette.HighlightGood;
                default:
                    return ColorPalette.HighlightBad;
            }
        }

        private T CreateSubsystem<T>(string name) where T : Component
        {
            var child = new GameObject(name);
            child.transform.SetParent(transform, false);
            return child.AddComponent<T>();
        }

        private void SetState(GameState nextState)
        {
            State = nextState;
        }
    }
}
