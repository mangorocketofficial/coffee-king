using System;
using CoffeeKing.Util;
using UnityEngine;
using UnityEngine.UI;

namespace CoffeeKing.UI
{
    public sealed class SettingsView
    {
        private readonly RectTransform root;
        private readonly Button closeButton;
        private readonly Slider bgmSlider;
        private readonly Slider sfxSlider;
        private readonly Toggle vibrationToggle;
        private readonly Text bgmValueText;
        private readonly Text sfxValueText;

        private bool isVisible;

        private static readonly Color PanelColor = new Color(0.96f, 0.93f, 0.88f, 0.98f);
        private static readonly Color AccentColor = new Color(0.27f, 0.18f, 0.11f, 1f);
        private static readonly Color TitleColor = new Color(0.20f, 0.14f, 0.10f);
        private static readonly Color LabelColor = new Color(0.25f, 0.18f, 0.13f);
        private static readonly Color ValueColor = new Color(0.40f, 0.32f, 0.25f);
        private static readonly Color SliderBgColor = new Color(0.80f, 0.76f, 0.70f);
        private static readonly Color SliderFillColor = new Color(0.29f, 0.67f, 0.45f);
        private static readonly Color SliderHandleColor = new Color(0.96f, 0.93f, 0.88f);
        private static readonly Color ToggleOnColor = new Color(0.29f, 0.67f, 0.45f);
        private static readonly Color ToggleOffColor = new Color(0.65f, 0.60f, 0.55f);
        private static readonly Color CloseButtonColor = new Color(0.52f, 0.38f, 0.30f);
        private static readonly Color BackdropColor = new Color(0.11f, 0.08f, 0.06f, 0.78f);

        private SettingsView(
            RectTransform root,
            Button closeButton,
            Slider bgmSlider,
            Slider sfxSlider,
            Toggle vibrationToggle,
            Text bgmValueText,
            Text sfxValueText)
        {
            this.root = root;
            this.closeButton = closeButton;
            this.bgmSlider = bgmSlider;
            this.sfxSlider = sfxSlider;
            this.vibrationToggle = vibrationToggle;
            this.bgmValueText = bgmValueText;
            this.sfxValueText = sfxValueText;

            closeButton.onClick.AddListener(HandleCloseClicked);
            bgmSlider.onValueChanged.AddListener(HandleBgmChanged);
            sfxSlider.onValueChanged.AddListener(HandleSfxChanged);
            vibrationToggle.onValueChanged.AddListener(HandleVibrationChanged);

            root.gameObject.SetActive(false);
        }

        public event Action CloseRequested;
        public event Action<int> BgmVolumeChanged;
        public event Action<int> SfxVolumeChanged;
        public event Action<bool> VibrationChanged;

        public static SettingsView Create(Transform parent)
        {
            var canvasRoot = new GameObject("SettingsCanvas");
            canvasRoot.transform.SetParent(parent, false);

            var canvas = canvasRoot.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 900;

            var scaler = canvasRoot.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;
            canvasRoot.AddComponent<GraphicRaycaster>();

            var root = UIBuilder.CreateStretchRoot("SettingsOverlay", canvasRoot.transform);

            var backdrop = UIBuilder.CreateImage("Backdrop", root, BackdropColor);
            UIBuilder.Stretch(backdrop.rectTransform);

            var panel = UIBuilder.CreateImage("Panel", root, PanelColor);
            SetRect(panel.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-340f, -300f), new Vector2(340f, 300f));

            var accent = UIBuilder.CreateImage("Accent", panel.transform, AccentColor);
            SetRect(accent.rectTransform, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0f, -18f), new Vector2(0f, 0f));

            var titleText = UIBuilder.CreateText("TitleText", panel.transform, "SETTINGS", 64, TitleColor, TextAnchor.MiddleCenter);
            titleText.fontStyle = FontStyle.Bold;
            SetRect(titleText.rectTransform, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(32f, -120f), new Vector2(-32f, -26f));

