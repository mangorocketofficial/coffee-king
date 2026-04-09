using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;
using InputSystemTouch = UnityEngine.InputSystem.EnhancedTouch.Touch;

namespace CoffeeKing.GameInput
{
    public readonly struct PointerGesture
    {
        public PointerGesture(
            int pointerId,
            Vector2 screenPosition,
            Vector2 worldPosition,
            Vector2 deltaScreen,
            Vector2 deltaWorld,
            float heldDuration,
            bool isTouch,
            bool hasDragStarted)
        {
            PointerId = pointerId;
            ScreenPosition = screenPosition;
            WorldPosition = worldPosition;
            DeltaScreen = deltaScreen;
            DeltaWorld = deltaWorld;
            HeldDuration = heldDuration;
            IsTouch = isTouch;
            HasDragStarted = hasDragStarted;
        }

        public int PointerId { get; }
        public Vector2 ScreenPosition { get; }
        public Vector2 WorldPosition { get; }
        public Vector2 DeltaScreen { get; }
        public Vector2 DeltaWorld { get; }
        public float HeldDuration { get; }
        public bool IsTouch { get; }
        public bool HasDragStarted { get; }
    }

    public readonly struct RotateGesture
    {
        public RotateGesture(
            int pointerId,
            Vector2 screenCenter,
            Vector2 worldCenter,
            float deltaDegrees,
            bool isMultiTouch)
        {
            PointerId = pointerId;
            ScreenCenter = screenCenter;
            WorldCenter = worldCenter;
            DeltaDegrees = deltaDegrees;
            IsMultiTouch = isMultiTouch;
        }

        public int PointerId { get; }
        public Vector2 ScreenCenter { get; }
        public Vector2 WorldCenter { get; }
        public float DeltaDegrees { get; }
        public bool IsMultiTouch { get; }
    }

    public sealed class GestureDetector : MonoBehaviour
    {
        private sealed class PointerState
        {
            public Vector2 StartScreenPosition;
            public Vector2 PreviousScreenPosition;
            public float StartTime;
            public bool DragStarted;
            public bool IsTouch;
        }

        [SerializeField] private float tapMaxDuration = 0.25f;
        [SerializeField] private float tapMaxDistancePixels = 18f;
        [SerializeField] private float dragThresholdPixels = 12f;

        private readonly Dictionary<int, PointerState> pointerStates = new Dictionary<int, PointerState>();

        private Camera cachedCamera;
        private bool hadMultiTouchLastFrame;
        private float previousMultiTouchAngle;

        public event Action<PointerGesture> PointerPressed;
        public event Action<PointerGesture> PointerDragged;
        public event Action<PointerGesture> PointerReleased;
        public event Action<PointerGesture> PointerTapped;
        public event Action<RotateGesture> Rotated;

        private void OnEnable()
        {
            EnhancedTouchSupport.Enable();
        }

        private void OnDisable()
        {
            EnhancedTouchSupport.Disable();
        }

        private void Update()
        {
            cachedCamera = ResolveCamera();

            if (InputSystemTouch.activeTouches.Count > 0)
            {
                ProcessTouches();
            }
            else
            {
                ProcessMouse();
            }

            ProcessRotation();
        }

        private void ProcessMouse()
        {
            var mouse = Mouse.current;
            if (mouse == null)
            {
                return;
            }

            const int mousePointerId = -1;
            var screenPosition = mouse.position.ReadValue();

            if (mouse.leftButton.wasPressedThisFrame)
            {
                BeginPointer(mousePointerId, screenPosition, false);
            }

            if (mouse.leftButton.isPressed)
            {
                MovePointer(mousePointerId, screenPosition);
            }

            if (mouse.leftButton.wasReleasedThisFrame)
            {
                EndPointer(mousePointerId, screenPosition);
            }
        }

        private void ProcessTouches()
        {
            var activeTouches = InputSystemTouch.activeTouches;
            for (var index = 0; index < activeTouches.Count; index++)
            {
                var touch = activeTouches[index];
                switch (touch.phase)
                {
                    case UnityEngine.InputSystem.TouchPhase.Began:
                        BeginPointer(touch.touchId, touch.screenPosition, true);
                        break;
                    case UnityEngine.InputSystem.TouchPhase.Moved:
                    case UnityEngine.InputSystem.TouchPhase.Stationary:
                        MovePointer(touch.touchId, touch.screenPosition);
                        break;
                    case UnityEngine.InputSystem.TouchPhase.Ended:
                    case UnityEngine.InputSystem.TouchPhase.Canceled:
                        EndPointer(touch.touchId, touch.screenPosition);
                        break;
                }
            }
        }

