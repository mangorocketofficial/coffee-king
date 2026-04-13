using System;
using CoffeeKing.Core;
using CoffeeKing.GameInput;
using CoffeeKing.View;
using UnityEngine;

namespace CoffeeKing.Mechanics
{
    public sealed class PourMechanic : MonoBehaviour
    {
        private GameConfig config;
        private GestureDetector gestureDetector;
        private GrayboxSceneContext sceneContext;
        private bool active;
        private int activePointerId = int.MinValue;
        private Vector3 dragOffset;
        private Vector3 restPosition;

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
            BeginStep(sceneContext.ShotGlassPosition);
        }

        public void BeginStep(Vector3 spawnPosition)
        {
            active = true;
            activePointerId = int.MinValue;
            restPosition = spawnPosition;
            sceneContext.ShotGlassRoot.position = restPosition;
            sceneContext.ShotGlassRoot.gameObject.SetActive(true);
        }

        public void CancelStep()
        {
            active = false;
            activePointerId = int.MinValue;
            restPosition = sceneContext != null ? sceneContext.ShotGlassPosition : Vector3.zero;
            Hide();
        }

        private void OnDestroy()
        {
            Unsubscribe();
        }

        private void HandlePointerPressed(PointerGesture gesture)
        {
            if (!active || sceneContext.ShotGlassRenderer == null)
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
            if (!active || gesture.PointerId != activePointerId)
            {
                return;
            }

            sceneContext.ShotGlassRoot.position = (Vector3)gesture.WorldPosition + dragOffset;
        }

        private void HandlePointerReleased(PointerGesture gesture)
        {
            if (!active || gesture.PointerId != activePointerId)
            {
                return;
            }

            activePointerId = int.MinValue;
            if (Vector2.Distance(sceneContext.ShotGlassRoot.position, sceneContext.CupAnchorPosition) <= config.ShotPourSnapDistance)
            {
                active = false;
                Hide();
                Completed?.Invoke();
            }
            else
            {
                sceneContext.ShotGlassRoot.position = restPosition;
            }
        }

        private void Hide()
        {
            if (sceneContext?.ShotGlassRoot != null)
            {
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
