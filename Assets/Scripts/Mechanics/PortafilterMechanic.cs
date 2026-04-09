using System;
using CoffeeKing.Core;
using CoffeeKing.GameInput;
using CoffeeKing.View;
using UnityEngine;

namespace CoffeeKing.Mechanics
{
    public sealed class PortafilterMechanic : MonoBehaviour
    {
        private enum PortafilterState
        {
            Idle,
            Dragging,
            Snapping,
            Locked
        }

        private GameConfig config;
        private GestureDetector gestureDetector;
        private GrayboxSceneContext sceneContext;

        private Transform portafilterRoot;
        private SpriteRenderer bodyRenderer;
        private PortafilterState state;
        private bool interactionEnabled;
        private int activePointerId = int.MinValue;
        private Vector3 dragOffset;
        private float lastPointerAngle;
        private float currentRotationAngle;
        private float rotationUnlockTime;

        public event Action Locked;

        public bool IsLocked => state == PortafilterState.Locked;

        public void Initialize(GameConfig runtimeConfig, GestureDetector detector, GrayboxSceneContext context)
        {
            Unsubscribe();

            config = runtimeConfig;
            gestureDetector = detector;
            sceneContext = context;

            CreateVisual();
            Subscribe();
            ResetMechanic();
        }

        public void BeginRound()
        {
            interactionEnabled = true;
            SetVisible(true);
            ResetMechanic();
        }

        public void CancelStep()
        {
            interactionEnabled = false;
            activePointerId = int.MinValue;
            if (portafilterRoot != null)
            {
                portafilterRoot.gameObject.SetActive(false);
            }

            if (sceneContext?.MachineSlotRenderer != null)
            {
                sceneContext.MachineSlotRenderer.color = config.MachineSlotIdleColor;
            }
        }

        public void SetVisible(bool isVisible)
        {
            if (portafilterRoot != null)
            {
                portafilterRoot.gameObject.SetActive(isVisible);
            }
        }

        private void OnDestroy()
        {
            Unsubscribe();
        }

        private void HandlePointerPressed(PointerGesture gesture)
        {
            if (!interactionEnabled || state == PortafilterState.Locked || bodyRenderer == null)
            {
                return;
            }

            if (!bodyRenderer.bounds.Contains(gesture.WorldPosition))
            {
                return;
            }

            activePointerId = gesture.PointerId;
            dragOffset = portafilterRoot.position - (Vector3)gesture.WorldPosition;
            state = PortafilterState.Dragging;
            bodyRenderer.sortingOrder = 20;
            sceneContext.SetStatus("Move the portafilter toward the highlighted slot.");
        }

        private void HandlePointerDragged(PointerGesture gesture)
        {
            if (!interactionEnabled || gesture.PointerId != activePointerId || portafilterRoot == null)
            {
                return;
            }

            if (state == PortafilterState.Dragging)
            {
                portafilterRoot.position = (Vector3)gesture.WorldPosition + dragOffset;

                if (Vector2.Distance(portafilterRoot.position, sceneContext.MachineSlotPosition) <= config.PortafilterSnapDistance)
                {
                    EnterSnapState(gesture.WorldPosition);
                }
            }
            else if (state == PortafilterState.Snapping)
            {
                UpdateRotationFromPointer(gesture.WorldPosition);
            }
        }

        private void HandlePointerReleased(PointerGesture gesture)
        {
            if (!interactionEnabled || gesture.PointerId != activePointerId)
            {
                return;
            }

            if (state != PortafilterState.Locked)
            {
                ResetMechanic();
            }

            activePointerId = int.MinValue;
        }

        private void HandleRotate(RotateGesture gesture)
        {
            if (!interactionEnabled || state != PortafilterState.Snapping || !gesture.IsMultiTouch)
            {
                return;
            }

            currentRotationAngle = Mathf.Clamp(currentRotationAngle + gesture.DeltaDegrees, -180f, 180f);
            ApplyRotation(currentRotationAngle);
            UpdateProgressStatus();

            if (Mathf.Abs(currentRotationAngle) >= config.PortafilterLockAngle)
            {
                LockPortafilter();
            }
        }

        private void CreateVisual()
        {
            if (portafilterRoot != null)
            {
                Destroy(portafilterRoot.gameObject);
            }

            var rootObject = new GameObject("Portafilter");
            portafilterRoot = rootObject.transform;
            portafilterRoot.SetParent(sceneContext.InteractiveRoot, false);

            var bodyObject = new GameObject("Body");
            bodyObject.transform.SetParent(portafilterRoot, false);
            bodyObject.transform.localPosition = new Vector3(config.PortafilterBodySize.x * 0.45f, 0f, 0f);

            bodyRenderer = bodyObject.AddComponent<SpriteRenderer>();
            bodyRenderer.sprite = CoffeeKing.Util.SpriteFactory.Load(
                CoffeeKing.Util.SpriteAssetNames.Portafilter,
                config.PortafilterBodySize,
                config.PortafilterIdleColor);
            bodyRenderer.sortingOrder = 10;
        }

