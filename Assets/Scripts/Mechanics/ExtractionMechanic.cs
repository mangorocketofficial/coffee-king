using System;
using CoffeeKing.Core;
using CoffeeKing.GameInput;
using CoffeeKing.Scoring;
using CoffeeKing.View;
using UnityEngine;

namespace CoffeeKing.Mechanics
{
    public sealed class ExtractionMechanic : MonoBehaviour
    {
        private GameConfig config;
        private GestureDetector gestureDetector;
        private GrayboxSceneContext sceneContext;

        private bool active;
        private bool extracting;
        private float currentValue;

        public event Action<MechanicScoreResult> Completed;

        public void Initialize(GameConfig runtimeConfig, GestureDetector detector, GrayboxSceneContext context)
        {
            Unsubscribe();
            config = runtimeConfig;
            gestureDetector = detector;
            sceneContext = context;
            Subscribe();
            HideVisuals();
        }

        public void BeginStep()
        {
            active = true;
            extracting = false;
            currentValue = config.ExtractionGaugeMin;
            sceneContext.ExtractionButtonRenderer.color = config.ExtractionButtonActiveColor;
            sceneContext.ExtractionButtonRenderer.gameObject.SetActive(true);
            sceneContext.GaugeView.SetVisible(true);
            sceneContext.GaugeView.Configure(
                "Extraction",
                config.GaugeFillColor,
                config.GaugeTargetColor,
                config.ExtractionPerfectMin / config.ExtractionGaugeMax,
                config.ExtractionPerfectMax / config.ExtractionGaugeMax);
            sceneContext.GaugeView.SetValue(0f, "Tap to start");
        }

        public void CancelStep()
        {
            active = false;
            extracting = false;
            currentValue = config.ExtractionGaugeMin;
            HideVisuals();
        }

        private void Update()
        {
            if (!active || !extracting)
            {
                return;
            }

            currentValue += config.ExtractionGaugeSpeed * Time.deltaTime;
            var normalized = currentValue / config.ExtractionGaugeMax;
            sceneContext.GaugeView.SetValue(normalized, $"{Mathf.Clamp(currentValue, 0f, config.ExtractionGaugeMax):0.0}");

            if (currentValue >= config.ExtractionGaugeMax)
            {
                Finish();
            }
        }

        private void OnDestroy()
        {
            Unsubscribe();
        }

        private void HandlePointerTapped(PointerGesture gesture)
        {
            if (!active || sceneContext.ExtractionButtonRenderer == null)
            {
                return;
            }

            if (!sceneContext.ExtractionButtonRenderer.bounds.Contains(gesture.WorldPosition))
            {
                return;
            }

            if (!extracting)
            {
                extracting = true;
                sceneContext.ExtractionButtonRenderer.color = config.ExtractionButtonRunningColor;
                sceneContext.GaugeView.SetValue(currentValue / config.ExtractionGaugeMax, "Brewing...");
            }
            else
            {
                Finish();
            }
        }

        private void Finish()
        {
            active = false;
            extracting = false;
            HideVisuals();
            Completed?.Invoke(EvaluateResult());
        }

        private MechanicScoreResult EvaluateResult()
        {
            var clampedValue = Mathf.Clamp(currentValue, config.ExtractionGaugeMin, config.ExtractionGaugeMax);
            if (clampedValue >= config.ExtractionPerfectMin && clampedValue <= config.ExtractionPerfectMax)
            {
                return new MechanicScoreResult("Extraction", QualityGrade.Perfect, 100, clampedValue);
            }

            if (clampedValue >= config.ExtractionGoodMin && clampedValue <= config.ExtractionGoodMax)
            {
                return new MechanicScoreResult("Extraction", QualityGrade.Good, 60, clampedValue);
            }

            return new MechanicScoreResult("Extraction", QualityGrade.Bad, 20, clampedValue);
        }

        private void HideVisuals()
        {
            if (sceneContext?.ExtractionButtonRenderer != null)
            {
                sceneContext.ExtractionButtonRenderer.gameObject.SetActive(false);
                sceneContext.ExtractionButtonRenderer.color = config.ExtractionButtonIdleColor;
            }

            sceneContext?.GaugeView?.SetVisible(false);
        }

        private void Subscribe()
        {
            if (gestureDetector != null)
            {
                gestureDetector.PointerTapped += HandlePointerTapped;
            }
        }

        private void Unsubscribe()
        {
            if (gestureDetector != null)
            {
                gestureDetector.PointerTapped -= HandlePointerTapped;
            }
        }
    }
}
