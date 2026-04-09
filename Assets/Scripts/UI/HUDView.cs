using UnityEngine;
using UnityEngine.UI;

namespace CoffeeKing.UI
{
    public sealed class HUDView
    {
        private readonly RectTransform root;
        private readonly Text scoreText;
        private readonly Text timerText;
        private readonly Text stageText;
        private readonly Text orderListText;
        private readonly Image progressFill;

        private HUDView(RectTransform root, Text scoreText, Text timerText, Text stageText, Text orderListText, Image progressFill)
        {
            this.root = root;
            this.scoreText = scoreText;
            this.timerText = timerText;
            this.stageText = stageText;
            this.orderListText = orderListText;
            this.progressFill = progressFill;
        }

        public static HUDView Create(Transform parent)
        {
            var root = UIBuilder.CreateStretchRoot("HUD", parent);

            var scoreText = UIBuilder.CreateText("ScoreText", root, string.Empty, 34, new Color(0.18f, 0.13f, 0.10f), TextAnchor.UpperLeft);
            SetRect(scoreText.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(28f, -24f), new Vector2(420f, 120f));

            var timerText = UIBuilder.CreateText("TimerText", root, string.Empty, 34, new Color(0.18f, 0.13f, 0.10f), TextAnchor.UpperRight);
            SetRect(timerText.rectTransform, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-430f, -24f), new Vector2(-28f, 120f));

            var progressFrame = UIBuilder.CreateImage("ProgressFrame", root, new Color(0.19f, 0.14f, 0.10f, 0.92f));
            SetRect(progressFrame.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(-260f, -28f), new Vector2(260f, 18f));

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

            var stageText = UIBuilder.CreateText("StageText", root, string.Empty, 28, new Color(0.18f, 0.13f, 0.10f), TextAnchor.MiddleCenter);
            SetRect(stageText.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(-320f, -78f), new Vector2(320f, -24f));

            var ordersPanel = UIBuilder.CreateImage("OrdersPanel", root, new Color(0.94f, 0.91f, 0.84f, 0.92f));
            SetRect(ordersPanel.rectTransform, new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(24f, 24f), new Vector2(420f, 250f));

            var orderListText = UIBuilder.CreateText("OrderListText", ordersPanel.transform, string.Empty, 28, new Color(0.18f, 0.13f, 0.10f), TextAnchor.UpperLeft);
            UIBuilder.Stretch(orderListText.rectTransform);
            orderListText.rectTransform.offsetMin = new Vector2(18f, 18f);
            orderListText.rectTransform.offsetMax = new Vector2(-18f, -18f);

            root.gameObject.SetActive(false);
            return new HUDView(root, scoreText, timerText, stageText, orderListText, progressFill);
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
        }

        public void SetStage(string text)
        {
            stageText.text = text;
        }

        public void SetOrderList(string text)
        {
            orderListText.text = text;
        }

        public void SetProgress(float normalized)
        {
            progressFill.fillAmount = Mathf.Clamp01(normalized);
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