        private void ResetMechanic()
        {
            state = PortafilterState.Idle;
            currentRotationAngle = 0f;
            activePointerId = int.MinValue;
            dragOffset = Vector3.zero;

            if (portafilterRoot != null)
            {
                portafilterRoot.position = config.PortafilterSpawnPosition;
                portafilterRoot.rotation = Quaternion.identity;
            }

            if (bodyRenderer != null)
            {
                bodyRenderer.color = config.PortafilterIdleColor;
                bodyRenderer.sortingOrder = 10;
            }

            if (sceneContext?.MachineSlotRenderer != null)
            {
                sceneContext.MachineSlotRenderer.color = config.MachineSlotIdleColor;
            }

            sceneContext?.SetStatus("Drag the portafilter into the slot.");
        }

        private void EnterSnapState(Vector2 pointerWorldPosition)
        {
            state = PortafilterState.Snapping;
            portafilterRoot.position = sceneContext.MachineSlotPosition;
            currentRotationAngle = 0f;
            lastPointerAngle = GetPointerAngle(pointerWorldPosition);
            rotationUnlockTime = Time.unscaledTime + 0.12f;

            if (bodyRenderer != null)
            {
                bodyRenderer.color = config.PortafilterSnapColor;
                bodyRenderer.sortingOrder = 12;
            }

            if (sceneContext.MachineSlotRenderer != null)
            {
                sceneContext.MachineSlotRenderer.color = config.MachineSlotSnapColor;
            }

            sceneContext.SetStatus("Move in a circle around the slot to lock the handle.");
        }

        private void UpdateRotationFromPointer(Vector2 pointerWorldPosition)
        {
            var offset = pointerWorldPosition - (Vector2)sceneContext.MachineSlotPosition;
            if (offset.sqrMagnitude < config.PortafilterRotateRadius * config.PortafilterRotateRadius)
            {
                return;
            }

            var pointerAngle = GetPointerAngle(pointerWorldPosition);
            if (Time.unscaledTime < rotationUnlockTime)
            {
                lastPointerAngle = pointerAngle;
                return;
            }

            var deltaAngle = Mathf.DeltaAngle(lastPointerAngle, pointerAngle);
            lastPointerAngle = pointerAngle;

            // Ignore sudden angle jumps caused by snapping into the slot.
            if (Mathf.Abs(deltaAngle) > 25f)
            {
                return;
            }

            if (Mathf.Abs(deltaAngle) < 0.1f)
            {
                return;
            }

            currentRotationAngle = Mathf.Clamp(currentRotationAngle + deltaAngle, -180f, 180f);
            ApplyRotation(currentRotationAngle);
            UpdateProgressStatus();

            if (Mathf.Abs(currentRotationAngle) >= config.PortafilterLockAngle)
            {
                LockPortafilter();
            }
        }

        private void UpdateProgressStatus()
        {
            var progress = Mathf.RoundToInt(Mathf.Abs(currentRotationAngle));
            sceneContext.SetStatus($"Rotate to lock: {progress}/{Mathf.RoundToInt(config.PortafilterLockAngle)}");
        }

        private void ApplyRotation(float degrees)
        {
            if (portafilterRoot != null)
            {
                portafilterRoot.rotation = Quaternion.Euler(0f, 0f, degrees);
            }
        }

        private float GetPointerAngle(Vector2 pointerWorldPosition)
        {
            var direction = pointerWorldPosition - (Vector2)sceneContext.MachineSlotPosition;
            return Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        }

        private void LockPortafilter()
        {
            state = PortafilterState.Locked;
            interactionEnabled = false;
            activePointerId = int.MinValue;

            if (bodyRenderer != null)
            {
                bodyRenderer.color = config.PortafilterLockedColor;
            }

            if (sceneContext.MachineSlotRenderer != null)
            {
                sceneContext.MachineSlotRenderer.color = config.MachineSlotLockedColor;
            }

            sceneContext.SetStatus("Locked. The grouphead is engaged.");
            Locked?.Invoke();
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
            gestureDetector.Rotated += HandleRotate;
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
            gestureDetector.Rotated -= HandleRotate;
        }
    }
}
