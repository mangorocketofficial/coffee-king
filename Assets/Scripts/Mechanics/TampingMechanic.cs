using System;
using CoffeeKing.Core;
using CoffeeKing.GameInput;
using CoffeeKing.Scoring;
using CoffeeKing.Util;
using CoffeeKing.View;
using UnityEngine;

namespace CoffeeKing.Mechanics
{
    public sealed class TampingMechanic : MonoBehaviour
    {
        private GameConfig config;
        private GestureDetector gestureDetector;
        private GrayboxSceneContext sceneContext;
        private Transform portafilterRoot;
        private SpriteRenderer portafilterRenderer;

        private bool active;
        private bool pressing;
        private int activePointerId = int.MinValue;
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
            pressing = false;
            activePointerId = int.MinValue;
            currentValue = config.TampingGaugeMin;

            portafilterRoot.position = sceneContext.PortafilterWorkbenchPosition;
            portafilterRoot.gameObject.SetActive(true);
            portafilterRenderer.sprite = SpriteFactory.Load(SpriteAssetNames.PortafilterFilled, config.PortafilterBodySize, config.PortafilterIdleColor);
            portafilterRenderer.color = config.PortafilterIdleColor;

            sceneContext.TamperRenderer.gameObject.SetActive(true);
            sceneContext.TamperRenderer.color = config.MachineSlotSnapColor;
            sceneContext.TamperRenderer.transform.position = sceneContext.TamperPosition;
            sceneContext.GaugeView.SetVisible(true);
            sceneContext.GaugeView.Configure(
                "Tamping",
                config.GaugeFillColor,
                config.GaugeTargetColor,
                config.TampingPerfectMin / config.TampingGaugeMax,
                config.TampingPerfectMax / config.TampingGaugeMax);
            sceneContext.GaugeView.SetValue(0f, "Hold");
        }

        public void CancelStep()
        {
            active = false;
            pressing = false;
            activePointerId = int.MinValue;
            currentValue = 0f;
            Hide();
        }

        private void Update()
        {
            if (!active || !pressing)
            {
                return;
            }

            currentValue += config.TampingGaugeSpeed * Time.deltaTime;
            sceneContext.GaugeView.SetValue(currentValue / config.TampingGaugeMax, $"{Mathf.Clamp(currentValue, 0f, config.TampingGaugeMax):0.0}");

            var progress = Mathf.Clamp01(currentValue / config.TampingGaugeMax);
            var downwardOffset = Mathf.Lerp(0f, 0.55f, progress);
            sceneContext.TamperRenderer.transform.position = sceneContext.TamperPosition + Vector3.down * downwardOffset;

            if (currentValue >= config.TampingGaugeMax)
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
            if (!active || sceneContext.TamperRenderer == null)
            {
                return;
            }

            if (!sceneContext.TamperRenderer.bounds.Contains(gesture.WorldPosition))
            {
                return;
            }

            activePointerId = gesture.PointerId;
            pressing = true;
        }

        private void HandlePointerReleased(PointerGesture gesture)
        {
            if (!active || gesture.PointerId != activePointerId)
            {
                return;
            }

            Finish();
        }

        private void Finish()
        {
            var result = EvaluateResult();
            active = false;
            pressing = false;
            activePointerId = int.MinValue;
            Hide();
            Completed?.Invoke(result);
        }

        private MechanicScoreResult EvaluateResult()
        {
            var value = Mathf.Clamp(currentValue, config.TampingGaugeMin, config.TampingGaugeMax);
            if (value >= config.TampingPerfectMin && value <= config.TampingPerfectMax)
            {
                return new MechanicScoreResult("Tamping", QualityGrade.Perfect, ScoreRules.TampingPerfectScore, value);
            }

            if (value >= config.TampingGoodMin && value <= config.TampingGoodMax)
            {
                return new MechanicScoreResult("Tamping", QualityGrade.Good, ScoreRules.TampingGoodScore, value);
            }

            return new MechanicScoreResult("Tamping", QualityGrade.Bad, ScoreRules.TampingBadScore, value);
        }

        private void CreateVisual()
        {
            var rootObject = new GameObject("TampingPortafilter");
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

            if (sceneContext?.TamperRenderer != null)
            {
                sceneContext.TamperRenderer.gameObject.SetActive(false);
                sceneContext.TamperRenderer.transform.position = sceneContext.TamperPosition;
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
            gestureDetector.PointerReleased += HandlePointerReleased;
        }

        private void Unsubscribe()
        {
            if (gestureDetector == null)
            {
                return;
            }

            gestureDetector.PointerPressed -= HandlePointerPressed;
            gestureDetector.PointerReleased -= HandlePointerReleased;
        }
    }
}
