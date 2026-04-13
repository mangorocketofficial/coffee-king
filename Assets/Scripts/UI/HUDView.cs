using System;
using CoffeeKing.Scoring;
using CoffeeKing.Util;
using UnityEngine;
using UnityEngine.UI;

namespace CoffeeKing.UI
{
    public sealed class HUDView
    {
        private static readonly Color TimerDefaultColor = new Color(0.18f, 0.13f, 0.10f);
        private static readonly Color TimerWarningColor = new Color(0.79f, 0.43f, 0.14f);
        private static readonly Color TimerCriticalColor = new Color(0.74f, 0.22f, 0.18f);

        private readonly RectTransform root;
        private readonly Text scoreText;
        private readonly Text timerText;
        private readonly Text stageText;
        private readonly Text earningsText;
        private readonly Text orderListText;
        private readonly Image progressFill;
        private readonly Button pauseButton;

        public event Action PauseRequested;

        private HUDView(
            RectTransform root,
            Text scoreText,
            Text timerText,
            Text stageText,
            Text earningsText,
            Text orderListText,
            Image progressFill,
            Button pauseButton)
        {
            this.root = root;
            this.scoreText = scoreText;
            this.timerText = timerText;
            this.stageText = stageText;
            this.earningsText = earningsText;
            this.orderListText = orderListText;
            this.progressFill = progressFill;
            this.pauseButton = pauseButton;

            this.pauseButton.onClick.AddListener(HandlePauseClicked);
        }

        public static HUDView Create(Transform parent)
        {
            var root = UIBuilder.CreateStretchRoot("HUD", parent);

            var scoreText = UIBuilder.CreateText("ScoreText", root, string.Empty, 28, new Color(0.18f, 0.13f, 0.10f), TextAnchor.UpperLeft);
            SetRect(scoreText.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(24f, -20f), new Vector2(360f, 90f));

            var timerText = UIBuilder.CreateText("TimerText", root, string.Empty, 30, TimerDefaultColor, TextAnchor.UpperRight);
            SetRect(timerText.rectTransform, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-360f, -20f), new Vector2(-24f, 80f));

            var progressFrame = UIBuilder.CreateImage("ProgressFrame", root, new Color(0.19f, 0.14f, 0.10f, 0.92f));
            SetRect(progressFrame.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(-250f, -22f), new Vector2(250f, 8f));

            var progressBack = UIBuilder.CreateImage("ProgressBack", progressFrame.transform, new Color(0.89f, 0.86f, 0.80f, 1f));
            UIBuilder.Stretch(progressBack.rectTransform);
            progressBack.rectTransform.offsetMin = new Vector2(4f, 4f);
            progressBack.rectTransform.offsetMax = new Vector2(-4f, -4f);

            var progressFill = UIBuilder.CreateImage("ProgressFill", progressBack.transform, new Color(0.31f, 0.70f, 0.46f));
            progressFill.type = Image.Type.Filled;
            progressFill.fillMethod = Image.FillMethod.Horizontal;
            progressFill.fillOrigin = 0;
            progressFill.fillAmount = 0f;
            UIBuilder.Stretch(progressFill.rectTransform);

            var stageText = UIBuilder.CreateText("StageText", root, string.Empty, 22, new Color(0.18f, 0.13f, 0.10f), TextAnchor.MiddleCenter);
            SetRect(stageText.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(-300f, -58f), new Vector2(300f, -24f));

            var earningsText = UIBuilder.CreateText("EarningsText", root, string.Empty, 26, new Color(0.18f, 0.50f, 0.22f), TextAnchor.UpperLeft);
            SetRect(earningsText.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(24f, -110f), new Vector2(360f, -80f));

            var ordersMonitor = UIBuilder.CreateImage("OrdersMonitor", root, new Color(0.24f, 0.25f, 0.28f, 0.96f));
            SetRect(ordersMonitor.rectTransform, new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(18f, 18f), new Vector2(360f, 228f));

