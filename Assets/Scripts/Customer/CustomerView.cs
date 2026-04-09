using CoffeeKing.Core;
using CoffeeKing.Util;
using UnityEngine;

namespace CoffeeKing.CustomerLogic
{
    public sealed class CustomerView
    {
        private readonly Transform root;
        private readonly SpriteRenderer bodyRenderer;
        private readonly SpriteRenderer bubbleRenderer;
        private readonly Transform patienceFill;
        private readonly SpriteRenderer patienceFillRenderer;
        private readonly TextMesh orderText;
        private readonly TextMesh labelText;
        private readonly Color baseColor;
        private readonly Color speechBubbleColor;

        private Vector3 lanePosition;
        private Vector3 spawnPosition;
        private Vector3 exitPosition;
        private bool exiting;

        private CustomerView(
            Transform root,
            SpriteRenderer bodyRenderer,
            SpriteRenderer bubbleRenderer,
            Transform patienceFill,
            SpriteRenderer patienceFillRenderer,
            TextMesh orderText,
            TextMesh labelText,
            Color baseColor,
            Color speechBubbleColor)
        {
            this.root = root;
            this.bodyRenderer = bodyRenderer;
            this.bubbleRenderer = bubbleRenderer;
            this.patienceFill = patienceFill;
            this.patienceFillRenderer = patienceFillRenderer;
            this.orderText = orderText;
            this.labelText = labelText;
            this.baseColor = baseColor;
            this.speechBubbleColor = speechBubbleColor;
        }

        public static CustomerView Create(Transform parent, GameConfig config)
        {
            var rootObject = new GameObject("CustomerView");
            rootObject.transform.SetParent(parent, false);

            var body = CreateRect("Body", SpriteAssetNames.Customer, rootObject.transform, Vector3.zero, config.CustomerSize, config.CustomerColor, 11);
            var bubble = CreateRect("Bubble", SpriteAssetNames.SpeechBubble, rootObject.transform, new Vector3(0f, 1.55f, 0f), config.SpeechBubbleSize, config.SpeechBubbleColor, 12);
            var patienceFrame = CreateRect("PatienceFrame", rootObject.transform, new Vector3(-0.8f, -1.38f, 0f), new Vector2(1.6f, 0.16f), config.GaugeFrameColor, 12, new Vector2(0f, 0.5f));
            var patienceBack = CreateRect("PatienceBack", rootObject.transform, new Vector3(-0.76f, -1.38f, 0f), new Vector2(1.52f, 0.10f), config.GaugeBackgroundColor, 13, new Vector2(0f, 0.5f));
            var patienceFill = CreateRect("PatienceFill", rootObject.transform, new Vector3(-0.76f, -1.38f, 0f), new Vector2(1.52f, 0.10f), ColorPalette.HighlightGood, 14, new Vector2(0f, 0.5f));
            patienceFill.transform.localScale = Vector3.one;

            var order = CreateText("OrderText", rootObject.transform, new Vector3(0f, 1.58f, 0f), string.Empty, config.InstructionTextColor, 0.07f, 15);
            var label = CreateText("LabelText", rootObject.transform, new Vector3(0f, -1.68f, 0f), string.Empty, config.SecondaryTextColor, 0.065f, 15);

            rootObject.SetActive(false);

            return new CustomerView(
                rootObject.transform,
                body,
                bubble,
                patienceFill.transform,
                patienceFill,
                order,
                label,
                config.CustomerColor,
                config.SpeechBubbleColor);
        }

        public void Bind(Customer customer, Vector3 targetLanePosition)
        {
            lanePosition = targetLanePosition;
            spawnPosition = targetLanePosition + new Vector3(0f, 2.5f, 0f);
            exitPosition = targetLanePosition + new Vector3(0f, 2.9f, 0f);
            root.gameObject.SetActive(true);
            root.position = spawnPosition;
            exiting = false;

            orderText.text = customer.Order.DisplayName;
            labelText.text = $"Customer {customer.SequenceNumber}";
            bodyRenderer.color = baseColor;
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
        }

        public void SetFocused(bool isFocused)
        {
            bodyRenderer.color = isFocused ? ColorPalette.ServingAreaActive : baseColor;
        }

        public void MarkServed()
        {
            bodyRenderer.color = ColorPalette.HighlightGood;
            bubbleRenderer.color = new Color(0.89f, 0.97f, 0.90f);
            orderText.text = "Served";
            exiting = true;
        }

        public void MarkTimedOut()
        {
            bodyRenderer.color = ColorPalette.HighlightBad;
            bubbleRenderer.color = new Color(0.98f, 0.89f, 0.87f);
            orderText.text = "Timed Out";
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
            renderer.color = color;
            renderer.sortingOrder = sortingOrder;
            return renderer;
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
