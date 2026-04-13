using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace CoffeeKing.UI
{
    public sealed class ScorePopup : MonoBehaviour
    {
        private const float AnimDuration = 0.8f;
        private const float FloatDistance = 80f;

        public static void Spawn(Transform canvasParent, int score, Color color)
        {
            var go = new GameObject("ScorePopup", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
            go.transform.SetParent(canvasParent, false);

            var text = go.GetComponent<Text>();
            text.font = UIBuilder.GetFont();
            text.text = score >= 0 ? $"+{score}" : $"{score}";
            text.fontSize = 36;
            text.color = color;
            text.fontStyle = FontStyle.Bold;
            text.alignment = TextAnchor.MiddleCenter;
            text.horizontalOverflow = HorizontalWrapMode.Overflow;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            text.raycastTarget = false;

            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 1f);
            rect.anchorMax = new Vector2(0.5f, 1f);
            rect.anchoredPosition = new Vector2(0f, -200f);
            rect.sizeDelta = new Vector2(300f, 60f);

            var popup = go.AddComponent<ScorePopup>();
            popup.StartCoroutine(popup.Animate(rect, text));
        }

        private IEnumerator Animate(RectTransform rect, Text text)
        {
            var startPos = rect.anchoredPosition;
            var endPos = startPos + new Vector2(0f, FloatDistance);
            var startColor = text.color;
            var endColor = new Color(startColor.r, startColor.g, startColor.b, 0f);

            var elapsed = 0f;
            while (elapsed < AnimDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                var t = Mathf.Clamp01(elapsed / AnimDuration);

                // Ease out quad
                var eased = 1f - (1f - t) * (1f - t);

                rect.anchoredPosition = Vector2.Lerp(startPos, endPos, eased);
                text.color = Color.Lerp(startColor, endColor, t);
                yield return null;
            }

            Destroy(gameObject);
        }
    }
}
