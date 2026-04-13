using System;
using CoffeeKing.Core;
using CoffeeKing.GameInput;
using CoffeeKing.Util;
using CoffeeKing.View;
using UnityEngine;

namespace CoffeeKing.Mechanics
{
    public sealed class HotWaterCupMechanic : MonoBehaviour
    {
        private enum StepState
        {
            Hidden,
            CarryingEmptyCup,
            ReadyToPour
        }

        private GameConfig config;
        private GestureDetector gestureDetector;
        private GrayboxSceneContext sceneContext;
        private StepState stepState;
        private int activePointerId = int.MinValue;
        private Vector3 dragOffset;

        public event Action Completed;

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
            stepState = StepState.CarryingEmptyCup;
            activePointerId = int.MinValue;
            sceneContext.ShotGlassRoot.position = config.HotWaterPourPosition;
            sceneContext.SetShotGlassVisual(SpriteAssetNames.WaterCupEmpty, config.CupSize, config.CupEmptyColor);
            sceneContext.ShotGlassRoot.gameObject.SetActive(true);
        }

        public void CancelStep()
        {
            stepState = StepState.Hidden;
            activePointerId = int.MinValue;
            Hide();
        }

        private void OnDestroy()
        {
            Unsubscribe();
        }

        private void HandlePointerPressed(PointerGesture gesture)
        {
            if (stepState != StepState.CarryingEmptyCup || sceneContext?.ShotGlassRenderer == null)
            {
                return;
            }

            if (!sceneContext.ShotGlassRenderer.bounds.Contains(gesture.WorldPosition))
            {
                return;
            }

            activePointerId = gesture.PointerId;
            dragOffset = sceneContext.ShotGlassRoot.position - (Vector3)gesture.WorldPosition;
        }

        private void HandlePointerDragged(PointerGesture gesture)
        {
            if (stepState != StepState.CarryingEmptyCup || gesture.PointerId != activePointerId)
            {
                return;
            }

            sceneContext.ShotGlassRoot.position = (Vector3)gesture.WorldPosition + dragOffset;
        }

        private void HandlePointerReleased(PointerGesture gesture)
        {
            if (stepState != StepState.CarryingEmptyCup || gesture.PointerId != activePointerId)
            {
                return;
            }

            activePointerId = int.MinValue;
            var dispenserPosition = sceneContext.HotWaterDispenserRenderer.transform.position;
            if (Vector2.Distance(sceneContext.ShotGlassRoot.position, dispenserPosition) <= config.GrinderSnapDistance)
            {
                stepState = StepState.ReadyToPour;
                sceneContext.ShotGlassRoot.position = dispenserPosition + new Vector3(0f, -0.1f, 0f);
                sceneContext.SetShotGlassVisual(SpriteAssetNames.WaterCupFull, config.CupSize, config.CupEmptyColor);
                return;
            }

            sceneContext.ShotGlassRoot.position = config.HotWaterPourPosition;
        }

        private void HandlePointerTapped(PointerGesture gesture)
        {
            if (stepState != StepState.ReadyToPour || sceneContext?.ShotGlassRenderer == null)
            {
                return;
            }

            var tappedWaterCup = sceneContext.ShotGlassRenderer.bounds.Contains(gesture.WorldPosition);
            if (!tappedWaterCup)
            {
                return;
            }

            stepState = StepState.Hidden;
            Hide();
            Completed?.Invoke();
        }

        private void Hide()
        {
            if (sceneContext?.ShotGlassRoot != null)
            {
                sceneContext.ShotGlassRoot.position = sceneContext.ShotGlassPosition;
                sceneContext.ShotGlassRoot.gameObject.SetActive(false);
            }
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
