using System;
using CoffeeKing.GameInput;
using CoffeeKing.Scoring;
using CoffeeKing.StageFlow;
using CoffeeKing.Util;
using UnityEngine;

namespace CoffeeKing.UI
{
    public sealed class ResultScreenView
    {
        private readonly Transform root;
        private readonly SpriteRenderer nextButtonRenderer;
        private readonly SpriteRenderer retryButtonRenderer;
        private readonly TextMesh titleText;
        private readonly TextMesh scoreText;
        private readonly TextMesh starsText;
        private readonly TextMesh summaryText;
        private readonly TextMesh nextLabelText;
        private readonly TextMesh retryLabelText;
        private readonly GestureDetector gestureDetector;

        private bool isVisible;
        private bool canAdvance;

        public ResultScreenView(Transform root, GestureDetector gestureDetector)
        {
            this.root = root;
            this.gestureDetector = gestureDetector;
            nextButtonRenderer = root.Find("NextButton").GetComponent<SpriteRenderer>();
            retryButtonRenderer = root.Find("RetryButton").GetComponent<SpriteRenderer>();
            titleText = root.Find("TitleText").GetComponent<TextMesh>();
            scoreText = root.Find("ScoreText").GetComponent<TextMesh>();
            starsText = root.Find("StarsText").GetComponent<TextMesh>();
            summaryText = root.Find("SummaryText").GetComponent<TextMesh>();
            nextLabelText = root.Find("NextButton/Label").GetComponent<TextMesh>();
            retryLabelText = root.Find("RetryButton/Label").GetComponent<TextMesh>();

            root.gameObject.SetActive(false);
            gestureDetector.PointerTapped += HandlePointerTapped;
        }

        public event Action NextRequested;
        public event Action RetryRequested;

        public void Show(StageResult result, string summary, bool showNextButton)
        {
            isVisible = true;
            canAdvance = showNextButton;
            root.gameObject.SetActive(true);

            titleText.text = result.Passed ? $"{result.Stage.DisplayName} Clear" : $"{result.Stage.DisplayName} Failed";
            scoreText.text = $"Score {result.Score}/{result.MaxScore}  {(result.Percentage * 100f):0}%";
            starsText.text = $"Stars {StarRating.ToDisplayString(result.Stars)}";
            summaryText.text = summary;

            nextButtonRenderer.gameObject.SetActive(showNextButton);
            nextLabelText.gameObject.SetActive(showNextButton);
            nextLabelText.text = result.Stage.Number >= 5 ? "Replay" : "Next Stage";
            retryLabelText.text = "Retry";
        }

        public void Hide()
        {
            isVisible = false;
            canAdvance = false;
            root.gameObject.SetActive(false);
        }

        public void Dispose()
        {
            gestureDetector.PointerTapped -= HandlePointerTapped;
        }

        private void HandlePointerTapped(PointerGesture gesture)
        {
            if (!isVisible)
            {
                return;
            }

            if (retryButtonRenderer.bounds.Contains(gesture.WorldPosition))
            {
                RetryRequested?.Invoke();
                return;
            }

            if (canAdvance && nextButtonRenderer.bounds.Contains(gesture.WorldPosition))
            {
                NextRequested?.Invoke();
            }
        }

        public static ResultScreenView Create(Transform parent, GestureDetector gestureDetector)
        {
            var root = new GameObject("ResultScreen").transform;
            root.SetParent(parent, false);

            CreateRect("Backdrop", root, Vector3.zero, new Vector2(12f, 7.2f), new Color(0.10f, 0.08f, 0.06f, 0.92f), 90);
            CreateRect("Panel", root, Vector3.zero, new Vector2(8.4f, 5.8f), new Color(0.91f, 0.86f, 0.77f), 91);
            CreateText("TitleText", root, new Vector3(0f, 2.0f, 0f), string.Empty, ColorPalette.InstructionText, 0.12f, 94);
            CreateText("ScoreText", root, new Vector3(0f, 1.25f, 0f), string.Empty, ColorPalette.SecondaryText, 0.09f, 94);
            CreateText("StarsText", root, new Vector3(0f, 0.55f, 0f), string.Empty, ColorPalette.HighlightGood, 0.11f, 94);

            var summary = CreateText("SummaryText", root, new Vector3(0f, -0.65f, 0f), string.Empty, ColorPalette.SecondaryText, 0.075f, 94);
            summary.anchor = TextAnchor.UpperCenter;

            var retryButton = CreateRect("RetryButton", root, new Vector3(-1.7f, -2.1f, 0f), new Vector2(2.4f, 0.8f), new Color(0.76f, 0.50f, 0.20f), 92);
            CreateText("Label", retryButton.transform, Vector3.zero, "Retry", Color.white, 0.09f, 94);

            var nextButton = CreateRect("NextButton", root, new Vector3(1.7f, -2.1f, 0f), new Vector2(2.4f, 0.8f), new Color(0.29f, 0.67f, 0.45f), 92);
            CreateText("Label", nextButton.transform, Vector3.zero, "Next", Color.white, 0.09f, 94);

            return new ResultScreenView(root, gestureDetector);
        }

        private static SpriteRenderer CreateRect(string name, Transform parent, Vector3 localPosition, Vector2 size, Color color, int sortingOrder)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPosition;
            var renderer = go.AddComponent<SpriteRenderer>();
            renderer.sprite = SpriteFactory.CreateRect(name, size, color);
            renderer.color = color;
            renderer.sortingOrder = sortingOrder;
            return renderer;
        }

        private static TextMesh CreateText(string name, Transform parent, Vector3 localPosition, string text, Color color, float characterSize, int sortingOrder)
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
