using System;
using CoffeeKing.Util;
using UnityEngine;
using UnityEngine.UI;

namespace CoffeeKing.UI
{
    public sealed class PauseOverlayView
    {
        private readonly RectTransform root;
        private readonly Button resumeButton;
        private readonly Button restartButton;
        private readonly Button mainMenuButton;
        private readonly Button settingsButton;

        private bool isVisible;

        private PauseOverlayView(
            RectTransform root,
            Button resumeButton,
            Button restartButton,
            Button mainMenuButton,
            Button settingsButton)
        {
            this.root = root;
            this.resumeButton = resumeButton;
            this.restartButton = restartButton;
            this.mainMenuButton = mainMenuButton;
            this.settingsButton = settingsButton;

            resumeButton.onClick.AddListener(HandleResumeClicked);
            restartButton.onClick.AddListener(HandleRestartClicked);
            mainMenuButton.onClick.AddListener(HandleMainMenuClicked);
            settingsButton.onClick.AddListener(HandleSettingsClicked);
            root.gameObject.SetActive(false);
        }

        public event Action ResumeRequested;
        public event Action RestartRequested;
        public event Action MainMenuRequested;
        public event Action SettingsRequested;

        public static PauseOverlayView Create(Transform parent)
        {
            var canvasRoot = new GameObject("PauseOverlayCanvas");
            canvasRoot.transform.SetParent(parent, false);

            var canvas = canvasRoot.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 800;

            var scaler = canvasRoot.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;
            canvasRoot.AddComponent<GraphicRaycaster>();

            var root = UIBuilder.CreateStretchRoot("PauseOverlay", canvasRoot.transform);

            var backdrop = UIBuilder.CreateImage("Backdrop", root, new Color(0.11f, 0.08f, 0.06f, 0.78f));
            UIBuilder.Stretch(backdrop.rectTransform);

            var panel = UIBuilder.CreateImage("Panel", root, new Color(0.96f, 0.93f, 0.88f, 0.98f));
            SetRect(panel.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-300f, -290f), new Vector2(300f, 240f));

            var accent = UIBuilder.CreateImage("Accent", panel.transform, new Color(0.27f, 0.18f, 0.11f, 1f));
            SetRect(accent.rectTransform, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0f, -18f), new Vector2(0f, 0f));

            var titleText = UIBuilder.CreateText("TitleText", panel.transform, "PAUSED", 64, new Color(0.20f, 0.14f, 0.10f), TextAnchor.MiddleCenter);
            titleText.fontStyle = FontStyle.Bold;
            SetRect(titleText.rectTransform, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(32f, -120f), new Vector2(-32f, -26f));

            var resumeButton = UIBuilder.CreateButton("ResumeButton", panel.transform, "Resume", new Color(0.29f, 0.67f, 0.45f), Color.white);
            SetRect(resumeButton.GetComponent<RectTransform>(), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-200f, 50f), new Vector2(200f, 120f));

            var restartButton = UIBuilder.CreateButton("RestartButton", panel.transform, "Restart", new Color(0.74f, 0.48f, 0.20f), Color.white);
            SetRect(restartButton.GetComponent<RectTransform>(), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-200f, -40f), new Vector2(200f, 30f));

            var settingsButton = UIBuilder.CreateButton("SettingsButton", panel.transform, "Settings", new Color(0.55f, 0.42f, 0.32f), Color.white);
            SetRect(settingsButton.GetComponent<RectTransform>(), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-200f, -130f), new Vector2(200f, -60f));

            var mainMenuButton = UIBuilder.CreateButton("MainMenuButton", panel.transform, "Main Menu", new Color(0.52f, 0.38f, 0.30f), Color.white);
            SetRect(mainMenuButton.GetComponent<RectTransform>(), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-200f, -220f), new Vector2(200f, -150f));

            return new PauseOverlayView(root, resumeButton, restartButton, mainMenuButton, settingsButton);
        }

        public void Show()
        {
            isVisible = true;
            root.gameObject.SetActive(true);
        }

        public void Hide()
        {
            isVisible = false;
            root.gameObject.SetActive(false);
        }

        public void Dispose()
        {
            resumeButton.onClick.RemoveListener(HandleResumeClicked);
            restartButton.onClick.RemoveListener(HandleRestartClicked);
            mainMenuButton.onClick.RemoveListener(HandleMainMenuClicked);
            settingsButton.onClick.RemoveListener(HandleSettingsClicked);
        }

        private void HandleResumeClicked()
        {
            if (isVisible)
            {
                ResumeRequested?.Invoke();
            }
        }

        private void HandleRestartClicked()
        {
            if (isVisible)
            {
                RestartRequested?.Invoke();
            }
        }

        private void HandleMainMenuClicked()
        {
            if (isVisible)
            {
                MainMenuRequested?.Invoke();
            }
        }

        private void HandleSettingsClicked()
        {
            if (isVisible)
            {
                SettingsRequested?.Invoke();
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
