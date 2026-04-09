using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace CoffeeKing.UI
{
    public sealed class UIContext
    {
        public UIContext(Canvas canvas, HUDView hudView, TitleScreenView titleScreenView, TutorialOverlay tutorialOverlay)
        {
            Canvas = canvas;
            HUDView = hudView;
            TitleScreenView = titleScreenView;
            TutorialOverlay = tutorialOverlay;
        }

        public Canvas Canvas { get; }
        public HUDView HUDView { get; }
        public TitleScreenView TitleScreenView { get; }
        public TutorialOverlay TutorialOverlay { get; }
    }

    public static class UIBuilder
    {
        private static Font cachedFont;

        public static UIContext Build(Transform parent)
        {
            EnsureEventSystem();

            var canvasObject = new GameObject("UICanvas");
            canvasObject.transform.SetParent(parent, false);

            var canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 500;

            canvasObject.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasObject.GetComponent<CanvasScaler>().referenceResolution = new Vector2(1920f, 1080f);
            canvasObject.GetComponent<CanvasScaler>().matchWidthOrHeight = 0.5f;
            canvasObject.AddComponent<GraphicRaycaster>();

            var hudView = HUDView.Create(canvasObject.transform);
            var titleScreenView = TitleScreenView.Create(canvasObject.transform);
            var tutorialOverlay = TutorialOverlay.Create(canvasObject.transform);

            return new UIContext(canvas, hudView, titleScreenView, tutorialOverlay);
        }

        public static Font GetFont()
        {
            if (cachedFont == null)
            {
                cachedFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            }

            return cachedFont;
        }

        public static RectTransform CreateStretchRoot(string name, Transform parent)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            var rectTransform = go.GetComponent<RectTransform>();
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;
            return rectTransform;
        }

        public static Image CreateImage(string name, Transform parent, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            go.transform.SetParent(parent, false);
            var image = go.GetComponent<Image>();
            image.color = color;
            return image;
        }

        public static Text CreateText(string name, Transform parent, string content, int fontSize, Color color, TextAnchor alignment)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
            go.transform.SetParent(parent, false);
            var text = go.GetComponent<Text>();
            text.font = GetFont();
            text.text = content;
            text.fontSize = fontSize;
            text.color = color;
            text.alignment = alignment;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            return text;
        }

        public static Button CreateButton(string name, Transform parent, string label, Color backgroundColor, Color textColor)
        {
            var image = CreateImage(name, parent, backgroundColor);
            var button = image.gameObject.AddComponent<Button>();
            var buttonColors = button.colors;
            buttonColors.normalColor = backgroundColor;
            buttonColors.highlightedColor = backgroundColor * 1.1f;
            buttonColors.pressedColor = backgroundColor * 0.9f;
            buttonColors.selectedColor = backgroundColor;
            buttonColors.disabledColor = backgroundColor * 0.6f;
            button.colors = buttonColors;

            var text = CreateText("Label", image.transform, label, 42, textColor, TextAnchor.MiddleCenter);
            Stretch(text.rectTransform);

            return button;
        }

        public static void Stretch(RectTransform rectTransform)
        {
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;
        }

        private static void EnsureEventSystem()
        {
            var existingEventSystem = Object.FindAnyObjectByType<EventSystem>();
            if (existingEventSystem != null)
            {
                var existingStandalone = existingEventSystem.GetComponent<StandaloneInputModule>();
                if (existingStandalone != null)
                {
                    Object.Destroy(existingStandalone);
                }

                if (existingEventSystem.GetComponent<InputSystemUIInputModule>() == null)
                {
                    existingEventSystem.gameObject.AddComponent<InputSystemUIInputModule>();
                }

                return;
            }

            var eventSystemObject = new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
            Object.DontDestroyOnLoad(eventSystemObject);
        }
    }
}