            var monitorBezel = UIBuilder.CreateImage("MonitorBezel", ordersMonitor.transform, new Color(0.83f, 0.85f, 0.89f, 1f));
            SetRect(monitorBezel.rectTransform, new Vector2(0f, 0f), new Vector2(1f, 1f), new Vector2(12f, 12f), new Vector2(-12f, -26f));

            var monitorScreen = UIBuilder.CreateImage("MonitorScreen", ordersMonitor.transform, new Color(0.08f, 0.10f, 0.12f, 1f));
            SetRect(monitorScreen.rectTransform, new Vector2(0f, 0f), new Vector2(1f, 1f), new Vector2(22f, 22f), new Vector2(-22f, -38f));

            var monitorTitle = UIBuilder.CreateText("MonitorTitle", ordersMonitor.transform, "ORDERS", 16, new Color(0.42f, 0.90f, 1f), TextAnchor.MiddleCenter);
            SetRect(monitorTitle.rectTransform, new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(22f, 6f), new Vector2(-22f, 24f));

            var monitorLed = UIBuilder.CreateImage("MonitorLed", ordersMonitor.transform, new Color(0.30f, 0.98f, 0.88f, 1f));
            SetRect(monitorLed.rectTransform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(-5f, 8f), new Vector2(5f, 18f));

            var orderListText = UIBuilder.CreateText("OrderListText", monitorScreen.transform, string.Empty, 24, Color.white, TextAnchor.UpperLeft);
            UIBuilder.Stretch(orderListText.rectTransform);
            orderListText.rectTransform.offsetMin = new Vector2(16f, 14f);
            orderListText.rectTransform.offsetMax = new Vector2(-16f, -14f);

            var pauseButton = UIBuilder.CreateButton("PauseButton", root, "||", new Color(0.27f, 0.18f, 0.11f, 0.90f), Color.white);
            SetRect(pauseButton.GetComponent<RectTransform>(), new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-110f, -80f), new Vector2(-24f, -20f));

            root.gameObject.SetActive(false);
            return new HUDView(root, scoreText, timerText, stageText, earningsText, orderListText, progressFill, pauseButton);
        }

        public void SetVisible(bool isVisible)
        {
            root.gameObject.SetActive(isVisible);
        }

        public void SetScore(string text)
        {
            scoreText.text = text;
        }

        public void SetTimer(float secondsRemaining)
        {
            var minutes = Mathf.FloorToInt(secondsRemaining / 60f);
            var seconds = Mathf.FloorToInt(secondsRemaining % 60f);
            timerText.text = $"Time {minutes:00}:{seconds:00}";

            if (secondsRemaining <= 10f)
            {
                timerText.color = TimerCriticalColor;
            }
            else if (secondsRemaining <= 25f)
            {
                timerText.color = TimerWarningColor;
            }
            else
            {
                timerText.color = TimerDefaultColor;
            }
        }

        public void SetStage(string text)
        {
            stageText.text = text;
        }

        public void SetEarnings(string text)
        {
            earningsText.text = text;
        }

        public void SetOrderList(string text)
        {
            orderListText.text = text;
        }

        public void SetCurrentOrder(string text) { }

        public void SetInstruction(string text) { }

        public void SetStatus(string text) { }

        public void SetFeedback(string text, Color color) { }

        public void SetProgress(float normalized)
        {
            progressFill.fillAmount = Mathf.Clamp01(normalized);
        }

        public void ShowScorePopup(int score, QualityGrade grade)
        {
            var color = grade == QualityGrade.Bad ? ColorPalette.HighlightBad : ColorPalette.HighlightGood;
            ScorePopup.Spawn(root, score, color);
        }

        public void Dispose()
        {
            pauseButton.onClick.RemoveListener(HandlePauseClicked);
        }

        private void HandlePauseClicked()
        {
            PauseRequested?.Invoke();
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
