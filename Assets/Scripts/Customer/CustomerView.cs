using CoffeeKing.Core;
using CoffeeKing.Util;
using UnityEngine;

namespace CoffeeKing.CustomerLogic
{
    public sealed class CustomerView
    {
        private static readonly Vector3 BubbleLocalPosition = new Vector3(0f, 1.1f, 0f);
        private const float BubbleHeight = 0.5f;
        private const float BubbleMinWidth = 1.4f;
        private const float BubbleMaxWidth = 2.8f;
        private const float BubblePadding = 0.7f;
        private const float BubbleHideDelay = 2.6f;

        private readonly Transform root;
        private readonly SpriteRenderer bodyRenderer;
        private readonly SpriteRenderer bubbleRenderer;
        private readonly Transform patienceFill;
        private readonly SpriteRenderer patienceFillRenderer;
        private readonly TextMesh orderText;
        private readonly Vector2 bodySize;
        private readonly Color baseColor;
        private readonly Color speechBubbleColor;

        private Vector3 lanePosition;
        private Vector3 spawnPosition;
        private Vector3 exitPosition;
        private bool exiting;
        private float orderBubbleTimer;

        private CustomerView(
            Transform root,
            SpriteRenderer bodyRenderer,
            SpriteRenderer bubbleRenderer,
            Transform patienceFill,
            SpriteRenderer patienceFillRenderer,
            TextMesh orderText,
            Vector2 bodySize,
            Color baseColor,
            Color speechBubbleColor)
        {
            this.root = root;
            this.bodyRenderer = bodyRenderer;
            this.bubbleRenderer = bubbleRenderer;
            this.patienceFill = patienceFill;
            this.patienceFillRenderer = patienceFillRenderer;
            this.orderText = orderText;
            this.bodySize = bodySize;
            this.baseColor = baseColor;
            this.speechBubbleColor = speechBubbleColor;
        }

        public static CustomerView Create(Transform parent, GameConfig config)
        {
            var rootObject = new GameObject("CustomerView");
            rootObject.transform.SetParent(parent, false);

            var body = CreateRect("Body", SpriteAssetNames.Customer, rootObject.transform, Vector3.zero, config.CustomerSize, config.CustomerColor, 40);
            var bubble = CreateRect("Bubble", rootObject.transform, BubbleLocalPosition, config.SpeechBubbleSize, config.SpeechBubbleColor, 41, new Vector2(0.5f, 0.5f));
            var halfH = config.CustomerSize.y * 0.5f;
            var barY = halfH + 0.08f;
            var patienceFrame = CreateRect("PatienceFrame", rootObject.transform, new Vector3(-0.5f, barY, 0f), new Vector2(1.0f, 0.1f), config.GaugeFrameColor, 42, new Vector2(0f, 0.5f));
            var patienceBack = CreateRect("PatienceBack", rootObject.transform, new Vector3(-0.47f, barY, 0f), new Vector2(0.94f, 0.06f), config.GaugeBackgroundColor, 43, new Vector2(0f, 0.5f));
            var patienceFill = CreateRect("PatienceFill", rootObject.transform, new Vector3(-0.47f, barY, 0f), new Vector2(0.94f, 0.06f), ColorPalette.HighlightGood, 44, new Vector2(0f, 0.5f));
            patienceFill.transform.localScale = Vector3.one;

            var order = CreateText("OrderText", rootObject.transform, BubbleLocalPosition, string.Empty, config.InstructionTextColor, 0.055f, 45);

            rootObject.SetActive(false);

            return new CustomerView(
                rootObject.transform,
                body,
                bubble,
                patienceFill.transform,
                patienceFill,
                order,
                config.CustomerSize,
                config.CustomerColor,
                config.SpeechBubbleColor);
        }

        public void Bind(Customer customer, Vector3 targetLanePosition)
        {
            lanePosition = targetLanePosition;
            spawnPosition = targetLanePosition + new Vector3(0f, 1.2f, 0f);
            exitPosition = targetLanePosition + new Vector3(0f, 1.5f, 0f);
            root.gameObject.SetActive(true);
            root.position = spawnPosition;
            exiting = false;

            bodyRenderer.sprite = SpriteFactory.Load(customer.AppearanceAssetName, bodySize, baseColor);
            bodyRenderer.color = Color.white;
            ShowOrderBubble(customer.Order.DisplayName);
            bubbleRenderer.color = speechBubbleColor;
            SetPatience(customer.PatienceNormalized);
        }

        public void Tick(float deltaTime)
        {
            if (!root.gameObject.activeSelf)
            {
                return;
            }

            var target = exiting ? exitPosition : lanePosition;
            root.position = Vector3.MoveTowards(root.position, target, deltaTime * 6f);

            if (orderBubbleTimer > 0f)
            {
                orderBubbleTimer = Mathf.Max(0f, orderBubbleTimer - deltaTime);
                if (orderBubbleTimer <= 0f)
                {
                    SetOrderBubbleVisible(false);
                }
            }

            if (exiting && Vector3.Distance(root.position, target) < 0.01f)
            {
                root.gameObject.SetActive(false);
            }
        }

