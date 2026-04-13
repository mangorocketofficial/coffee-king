using System;
using CoffeeKing.GameInput;
using CoffeeKing.View;
using UnityEngine;

namespace CoffeeKing.Mechanics
{
    public sealed class IngredientMechanic : MonoBehaviour
    {
        private GestureDetector gestureDetector;
        private GrayboxSceneContext sceneContext;
        private bool active;
        private SpriteRenderer targetRenderer;

        public event Action Completed;

        public void Initialize(GestureDetector detector, GrayboxSceneContext context)
        {
            Unsubscribe();
            gestureDetector = detector;
            sceneContext = context;
            Subscribe();
            HideAll();
        }

        public void BeginStep(SpriteRenderer sourceRenderer)
        {
            active = true;
            targetRenderer = sourceRenderer;
            HideAll();

            if (targetRenderer != null)
            {
                targetRenderer.gameObject.SetActive(true);
            }
        }

        public void CancelStep()
        {
            active = false;
            targetRenderer = null;
            HideAll();
        }

        private void OnDestroy()
        {
            Unsubscribe();
        }

        private void HandlePointerTapped(PointerGesture gesture)
        {
            if (!active || targetRenderer == null)
            {
                return;
            }

            if (!targetRenderer.bounds.Contains(gesture.WorldPosition))
            {
                return;
            }

            active = false;
            HideAll();
            targetRenderer = null;
            Completed?.Invoke();
        }

        private void HideAll()
        {
            if (sceneContext?.WaterBottleRenderer != null)
            {
                sceneContext.WaterBottleRenderer.gameObject.SetActive(false);
            }

            if (sceneContext?.PitcherRenderer != null)
            {
                sceneContext.PitcherRenderer.gameObject.SetActive(false);
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
