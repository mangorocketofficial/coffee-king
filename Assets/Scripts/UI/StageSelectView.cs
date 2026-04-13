using System;
using CoffeeKing.Util;
using UnityEngine;
using UnityEngine.UI;

namespace CoffeeKing.UI
{
    public sealed class StageSelectView
    {
        private const int StageCount = 5;

        private readonly RectTransform root;
        private readonly Button backButton;
        private readonly Button[] stageButtons;
        private readonly Text[] stageNumberTexts;
        private readonly Text[] stageStarTexts;
        private readonly Text[] stageScoreTexts;
        private readonly Image[] stageNodeImages;

        private bool isVisible;

        private static readonly Color NodeUnlockedColor = new Color(0.96f, 0.93f, 0.88f, 1f);
        private static readonly Color NodeLockedColor = new Color(0.60f, 0.58f, 0.55f, 0.70f);
        private static readonly Color NodeSelectedColor = new Color(0.29f, 0.67f, 0.45f, 1f);
        private static readonly Color TextDarkColor = new Color(0.20f, 0.14f, 0.10f);
        private static readonly Color TextLockedColor = new Color(0.50f, 0.48f, 0.45f);
        private static readonly Color StarColor = new Color(0.23f, 0.56f, 0.37f);
        private static readonly Color BackdropColor = new Color(0.14f, 0.09f, 0.06f, 1f);
        private static readonly Color AccentColor = new Color(0.27f, 0.18f, 0.11f, 1f);
        private static readonly Color BackButtonColor = new Color(0.52f, 0.38f, 0.30f);

        private StageSelectView(
            RectTransform root,
            Button backButton,
            Button[] stageButtons,
            Text[] stageNumberTexts,
            Text[] stageStarTexts,
            Text[] stageScoreTexts,
            Image[] stageNodeImages)
        {
            this.root = root;
            this.backButton = backButton;
            this.stageButtons = stageButtons;
            this.stageNumberTexts = stageNumberTexts;
            this.stageStarTexts = stageStarTexts;
            this.stageScoreTexts = stageScoreTexts;
            this.stageNodeImages = stageNodeImages;

            backButton.onClick.AddListener(HandleBackClicked);

            for (var i = 0; i < StageCount; i++)
            {
                var index = i;
                stageButtons[i].onClick.AddListener(() => HandleStageClicked(index));
            }

            root.gameObject.SetActive(false);
        }

        public event Action<int> StageSelected;
        public event Action BackRequested;

