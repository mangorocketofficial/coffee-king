using System;
using CoffeeKing.Util;
using UnityEngine;
using UnityEngine.UI;

namespace CoffeeKing.UI
{
    public sealed class TitleScreenView
    {
        private readonly RectTransform root;
        private readonly Button startButton;
        private readonly Text subtitleText;

        private TitleScreenView(RectTransform root, Button startButton, Text subtitleText)
        {
            this.root = root;
            this.startButton = startButton;
            this.subtitleText = subtitleText;
            this.startButton.onClick.AddListener(HandleStartClicked);
        }

        public event Action StartRequested;

        public static TitleScreenView Create(Transform parent)
        {
            var root = UIBuilder.CreateStretchRoot("TitleScreen", parent);

            var backdrop = UIBuilder.CreateImage("Backdrop", root, new Color(0.14f, 0.09f, 0.06f, 0.85f));
            UIBuilder.Stretch(backdrop.rectTransform);

            var title = UIBuilder.CreateText("Title", root, "Coffee King", 88, new Color(0.97f, 0.93f, 0.86f), TextAnchor.MiddleCenter);
            title.fontStyle = FontStyle.Bold;
            SetRect(title.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-480f, 120f), new Vector2(480f, 260f));

            var subtitle = UIBuilder.CreateText("Subtitle", root, "Graybox cafe rush", 36, ColorPalette.SecondaryText, TextAnchor.MiddleCenter);
            SetRect(subtitle.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-360f, 20f), new Vector2(360f, 90f));

            var startButton = UIBuilder.CreateButton("StartButton", root, "Start", new Color(0.30f, 0.67f, 0.45f), Color.white);
            SetRect(startButton.GetComponent<RectTransform>(), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-140f, -110f), new Vector2(140f, -20f));

            root.gameObject.SetActive(false);
            return new TitleScreenView(root, startButton, subtitle);
        }

        public void Show(string subtitle)
        {
            subtitleText.text = subtitle;
            root.gameObject.SetActive(true);
        }

        public void Hide()
        {
            root.gameObject.SetActive(false);
        }

        private void HandleStartClicked()
        {
            StartRequested?.Invoke();
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
