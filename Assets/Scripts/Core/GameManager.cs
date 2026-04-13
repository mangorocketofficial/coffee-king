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
        StageSelect,
        WaitingForOrder,
        GrindingStep,
        TampingStep,
        PortafilterStep,
        ExtractionStep,
        SteamMilkStep,
        PourShotStep,
        IngredientStep,
        LidStep,
        ServingStep,
        StageResult,
        Paused
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
        private SteamMilkMechanic steamMilkMechanic;
        private PourMechanic pourMechanic;
        private IngredientMechanic ingredientMechanic;
        private HotWaterCupMechanic hotWaterCupMechanic;
        private LidMechanic lidMechanic;
        private ServingMechanic servingMechanic;
        private ScoreManager scoreManager;
        private CustomerSpawner customerSpawner;
        private ResultScreenView resultScreenView;
        private StageManager stageManager;
        private UIContext uiContext;
        private DrinkFlowController drinkFlowController;

        private SaveManager saveManager;
        private GameState stateBeforePause;
        private bool settingsOpenedFromPause;

        private Customer currentCustomer;
        private DrinkRecipe currentRecipe;
        private Coroutine introCoroutine;
        private Coroutine cupSetupCoroutine;
        private Coroutine tutorialHideCoroutine;
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
            if (!initialized || stageManager == null)
            {
                return;
            }

            if (State == GameState.Paused)
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

                if (currentCustomer == null && customerSpawner.IsFinished)
                {
                    CompleteStage();
                }
                else if (stageManager.HasTimeExpired)
                {
                    HandleStageTimeExpired();
                }
                else if (currentCustomer == null && customerSpawner.TryClaimNextCustomer(out var nextCustomer))
                {
                    BeginOrder(nextCustomer);
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

            if (steamMilkMechanic != null)
            {
                steamMilkMechanic.Completed -= HandleSteamMilkCompleted;
            }

            if (pourMechanic != null)
            {
                pourMechanic.Completed -= HandlePourCompleted;
            }

            if (ingredientMechanic != null)
            {
                ingredientMechanic.Completed -= HandleIngredientCompleted;
            }

            if (hotWaterCupMechanic != null)
            {
                hotWaterCupMechanic.Completed -= HandleHotWaterCupCompleted;
            }

            if (lidMechanic != null)
            {
                lidMechanic.Completed -= HandleLidCompleted;
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
                uiContext.StageSelectView.StageSelected -= HandleStageSelected;
                uiContext.StageSelectView.BackRequested -= HandleStageSelectBackRequested;
                uiContext.StageSelectView.Dispose();
                uiContext.HUDView.PauseRequested -= HandlePauseRequested;
                uiContext.HUDView.Dispose();
                uiContext.PauseOverlayView.ResumeRequested -= HandleResumeRequested;
                uiContext.PauseOverlayView.RestartRequested -= HandlePauseRestartRequested;
                uiContext.PauseOverlayView.MainMenuRequested -= HandlePauseMainMenuRequested;
                uiContext.PauseOverlayView.SettingsRequested -= HandlePauseSettingsRequested;
                uiContext.PauseOverlayView.Dispose();
                uiContext.TitleScreenView.SettingsRequested -= HandleTitleSettingsRequested;
                uiContext.SettingsView.CloseRequested -= HandleSettingsClosed;
                uiContext.SettingsView.BgmVolumeChanged -= HandleBgmVolumeChanged;
                uiContext.SettingsView.SfxVolumeChanged -= HandleSfxVolumeChanged;
                uiContext.SettingsView.VibrationChanged -= HandleVibrationChanged;
                uiContext.SettingsView.Dispose();
            }

            if (introCoroutine != null)
            {
                StopCoroutine(introCoroutine);
            }

            if (cupSetupCoroutine != null)
            {
                StopCoroutine(cupSetupCoroutine);
            }

            if (tutorialHideCoroutine != null)
            {
                StopCoroutine(tutorialHideCoroutine);
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

            saveManager = new SaveManager();

            config = GameConfig.CreateRuntimeDefault();
            sceneContext = new GameSceneBuilder().Build(config);
            scoreManager = new ScoreManager();
            drinkFlowController = new DrinkFlowController();

            var recipes = DrinkLibrary.CreateHotDrinksSet(config);
            stageManager = new StageManager(recipes);

            gestureDetector = CreateSubsystem<GestureDetector>("[GestureDetector]");
            audioManager = CreateSubsystem<AudioManager>("[AudioManager]");
            grindingMechanic = CreateSubsystem<GrindingMechanic>("[GrindingMechanic]");
            tampingMechanic = CreateSubsystem<TampingMechanic>("[TampingMechanic]");
            portafilterMechanic = CreateSubsystem<PortafilterMechanic>("[PortafilterMechanic]");
            extractionMechanic = CreateSubsystem<ExtractionMechanic>("[ExtractionMechanic]");
            steamMilkMechanic = CreateSubsystem<SteamMilkMechanic>("[SteamMilkMechanic]");
            pourMechanic = CreateSubsystem<PourMechanic>("[PourMechanic]");
            ingredientMechanic = CreateSubsystem<IngredientMechanic>("[IngredientMechanic]");
            hotWaterCupMechanic = CreateSubsystem<HotWaterCupMechanic>("[HotWaterCupMechanic]");
            lidMechanic = CreateSubsystem<LidMechanic>("[LidMechanic]");
            servingMechanic = CreateSubsystem<ServingMechanic>("[ServingMechanic]");

            grindingMechanic.Initialize(config, gestureDetector, sceneContext);
            tampingMechanic.Initialize(config, gestureDetector, sceneContext);
            portafilterMechanic.Initialize(config, gestureDetector, sceneContext);
            extractionMechanic.Initialize(config, gestureDetector, sceneContext);
            steamMilkMechanic.Initialize(config, gestureDetector, sceneContext);
            pourMechanic.Initialize(config, gestureDetector, sceneContext);
            ingredientMechanic.Initialize(gestureDetector, sceneContext);
            hotWaterCupMechanic.Initialize(config, gestureDetector, sceneContext);
            lidMechanic.Initialize(config, gestureDetector, sceneContext);
            servingMechanic.Initialize(config, gestureDetector, sceneContext);

            grindingMechanic.Completed += HandleGrindingCompleted;
            tampingMechanic.Completed += HandleTampingCompleted;
            portafilterMechanic.Locked += HandlePortafilterLocked;
            extractionMechanic.Completed += HandleExtractionCompleted;
            steamMilkMechanic.Completed += HandleSteamMilkCompleted;
            pourMechanic.Completed += HandlePourCompleted;
            ingredientMechanic.Completed += HandleIngredientCompleted;
            hotWaterCupMechanic.Completed += HandleHotWaterCupCompleted;
            lidMechanic.Completed += HandleLidCompleted;
            servingMechanic.Served += HandleServed;

            customerSpawner = new CustomerSpawner();
            customerSpawner.Initialize(config, sceneContext);
            customerSpawner.CustomerTimedOut += HandleCustomerTimedOut;

            resultScreenView = ResultScreenView.Create(sceneContext.OverlayLayer, gestureDetector);
            resultScreenView.NextRequested += HandleNextStageRequested;
            resultScreenView.RetryRequested += HandleRetryRequested;

            uiContext = UIBuilder.Build(transform);
            sceneContext.HUDView = uiContext.HUDView;
            uiContext.TitleScreenView.StartRequested += HandleTitleStartRequested;
            uiContext.StageSelectView.StageSelected += HandleStageSelected;
            uiContext.StageSelectView.BackRequested += HandleStageSelectBackRequested;
            uiContext.HUDView.PauseRequested += HandlePauseRequested;
            uiContext.PauseOverlayView.ResumeRequested += HandleResumeRequested;
            uiContext.PauseOverlayView.RestartRequested += HandlePauseRestartRequested;
            uiContext.PauseOverlayView.MainMenuRequested += HandlePauseMainMenuRequested;
            uiContext.PauseOverlayView.SettingsRequested += HandlePauseSettingsRequested;
            uiContext.TitleScreenView.SettingsRequested += HandleTitleSettingsRequested;
            uiContext.SettingsView.CloseRequested += HandleSettingsClosed;
            uiContext.SettingsView.BgmVolumeChanged += HandleBgmVolumeChanged;
            uiContext.SettingsView.SfxVolumeChanged += HandleSfxVolumeChanged;
            uiContext.SettingsView.VibrationChanged += HandleVibrationChanged;

            ApplySavedSettings();

            initialized = true;
            ShowTitleScreen();
        }

        private void ShowTitleScreen()
        {
            HideTutorialHint();
            CancelCurrentOrder();
            resultScreenView.Hide();
            uiContext.HUDView.SetVisible(false);
            uiContext.TutorialOverlay.Hide();
            uiContext.PauseOverlayView.Hide();
            uiContext.StageSelectView.Hide();
            uiContext.SettingsView.Hide();
            uiContext.TitleScreenView.Show($"Day {saveManager.NextDay}");

            sceneContext.SetInstruction("Coffee King");
            sceneContext.SetStatus("Press start");
            sceneContext.SetFeedback(string.Empty, ColorPalette.HighlightGood);
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
            uiContext.ScreenFader.FadeTransition(() =>
            {
                uiContext.TitleScreenView.Hide();
                StartStage(stageManager.StartDay(saveManager.NextDay));
            });
        }

        private void ShowStageSelect()
        {
            HideTutorialHint();
            CancelCurrentOrder();
            resultScreenView.Hide();
            uiContext.HUDView.SetVisible(false);
            uiContext.TitleScreenView.Hide();
            uiContext.TutorialOverlay.Hide();
            uiContext.PauseOverlayView.Hide();
            uiContext.SettingsView.Hide();

            currentCustomer = null;
            currentRecipe = null;
            drinkFlowController.Reset();

            sceneContext.SetInstruction("Select a day");
            sceneContext.SetStatus("Choose your day");
            sceneContext.SetFeedback(string.Empty, config.SecondaryTextColor);
            sceneContext.SetActiveOrder(string.Empty);
            sceneContext.SetQueueSummary(string.Empty);
            sceneContext.SetScore(string.Empty);
            sceneContext.SetRound(string.Empty);

            uiContext.StageSelectView.Show();
            SetState(GameState.StageSelect);
        }

        private void HandleStageSelected(int stageIndex)
        {
            // Stage select is no longer used in the flow, but handler kept for compatibility
            audioManager.PlayUiClick();
            uiContext.ScreenFader.FadeTransition(() =>
            {
                uiContext.StageSelectView.Hide();
                StartStage(stageManager.StartDay(stageIndex + 1));
            });
        }

        private void HandleStageSelectBackRequested()
        {
            audioManager.PlayUiClick();
            uiContext.ScreenFader.FadeTransition(() =>
            {
                uiContext.StageSelectView.Hide();
                ShowTitleScreen();
            });
        }

        private void StartStage(StageData stage)
        {
            if (introCoroutine != null)
            {
                StopCoroutine(introCoroutine);
            }

            HideTutorialHint();
            CancelCurrentOrder();
            resultScreenView.Hide();
            uiContext.HUDView.SetVisible(true);
            uiContext.TitleScreenView.Hide();
            uiContext.StageSelectView.Hide();
            uiContext.TutorialOverlay.Hide();
            uiContext.PauseOverlayView.Hide();

            currentCustomer = null;
            currentRecipe = null;

            audioManager.StartBgm();
            customerSpawner.BeginStage(stage, System.Environment.TickCount ^ stage.Number);
            scoreManager.BeginStage(stage, customerSpawner.PlannedCustomers);

            sceneContext.SetRound($"{stage.DisplayName}   Customers {stage.CustomerCount}   Cap {stage.MaxSimultaneousCustomers}");
            sceneContext.SetInstruction(stage.DisplayName);
            sceneContext.SetStatus("Day starting");
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
            sceneContext.SetInstruction("Make drinks before patience runs out.");
            sceneContext.SetStatus("Queue open");
            sceneContext.SetFeedback(string.Empty, config.SecondaryTextColor);
            SetState(GameState.WaitingForOrder);
        }

        private void BeginOrder(Customer customer)
        {
            audioManager.PlayCustomerArrival();
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
            HideTutorialHint();

            switch (state)
            {
                case DrinkFlowState.MoveToGrinder:
                    sceneContext.SetActiveOrder($"Now making\n{currentRecipe.DisplayName}");
                    sceneContext.SetInstruction("Drag the empty portafilter to the grinder.");
                    sceneContext.SetStatus("Move to grinder, then hold to grind.");
                    sceneContext.SetFeedback(string.Empty, config.SecondaryTextColor);
                    audioManager.StartGrindingLoop();
                    grindingMechanic.BeginStep();
                    SetState(GameState.GrindingStep);
                    TryShowTutorialHint(state);
                    break;

                case DrinkFlowState.Tamping:
                    sceneContext.SetInstruction("Hold the tamper and release in the green zone.");
                    sceneContext.SetStatus("Compress the coffee bed.");
                    tampingMechanic.BeginStep();
                    SetState(GameState.TampingStep);
                    TryShowTutorialHint(state);
                    break;

                case DrinkFlowState.PortafilterLocking:
                    sceneContext.SetInstruction("Drag the tamped portafilter to the machine and rotate to lock.");
                    sceneContext.SetStatus("Lock the portafilter into the machine.");
                    sceneContext.SetMachineVisual(SpriteAssetNames.MachineEmpty, config.MachineSize, config.MachineColor);
                    lockStepStartTime = Time.time;
                    portafilterMechanic.BeginRound();
                    SetState(GameState.PortafilterStep);
                    TryShowTutorialHint(state);
                    break;

                case DrinkFlowState.Extracting:
                    sceneContext.SetInstruction("Tap to start extraction, then tap in the green zone.");
                    sceneContext.SetStatus("Pull the espresso shot.");
                    extractionMechanic.BeginStep();
                    audioManager.StartExtractionLoop();
                    SetState(GameState.ExtractionStep);
                    TryShowTutorialHint(state);
                    break;

                case DrinkFlowState.SteamMilk:
                    sceneContext.SetInstruction("Drag the steam wand into the pitcher, move it up or down, then tap to stop.");
                    sceneContext.SetStatus("Steam the milk for the latte.");
                    steamMilkMechanic.BeginStep();
                    audioManager.StartSteamLoop();
                    SetState(GameState.SteamMilkStep);
                    TryShowTutorialHint(state);
                    break;

                case DrinkFlowState.CupSetup:
                    sceneContext.SetInstruction(currentRecipe.CupType == CupType.Plastic ? "Set up the iced cup." : "Set up the hot cup.");
                    sceneContext.SetStatus(currentRecipe.CupType == CupType.Plastic ? "Cup and ice are being prepared." : "Hot cup is being prepared.");
                    if (cupSetupCoroutine != null)
                    {
                        StopCoroutine(cupSetupCoroutine);
                    }

                    cupSetupCoroutine = StartCoroutine(RunCupSetup());
                    break;

                case DrinkFlowState.PourShotToCup:
                    sceneContext.SetInstruction("Drag the shot glass into the cup.");
                    sceneContext.SetStatus("Pour the espresso shot.");
                    pourMechanic.BeginStep();
                    SetState(GameState.PourShotStep);
                    TryShowTutorialHint(state);
                    break;

                case DrinkFlowState.PourIngredient:
                    if (RequiresHotWaterPour())
                    {
                        sceneContext.SetInstruction("Drag the empty water cup to the hot water dispenser.");
                        sceneContext.SetStatus("Fill the cup, then tap the full water cup on the dispenser.");
                        hotWaterCupMechanic.BeginStep();
                    }
                    else
                    {
                        sceneContext.SetInstruction(currentRecipe.IngredientType == IngredientType.Milk ? "Tap the milk pitcher to pour milk." : "Tap the water bottle.");
                        sceneContext.SetStatus("Add the final liquid.");
                        ingredientMechanic.BeginStep(ResolveIngredientRenderer());
                    }
                    SetState(GameState.IngredientStep);
                    TryShowTutorialHint(state);
                    break;

                case DrinkFlowState.Lid:
                    sceneContext.SetInstruction("Drag the lid onto the cup.");
                    sceneContext.SetStatus("Seal the drink.");
                    sceneContext.LidRenderer.sprite = SpriteFactory.Load(ResolveLidAsset(), ResolveLidSize(), config.CupEmptyColor);
                    sceneContext.LidRenderer.color = Color.white;
                    lidMechanic.BeginStep();
                    SetState(GameState.LidStep);
                    TryShowTutorialHint(state);
                    break;

                case DrinkFlowState.Serving:
                    sceneContext.SetInstruction("Drag the finished drink to the customer.");
                    sceneContext.SetStatus($"Serve the {currentRecipe.DisplayName.ToLowerInvariant()}.");
                    servingMechanic.BeginStep();
                    SetState(GameState.ServingStep);
                    TryShowTutorialHint(state);
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

            if (currentRecipe.CupType == CupType.Plastic)
            {
                sceneContext.SetCupVisual(SpriteAssetNames.CupPlasticEmpty, config.CupSize, config.CupEmptyColor);
                yield return new WaitForSeconds(0.35f);
                sceneContext.SetCupVisual(SpriteAssetNames.CupPlasticIce, config.CupSize, config.CupEmptyColor);
                yield return new WaitForSeconds(0.35f);
            }
            else
            {
                sceneContext.SetCupVisual(SpriteAssetNames.CupHotEmpty, config.CupSize, config.CupEmptyColor);
                yield return new WaitForSeconds(0.25f);
            }

            cupSetupCoroutine = null;
            TransitionDrinkState(drinkFlowController.Advance());
        }

        private void HandleGrindingCompleted(MechanicScoreResult result)
        {
            if (currentCustomer == null || State != GameState.GrindingStep)
            {
                return;
            }

            audioManager.StopLoop();
            audioManager.PlayGauge(result.Grade);
            scoreManager.AddResult(result);
            sceneContext.SetFeedback($"{FormatGrade(result.Grade)} grinding {result.MeasuredValue:0.0}", GradeToColor(result.Grade));
            ShowGradeFeedback(result);
            TransitionDrinkState(drinkFlowController.Advance());
        }

        private void HandleTampingCompleted(MechanicScoreResult result)
        {
            if (currentCustomer == null || State != GameState.TampingStep)
            {
                return;
            }

            audioManager.PlayTampImpact();
            audioManager.PlayGauge(result.Grade);
            scoreManager.AddResult(result);
            sceneContext.SetFeedback($"{FormatGrade(result.Grade)} tamping {result.MeasuredValue:0.0}", GradeToColor(result.Grade));
            ShowGradeFeedback(result);
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
            ShowGradeFeedback(result);
            TransitionDrinkState(drinkFlowController.Advance());
        }

        private void HandleExtractionCompleted(MechanicScoreResult result)
        {
            if (currentCustomer == null || State != GameState.ExtractionStep)
            {
                return;
            }

            audioManager.StopLoop();
            audioManager.PlayGauge(result.Grade);
            scoreManager.AddResult(result);
            sceneContext.SetFeedback($"{FormatGrade(result.Grade)} extraction {result.MeasuredValue:0.0}", GradeToColor(result.Grade));
            ShowGradeFeedback(result);
            TransitionDrinkState(drinkFlowController.Advance());
        }

        private void HandleSteamMilkCompleted(MechanicScoreResult result)
        {
            if (currentCustomer == null || State != GameState.SteamMilkStep)
            {
                return;
            }

            audioManager.StopLoop();
            audioManager.PlayGauge(result.Grade);
            scoreManager.AddResult(result);
            sceneContext.SetFeedback($"{FormatGrade(result.Grade)} steam {result.MeasuredValue:0.0}C", GradeToColor(result.Grade));
            ShowGradeFeedback(result);
            TransitionDrinkState(drinkFlowController.Advance());
        }

        private void HandlePourCompleted()
        {
            if (currentCustomer == null)
            {
                return;
            }

            audioManager.PlayPour();
            if (State == GameState.PourShotStep)
            {
                if (currentRecipe.CupType == CupType.Plastic)
                {
                    sceneContext.SetCupVisual(SpriteAssetNames.CupPlasticShot, config.CupSize, currentRecipe.BaseCupColor);
                }

                sceneContext.SetFeedback("Shot poured into the cup", ColorPalette.HighlightGood);
                TransitionDrinkState(drinkFlowController.Advance());
                return;
            }
        }

        private void HandleIngredientCompleted()
        {
            if (currentCustomer == null || State != GameState.IngredientStep)
            {
                return;
            }

            audioManager.PlayPour();
            sceneContext.SetCupVisual(ResolveFinalCupAsset(), config.CupSize, currentRecipe.FinalCupColor);
            sceneContext.SetFeedback(currentRecipe.IngredientType == IngredientType.Milk ? "Milk added" : "Water added", ColorPalette.HighlightGood);
            TransitionDrinkState(drinkFlowController.Advance());
        }

        private void HandleHotWaterCupCompleted()
        {
            if (currentCustomer == null || State != GameState.IngredientStep || !RequiresHotWaterPour())
            {
                return;
            }

            audioManager.PlayPour();
            sceneContext.SetCupVisual(ResolveFinalCupAsset(), config.CupSize, currentRecipe.FinalCupColor);
            sceneContext.SetFeedback("Hot water added", ColorPalette.HighlightGood);
            TransitionDrinkState(drinkFlowController.Advance());
        }

        private void HandleLidCompleted()
        {
            if (currentCustomer == null || State != GameState.LidStep)
            {
                return;
            }

            audioManager.PlayLidSnap();
            sceneContext.SetCupVisual(ResolveLiddedCupAsset(), config.CupSize, currentRecipe.FinalCupColor);
            sceneContext.SetFeedback("Lid placed", ColorPalette.HighlightGood);
            TransitionDrinkState(drinkFlowController.Advance());
        }

        private void HandleServed()
        {
            if (currentCustomer == null || State != GameState.ServingStep)
            {
                return;
            }

            var madeRecipe = drinkFlowController.CurrentRecipe;
            var orderedRecipe = currentCustomer.Order;
            var isWrongDrink = madeRecipe != null && orderedRecipe != null && madeRecipe.Id != orderedRecipe.Id;

            if (isWrongDrink)
            {
                audioManager.PlayCustomerTimeout();
                scoreManager.RegisterWrongDrink(currentCustomer);
                customerSpawner.MarkRejected(currentCustomer);

                sceneContext.SetFeedback("Wrong drink!", ColorPalette.HighlightBad);
                sceneContext.SetStatus("Looking for next ticket.");
                drinkFlowController.Reset();
                currentCustomer = null;
                currentRecipe = null;
                ResetDrinkVisuals();
                SetState(GameState.WaitingForOrder);
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
            audioManager.PlayCustomerTimeout();
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

        private void HandleStageTimeExpired()
        {
            HideTutorialHint();
            CancelCurrentOrder();

            var unresolvedCustomers = customerSpawner.ResolveRemainingCustomersAsTimedOut();
            for (var index = 0; index < unresolvedCustomers.Count; index++)
            {
                scoreManager.RegisterTimeout(unresolvedCustomers[index], "missed at closing");
            }

            currentCustomer = null;
            currentRecipe = null;
            drinkFlowController.Reset();
            CompleteStage(StageEndReason.TimeExpired);
        }

        private void CompleteStage(StageEndReason endReason = StageEndReason.ClearedOrders)
        {
            HideTutorialHint();
            stageManager.MarkStageComplete();
            CancelCurrentOrder();

            if (endReason == StageEndReason.ClearedOrders)
            {
                scoreManager.FinalizeNoMistakeBonus();
            }

            var stars = StarRating.FromScore(scoreManager.StageScore, scoreManager.StageMaxScore);

            // Record day result and earnings
            var dailyEarnings = scoreManager.DailyEarnings;
            saveManager.RecordDayResult(stageManager.CurrentDayNumber, dailyEarnings);
            var totalEarnings = saveManager.TotalEarnings;

            var result = stageManager.BuildResult(scoreManager.StageScore, scoreManager.StageMaxScore, stars, endReason, dailyEarnings, totalEarnings);

            audioManager.PlayStageComplete();

            uiContext.HUDView.SetVisible(false);
            sceneContext.SetInstruction("Day complete");
            sceneContext.SetStatus(endReason == StageEndReason.TimeExpired ? "Closing time! See your results." : "Great work! See your results.");
            sceneContext.SetFeedback("Day complete", ColorPalette.HighlightGood);
            sceneContext.SetActiveOrder("Result screen open");

            var resultReason = endReason == StageEndReason.TimeExpired
                ? "Closing time! Unresolved orders were missed."
                : "All customers resolved!";
            var noMistakeLine = scoreManager.NoMistakeBonusAwarded > 0
                ? $"\nNo Mistake Bonus: +{scoreManager.NoMistakeBonusAwarded}"
                : string.Empty;
            var summary =
                $"{resultReason}\n" +
                $"Served {scoreManager.ServedCount}   Timed Out {scoreManager.TimedOutCount}{noMistakeLine}\n" +
                scoreManager.StageSummary;

            uiContext.ScreenFader.FadeTransition(() =>
            {
                resultScreenView.Show(result, summary);
                SetState(GameState.StageResult);
            });
        }

        private void HandleNextStageRequested()
        {
            audioManager.PlayUiClick();
            uiContext.ScreenFader.FadeTransition(() =>
            {
                resultScreenView.Hide();
                StartStage(stageManager.StartNextDay());
            });
        }

        private void HandleRetryRequested()
        {
            audioManager.PlayUiClick();
            uiContext.ScreenFader.FadeTransition(() =>
            {
                resultScreenView.Hide();
                StartStage(stageManager.RetryCurrentDay());
            });
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
            sceneContext.LidRoot.position = sceneContext.LidPosition;
            sceneContext.LidRoot.gameObject.SetActive(false);
            sceneContext.LidRenderer.sprite = SpriteFactory.Load(SpriteAssetNames.DomeLid, config.IcedLidSize, config.CupEmptyColor);
            sceneContext.LidRenderer.color = Color.white;
            if (sceneContext.PitcherRenderer != null)
            {
                sceneContext.PitcherRenderer.sprite = SpriteFactory.Load(SpriteAssetNames.Pitcher, config.PitcherSize, config.PitcherColor);
                sceneContext.PitcherRenderer.color = Color.white;
            }
        }

        private void CancelCurrentOrder()
        {
            HideTutorialHint();
            audioManager.StopLoop();
            grindingMechanic.CancelStep();
            tampingMechanic.CancelStep();
            extractionMechanic.CancelStep();
            steamMilkMechanic.CancelStep();
            pourMechanic.CancelStep();
            ingredientMechanic.CancelStep();
            hotWaterCupMechanic.CancelStep();
            lidMechanic.CancelStep();
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

            var hudVisible = State != GameState.TitleScreen && State != GameState.StageSelect && State != GameState.StageResult;
            uiContext.HUDView.SetVisible(hudVisible);
            uiContext.HUDView.SetScore(
                $"Score {scoreManager.StageScore}/{scoreManager.StageMaxScore}\n" +
                $"Served {scoreManager.ServedCount}   Timeout {scoreManager.TimedOutCount}");
            uiContext.HUDView.SetStage($"{stageManager.CurrentStage.DisplayName}   {stageManager.FlowState}");
            uiContext.HUDView.SetTimer(stageManager.TimeRemaining);
            uiContext.HUDView.SetEarnings($"Today {ScoreManager.FormatWon(scoreManager.DailyEarnings)}");
            uiContext.HUDView.SetOrderList(customerSpawner.GetQueueSummary());
            uiContext.HUDView.SetProgress((scoreManager.ServedCount + scoreManager.TimedOutCount) / (float)stageManager.CurrentStage.CustomerCount);

            sceneContext.SetQueueSummary(customerSpawner.GetQueueSummary());
            sceneContext.SetScore(
                $"Score {scoreManager.StageScore}/{scoreManager.StageMaxScore}\n" +
                $"Served {scoreManager.ServedCount}   Timeout {scoreManager.TimedOutCount}\n" +
                $"Earnings {ScoreManager.FormatWon(scoreManager.DailyEarnings)}\n" +
                scoreManager.Breakdown);
            sceneContext.SetRound($"{stageManager.CurrentStage.DisplayName}   {stageManager.FlowState}");
        }

        private SpriteRenderer ResolveIngredientRenderer()
        {
            if (currentRecipe.IngredientType == IngredientType.Milk)
            {
                return sceneContext.PitcherRenderer;
            }

            return currentRecipe.CupType == CupType.Hot
                ? sceneContext.HotWaterDispenserRenderer
                : sceneContext.WaterBottleRenderer;
        }

        private bool RequiresHotWaterPour()
        {
            return currentRecipe != null &&
                currentRecipe.CupType == CupType.Hot &&
                currentRecipe.IngredientType == IngredientType.Water;
        }

        private string ResolveFinalCupAsset()
        {
            if (currentRecipe.CupType == CupType.Hot)
            {
                return currentRecipe.IngredientType == IngredientType.Milk
                    ? SpriteAssetNames.CupHotLatte
                    : SpriteAssetNames.CupHotAmericano;
            }

            return currentRecipe.IngredientType == IngredientType.Milk
                ? SpriteAssetNames.CupPlasticLatte
                : SpriteAssetNames.CupPlasticAmericano;
        }

        private string ResolveLiddedCupAsset()
        {
            if (currentRecipe.CupType == CupType.Hot)
            {
                return currentRecipe.IngredientType == IngredientType.Milk
                    ? SpriteAssetNames.CupHotLatteLidded
                    : SpriteAssetNames.CupHotAmericanoLidded;
            }

            return currentRecipe.IngredientType == IngredientType.Milk
                ? SpriteAssetNames.CupLatteLidded
                : SpriteAssetNames.CupAmericanoLidded;
        }

        private string ResolveLidAsset()
        {
            return currentRecipe != null && currentRecipe.CupType == CupType.Hot
                ? SpriteAssetNames.HotLid
                : SpriteAssetNames.DomeLid;
        }

        private Vector2 ResolveLidSize()
        {
            return currentRecipe != null && currentRecipe.CupType == CupType.Hot
                ? config.HotLidSize
                : config.IcedLidSize;
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

        private void ShowGradeFeedback(MechanicScoreResult result)
        {
            // Score popup
            uiContext.HUDView.ShowScorePopup(result.Score, result.Grade);

            // Grade-specific screen flash
            if (result.Grade == QualityGrade.Perfect)
            {
                uiContext.ScreenFader.Flash(new Color(1f, 0.85f, 0.3f), 0.2f); // golden flash
            }
            else if (result.Grade == QualityGrade.Bad)
            {
                uiContext.ScreenFader.Flash(new Color(0.8f, 0.2f, 0.15f), 0.15f); // red flash
            }
        }

        private void TryShowTutorialHint(DrinkFlowState state)
        {
            if (uiContext?.TutorialOverlay == null ||
                stageManager?.CurrentStage == null ||
                stageManager.CurrentStage.Number != 1 ||
                currentCustomer == null)
            {
                return;
            }

            switch (state)
            {
                case DrinkFlowState.MoveToGrinder:
                    if (saveManager.HasSeenTutorial("grinder"))
                    {
                        return;
                    }

                    saveManager.MarkTutorialSeen("grinder");
                    ShowTutorialHint(
                        "Move To Grinder",
                        "Drag the portafilter to the grinder, then hold on the grinder to fill it.",
                        new Vector2(-360f, 340f),
                        new Vector2(-210f, 200f),
                        "v");
                    break;

                case DrinkFlowState.Tamping:
                    if (saveManager.HasSeenTutorial("gauge"))
                    {
                        return;
                    }

                    saveManager.MarkTutorialSeen("gauge");
                    ShowTutorialHint(
                        "Stop In The Green",
                        "Release inside the green zone for stronger timing scores.",
                        new Vector2(0f, 340f),
                        new Vector2(0f, 250f),
                        "v");
                    break;

                case DrinkFlowState.PortafilterLocking:
                    if (saveManager.HasSeenTutorial("lock"))
                    {
                        return;
                    }

                    saveManager.MarkTutorialSeen("lock");
                    ShowTutorialHint(
                        "Rotate To Lock",
                        "Move the tamped portafilter to the machine, then rotate until it locks in place.",
                        new Vector2(-20f, 340f),
                        new Vector2(180f, 200f),
                        "v");
                    break;

                case DrinkFlowState.Extracting:
                    if (saveManager.HasSeenTutorial("extraction"))
                    {
                        return;
                    }

                    saveManager.MarkTutorialSeen("extraction");
                    ShowTutorialHint(
                        "Tap To Extract",
                        "Tap the button to start brewing, then tap again in the green zone to stop.",
                        new Vector2(0f, 340f),
                        new Vector2(0f, 250f),
                        "v");
                    break;

                case DrinkFlowState.SteamMilk:
                    if (saveManager.HasSeenTutorial("steam"))
                    {
                        return;
                    }

                    saveManager.MarkTutorialSeen("steam");
                    ShowTutorialHint(
                        "Steam The Milk",
                        "Drag the steam wand into the pitcher, then move it up or down to control heat. Tap to stop in the green zone.",
                        new Vector2(200f, 340f),
                        new Vector2(200f, 200f),
                        "v");
                    break;

                case DrinkFlowState.PourShotToCup:
                    if (saveManager.HasSeenTutorial("pour_shot"))
                    {
                        return;
                    }

                    saveManager.MarkTutorialSeen("pour_shot");
                    ShowTutorialHint(
                        "Pour The Shot",
                        "Drag the shot glass over the cup and release to pour the espresso.",
                        new Vector2(0f, 340f),
                        new Vector2(0f, 200f),
                        "v");
                    break;

                case DrinkFlowState.PourIngredient:
                    if (saveManager.HasSeenTutorial("ingredient"))
                    {
                        return;
                    }

                    saveManager.MarkTutorialSeen("ingredient");
                    ShowTutorialHint(
                        "Add Ingredient",
                        "Tap the ingredient source to pour it into the cup.",
                        new Vector2(-300f, 340f),
                        new Vector2(-300f, 200f),
                        "v");
                    break;

                case DrinkFlowState.Lid:
                    if (saveManager.HasSeenTutorial("lid"))
                    {
                        return;
                    }

                    saveManager.MarkTutorialSeen("lid");
                    ShowTutorialHint(
                        "Place The Lid",
                        "Drag the lid onto the top of the cup to seal the drink.",
                        new Vector2(-300f, 340f),
                        new Vector2(-180f, 200f),
                        "v");
                    break;

                case DrinkFlowState.Serving:
                    if (saveManager.HasSeenTutorial("serving"))
                    {
                        return;
                    }

                    saveManager.MarkTutorialSeen("serving");
                    ShowTutorialHint(
                        "Serve The Drink",
                        "Drag the finished drink to the serving area near the customer.",
                        new Vector2(300f, 340f),
                        new Vector2(300f, 200f),
                        "v");
                    break;
            }
        }

        private void ShowTutorialHint(string title, string body, Vector2 panelOffset, Vector2 arrowOffset, string arrowSymbol)
        {
            HideTutorialHint();
            uiContext.TutorialOverlay.Show(title, body, panelOffset, arrowOffset, arrowSymbol);
            tutorialHideCoroutine = StartCoroutine(HideTutorialHintAfterDelay(4f));
        }

        private IEnumerator HideTutorialHintAfterDelay(float delaySeconds)
        {
            yield return new WaitForSeconds(delaySeconds);
            tutorialHideCoroutine = null;
            uiContext?.TutorialOverlay.Hide();
        }

        private void HideTutorialHint()
        {
            if (tutorialHideCoroutine != null)
            {
                StopCoroutine(tutorialHideCoroutine);
                tutorialHideCoroutine = null;
            }

            uiContext?.TutorialOverlay.Hide();
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

        private bool IsGameplayState(GameState state)
        {
            switch (state)
            {
                case GameState.WaitingForOrder:
                case GameState.GrindingStep:
                case GameState.TampingStep:
                case GameState.PortafilterStep:
                case GameState.ExtractionStep:
                case GameState.SteamMilkStep:
                case GameState.PourShotStep:
                case GameState.IngredientStep:
                case GameState.LidStep:
                case GameState.ServingStep:
                    return true;
                default:
                    return false;
            }
        }

        private void PauseGame()
        {
            if (State == GameState.Paused || !IsGameplayState(State))
            {
                return;
            }

            stateBeforePause = State;
            SetState(GameState.Paused);
            Time.timeScale = 0f;
            uiContext.PauseOverlayView.Show();
        }

        private void ResumeGame()
        {
            if (State != GameState.Paused)
            {
                return;
            }

            uiContext.PauseOverlayView.Hide();
            Time.timeScale = 1f;
            SetState(stateBeforePause);
        }

        private void HandlePauseRequested()
        {
            PauseGame();
        }

        private void HandleResumeRequested()
        {
            ResumeGame();
        }

        private void HandlePauseRestartRequested()
        {
            if (State != GameState.Paused)
            {
                return;
            }

            uiContext.PauseOverlayView.Hide();
            Time.timeScale = 1f;
            SetState(stateBeforePause);
            HandleRetryRequested();
        }

        private void HandlePauseMainMenuRequested()
        {
            if (State != GameState.Paused)
            {
                return;
            }

            Time.timeScale = 1f;
            uiContext.ScreenFader.FadeTransition(() =>
            {
                uiContext.PauseOverlayView.Hide();
                ShowTitleScreen();
            });
        }

        private void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus && initialized && IsGameplayState(State))
            {
                PauseGame();
            }
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            if (!hasFocus && initialized && IsGameplayState(State))
            {
                PauseGame();
            }
        }

        private void ApplySavedSettings()
        {
            var bgmNormalized = saveManager.BgmVolume / 100f;
            var sfxNormalized = saveManager.SfxVolume / 100f;
            audioManager.SetBgmVolume(bgmNormalized);
            audioManager.SetSfxVolume(sfxNormalized);
        }

        private void ShowSettings(bool fromPause)
        {
            settingsOpenedFromPause = fromPause;
            audioManager.PlayUiClick();
            uiContext.SettingsView.Show(saveManager.BgmVolume, saveManager.SfxVolume, saveManager.VibrationEnabled);
        }

        private void HandleTitleSettingsRequested()
        {
            ShowSettings(false);
        }

        private void HandlePauseSettingsRequested()
        {
            ShowSettings(true);
        }

        private void HandleSettingsClosed()
        {
            audioManager.PlayUiClick();
            uiContext.SettingsView.Hide();
        }

        private void HandleBgmVolumeChanged(int volume)
        {
            saveManager.SetBgmVolume(volume);
            audioManager.SetBgmVolume(volume / 100f);
        }

        private void HandleSfxVolumeChanged(int volume)
        {
            saveManager.SetSfxVolume(volume);
            audioManager.SetSfxVolume(volume / 100f);
        }

        private void HandleVibrationChanged(bool enabled)
        {
            saveManager.SetVibrationEnabled(enabled);
#if UNITY_ANDROID || UNITY_IOS
            if (enabled)
            {
                Handheld.Vibrate();
            }
#endif
        }
    }
}
