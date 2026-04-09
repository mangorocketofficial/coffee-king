using CoffeeKing.Util;
using UnityEngine;
using UnityEngine.UI;

namespace CoffeeKing.UI
{
    public sealed class TutorialOverlay
    {
        private readonly RectTransform root;
        private readonly Text titleText;
        private readonly Text bodyText;
        private readonly Text arrowText;

        private TutorialOverlay(RectTransform root, Text titleText, Text bodyText, Text arrowText)
        {
            this.root = root;
            this.titleText = titleText;
            this.bodyText = bodyText;
            this.arrowText = arrowText;
        }

        public static TutorialOverlay Create(Transform parent)
        {
            var root = UIBuilder.CreateStretchRoot("TutorialOverlay", parent);

            var panel = UIBuilder.CreateImage("HintPanel", root, new Color(0.97f, 0.95f, 0.89f, 0.94f));
            SetRect(panel.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-260f, 250f), new Vector2(260f, 390f));

            var title = UIBuilder.CreateText("HintTitle", panel.transform, string.Empty, 34, ColorPalette.InstructionText, TextAnchor.UpperCenter);
            UIBuilder.Stretch(title.rectTransform);
            title.rectTransform.offsetMin = new Vector2(18f, 66f);
            title.rectTransform.offsetMax = new Vector2(-18f, -18f);

            var body = UIBuilder.CreateText("HintBody", panel.transform, string.Empty, 28, ColorPalette.SecondaryText, TextAnchor.MiddleCenter);
            UIBuilder.Stretch(body.rectTransform);
            body.rectTransform.offsetMin = new Vector2(18f, 18f);
            body.rectTransform.offsetMax = new Vector2(-18f, -42f);

            var arrow = UIBuilder.CreateText("Arrow", root, "->", 64, new Color(0.86f, 0.46f, 0.19f), TextAnchor.MiddleCenter);
            SetRect(arrow.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-40f, 160f), new Vector2(40f, 240f));

            root.gameObject.SetActive(false);
            return new TutorialOverlay(root, title, body, arrow);
        }

        public void Show(string title, string body, Vector2 panelOffset, Vector2 arrowOffset)
        {
            root.gameObject.SetActive(true);
            titleText.text = title;
            bodyText.text = body;

            var panelRect = titleText.transform.parent.GetComponent<RectTransform>();
            panelRect.anchoredPosition = panelOffset;
            arrowText.rectTransform.anchoredPosition = arrowOffset;
        }

        public void Hide()
        {
            root.gameObject.SetActive(false);
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
