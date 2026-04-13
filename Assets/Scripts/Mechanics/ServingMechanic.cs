using System;
using CoffeeKing.Core;
using CoffeeKing.GameInput;
using CoffeeKing.View;
using UnityEngine;

namespace CoffeeKing.Mechanics
{
    public sealed class ServingMechanic : MonoBehaviour
    {
        private GameConfig config;
        private GestureDetector gestureDetector;
        private GrayboxSceneContext sceneContext;

        private bool active;
        private int activePointerId = int.MinValue;
        private Vector3 dragOffset;

        public event Action Served;

        public void Initialize(GameConfig runtimeConfig, GestureDetector detector, GrayboxSceneContext context)
        {
            Unsubscribe();
            config = runtimeConfig;
            gestureDetector = detector;
            sceneContext = context;
            Subscribe();
            ResetCup();
        }

        public void BeginStep()
        {
            active = true;
            sceneContext.ServingAreaRenderer.color = Color.white;
            sceneContext.CupRoot.position = sceneContext.CupAnchorPosition;
        }

        public void CancelStep()
        {
            ResetCup();
        }

        public void ResetCup()
        {
            active = false;
            activePointerId = int.MinValue;

            if (sceneContext?.CupRoot != null)
            {
                sceneContext.CupRoot.position = sceneContext.CupAnchorPosition;
            }

            if (sceneContext?.ServingAreaRenderer != null)
            {
                sceneContext.ServingAreaRenderer.color = Color.white;
            }
        }

        private void OnDestroy()
        {
            Unsubscribe();
        }

        private void HandlePointerPressed(PointerGesture gesture)
        {
            if (!active || sceneContext.CupRenderer == null)
            {
                return;
            }

            if (!sceneContext.CupRenderer.bounds.Contains(gesture.WorldPosition))
            {
                return;
            }

            activePointerId = gesture.PointerId;
            dragOffset = sceneContext.CupRoot.position - (Vector3)gesture.WorldPosition;
        }

        private void HandlePointerDragged(PointerGesture gesture)
        {
            if (!active || gesture.PointerId != activePointerId)
            {
                return;
            }

            sceneContext.CupRoot.position = (Vector3)gesture.WorldPosition + dragOffset;
        }

        private void HandlePointerReleased(PointerGesture gesture)
        {
            if (!active || gesture.PointerId != activePointerId)
            {
                return;
            }

            activePointerId = int.MinValue;

            if (Vector2.Distance(sceneContext.CupRoot.position, sceneContext.ServingAreaPosition) <= config.ServingSnapDistance)
            {
                sceneContext.CupRoot.position = sceneContext.ServingAreaPosition;
                active = false;
                sceneContext.ServingAreaRenderer.color = Color.white;
                Served?.Invoke();
            }
            else
            {
                sceneContext.CupRoot.position = sceneContext.CupAnchorPosition;
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
