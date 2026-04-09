using System;
using CoffeeKing.GameInput;
using CoffeeKing.View;
using UnityEngine;

namespace CoffeeKing.Mechanics
{
    public sealed class SyrupMechanic : MonoBehaviour
    {
        private GestureDetector gestureDetector;
        private GrayboxSceneContext sceneContext;
        private bool active;

        public event Action Completed;

        public void Initialize(GestureDetector detector, GrayboxSceneContext context)
        {
            Unsubscribe();
            gestureDetector = detector;
            sceneContext = context;
            Subscribe();
            Hide();
        }

        public void BeginStep()
        {
            active = true;
            sceneContext.SyrupBottleRenderer.gameObject.SetActive(true);
        }

        public void CancelStep()
        {
            active = false;
            Hide();
        }

        private void OnDestroy()
        {
            Unsubscribe();
        }

        private void HandlePointerTapped(PointerGesture gesture)
        {
            if (!active || sceneContext.SyrupBottleRenderer == null)
            {
                return;
            }

            if (!sceneContext.SyrupBottleRenderer.bounds.Contains(gesture.WorldPosition))
            {
                return;
            }

            active = false;
            Hide();
            Completed?.Invoke();
        }

        private void Hide()
        {
            if (sceneContext?.SyrupBottleRenderer != null)
            {
                sceneContext.SyrupBottleRenderer.gameObject.SetActive(false);
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
