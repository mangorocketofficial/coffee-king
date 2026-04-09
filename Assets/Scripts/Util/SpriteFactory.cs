using System.Collections.Generic;
using UnityEngine;

namespace CoffeeKing.Util
{
    public static class SpriteFactory
    {
        private const float PixelsPerUnit = 100f;
        private const string GrayboxRoot = "Sprites/Graybox";
        private const string FinalRoot = "Sprites/Final";

        public static bool UseGraybox = true;

        private static readonly Dictionary<string, Sprite> Cache = new Dictionary<string, Sprite>();

        public static Sprite Load(string assetName, Vector2 fallbackWorldSize, Color fallbackColor)
        {
            return Load(assetName, fallbackWorldSize, fallbackColor, new Vector2(0.5f, 0.5f));
        }

        public static Sprite Load(string assetName, Vector2 fallbackWorldSize, Color fallbackColor, Vector2 pivot)
        {
            var cacheKey = $"asset_{assetName}_{UseGraybox}_{fallbackWorldSize.x:F2}_{fallbackWorldSize.y:F2}_{ColorUtility.ToHtmlStringRGBA(fallbackColor)}_{pivot.x:F2}_{pivot.y:F2}";
            if (Cache.TryGetValue(cacheKey, out var cachedSprite))
            {
                return cachedSprite;
            }

            var loadedSprite = TryLoadFromResources(assetName);
            if (loadedSprite != null)
            {
                Cache[cacheKey] = loadedSprite;
                return loadedSprite;
            }

            var fallback = CreateRect($"fallback_{assetName}", fallbackWorldSize, fallbackColor, pivot);
            Cache[cacheKey] = fallback;
            return fallback;
        }

        public static Sprite CreateRect(string key, Vector2 worldSize, Color color)
        {
            return CreateRect(key, worldSize, color, new Vector2(0.5f, 0.5f));
        }

        public static Sprite CreateRect(string key, Vector2 worldSize, Color color, Vector2 pivot)
        {
            var width = Mathf.Max(2, Mathf.RoundToInt(worldSize.x * PixelsPerUnit));
            var height = Mathf.Max(2, Mathf.RoundToInt(worldSize.y * PixelsPerUnit));
            var cacheKey = $"rect_{key}_{width}x{height}_{ColorUtility.ToHtmlStringRGBA(color)}_{pivot.x:F2}_{pivot.y:F2}";

            if (Cache.TryGetValue(cacheKey, out var cachedSprite))
            {
                return cachedSprite;
            }

            var texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
            texture.name = cacheKey;
            texture.filterMode = FilterMode.Point;
            texture.wrapMode = TextureWrapMode.Clamp;
            texture.hideFlags = HideFlags.DontSave;

            var pixels = new Color32[width * height];
            var fillColor = (Color32)color;
            for (var index = 0; index < pixels.Length; index++)
            {
                pixels[index] = fillColor;
            }

            texture.SetPixels32(pixels);
            texture.Apply();

            var sprite = Sprite.Create(texture, new Rect(0f, 0f, width, height), pivot, PixelsPerUnit);
            sprite.name = cacheKey;
            sprite.hideFlags = HideFlags.DontSave;

            Cache[cacheKey] = sprite;
            return sprite;
        }

        private static Sprite TryLoadFromResources(string assetName)
        {
            if (UseGraybox)
            {
                return Resources.Load<Sprite>($"{GrayboxRoot}/{assetName}");
            }

            var finalSprite = Resources.Load<Sprite>($"{FinalRoot}/{assetName}");
            if (finalSprite != null)
            {
                return finalSprite;
            }

            return Resources.Load<Sprite>($"{GrayboxRoot}/{assetName}");
        }
    }
}
