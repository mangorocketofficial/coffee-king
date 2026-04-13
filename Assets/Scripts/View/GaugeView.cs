using CoffeeKing.Util;
using UnityEngine;

namespace CoffeeKing.View
{
    public sealed class GaugeView
    {
        private readonly Transform root;
        private readonly Transform fillTransform;
        private readonly Transform targetTransform;
        private readonly SpriteRenderer fillRenderer;
        private readonly Vector2 size;

        private GaugeView(
            Transform root,
            Transform fillTransform,
            Transform targetTransform,
            SpriteRenderer fillRenderer,
            Vector2 size)
        {
            this.root = root;
            this.fillTransform = fillTransform;
            this.targetTransform = targetTransform;
            this.fillRenderer = fillRenderer;
            this.size = size;
        }

        public static GaugeView Create(Transform parent, Vector3 position, Vector2 size, Color frameColor, Color backgroundColor)
        {
            var rootObject = new GameObject("Gauge");
            rootObject.transform.SetParent(parent, false);
            rootObject.transform.position = position;

            var frame = CreateRect(
                "Frame",
                rootObject.transform,
                new Vector3(-(size.x + 0.12f) * 0.5f, 0f, 0f),
                size + new Vector2(0.12f, 0.12f),
                frameColor,
                20);
            var background = CreateRect(
                "Background",
                rootObject.transform,
                new Vector3(-size.x * 0.5f, 0f, 0f),
                size,
                backgroundColor,
                21);
            var target = CreateRect(
                "Target",
                rootObject.transform,
                new Vector3(-size.x * 0.5f, 0f, 0f),
                new Vector2(0.6f, size.y - 0.12f),
                Color.white,
                22);
            var fill = CreateRect("Fill", rootObject.transform, new Vector3(-size.x * 0.5f, 0f, 0f), new Vector2(size.x, size.y - 0.16f), Color.white, 23);

            fill.transform.localScale = new Vector3(0f, 1f, 1f);

            rootObject.SetActive(false);

            return new GaugeView(rootObject.transform, fill.transform, target.transform, fill, size);
        }

        public Transform Transform => root;

        public void SetVisible(bool isVisible)
        {
            if (root != null)
            {
                root.gameObject.SetActive(isVisible);
            }
        }

        public void Configure(string title, Color fillColor, Color targetColor, float minNormalized, float maxNormalized)
        {
            fillRenderer.color = fillColor;
            SetTargetZone(minNormalized, maxNormalized, targetColor);
            SetValue(0f, string.Empty);
        }

        public void SetValue(float normalized, string valueLabel)
        {
            var clamped = Mathf.Clamp01(normalized);
            fillTransform.localScale = new Vector3(clamped, 1f, 1f);
        }

        public void SetTargetZone(float minNormalized, float maxNormalized, Color color)
        {
            var clampedMin = Mathf.Clamp01(minNormalized);
            var clampedMax = Mathf.Clamp01(maxNormalized);
            var width = Mathf.Max(0.01f, (clampedMax - clampedMin) * size.x);
            targetTransform.localPosition = new Vector3((-size.x * 0.5f) + (clampedMin * size.x), 0f, 0f);
            targetTransform.localScale = Vector3.one;
            var renderer = targetTransform.GetComponent<SpriteRenderer>();
            renderer.sprite = SpriteFactory.CreateRect("GaugeTarget", new Vector2(width, size.y - 0.12f), color);
            renderer.color = color;
        }

        private static SpriteRenderer CreateRect(
            string name,
            Transform parent,
            Vector3 localPosition,
            Vector2 rectSize,
            Color color,
            int sortingOrder)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPosition;
            var renderer = go.AddComponent<SpriteRenderer>();
            renderer.sprite = SpriteFactory.CreateRect(name, rectSize, color, new Vector2(0f, 0.5f));
            renderer.color = color;
            renderer.sortingOrder = sortingOrder;
            return renderer;
        }

    }
}
