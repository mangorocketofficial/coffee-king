using System;
using CoffeeKing.Core;
using CoffeeKing.GameInput;
using CoffeeKing.Scoring;
using CoffeeKing.Util;
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
        private float blinkTimer;

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
            blinkTimer = 0f;
            currentValue = config.ExtractionGaugeMin;
            if (sceneContext?.ShotGlassRoot != null)
            {
                sceneContext.ShotGlassRoot.position = sceneContext.ShotGlassPosition;
                sceneContext.ShotGlassRoot.gameObject.SetActive(true);
                sceneContext.SetShotGlassVisual(SpriteAssetNames.ShotGlassEmpty, config.ShotGlassSize, config.CupEspressoColor);
            }

            var buttonGroup = sceneContext.ExtractionButtonRenderer.transform.parent.gameObject;
            buttonGroup.SetActive(true);
            sceneContext.ExtractionButtonRenderer.color = config.ExtractionButtonActiveColor;
            if (sceneContext.ExtractionButtonLabel != null)
            {
                sceneContext.ExtractionButtonLabel.text = "BREW";
                sceneContext.ExtractionButtonLabel.color = Color.white;
            }
            if (sceneContext.ExtractionButtonRingRenderer != null)
            {
                sceneContext.ExtractionButtonRingRenderer.color = ColorPalette.ExtractionButtonRing;
            }

            sceneContext.GaugeView.SetVisible(true);
            sceneContext.GaugeView.Configure(
                "Extraction",
                config.GaugeFillColor,
                config.GaugeTargetColor,
                config.ExtractionPerfectMin / config.ExtractionGaugeMax,
                config.ExtractionPerfectMax / config.ExtractionGaugeMax);
            sceneContext.GaugeView.SetValue(0f, string.Empty);
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

            if (sceneContext?.GaugeView == null)
            {
                return;
            }

            currentValue += config.ExtractionGaugeSpeed * Time.deltaTime;
            var normalized = currentValue / config.ExtractionGaugeMax;
            sceneContext.GaugeView.SetValue(normalized, string.Empty);
            UpdateShotGlassVisual(normalized);
            UpdateBlink();

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
                blinkTimer = 0f;
                sceneContext.ExtractionButtonRenderer.color = config.ExtractionButtonRunningColor;
                if (sceneContext.ExtractionButtonLabel != null)
                {
                    sceneContext.ExtractionButtonLabel.text = "STOP";
                }
                sceneContext.GaugeView.SetValue(currentValue / config.ExtractionGaugeMax, string.Empty);
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
            sceneContext?.SetShotGlassVisual(SpriteAssetNames.ShotGlassFull, config.ShotGlassSize, config.CupEspressoColor);
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
                var buttonGroup = sceneContext.ExtractionButtonRenderer.transform.parent.gameObject;
                buttonGroup.SetActive(false);
                sceneContext.ExtractionButtonRenderer.color = config.ExtractionButtonIdleColor;
            }

            sceneContext?.GaugeView?.SetVisible(false);
        }

        private void UpdateBlink()
        {
            if (sceneContext?.ExtractionButtonRenderer == null)
            {
                return;
            }

            blinkTimer += Time.deltaTime * 4f;
            var alpha = 0.6f + 0.4f * Mathf.Sin(blinkTimer * Mathf.PI);
            var runColor = config.ExtractionButtonRunningColor;
            sceneContext.ExtractionButtonRenderer.color = new Color(runColor.r, runColor.g, runColor.b, alpha);

            if (sceneContext.ExtractionButtonRingRenderer != null)
            {
                var ringBase = ColorPalette.ExtractionButtonRing;
                var ringGlow = config.ExtractionButtonRunningColor;
                var t = 0.5f + 0.5f * Mathf.Sin(blinkTimer * Mathf.PI);
                sceneContext.ExtractionButtonRingRenderer.color = Color.Lerp(ringBase, ringGlow, t);
            }
        }

        private void UpdateShotGlassVisual(float normalized)
        {
            if (sceneContext?.ShotGlassRenderer == null)
            {
                return;
            }

            if (normalized < 0.35f)
            {
                sceneContext.SetShotGlassVisual(SpriteAssetNames.ShotGlassEmpty, config.ShotGlassSize, config.CupEspressoColor);
            }
            else if (normalized < 0.8f)
            {
                sceneContext.SetShotGlassVisual(SpriteAssetNames.ShotGlassFilling, config.ShotGlassSize, config.CupEspressoColor);
            }
            else
            {
                sceneContext.SetShotGlassVisual(SpriteAssetNames.ShotGlassFull, config.ShotGlassSize, config.CupEspressoColor);
            }
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
