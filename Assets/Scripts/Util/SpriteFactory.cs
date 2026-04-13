using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace CoffeeKing.Util
{
    public static class SpriteFactory
    {
        private const float PixelsPerUnit = 100f;
        private const string GrayboxRoot = "Sprites/Graybox";
        private const string FinalRoot = "Sprites/Final";

        public static bool UseGraybox = false;

        private static readonly Dictionary<string, Sprite> Cache = new Dictionary<string, Sprite>();

        public static void ClearCache()
        {
            Cache.Clear();
        }

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
                var fittedSprite = FitLoadedSprite(assetName, loadedSprite, fallbackWorldSize);
                Cache[cacheKey] = fittedSprite;
                return fittedSprite;
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

        public static Sprite CreateEllipse(string key, Vector2 worldSize, Color color)
        {
            return CreateEllipse(key, worldSize, color, new Vector2(0.5f, 0.5f));
        }

        public static Sprite CreateEllipse(string key, Vector2 worldSize, Color color, Vector2 pivot)
        {
            var width = Mathf.Max(4, Mathf.RoundToInt(worldSize.x * PixelsPerUnit));
            var height = Mathf.Max(4, Mathf.RoundToInt(worldSize.y * PixelsPerUnit));
            var cacheKey = $"ellipse_{key}_{width}x{height}_{ColorUtility.ToHtmlStringRGBA(color)}_{pivot.x:F2}_{pivot.y:F2}";

            if (Cache.TryGetValue(cacheKey, out var cachedSprite))
            {
                return cachedSprite;
            }

            var texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
            texture.name = cacheKey;
            texture.filterMode = FilterMode.Bilinear;
            texture.wrapMode = TextureWrapMode.Clamp;
            texture.hideFlags = HideFlags.DontSave;

            var pixels = new Color32[width * height];
            var fillColor = (Color32)color;
            var transparent = new Color32(0, 0, 0, 0);

            var radiusX = (width - 1) * 0.5f;
            var radiusY = (height - 1) * 0.5f;
            var centerX = radiusX;
            var centerY = radiusY;

            for (var y = 0; y < height; y++)
            {
                for (var x = 0; x < width; x++)
                {
                    var dx = (x - centerX) / Mathf.Max(radiusX, 1f);
                    var dy = (y - centerY) / Mathf.Max(radiusY, 1f);
                    var inside = (dx * dx) + (dy * dy) <= 1f;
                    pixels[(y * width) + x] = inside ? fillColor : transparent;
                }
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
                return TryLoadSpriteAtPath($"{GrayboxRoot}/{assetName}");
            }

            var finalSprite = TryLoadSpriteAtPath($"{FinalRoot}/{assetName}");
            if (finalSprite != null)
            {
                return finalSprite;
            }

            return TryLoadSpriteAtPath($"{GrayboxRoot}/{assetName}");
        }

        private static Sprite TryLoadSpriteAtPath(string path)
        {
            var sprite = Resources.Load<Sprite>(path);
            if (sprite != null)
            {
                return sprite;
            }

            var sprites = Resources.LoadAll<Sprite>(path);
            if (sprites != null && sprites.Length > 0)
            {
                return sprites[0];
            }

            var directory = Path.GetDirectoryName(path)?.Replace('\\', '/');
            var assetName = Path.GetFileName(path);
            if (!string.IsNullOrEmpty(directory))
            {
                var directorySprites = Resources.LoadAll<Sprite>(directory);
                for (var index = 0; index < directorySprites.Length; index++)
                {
                    var candidate = directorySprites[index];
                    if (candidate == null)
                    {
                        continue;
                    }

                    if (candidate.name == assetName || candidate.name.StartsWith(assetName + "_"))
                    {
                        return candidate;
                    }
                }
            }

            return null;
        }

        private static Sprite FitLoadedSprite(string assetName, Sprite sourceSprite, Vector2 targetWorldSize)
        {
            var width = Mathf.Max(targetWorldSize.x, 0.01f);
            var height = Mathf.Max(targetWorldSize.y, 0.01f);
            // Fit inside: use the larger ratio so the image fits within the target size while keeping aspect ratio
            var pixelsPerUnit = Mathf.Max(1f, Mathf.Max(sourceSprite.rect.width / width, sourceSprite.rect.height / height));

            var normalizedPivot = new Vector2(
                sourceSprite.pivot.x / sourceSprite.rect.width,
                sourceSprite.pivot.y / sourceSprite.rect.height);

            var fittedSprite = Sprite.Create(
                sourceSprite.texture,
                sourceSprite.rect,
                normalizedPivot,
                pixelsPerUnit);

            fittedSprite.name = $"fitted_{assetName}_{width:F2}_{height:F2}";
            fittedSprite.hideFlags = HideFlags.DontSave;
            return fittedSprite;
        }
    }
}
