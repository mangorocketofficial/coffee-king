using System;
using CoffeeKing.Scoring;
using CoffeeKing.StageFlow;
using CoffeeKing.Util;
using UnityEngine;
using UnityEngine.UI;

namespace CoffeeKing.UI
{
    public sealed class ResultScreenView
    {
        private readonly RectTransform root;
        private readonly Text titleText;
        private readonly Text scoreText;
        private readonly Text earningsText;
        private readonly Text summaryText;
        private readonly Button nextButton;
        private readonly Button retryButton;
        private readonly Text nextLabelText;
        private readonly Text retryLabelText;
        private readonly ResultAnimator resultAnimator;

        private bool isVisible;

        private ResultScreenView(
            RectTransform root,
            Text titleText,
            Text scoreText,
            Text earningsText,
            Text summaryText,
            Button nextButton,
            Button retryButton,
            Text nextLabelText,
            Text retryLabelText,
            ResultAnimator resultAnimator)
        {
            this.root = root;
            this.titleText = titleText;
            this.scoreText = scoreText;
            this.earningsText = earningsText;
            this.summaryText = summaryText;
            this.nextButton = nextButton;
            this.retryButton = retryButton;
            this.nextLabelText = nextLabelText;
            this.retryLabelText = retryLabelText;
            this.resultAnimator = resultAnimator;

            nextButton.onClick.AddListener(HandleNextClicked);
            retryButton.onClick.AddListener(HandleRetryClicked);
            root.gameObject.SetActive(false);
        }

        public event Action NextRequested;
        public event Action RetryRequested;

        public static ResultScreenView Create(Transform parent, CoffeeKing.GameInput.GestureDetector gestureDetector)
        {
            var canvasRoot = new GameObject("ResultScreenCanvas");
            canvasRoot.transform.SetParent(parent, false);

            var canvas = canvasRoot.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 700;

            var scaler = canvasRoot.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;
            canvasRoot.AddComponent<GraphicRaycaster>();

            var root = UIBuilder.CreateStretchRoot("ResultScreen", canvasRoot.transform);

            var backdrop = UIBuilder.CreateImage("Backdrop", root, new Color(0.11f, 0.08f, 0.06f, 0.82f));
            UIBuilder.Stretch(backdrop.rectTransform);

            var panel = UIBuilder.CreateImage("Panel", root, new Color(0.96f, 0.93f, 0.88f, 0.98f));
            SetRect(panel.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-420f, -320f), new Vector2(420f, 320f));

            var accent = UIBuilder.CreateImage("Accent", panel.transform, new Color(0.27f, 0.18f, 0.11f, 1f));
            SetRect(accent.rectTransform, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0f, -18f), new Vector2(0f, 0f));

            var titleText = UIBuilder.CreateText("TitleText", panel.transform, string.Empty, 54, new Color(0.20f, 0.14f, 0.10f), TextAnchor.MiddleCenter);
            titleText.fontStyle = FontStyle.Bold;
            SetRect(titleText.rectTransform, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(32f, -86f), new Vector2(-32f, -26f));

            var scoreCard = UIBuilder.CreateImage("ScoreCard", panel.transform, new Color(0.90f, 0.86f, 0.79f, 1f));
            SetRect(scoreCard.rectTransform, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(32f, -162f), new Vector2(-32f, -92f));

            var scoreText = UIBuilder.CreateText("ScoreText", scoreCard.transform, string.Empty, 30, new Color(0.20f, 0.14f, 0.10f), TextAnchor.MiddleCenter);
            UIBuilder.Stretch(scoreText.rectTransform);
            scoreText.rectTransform.offsetMin = new Vector2(20f, 6f);
            scoreText.rectTransform.offsetMax = new Vector2(-20f, -6f);

            var earningsCard = UIBuilder.CreateImage("EarningsCard", panel.transform, new Color(0.85f, 0.92f, 0.86f, 1f));
            SetRect(earningsCard.rectTransform, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(32f, -238f), new Vector2(-32f, -168f));

            var earningsText = UIBuilder.CreateText("EarningsText", earningsCard.transform, string.Empty, 30, new Color(0.13f, 0.42f, 0.20f), TextAnchor.MiddleCenter);
            earningsText.fontStyle = FontStyle.Bold;
            UIBuilder.Stretch(earningsText.rectTransform);
            earningsText.rectTransform.offsetMin = new Vector2(20f, 6f);
            earningsText.rectTransform.offsetMax = new Vector2(-20f, -6f);

            var summaryPanel = UIBuilder.CreateImage("SummaryPanel", panel.transform, new Color(0.98f, 0.97f, 0.93f, 1f));
            SetRect(summaryPanel.rectTransform, new Vector2(0f, 0f), new Vector2(1f, 1f), new Vector2(32f, 92f), new Vector2(-32f, -252f));

            var summaryText = UIBuilder.CreateText("SummaryText", summaryPanel.transform, string.Empty, 26, ColorPalette.SecondaryText, TextAnchor.UpperLeft);
            UIBuilder.Stretch(summaryText.rectTransform);
            summaryText.rectTransform.offsetMin = new Vector2(24f, 20f);
            summaryText.rectTransform.offsetMax = new Vector2(-24f, -20f);

            var retryButton = UIBuilder.CreateButton("RetryButton", panel.transform, "Retry", new Color(0.74f, 0.48f, 0.20f), Color.white);
            SetRect(retryButton.GetComponent<RectTransform>(), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(-280f, 24f), new Vector2(-20f, 84f));

            var nextButton = UIBuilder.CreateButton("NextButton", panel.transform, "Next Day", new Color(0.29f, 0.67f, 0.45f), Color.white);
            SetRect(nextButton.GetComponent<RectTransform>(), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(20f, 24f), new Vector2(280f, 84f));

            var nextLabelText = nextButton.GetComponentInChildren<Text>();
            var retryLabelText = retryButton.GetComponentInChildren<Text>();

            var resultAnimator = canvasRoot.AddComponent<ResultAnimator>();
            resultAnimator.Initialize(scoreText, earningsText);

            return new ResultScreenView(root, titleText, scoreText, earningsText, summaryText, nextButton, retryButton, nextLabelText, retryLabelText, resultAnimator);
        }

        public void Show(StageResult result, string summary)
        {
            isVisible = true;
            root.gameObject.SetActive(true);

            titleText.text = $"{result.Stage.DisplayName} Complete";
            titleText.color = new Color(0.20f, 0.41f, 0.24f);

            summaryText.text = summary;

            nextButton.gameObject.SetActive(true);
            nextLabelText.gameObject.SetActive(true);
            nextLabelText.text = "Next Day";
            retryLabelText.text = "Retry";

            resultAnimator.PlayReveal(result.Score, result.MaxScore, result.DailyEarnings, result.TotalEarnings);
        }

        public void Hide()
        {
            isVisible = false;
            resultAnimator.StopAnimation();
            root.gameObject.SetActive(false);
        }

        public void Dispose()
        {
            nextButton.onClick.RemoveListener(HandleNextClicked);
            retryButton.onClick.RemoveListener(HandleRetryClicked);
        }

        private void HandleNextClicked()
        {
            if (isVisible)
            {
                NextRequested?.Invoke();
            }
        }

        private void HandleRetryClicked()
        {
            if (isVisible)
            {
                RetryRequested?.Invoke();
            }
        }

        private static void SetRect(RectTransform rectTransform, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax)
        {
            rectTransform.anchorMin = anchorMin;
            rectTransform.anchorMax = anchorMax;
            rectTransform.offsetMin = offsetMin;
            rectTransform.offsetMax = offsetMax;
        }
    }
}