        private void ProcessRotation()
        {
            var activeTouches = InputSystemTouch.activeTouches;
            if (activeTouches.Count < 2)
            {
                hadMultiTouchLastFrame = false;
                return;
            }

            var firstTouch = activeTouches[0];
            var secondTouch = activeTouches[1];
            var delta = secondTouch.screenPosition - firstTouch.screenPosition;

            if (delta.sqrMagnitude < 0.001f)
            {
                return;
            }

            var angle = Mathf.Atan2(delta.y, delta.x) * Mathf.Rad2Deg;
            if (hadMultiTouchLastFrame)
            {
                var center = (firstTouch.screenPosition + secondTouch.screenPosition) * 0.5f;
                var deltaDegrees = Mathf.DeltaAngle(previousMultiTouchAngle, angle);
                if (Mathf.Abs(deltaDegrees) > 0.01f)
                {
                    Rotated?.Invoke(new RotateGesture(
                        firstTouch.touchId,
                        center,
                        ScreenToWorld(center),
                        deltaDegrees,
                        true));
                }
            }

            previousMultiTouchAngle = angle;
            hadMultiTouchLastFrame = true;
        }

        private void BeginPointer(int pointerId, Vector2 screenPosition, bool isTouch)
        {
            pointerStates[pointerId] = new PointerState
            {
                StartScreenPosition = screenPosition,
                PreviousScreenPosition = screenPosition,
                StartTime = Time.unscaledTime,
                DragStarted = false,
                IsTouch = isTouch
            };

            PointerPressed?.Invoke(CreatePointerGesture(pointerId, screenPosition, Vector2.zero, Vector2.zero));
        }

        private void MovePointer(int pointerId, Vector2 screenPosition)
        {
            if (!pointerStates.TryGetValue(pointerId, out var pointerState))
            {
                return;
            }

            var deltaScreen = screenPosition - pointerState.PreviousScreenPosition;
            if (!pointerState.DragStarted)
            {
                var totalDelta = screenPosition - pointerState.StartScreenPosition;
                if (totalDelta.sqrMagnitude >= dragThresholdPixels * dragThresholdPixels)
                {
                    pointerState.DragStarted = true;
                }
            }

            pointerState.PreviousScreenPosition = screenPosition;

            if (pointerState.DragStarted || deltaScreen.sqrMagnitude > 0f)
            {
                PointerDragged?.Invoke(CreatePointerGesture(pointerId, screenPosition, deltaScreen, ScreenDeltaToWorld(deltaScreen)));
            }
        }

        private void EndPointer(int pointerId, Vector2 screenPosition)
        {
            if (!pointerStates.TryGetValue(pointerId, out var pointerState))
            {
                return;
            }

            var totalDelta = screenPosition - pointerState.StartScreenPosition;
            var heldDuration = Time.unscaledTime - pointerState.StartTime;
            var isTap = totalDelta.sqrMagnitude <= tapMaxDistancePixels * tapMaxDistancePixels &&
                        heldDuration <= tapMaxDuration;

            var gesture = CreatePointerGesture(pointerId, screenPosition, Vector2.zero, Vector2.zero);
            if (isTap)
            {
                PointerTapped?.Invoke(gesture);
            }

            PointerReleased?.Invoke(gesture);
            pointerStates.Remove(pointerId);
        }

        private PointerGesture CreatePointerGesture(
            int pointerId,
            Vector2 screenPosition,
            Vector2 deltaScreen,
            Vector2 deltaWorld)
        {
            pointerStates.TryGetValue(pointerId, out var pointerState);
            var heldDuration = pointerState == null ? 0f : Time.unscaledTime - pointerState.StartTime;
            var isTouch = pointerState != null && pointerState.IsTouch;
            var dragStarted = pointerState != null && pointerState.DragStarted;

            return new PointerGesture(
                pointerId,
                screenPosition,
                ScreenToWorld(screenPosition),
                deltaScreen,
                deltaWorld,
                heldDuration,
                isTouch,
                dragStarted);
        }

        private Vector2 ScreenDeltaToWorld(Vector2 deltaScreen)
        {
            if (cachedCamera == null)
            {
                return Vector2.zero;
            }

            var originWorld = cachedCamera.ScreenToWorldPoint(Vector3.zero);
            var targetWorld = cachedCamera.ScreenToWorldPoint(deltaScreen);
            return targetWorld - originWorld;
        }

        private Vector2 ScreenToWorld(Vector2 screenPosition)
        {
            if (cachedCamera == null)
            {
                return Vector2.zero;
            }

            return cachedCamera.ScreenToWorldPoint(screenPosition);
        }

        private static Camera ResolveCamera()
        {
            if (Camera.main != null)
            {
                return Camera.main;
            }

            return Camera.allCamerasCount > 0 ? Camera.allCameras[0] : null;
        }
    }
}