            // BGM Volume
            var bgmLabel = UIBuilder.CreateText("BgmLabel", panel.transform, "BGM Volume", 34, LabelColor, TextAnchor.MiddleLeft);
            SetRect(bgmLabel.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-260f, 110f), new Vector2(60f, 160f));

            var bgmValueText = UIBuilder.CreateText("BgmValue", panel.transform, "50", 34, ValueColor, TextAnchor.MiddleRight);
            SetRect(bgmValueText.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(160f, 110f), new Vector2(260f, 160f));

            var bgmSlider = CreateSlider("BgmSlider", panel.transform, new Vector2(-260f, 60f), new Vector2(260f, 100f));

            // SFX Volume
            var sfxLabel = UIBuilder.CreateText("SfxLabel", panel.transform, "SFX Volume", 34, LabelColor, TextAnchor.MiddleLeft);
            SetRect(sfxLabel.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-260f, -10f), new Vector2(60f, 40f));

            var sfxValueText = UIBuilder.CreateText("SfxValue", panel.transform, "50", 34, ValueColor, TextAnchor.MiddleRight);
            SetRect(sfxValueText.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(160f, -10f), new Vector2(260f, 40f));

            var sfxSlider = CreateSlider("SfxSlider", panel.transform, new Vector2(-260f, -60f), new Vector2(260f, -20f));

            // Vibration Toggle
            var vibLabel = UIBuilder.CreateText("VibLabel", panel.transform, "Vibration", 34, LabelColor, TextAnchor.MiddleLeft);
            SetRect(vibLabel.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-260f, -120f), new Vector2(60f, -70f));

            var vibrationToggle = CreateToggle("VibrationToggle", panel.transform, new Vector2(160f, -120f), new Vector2(260f, -70f));

            // Close button
            var closeButton = UIBuilder.CreateButton("CloseButton", panel.transform, "Close", CloseButtonColor, Color.white);
            SetRect(closeButton.GetComponent<RectTransform>(), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-140f, -260f), new Vector2(140f, -190f));

            return new SettingsView(root, closeButton, bgmSlider, sfxSlider, vibrationToggle, bgmValueText, sfxValueText);
        }

        public void Show(int bgmVolume, int sfxVolume, bool vibrationEnabled)
        {
            bgmSlider.SetValueWithoutNotify(bgmVolume);
            sfxSlider.SetValueWithoutNotify(sfxVolume);
            vibrationToggle.SetIsOnWithoutNotify(vibrationEnabled);
            bgmValueText.text = bgmVolume.ToString();
            sfxValueText.text = sfxVolume.ToString();
            UpdateToggleVisual(vibrationToggle, vibrationEnabled);
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
            closeButton.onClick.RemoveListener(HandleCloseClicked);
            bgmSlider.onValueChanged.RemoveListener(HandleBgmChanged);
            sfxSlider.onValueChanged.RemoveListener(HandleSfxChanged);
            vibrationToggle.onValueChanged.RemoveListener(HandleVibrationChanged);
        }

        private void HandleCloseClicked()
        {
            if (isVisible)
            {
                CloseRequested?.Invoke();
            }
        }

        private void HandleBgmChanged(float value)
        {
            var intValue = Mathf.RoundToInt(value);
            bgmValueText.text = intValue.ToString();
            BgmVolumeChanged?.Invoke(intValue);
        }

        private void HandleSfxChanged(float value)
        {
            var intValue = Mathf.RoundToInt(value);
            sfxValueText.text = intValue.ToString();
            SfxVolumeChanged?.Invoke(intValue);
        }

        private void HandleVibrationChanged(bool isOn)
        {
            UpdateToggleVisual(vibrationToggle, isOn);
            VibrationChanged?.Invoke(isOn);
        }

        private static void UpdateToggleVisual(Toggle toggle, bool isOn)
        {
            var bgImage = toggle.GetComponent<Image>();
            if (bgImage != null)
            {
                bgImage.color = isOn ? ToggleOnColor : ToggleOffColor;
            }
        }

        private static Slider CreateSlider(string name, Transform parent, Vector2 offsetMin, Vector2 offsetMax)
        {
            var sliderGo = new GameObject(name, typeof(RectTransform));
            sliderGo.transform.SetParent(parent, false);
            var sliderRect = sliderGo.GetComponent<RectTransform>();
            SetRect(sliderRect, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), offsetMin, offsetMax);

            // Background
            var bgImage = UIBuilder.CreateImage("Background", sliderGo.transform, SliderBgColor);
            UIBuilder.Stretch(bgImage.rectTransform);

            // Fill area
            var fillArea = new GameObject("Fill Area", typeof(RectTransform));
            fillArea.transform.SetParent(sliderGo.transform, false);
            var fillAreaRect = fillArea.GetComponent<RectTransform>();
            fillAreaRect.anchorMin = Vector2.zero;
            fillAreaRect.anchorMax = Vector2.one;
            fillAreaRect.offsetMin = new Vector2(5f, 5f);
            fillAreaRect.offsetMax = new Vector2(-5f, -5f);

            var fill = UIBuilder.CreateImage("Fill", fillArea.transform, SliderFillColor);
            fill.rectTransform.anchorMin = Vector2.zero;
            fill.rectTransform.anchorMax = Vector2.one;
            fill.rectTransform.offsetMin = Vector2.zero;
            fill.rectTransform.offsetMax = Vector2.zero;

            // Handle slide area
            var handleSlideArea = new GameObject("Handle Slide Area", typeof(RectTransform));
            handleSlideArea.transform.SetParent(sliderGo.transform, false);
            var handleSlideRect = handleSlideArea.GetComponent<RectTransform>();
            handleSlideRect.anchorMin = Vector2.zero;
            handleSlideRect.anchorMax = Vector2.one;
            handleSlideRect.offsetMin = new Vector2(10f, 0f);
            handleSlideRect.offsetMax = new Vector2(-10f, 0f);

            var handle = UIBuilder.CreateImage("Handle", handleSlideArea.transform, SliderHandleColor);
            handle.rectTransform.sizeDelta = new Vector2(30f, 0f);
            handle.rectTransform.anchorMin = new Vector2(0f, 0f);
            handle.rectTransform.anchorMax = new Vector2(0f, 1f);

            var slider = sliderGo.AddComponent<Slider>();
            slider.fillRect = fill.rectTransform;
            slider.handleRect = handle.rectTransform;
            slider.targetGraphic = handle;
            slider.minValue = 0f;
            slider.maxValue = 100f;
            slider.wholeNumbers = true;
            slider.value = 50f;

            return slider;
        }

        private static Toggle CreateToggle(string name, Transform parent, Vector2 offsetMin, Vector2 offsetMax)
        {
            var toggleGo = new GameObject(name, typeof(RectTransform));
            toggleGo.transform.SetParent(parent, false);
            var toggleRect = toggleGo.GetComponent<RectTransform>();
            SetRect(toggleRect, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), offsetMin, offsetMax);

            var bgImage = toggleGo.AddComponent<Image>();
            bgImage.color = ToggleOnColor;

            var checkmark = UIBuilder.CreateText("Label", toggleGo.transform, "ON", 28, Color.white, TextAnchor.MiddleCenter);
            UIBuilder.Stretch(checkmark.rectTransform);

            var toggle = toggleGo.AddComponent<Toggle>();
            toggle.targetGraphic = bgImage;
            toggle.isOn = true;

            toggle.onValueChanged.AddListener(isOn =>
            {
                checkmark.text = isOn ? "ON" : "OFF";
            });

            return toggle;
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
