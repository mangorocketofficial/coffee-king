using System;
using CoffeeKing.Core;
using CoffeeKing.GameInput;
using CoffeeKing.Scoring;
using CoffeeKing.View;
using UnityEngine;

namespace CoffeeKing.Mechanics
{
    public sealed class SteamMilkMechanic : MonoBehaviour
    {
        private GameConfig config;
        private GestureDetector gestureDetector;
        private GrayboxSceneContext sceneContext;

        private bool active;
        private bool snapped;
        private int activePointerId = int.MinValue;
        private Vector3 dragOffset;
        private float currentTemperature;

        public event Action<MechanicScoreResult> Completed;

        public void Initialize(GameConfig runtimeConfig, GestureDetector detector, GrayboxSceneContext context)
        {
            Unsubscribe();
            config = runtimeConfig;
            gestureDetector = detector;
            sceneContext = context;
            Subscribe();
            Hide();
        }

        public void BeginStep()
        {
            active = true;
            snapped = false;
            activePointerId = int.MinValue;
            currentTemperature = config.SteamStartTemperature;

            sceneContext.PitcherRenderer.gameObject.SetActive(true);
            sceneContext.SteamWandRenderer.gameObject.SetActive(true);
            sceneContext.SteamWandRenderer.color = config.SteamWandColor;
            sceneContext.SteamWandRoot.position = sceneContext.SteamWandSpawnPosition;
            sceneContext.GaugeView.SetVisible(true);
            sceneContext.GaugeView.Configure(
                "Steam Milk",
                config.CupLatteColor,
                config.GaugeTargetColor,
                config.SteamPerfectMin / config.SteamBurnTemperature,
                config.SteamPerfectMax / config.SteamBurnTemperature);
            sceneContext.GaugeView.SetValue(currentTemperature / config.SteamBurnTemperature, $"{currentTemperature:0.0}C");
        }

        public void CancelStep()
        {
            active = false;
            snapped = false;
            activePointerId = int.MinValue;
            currentTemperature = config.SteamStartTemperature;
            Hide();
            if (sceneContext?.SteamWandRoot != null)
            {
                sceneContext.SteamWandRoot.position = sceneContext.SteamWandSpawnPosition;
            }
        }

        private void Update()
        {
            if (!active || !snapped)
            {
                return;
            }

            var depth = GetDepthNormalized();
            var heatRate = Mathf.Lerp(config.SteamHeatMinPerSecond, config.SteamHeatMaxPerSecond, depth);
            currentTemperature += heatRate * Time.deltaTime;
            sceneContext.GaugeView.SetValue(
                Mathf.Clamp01(currentTemperature / config.SteamBurnTemperature),
                $"{currentTemperature:0.0}C");

            if (currentTemperature >= config.SteamBurnTemperature)
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
            if (!active || sceneContext.SteamWandRenderer == null)
            {
                return;
            }

            if (!sceneContext.SteamWandRenderer.bounds.Contains(gesture.WorldPosition))
            {
                return;
            }

            activePointerId = gesture.PointerId;
            dragOffset = sceneContext.SteamWandRoot.position - (Vector3)gesture.WorldPosition;
        }

        private void HandlePointerDragged(PointerGesture gesture)
        {
            if (!active || gesture.PointerId != activePointerId)
            {
                return;
            }

            if (!snapped)
            {
                sceneContext.SteamWandRoot.position = (Vector3)gesture.WorldPosition + dragOffset;
                if (Vector2.Distance(sceneContext.SteamWandRoot.position, sceneContext.SteamWandSnapPosition) <= config.SteamWandSnapDistance)
                {
                    snapped = true;
                    sceneContext.SteamWandRenderer.color = config.ServingAreaActiveColor;
                    sceneContext.SteamWandRoot.position = new Vector3(
                        sceneContext.SteamWandSnapPosition.x,
                        config.SteamWandMaxY,
                        sceneContext.SteamWandSnapPosition.z);
                }

                return;
            }

            var targetY = Mathf.Clamp(gesture.WorldPosition.y + dragOffset.y, config.SteamWandMinY, config.SteamWandMaxY);
            sceneContext.SteamWandRoot.position = new Vector3(
                sceneContext.SteamWandSnapPosition.x,
                targetY,
                sceneContext.SteamWandSnapPosition.z);
        }

        private void HandlePointerReleased(PointerGesture gesture)
        {
            if (!active || gesture.PointerId != activePointerId)
            {
                return;
            }

            activePointerId = int.MinValue;
            if (!snapped)
            {
                sceneContext.SteamWandRoot.position = sceneContext.SteamWandSpawnPosition;
            }
        }

        private void HandlePointerTapped(PointerGesture gesture)
        {
            if (!active || !snapped)
            {
                return;
            }

            var tappedPitcher = sceneContext.PitcherRenderer.bounds.Contains(gesture.WorldPosition);
            var tappedWand = sceneContext.SteamWandRenderer.bounds.Contains(gesture.WorldPosition);
            if (tappedPitcher || tappedWand)
            {
                Finish();
            }
        }

        private void Finish()
        {
            active = false;
            activePointerId = int.MinValue;
            var result = EvaluateResult();
            Hide();
            Completed?.Invoke(result);
        }

        private MechanicScoreResult EvaluateResult()
        {
            if (currentTemperature >= config.SteamPerfectMin && currentTemperature <= config.SteamPerfectMax)
            {
                return new MechanicScoreResult("Steam", QualityGrade.Perfect, 100, currentTemperature);
            }

            if (currentTemperature >= config.SteamGoodMin && currentTemperature <= config.SteamGoodMax)
            {
                return new MechanicScoreResult("Steam", QualityGrade.Good, 60, currentTemperature);
            }

            return new MechanicScoreResult("Steam", QualityGrade.Bad, 20, currentTemperature);
        }

        private float GetDepthNormalized()
        {
            return Mathf.InverseLerp(config.SteamWandMaxY, config.SteamWandMinY, sceneContext.SteamWandRoot.position.y);
        }

        private void Hide()
        {
            if (sceneContext?.PitcherRenderer != null)
            {
                sceneContext.PitcherRenderer.gameObject.SetActive(false);
            }

            if (sceneContext?.SteamWandRenderer != null)
            {
                sceneContext.SteamWandRenderer.gameObject.SetActive(false);
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
            gestureDetector.PointerTapped += HandlePointerTapped;
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
            gestureDetector.PointerTapped -= HandlePointerTapped;
        }
    }
}