        public void SetPatience(float normalized)
        {
            var clamped = Mathf.Clamp01(normalized);
            patienceFill.localScale = new Vector3(clamped, 1f, 1f);
            patienceFillRenderer.color = Color.Lerp(ColorPalette.HighlightBad, ColorPalette.HighlightGood, clamped);

            // Expression tint based on patience level
            if (!exiting)
            {
                if (clamped < 0.2f)
                {
                    bodyRenderer.color = new Color(1f, 0.75f, 0.70f); // critical red tint
                }
                else if (clamped < 0.5f)
                {
                    bodyRenderer.color = new Color(1f, 0.90f, 0.75f); // impatient orange tint
                }
                else
                {
                    bodyRenderer.color = Color.white; // neutral
                }
            }
        }

        public void SetFocused(bool isFocused)
        {
            bodyRenderer.color = isFocused ? new Color(0.84f, 0.96f, 0.88f) : Color.white;
        }

        public void MarkServed()
        {
            bodyRenderer.color = new Color(0.80f, 1f, 0.85f); // happy green tint on serve
            SetOrderBubbleVisible(false);
            exiting = true;
        }

        public void MarkRejected()
        {
            bodyRenderer.color = new Color(1f, 0.55f, 0.55f);
            SetOrderBubbleVisible(false);
            exiting = true;
        }

        public void MarkTimedOut()
        {
            bodyRenderer.color = new Color(1f, 0.82f, 0.82f);
            SetOrderBubbleVisible(false);
            SetPatience(0f);
            exiting = true;
        }

        public void Destroy()
        {
            if (root != null)
            {
                Object.Destroy(root.gameObject);
            }
        }

        private static SpriteRenderer CreateRect(
            string name,
            string assetName,
            Transform parent,
            Vector3 localPosition,
            Vector2 size,
            Color color,
            int sortingOrder)
        {
            return CreateRect(name, assetName, parent, localPosition, size, color, sortingOrder, new Vector2(0.5f, 0.5f));
        }

        private static SpriteRenderer CreateRect(
            string name,
            Transform parent,
            Vector3 localPosition,
            Vector2 size,
            Color color,
            int sortingOrder,
            Vector2 pivot)
        {
            return CreateRect(name, null, parent, localPosition, size, color, sortingOrder, pivot);
        }

        private static SpriteRenderer CreateRect(
            string name,
            string assetName,
            Transform parent,
            Vector3 localPosition,
            Vector2 size,
            Color color,
            int sortingOrder,
            Vector2 pivot)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPosition;
            var renderer = go.AddComponent<SpriteRenderer>();
            renderer.sprite = assetName == null
                ? SpriteFactory.CreateRect(name, size, color, pivot)
                : SpriteFactory.Load(assetName, size, color, pivot);
            renderer.color = assetName == null ? color : Color.white;
            renderer.sortingOrder = sortingOrder;
            return renderer;
        }

        private void ShowOrderBubble(string text)
        {
            orderText.text = text;

            var estimatedWidth = Mathf.Clamp((text.Length * 0.18f) + BubblePadding, BubbleMinWidth, BubbleMaxWidth);
            var bubbleSize = new Vector2(estimatedWidth, BubbleHeight);

            bubbleRenderer.sprite = SpriteFactory.CreateEllipse("customer_order_bubble", bubbleSize, speechBubbleColor);
            bubbleRenderer.color = speechBubbleColor;
            bubbleRenderer.transform.localPosition = BubbleLocalPosition;
            orderText.transform.localPosition = BubbleLocalPosition + new Vector3(0f, 0.01f, 0f);

            SetOrderBubbleVisible(true);
            orderBubbleTimer = BubbleHideDelay;
        }

        private void SetOrderBubbleVisible(bool isVisible)
        {
            if (bubbleRenderer != null)
            {
                bubbleRenderer.enabled = isVisible;
            }

            if (orderText != null)
            {
                var renderer = orderText.GetComponent<MeshRenderer>();
                if (renderer != null)
                {
                    renderer.enabled = isVisible;
                }
            }
        }

        private static TextMesh CreateText(
            string name,
            Transform parent,
            Vector3 localPosition,
            string text,
            Color color,
            float characterSize,
            int sortingOrder)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPosition;
            var textMesh = go.AddComponent<TextMesh>();
            textMesh.text = text;
            textMesh.color = color;
            textMesh.fontSize = 64;
            textMesh.characterSize = characterSize;
            textMesh.anchor = TextAnchor.MiddleCenter;
            textMesh.alignment = TextAlignment.Center;
            go.GetComponent<MeshRenderer>().sortingOrder = sortingOrder;
            return textMesh;
        }
    }
}
