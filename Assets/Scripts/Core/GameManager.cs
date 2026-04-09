using System.Collections;
using CoffeeKing.CustomerLogic;
using CoffeeKing.Flow;
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
        GrindingStep,
        TampingStep,
        PortafilterStep,
        ExtractionStep,
        PourShotStep,
        IngredientStep,
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
        private AudioManager audioManager;
        private GrindingMechanic grindingMechanic;
        private TampingMechanic tampingMechanic;
        private PortafilterMechanic portafilterMechanic;
        private ExtractionMechanic extractionMechanic;
        private PourMechanic pourMechanic;
        private IngredientMechanic ingredientMechanic;
        private ServingMechanic servingMechanic;
        private ScoreManager scoreManager;
        private CustomerSpawner customerSpawner;
        private ResultScreenView resultScreenView;
        private StageManager stageManager;
        private UIContext uiContext;
        private DrinkFlowController drinkFlowController;

        private Customer currentCustomer;
        private DrinkRecipe currentRecipe;
        private Coroutine introCoroutine;
        private Coroutine cupSetupCoroutine;
        private float roundStartTime;
        private float lockStepStartTime;
        private bool initialized;

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
            if (grindingMechanic != null)
            {
                grindingMechanic.Completed -= HandleGrindingCompleted;
            }

            if (tampingMechanic != null)
            {
                tampingMechanic.Completed -= HandleTampingCompleted;
            }

            if (portafilterMechanic != null)
            {
                portafilterMechanic.Locked -= HandlePortafilterLocked;
            }

            if (extractionMechanic != null)
            {
                extractionMechanic.Completed -= HandleExtractionCompleted;
            }

            if (pourMechanic != null)
            {
                pourMechanic.Completed -= HandlePourCompleted;
            }

            if (ingredientMechanic != null)
            {
                ingredientMechanic.Completed -= HandleIngredientCompleted;
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

            if (cupSetupCoroutine != null)
            {
                StopCoroutine(cupSetupCoroutine);
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
            drinkFlowController = new DrinkFlowController();

            var recipes = DrinkLibrary.CreateRebootPhaseOne(config);
            stageManager = new StageManager(StageLibrary.Create(recipes));

            gestureDetector = CreateSubsystem<GestureDetector>("[GestureDetector]");
            audioManager = CreateSubsystem<AudioManager>("[AudioManager]");
            grindingMechanic = CreateSubsystem<GrindingMechanic>("[GrindingMechanic]");
            tampingMechanic = CreateSubsystem<TampingMechanic>("[TampingMechanic]");
            portafilterMechanic = CreateSubsystem<PortafilterMechanic>("[PortafilterMechanic]");
            extractionMechanic = CreateSubsystem<ExtractionMechanic>("[ExtractionMechanic]");
            pourMechanic = CreateSubsystem<PourMechanic>("[PourMechanic]");
            ingredientMechanic = CreateSubsystem<IngredientMechanic>("[IngredientMechanic]");
            servingMechanic = CreateSubsystem<ServingMechanic>("[ServingMechanic]");

            grindingMechanic.Initialize(config, gestureDetector, sceneContext);
            tampingMechanic.Initialize(config, gestureDetector, sceneContext);
            portafilterMechanic.Initialize(config, gestureDetector, sceneContext);
            extractionMechanic.Initialize(config, gestureDetector, sceneContext);
            pourMechanic.Initialize(config, gestureDetector, sceneContext);
            ingredientMechanic.Initialize(gestureDetector, sceneContext);
            servingMechanic.Initialize(config, gestureDetector, sceneContext);

            grindingMechanic.Completed += HandleGrindingCompleted;
            tampingMechanic.Completed += HandleTampingCompleted;
            portafilterMechanic.Locked += HandlePortafilterLocked;
            extractionMechanic.Completed += HandleExtractionCompleted;
            pourMechanic.Completed += HandlePourCompleted;
            ingredientMechanic.Completed += HandleIngredientCompleted;
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
            uiContext.TitleScreenView.Show("V2 Reboot - Iced Americano Slice");

            sceneContext.SetInstruction("Coffee King");
            sceneContext.SetStatus("Press start");
            sceneContext.SetFeedback("Reboot slice ready", ColorPalette.HighlightGood);
            sceneContext.SetActiveOrder(string.Empty);
            sceneContext.SetQueueSummary(string.Empty);
            sceneContext.SetScore(string.Empty);
            sceneContext.SetRound(string.Empty);

            currentCustomer = null;
            currentRecipe = null;
            drinkFlowController.Reset();
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

            CancelCurrentOrder();
            resultScreenView.Hide();
            uiContext.HUDView.SetVisible(true);
            uiContext.TitleScreenView.Hide();
            uiContext.TutorialOverlay.Hide();

            currentCustomer = null;
            currentRecipe = null;

            customerSpawner.BeginStage(stage, System.Environment.TickCount ^ stage.Number);
            scoreManager.BeginStage(stage, customerSpawner.PlannedCustomers);

            sceneContext.SetRound($"{stage.DisplayName}   Customers {stage.CustomerCount}   Americano Only");
            sceneContext.SetInstruction(stage.DisplayName);
            sceneContext.SetStatus("Stage intro");
            sceneContext.SetFeedback($"Get ready for {stage.DisplayName}", ColorPalette.HighlightGood);
            sceneContext.SetActiveOrder("No active order");
            sceneContext.SetQueueSummary("Queue closed during intro");
            RefreshHud();

            introCoroutine = StartCoroutine(RunStageIntro());
        }

        private IEnumerator RunStageIntro()
        {
            yield return new WaitForSeconds(2f);
            introCoroutine = null;

            stageManager.MarkPlaying();
            customerSpawner.StartSpawning();
            sceneContext.SetInstruction("Make iced americanos before patience runs out.");
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
            ResetDrinkVisuals();
            drinkFlowController.StartDrink(currentRecipe);
            TransitionDrinkState(drinkFlowController.CurrentState);
        }

        private void TransitionDrinkState(DrinkFlowState state)
        {
            switch (state)
            {
                case DrinkFlowState.MoveToGrinder:
                    sceneContext.SetActiveOrder($"Now making\n{currentRecipe.DisplayName}");
                    sceneContext.SetInstruction("Drag the empty portafilter to the grinder.");
                    sceneContext.SetStatus("Move to grinder, then hold to grind.");
                    sceneContext.SetFeedback(string.Empty, config.SecondaryTextColor);
                    grindingMechanic.BeginStep();
                    SetState(GameState.GrindingStep);
                    break;

                case DrinkFlowState.Tamping:
                    sceneContext.SetInstruction("Hold the tamper and release in the green zone.");
                    sceneContext.SetStatus("Compress the coffee bed.");
                    tampingMechanic.BeginStep();
                    SetState(GameState.TampingStep);
                    break;

                case DrinkFlowState.PortafilterLocking:
                    sceneContext.SetInstruction("Drag the tamped portafilter to the machine and rotate to lock.");
                    sceneContext.SetStatus("Lock the portafilter into the machine.");
                    sceneContext.SetMachineVisual(SpriteAssetNames.MachineEmpty, config.MachineSize, config.MachineColor);
                    lockStepStartTime = Time.time;
                    portafilterMechanic.BeginRound();
                    SetState(GameState.PortafilterStep);
                    break;

                case DrinkFlowState.Extracting:
                    sceneContext.SetInstruction("Tap to start extraction, then tap in the green zone.");
                    sceneContext.SetStatus("Pull the espresso shot.");
                    extractionMechanic.BeginStep();
                    SetState(GameState.ExtractionStep);
                    break;

                case DrinkFlowState.CupSetup:
                    sceneContext.SetInstruction("Set up the iced cup.");
                    sceneContext.SetStatus("Cup and ice are being prepared.");
                    if (cupSetupCoroutine != null)
                    {
                        StopCoroutine(cupSetupCoroutine);
                    }

                    cupSetupCoroutine = StartCoroutine(RunCupSetup());
                    break;

                case DrinkFlowState.PourShotToCup:
                    sceneContext.SetInstruction("Drag the shot glass into the iced cup.");
                    sceneContext.SetStatus("Pour the espresso shot.");
                    pourMechanic.BeginStep();
                    SetState(GameState.PourShotStep);
                    break;

                case DrinkFlowState.PourIngredient:
                    sceneContext.SetInstruction("Tap the water bottle to finish the americano.");
                    sceneContext.SetStatus("Pour water into the cup.");
                    ingredientMechanic.BeginStep();
                    SetState(GameState.IngredientStep);
                    break;

                case DrinkFlowState.Serving:
                    sceneContext.SetInstruction("Drag the finished drink to the customer.");
                    sceneContext.SetStatus("Serve the iced americano.");
                    servingMechanic.BeginStep();
                    SetState(GameState.ServingStep);
                    break;

                case DrinkFlowState.Scoring:
                    SetState(GameState.WaitingForOrder);
                    break;
            }
        }

        private IEnumerator RunCupSetup()
        {
            sceneContext.CupRoot.position = sceneContext.CupAnchorPosition;
            sceneContext.CupRoot.gameObject.SetActive(true);
            sceneContext.SetCupVisual(SpriteAssetNames.CupPlasticEmpty, config.CupSize, config.CupEmptyColor);
            yield return new WaitForSeconds(0.35f);
            sceneContext.SetCupVisual(SpriteAssetNames.CupPlasticIce, config.CupSize, config.CupEmptyColor);
            yield return new WaitForSeconds(0.35f);
            cupSetupCoroutine = null;
            TransitionDrinkState(drinkFlowController.Advance());
        }

        private void HandleGrindingCompleted(MechanicScoreResult result)
        {
            if (currentCustomer == null || State != GameState.GrindingStep)
            {
                return;
            }

            audioManager.PlayGauge(result.Grade);
            scoreManager.AddResult(result);
            sceneContext.SetFeedback($"{FormatGrade(result.Grade)} grinding {result.MeasuredValue:0.0}", GradeToColor(result.Grade));
            TransitionDrinkState(drinkFlowController.Advance());
        }

        private void HandleTampingCompleted(MechanicScoreResult result)
        {
            if (currentCustomer == null || State != GameState.TampingStep)
            {
                return;
            }

            audioManager.PlayGauge(result.Grade);
            scoreManager.AddResult(result);
            sceneContext.SetFeedback($"{FormatGrade(result.Grade)} tamping {result.MeasuredValue:0.0}", GradeToColor(result.Grade));
            TransitionDrinkState(drinkFlowController.Advance());
        }

        private void HandlePortafilterLocked()
        {
            if (currentCustomer == null || State != GameState.PortafilterStep)
            {
                return;
            }

            audioManager.PlaySnap();
            var result = EvaluateLockResult(Time.time - lockStepStartTime);
            scoreManager.AddResult(result);
            sceneContext.SetFeedback($"{FormatGrade(result.Grade)} lock", GradeToColor(result.Grade));
            TransitionDrinkState(drinkFlowController.Advance());
        }

        private void HandleExtractionCompleted(MechanicScoreResult result)
        {
            if (currentCustomer == null || State != GameState.ExtractionStep)
            {
                return;
            }

            audioManager.PlayGauge(result.Grade);
            scoreManager.AddResult(result);
            sceneContext.SetFeedback($"{FormatGrade(result.Grade)} extraction {result.MeasuredValue:0.0}", GradeToColor(result.Grade));
            TransitionDrinkState(drinkFlowController.Advance());
        }

        private void HandlePourCompleted()
        {
            if (currentCustomer == null || State != GameState.PourShotStep)
            {
                return;
            }

            sceneContext.SetCupVisual(SpriteAssetNames.CupPlasticShot, config.CupSize, currentRecipe.BaseCupColor);
            sceneContext.SetFeedback("Shot poured into the cup", ColorPalette.HighlightGood);
            TransitionDrinkState(drinkFlowController.Advance());
        }

        private void HandleIngredientCompleted()
        {
            if (currentCustomer == null || State != GameState.IngredientStep)
            {
                return;
            }

            sceneContext.SetCupVisual(SpriteAssetNames.CupPlasticAmericano, config.CupSize, currentRecipe.FinalCupColor);
            sceneContext.SetFeedback("Water added", ColorPalette.HighlightGood);
            TransitionDrinkState(drinkFlowController.Advance());
        }

        private void HandleServed()
        {
            if (currentCustomer == null || State != GameState.ServingStep)
            {
                return;
            }

            audioManager.PlayServe();
            var orderDuration = Time.time - roundStartTime;
            scoreManager.FinalizeServedRound(currentCustomer, orderDuration);
            customerSpawner.MarkServed(currentCustomer);

            sceneContext.SetFeedback($"{currentRecipe.DisplayName} served", ColorPalette.HighlightGood);
            sceneContext.SetStatus("Looking for next ticket.");
            drinkFlowController.Reset();
            currentCustomer = null;
            currentRecipe = null;
            ResetDrinkVisuals();
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
                drinkFlowController.Reset();
                SetState(GameState.WaitingForOrder);
            }
        }

        private void CompleteStage()
        {
            stageManager.MarkStageComplete();
            CancelCurrentOrder();

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

        private void ResetDrinkVisuals()
        {
            sceneContext.SetMachineVisual(SpriteAssetNames.MachineEmpty, config.MachineSize, config.MachineColor);
            sceneContext.CupRoot.position = sceneContext.CupAnchorPosition;
            sceneContext.CupRoot.gameObject.SetActive(false);
            sceneContext.SetCupVisual(SpriteAssetNames.CupPlasticEmpty, config.CupSize, config.CupEmptyColor);
            sceneContext.ShotGlassRoot.position = sceneContext.ShotGlassPosition;
            sceneContext.ShotGlassRoot.gameObject.SetActive(false);
            sceneContext.SetShotGlassVisual(SpriteAssetNames.ShotGlassEmpty, config.ShotGlassSize, config.CupEspressoColor);
        }

        private void CancelCurrentOrder()
        {
            grindingMechanic.CancelStep();
            tampingMechanic.CancelStep();
            extractionMechanic.CancelStep();
            pourMechanic.CancelStep();
            ingredientMechanic.CancelStep();
            servingMechanic.CancelStep();
            portafilterMechanic.CancelStep();

            if (cupSetupCoroutine != null)
            {
                StopCoroutine(cupSetupCoroutine);
                cupSetupCoroutine = null;
            }

            if (currentCustomer != null)
            {
                customerSpawner.ReleaseClaim(currentCustomer);
            }

            ResetDrinkVisuals();
            sceneContext.SetActiveOrder("No active order");
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

        private static MechanicScoreResult EvaluateLockResult(float durationSeconds)
        {
            if (durationSeconds <= 2f)
            {
                return new MechanicScoreResult("Lock", QualityGrade.Perfect, ScoreRules.PortafilterLockPerfectScore, durationSeconds);
            }

            if (durationSeconds <= 4f)
            {
                return new MechanicScoreResult("Lock", QualityGrade.Good, ScoreRules.PortafilterLockGoodScore, durationSeconds);
            }

            return new MechanicScoreResult("Lock", QualityGrade.Bad, ScoreRules.PortafilterLockBadScore, durationSeconds);
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