        public static StageSelectView Create(Transform parent)
        {
            var canvasRoot = new GameObject("StageSelectCanvas");
            canvasRoot.transform.SetParent(parent, false);

            var canvas = canvasRoot.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 600;

            var scaler = canvasRoot.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;
            canvasRoot.AddComponent<GraphicRaycaster>();

            var root = UIBuilder.CreateStretchRoot("StageSelect", canvasRoot.transform);

            var backdrop = UIBuilder.CreateImage("Backdrop", root, BackdropColor);
            UIBuilder.Stretch(backdrop.rectTransform);

            var titleText = UIBuilder.CreateText("Title", root, "Select Stage", 72, new Color(0.97f, 0.93f, 0.86f), TextAnchor.MiddleCenter);
            titleText.fontStyle = FontStyle.Bold;
            SetRect(titleText.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(-400f, -140f), new Vector2(400f, -40f));

            var accent = UIBuilder.CreateImage("Accent", root, AccentColor);
            SetRect(accent.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(-400f, -148f), new Vector2(400f, -140f));

            var stageButtons = new Button[StageCount];
            var stageNumberTexts = new Text[StageCount];
            var stageStarTexts = new Text[StageCount];
            var stageScoreTexts = new Text[StageCount];
            var stageNodeImages = new Image[StageCount];

            var nodeWidth = 200f;
            var nodeHeight = 240f;
            var nodeSpacing = 40f;
            var totalWidth = (nodeWidth * StageCount) + (nodeSpacing * (StageCount - 1));
            var startX = -totalWidth / 2f;

            for (var i = 0; i < StageCount; i++)
            {
                var nodeX = startX + (i * (nodeWidth + nodeSpacing)) + (nodeWidth / 2f);

                var nodeImage = UIBuilder.CreateImage($"StageNode{i + 1}", root, NodeUnlockedColor);
                SetRect(nodeImage.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                    new Vector2(nodeX - nodeWidth / 2f, -nodeHeight / 2f + 20f),
                    new Vector2(nodeX + nodeWidth / 2f, nodeHeight / 2f + 20f));
                stageNodeImages[i] = nodeImage;

                var button = nodeImage.gameObject.AddComponent<Button>();
                var buttonColors = button.colors;
                buttonColors.normalColor = Color.white;
                buttonColors.highlightedColor = new Color(1f, 1f, 1f, 0.9f);
                buttonColors.pressedColor = new Color(0.9f, 0.9f, 0.9f, 1f);
                buttonColors.selectedColor = Color.white;
                buttonColors.disabledColor = new Color(0.6f, 0.6f, 0.6f, 0.7f);
                button.colors = buttonColors;
                stageButtons[i] = button;

                var numberText = UIBuilder.CreateText($"Number{i + 1}", nodeImage.transform,
                    $"Stage {i + 1}", 38, TextDarkColor, TextAnchor.MiddleCenter);
                numberText.fontStyle = FontStyle.Bold;
                SetRect(numberText.rectTransform, new Vector2(0f, 1f), new Vector2(1f, 1f),
                    new Vector2(8f, -80f), new Vector2(-8f, -16f));
                stageNumberTexts[i] = numberText;

                var starText = UIBuilder.CreateText($"Stars{i + 1}", nodeImage.transform,
                    "---", 48, StarColor, TextAnchor.MiddleCenter);
                starText.fontStyle = FontStyle.Bold;
                SetRect(starText.rectTransform, new Vector2(0f, 0.5f), new Vector2(1f, 0.5f),
                    new Vector2(8f, -20f), new Vector2(-8f, 40f));
                stageStarTexts[i] = starText;

                var scoreText = UIBuilder.CreateText($"Score{i + 1}", nodeImage.transform,
                    string.Empty, 26, ColorPalette.SecondaryText, TextAnchor.MiddleCenter);
                SetRect(scoreText.rectTransform, new Vector2(0f, 0f), new Vector2(1f, 0f),
                    new Vector2(8f, 16f), new Vector2(-8f, 72f));
                stageScoreTexts[i] = scoreText;

                // Connector line between nodes (except after the last)
                if (i < StageCount - 1)
                {
                    var lineX = nodeX + nodeWidth / 2f;
                    var lineImage = UIBuilder.CreateImage($"Connector{i + 1}", root, new Color(0.50f, 0.45f, 0.40f, 0.6f));
                    SetRect(lineImage.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                        new Vector2(lineX, 18f), new Vector2(lineX + nodeSpacing, 22f));
                }
            }

            var backButton = UIBuilder.CreateButton("BackButton", root, "Back", BackButtonColor, Color.white);
            SetRect(backButton.GetComponent<RectTransform>(), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
                new Vector2(-120f, 40f), new Vector2(120f, 110f));

            return new StageSelectView(root, backButton, stageButtons, stageNumberTexts, stageStarTexts, stageScoreTexts, stageNodeImages);
        }

        public void Show()
        {
            isVisible = true;
            RefreshNodes();
            root.gameObject.SetActive(true);
        }

        public void Hide()
        {
            isVisible = false;
            root.gameObject.SetActive(false);
        }

        public void Dispose()
        {
            backButton.onClick.RemoveListener(HandleBackClicked);

            for (var i = 0; i < StageCount; i++)
            {
                stageButtons[i].onClick.RemoveAllListeners();
            }
        }

        private void RefreshNodes()
        {
            // Stage select is no longer used in the main flow (day-based system).
            // Nodes are shown as all unlocked for compatibility.
            for (var i = 0; i < StageCount; i++)
            {
                stageButtons[i].interactable = true;
                stageNodeImages[i].color = NodeUnlockedColor;
                stageNumberTexts[i].color = TextDarkColor;
                stageStarTexts[i].color = StarColor;
                stageScoreTexts[i].color = ColorPalette.SecondaryText;
                stageStarTexts[i].text = $"Day {i + 1}";
                stageScoreTexts[i].text = string.Empty;
            }
        }

        private void HandleStageClicked(int stageIndex)
        {
            if (isVisible)
            {
                StageSelected?.Invoke(stageIndex);
            }
        }

        private void HandleBackClicked()
        {
            if (isVisible)
            {
                BackRequested?.Invoke();
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
