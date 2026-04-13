using System;
using CoffeeKing.Core;
using CoffeeKing.GameInput;
using CoffeeKing.View;
using UnityEngine;

namespace CoffeeKing.Mechanics
{
    public sealed class LidMechanic : MonoBehaviour
    {
        private GameConfig config;
        private GestureDetector gestureDetector;
        private GrayboxSceneContext sceneContext;
        private bool active;
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
            active = true;
            activePointerId = int.MinValue;
            sceneContext.LidRoot.position = sceneContext.LidPosition;
            sceneContext.LidRoot.gameObject.SetActive(true);
        }

        public void CancelStep()
        {
            active = false;
            activePointerId = int.MinValue;
            Hide();
        }

        private void OnDestroy()
        {
            Unsubscribe();
        }

        private void HandlePointerPressed(PointerGesture gesture)
        {
            if (!active || sceneContext.LidRenderer == null)
            {
                return;
            }

            if (!sceneContext.LidRenderer.bounds.Contains(gesture.WorldPosition))
            {
                return;
            }

            activePointerId = gesture.PointerId;
            dragOffset = sceneContext.LidRoot.position - (Vector3)gesture.WorldPosition;
        }

        private void HandlePointerDragged(PointerGesture gesture)
        {
            if (!active || gesture.PointerId != activePointerId)
            {
                return;
            }

            sceneContext.LidRoot.position = (Vector3)gesture.WorldPosition + dragOffset;
        }

        private void HandlePointerReleased(PointerGesture gesture)
        {
            if (!active || gesture.PointerId != activePointerId)
            {
                return;
            }

            activePointerId = int.MinValue;
            var cupTop = sceneContext.CupRoot.position + new Vector3(0f, config.CupSize.y * 0.42f, 0f);
            if (Vector2.Distance(sceneContext.LidRoot.position, cupTop) <= 0.65f)
            {
                sceneContext.LidRoot.position = cupTop;
                active = false;
                Hide();
                Completed?.Invoke();
            }
            else
            {
                sceneContext.LidRoot.position = sceneContext.LidPosition;
            }
        }

        private void Hide()
        {
            if (sceneContext?.LidRoot != null)
            {
                sceneContext.LidRoot.gameObject.SetActive(false);
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
