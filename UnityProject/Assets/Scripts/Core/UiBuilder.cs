using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ExpeditionCamp
{
    public static class UiBuilder
    {
        public static readonly Color PanelColor = new Color(0.16f, 0.18f, 0.24f, 1f);
        public static readonly Color TextColor = new Color(0.94f, 0.95f, 0.98f, 1f);
        public static readonly Color FrameColor = new Color(0.10f, 0.11f, 0.16f, 0.82f);

        public const string ButtonSpritePath = "Kenney/button_blue_rectangle_depth_gloss";
        public const string PanelSpritePath = "Kenney/panel_grey_square_depth_flat";
        public const string FrameSpritePath = "Kenney/panel_grey_square_border";

        public static bool ApplySlicedSprite(Image image, string spritePath)
        {
            Sprite sprite = Resources.Load<Sprite>(spritePath);
            if (sprite == null)
            {
                return false;
            }
            image.sprite = sprite;
            image.type = Image.Type.Sliced;
            return true;
        }

        public static Image CreateIcon(string name, Transform parent, Sprite sprite)
        {
            GameObject iconObject = new GameObject(name, typeof(RectTransform));
            iconObject.transform.SetParent(parent, false);
            Image image = iconObject.AddComponent<Image>();
            image.sprite = sprite;
            image.preserveAspect = true;
            image.raycastTarget = false;
            return image;
        }

        public static void PlaceLeftMiddle(RectTransform rect, float size, float left)
        {
            rect.anchorMin = new Vector2(0f, 0.5f);
            rect.anchorMax = new Vector2(0f, 0.5f);
            rect.pivot = new Vector2(0f, 0.5f);
            rect.sizeDelta = new Vector2(size, size);
            rect.anchoredPosition = new Vector2(left, 0f);
        }

        public static GameObject CreatePanel(string name, Transform parent, Vector2 size, Color color)
        {
            GameObject panel = new GameObject(name, typeof(RectTransform));
            panel.transform.SetParent(parent, false);
            RectTransform rect = panel.GetComponent<RectTransform>();
            rect.sizeDelta = size;
            Image image = panel.AddComponent<Image>();
            image.color = color;
            return panel;
        }

        public static TMP_Text CreateText(string name, Transform parent, string content,
            float fontSize, TMP_FontAsset font, TextAlignmentOptions alignment)
        {
            GameObject textObject = new GameObject(name, typeof(RectTransform));
            textObject.transform.SetParent(parent, false);
            TextMeshProUGUI text = textObject.AddComponent<TextMeshProUGUI>();
            text.text = content;
            text.fontSize = fontSize;
            text.color = TextColor;
            text.alignment = alignment;
            text.enableWordWrapping = true;
            text.raycastTarget = false;
            if (font != null)
            {
                text.font = font;
            }
            return text;
        }

        public static Button CreateButton(string name, string labelName, Transform parent,
            string content, Vector2 size, TMP_FontAsset font)
        {
            GameObject buttonObject = new GameObject(name, typeof(RectTransform));
            buttonObject.transform.SetParent(parent, false);
            RectTransform rect = buttonObject.GetComponent<RectTransform>();
            rect.sizeDelta = size;

            Image image = buttonObject.AddComponent<Image>();
            bool skinned = ApplySlicedSprite(image, ButtonSpritePath);
            if (skinned)
            {
                image.color = Color.white;
            }
            else
            {
                image.color = new Color(0.25f, 0.38f, 0.62f, 1f);
            }

            Button button = buttonObject.AddComponent<Button>();
            button.targetGraphic = image;

            TMP_Text label = CreateText(labelName, buttonObject.transform, content, 28f, font,
                TextAlignmentOptions.Center);
            RectTransform labelRect = label.rectTransform;
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.offsetMin = new Vector2(0f, 6f);
            labelRect.offsetMax = Vector2.zero;

            return button;
        }

        public static void Stretch(RectTransform rect, Vector2 anchorMin, Vector2 anchorMax,
            Vector2 offsetMin, Vector2 offsetMax)
        {
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.offsetMin = offsetMin;
            rect.offsetMax = offsetMax;
        }
    }
}
