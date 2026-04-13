using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace CoffeeKing.UI
{
    public sealed class ScreenFader : MonoBehaviour
    {
        private CanvasGroup canvasGroup;
        private Coroutine activeCoroutine;

        public float DefaultDuration { get; set; } = 0.35f;

        public static ScreenFader Create(Transform parent)
        {
            var canvasRoot = new GameObject("ScreenFaderCanvas");
            canvasRoot.transform.SetParent(parent, false);

            var canvas = canvasRoot.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 950;

            var scaler = canvasRoot.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;

            var root = UIBuilder.CreateStretchRoot("FaderRoot", canvasRoot.transform);
            var overlay = UIBuilder.CreateImage("FadeOverlay", root, Color.black);
            UIBuilder.Stretch(overlay.rectTransform);

            var group = canvasRoot.AddComponent<CanvasGroup>();
            group.alpha = 0f;
            group.blocksRaycasts = false;
            group.interactable = false;

            var fader = canvasRoot.AddComponent<ScreenFader>();
            fader.canvasGroup = group;

            return fader;
        }

        /// <summary>
        /// Fades out to black, invokes the action (screen switch), then fades back in.
        /// </summary>
        public void FadeTransition(Action onScreenSwitch, float duration = -1f)
        {
            if (duration < 0f)
            {
                duration = DefaultDuration;
            }

            if (activeCoroutine != null)
            {
                StopCoroutine(activeCoroutine);
            }

            activeCoroutine = StartCoroutine(RunFadeTransition(onScreenSwitch, duration));
        }

        /// <summary>
        /// Quick flash overlay (for grade feedback effects).
        /// </summary>
        public void Flash(Color color, float duration = 0.15f)
        {
            StartCoroutine(RunFlash(color, duration));
        }

        private IEnumerator RunFadeTransition(Action onScreenSwitch, float duration)
        {
            canvasGroup.blocksRaycasts = true;

            // Fade out (transparent -> opaque)
            var elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                canvasGroup.alpha = Mathf.Clamp01(elapsed / duration);
                yield return null;
            }

            canvasGroup.alpha = 1f;

            // Execute the screen switch
            onScreenSwitch?.Invoke();
            yield return null;

            // Fade in (opaque -> transparent)
            elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                canvasGroup.alpha = 1f - Mathf.Clamp01(elapsed / duration);
                yield return null;
            }

            canvasGroup.alpha = 0f;
            canvasGroup.blocksRaycasts = false;
            activeCoroutine = null;
        }

        private IEnumerator RunFlash(Color color, float duration)
        {
            // Temporarily change the overlay image color
            var overlay = GetComponentInChildren<Image>();
            var originalColor = overlay.color;
            overlay.color = color;

            canvasGroup.alpha = 0.35f;
            var elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                canvasGroup.alpha = 0.35f * (1f - Mathf.Clamp01(elapsed / duration));
                yield return null;
            }

            canvasGroup.alpha = 0f;
            overlay.color = originalColor;
        }
    }
}
