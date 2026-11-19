using System.Collections.Generic;
using UnityEngine;

namespace ExpeditionCamp
{
    public static class IconLoader
    {
        private const string ElementIconFolder = "Icons/Element/icon_element_";

        private static readonly Dictionary<string, Sprite> Cache = new Dictionary<string, Sprite>();
        private static Sprite _fallbackSprite;

        public static Sprite Load(string iconPath)
        {
            if (string.IsNullOrEmpty(iconPath))
            {
                return GetFallbackSprite();
            }

            Sprite cached;
            if (Cache.TryGetValue(iconPath, out cached))
            {
                return cached;
            }

            Sprite sprite = Resources.Load<Sprite>(iconPath);
            if (sprite == null)
            {
                Debug.LogWarning("[IconLoader] 아이콘을 불러오지 못했습니다: " + iconPath);
                sprite = GetFallbackSprite();
            }
            Cache[iconPath] = sprite;
            return sprite;
        }

        public static Sprite LoadElementIcon(string element)
        {
            if (string.IsNullOrEmpty(element))
            {
                return GetFallbackSprite();
            }
            return Load(ElementIconFolder + element.ToLowerInvariant());
        }

        public static Color GetGradeColor(string grade)
        {
            switch (grade)
            {
                case "N":
                    return new Color(0.62f, 0.64f, 0.68f, 1f);
                case "R":
                    return new Color(0.27f, 0.55f, 0.95f, 1f);
                case "SR":
                    return new Color(0.68f, 0.38f, 0.92f, 1f);
                case "SSR":
                    return new Color(0.98f, 0.78f, 0.20f, 1f);
                case "UR":
                    return new Color(0.93f, 0.26f, 0.26f, 1f);
                default:
                    return new Color(0.62f, 0.64f, 0.68f, 1f);
            }
        }

        public static Sprite GetFallbackSprite()
        {
            if (_fallbackSprite != null)
            {
                return _fallbackSprite;
            }

            Texture2D texture = new Texture2D(8, 8, TextureFormat.RGBA32, false);
            Color grey = new Color(0.5f, 0.5f, 0.5f, 1f);
            Color[] pixels = new Color[64];
            for (int index = 0; index < pixels.Length; index++)
            {
                pixels[index] = grey;
            }
            texture.SetPixels(pixels);
            texture.Apply();
            texture.name = "FallbackIcon";

            _fallbackSprite = Sprite.Create(texture, new Rect(0f, 0f, 8f, 8f), new Vector2(0.5f, 0.5f));
            _fallbackSprite.name = "FallbackIcon";
            return _fallbackSprite;
        }
    }
}
