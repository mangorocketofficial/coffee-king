using System;
using CoffeeKing.Core;
using CoffeeKing.GameInput;
using CoffeeKing.Scoring;
using CoffeeKing.Util;
using CoffeeKing.View;
using UnityEngine;

namespace CoffeeKing.Mechanics
{
    public sealed class GrindingMechanic : MonoBehaviour
    {
        private enum GrindingState
        {
            MoveToGrinder,
            ReadyToGrind,
            Grinding
        }

        private GameConfig config;
        private GestureDetector gestureDetector;
        private GrayboxSceneContext sceneContext;
        private Transform portafilterRoot;
        private SpriteRenderer portafilterRenderer;

        private GrindingState state;
        private bool active;
        private int activePointerId = int.MinValue;
        private Vector3 dragOffset;
        private float currentValue;

        public event Action<MechanicScoreResult> Completed;

        public void Initialize(GameConfig runtimeConfig, GestureDetector detector, GrayboxSceneContext context)
        {
            Unsubscribe();
            config = runtimeConfig;
            gestureDetector = detector;
            sceneContext = context;
            CreateVisual();
            Subscribe();
            Hide();
        }

        public void BeginStep()
        {
            active = true;
            state = GrindingState.MoveToGrinder;
            activePointerId = int.MinValue;
            currentValue = config.GrindingGaugeMin;

            portafilterRoot.position = sceneContext.PortafilterWorkbenchPosition;
            portafilterRoot.rotation = Quaternion.identity;
            portafilterRoot.gameObject.SetActive(true);
            portafilterRenderer.sprite = SpriteFactory.Load(SpriteAssetNames.PortafilterEmpty, config.PortafilterBodySize, config.PortafilterIdleColor);
            portafilterRenderer.color = Color.white;

            sceneContext.GrinderRenderer.gameObject.SetActive(true);
            sceneContext.GrinderRenderer.sprite = SpriteFactory.Load(SpriteAssetNames.Grinder, config.GrinderSize, config.IngredientTrayColor);
            sceneContext.GaugeView.SetVisible(false);
        }

        public void CancelStep()
        {
            active = false;
            activePointerId = int.MinValue;
            currentValue = 0f;
            Hide();
        }

        private void Update()
        {
            if (!active || state != GrindingState.Grinding)
            {
                return;
            }

            currentValue += config.GrindingGaugeSpeed * Time.deltaTime;
            sceneContext.GaugeView.SetValue(currentValue / config.GrindingGaugeMax, $"{Mathf.Clamp(currentValue, 0f, config.GrindingGaugeMax):0.0}");

            if (currentValue >= config.GrindingGaugeMax)
            {
                Finish();
            }
        }

        private void OnDestroy()
        {
            Unsubscribe();
        }

        private void HandlePointerPressed(PointerGesture gesture)
        {
            if (!active)
            {
                return;
            }

            if (state == GrindingState.MoveToGrinder && portafilterRenderer.bounds.Contains(gesture.WorldPosition))
            {
                activePointerId = gesture.PointerId;
                dragOffset = portafilterRoot.position - (Vector3)gesture.WorldPosition;
                return;
            }

            if (state == GrindingState.ReadyToGrind && sceneContext.GrinderRenderer.bounds.Contains(gesture.WorldPosition))
            {
                activePointerId = gesture.PointerId;
                state = GrindingState.Grinding;
                sceneContext.GaugeView.SetVisible(true);
                sceneContext.GaugeView.Configure(
                    "Grinding",
                    config.GaugeFillColor,
                    config.GaugeTargetColor,
                    config.GrindingPerfectMin / config.GrindingGaugeMax,
                    config.GrindingPerfectMax / config.GrindingGaugeMax);
                sceneContext.GaugeView.SetValue(currentValue / config.GrindingGaugeMax, "Hold");
            }
        }

        private void HandlePointerDragged(PointerGesture gesture)
        {
            if (!active || state != GrindingState.MoveToGrinder || gesture.PointerId != activePointerId)
            {
                return;
            }

            portafilterRoot.position = (Vector3)gesture.WorldPosition + dragOffset;
            if (Vector2.Distance(portafilterRoot.position, sceneContext.GrinderPosition) <= config.GrinderSnapDistance)
            {
                state = GrindingState.ReadyToGrind;
                activePointerId = int.MinValue;
                portafilterRoot.gameObject.SetActive(false);
                sceneContext.GrinderRenderer.sprite = SpriteFactory.Load(SpriteAssetNames.GrinderWithPortafilter, config.GrinderSize, config.IngredientTrayColor);
                sceneContext.GrinderRenderer.color = Color.white;
                sceneContext.SetStatus("Hold on the grinder to grind coffee.");
            }
        }

        private void HandlePointerReleased(PointerGesture gesture)
        {
            if (!active || gesture.PointerId != activePointerId)
            {
                return;
            }

            if (state == GrindingState.MoveToGrinder)
            {
                portafilterRoot.position = sceneContext.PortafilterWorkbenchPosition;
            }
            else if (state == GrindingState.Grinding)
            {
                Finish();
            }

            activePointerId = int.MinValue;
        }

        private void Finish()
        {
            var result = EvaluateResult();
            active = false;
            activePointerId = int.MinValue;
            Hide();
            Completed?.Invoke(result);
        }

        private MechanicScoreResult EvaluateResult()
        {
            var value = Mathf.Clamp(currentValue, config.GrindingGaugeMin, config.GrindingGaugeMax);
            if (value >= config.GrindingPerfectMin && value <= config.GrindingPerfectMax)
            {
                return new MechanicScoreResult("Grinding", QualityGrade.Perfect, ScoreRules.GrindingPerfectScore, value);
            }

            if (value >= config.GrindingGoodMin && value <= config.GrindingGoodMax)
            {
                return new MechanicScoreResult("Grinding", QualityGrade.Good, ScoreRules.GrindingGoodScore, value);
            }

            return new MechanicScoreResult("Grinding", QualityGrade.Bad, ScoreRules.GrindingBadScore, value);
        }

        private void CreateVisual()
        {
            var rootObject = new GameObject("GrindingPortafilter");
            portafilterRoot = rootObject.transform;
            portafilterRoot.SetParent(sceneContext.InteractiveRoot, false);
            portafilterRenderer = rootObject.AddComponent<SpriteRenderer>();
            portafilterRenderer.sortingOrder = 10;
        }

        private void Hide()
        {
            if (portafilterRoot != null)
            {
                portafilterRoot.gameObject.SetActive(false);
            }

            if (sceneContext?.GrinderRenderer != null)
            {
                sceneContext.GrinderRenderer.sprite = SpriteFactory.Load(SpriteAssetNames.Grinder, config.GrinderSize, config.IngredientTrayColor);
                sceneContext.GrinderRenderer.color = Color.white;
            }

            sceneContext?.GaugeView?.SetVisible(false);
        }

        private void Subscribe()
        {
            if (gestureDetector == null)
            {
                return;
            }

            gestureDetector.PointerPressed += HandlePointerPressed;
            gestureDetector.PointerDragged += HandlePointerDragged;
            gestureDetector.PointerReleased += HandlePointerReleased;
        }

        private void Unsubscribe()
        {
            if (gestureDetector == null)
            {
                return;
            }

            gestureDetector.PointerPressed -= HandlePointerPressed;
            gestureDetector.PointerDragged -= HandlePointerDragged;
            gestureDetector.PointerReleased -= HandlePointerReleased;
        }
    }
}
