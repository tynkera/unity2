using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace ExpeditionCamp.EditorTools
{
    public static class UiTheme
    {
        public static readonly Color Ink = new Color(0.065f, 0.14f, 0.14f, 1f);
        public static readonly Color Panel = new Color(0.105f, 0.205f, 0.20f, 1f);
        public static readonly Color Cream = new Color(0.96f, 0.92f, 0.80f, 1f);
        public static readonly Color Gold = new Color(0.79f, 0.62f, 0.32f, 1f);
        public static readonly Color Muted = new Color(0.64f, 0.75f, 0.69f, 1f);
        public static readonly Color Dim = new Color(0.015f, 0.035f, 0.035f, 0.88f);
        public static readonly Color ButtonHover = new Color(0.19f, 0.34f, 0.31f, 1f);
        public static readonly Color ButtonDisabled = new Color(0.12f, 0.17f, 0.17f, 0.55f);

        private const string KenneyFolder = "Assets/ThirdParty/Kenney/";
        private const string ResultPrefab = "UI_Popup_Result.prefab";

        public static void ApplyToUiPrefabs()
        {
            foreach (string guid in AssetDatabase.FindAssets("t:Prefab", new[] { PrefabBuilder.UiFolder }))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (path.EndsWith(ResultPrefab))
                {
                    continue;
                }
                GameObject root = PrefabUtility.LoadPrefabContents(path);
                Apply(root);
                PrefabUtility.SaveAsPrefabAsset(root, path);
                PrefabUtility.UnloadPrefabContents(root);
            }
        }

        public static void ApplyToScene(Scene scene)
        {
            foreach (GameObject root in scene.GetRootGameObjects())
            {
                Apply(root);
            }
        }

        public static void Apply(GameObject root)
        {
            foreach (Button button in root.GetComponentsInChildren<Button>(true))
            {
                if (IsOwnObject(button) && IsKenney(button.targetGraphic as Image))
                {
                    SkinButton(button);
                }
            }

            foreach (Image image in root.GetComponentsInChildren<Image>(true))
            {
                if (IsOwnObject(image))
                {
                    StyleImage(image);
                }
            }

            foreach (TextMeshProUGUI text in root.GetComponentsInChildren<TextMeshProUGUI>(true))
            {
                if (IsOwnObject(text) && SameColor(text.color, EditorUtil.TextColor))
                {
                    text.color = Cream;
                }
            }
        }

        private static void StyleImage(Image image)
        {
            string name = image.name;
            if (name.StartsWith("Popup_"))
            {
                image.color = Dim;
                return;
            }
            if (name == "AutoBattleCheck")
            {
                image.color = Gold;
                return;
            }
            bool surface = name == "Panel_Hud" || name == "Tgl_AutoBattle" || name == "Inner"
                || name == "UI_ItemSlot" || name == "UI_ShopSlot";
            if (!surface && !IsKenney(image))
            {
                return;
            }
            Flatten(image);
            if (name != "Panel_DataError")
            {
                image.color = Ink;
            }
        }

        private static void SkinButton(Button button)
        {
            Image image = (Image)button.targetGraphic;
            Flatten(image);
            image.color = Color.white;
            ColorBlock colors = button.colors;
            colors.normalColor = Panel;
            colors.highlightedColor = ButtonHover;
            colors.selectedColor = ButtonHover;
            colors.pressedColor = Gold;
            colors.disabledColor = ButtonDisabled;
            colors.fadeDuration = 0.12f;
            button.colors = colors;
            foreach (TextMeshProUGUI text in button.GetComponentsInChildren<TextMeshProUGUI>(true))
            {
                text.color = Cream;
            }
        }

        private static void Flatten(Image image)
        {
            if (IsKenney(image))
            {
                image.sprite = null;
                image.type = Image.Type.Simple;
            }
        }

        private static bool IsKenney(Image image)
        {
            return image != null && image.sprite != null && AssetDatabase.GetAssetPath(image.sprite).StartsWith(KenneyFolder);
        }

        private static bool IsOwnObject(Component component)
        {
            return !PrefabUtility.IsPartOfPrefabInstance(component);
        }

        private static bool SameColor(Color a, Color b)
        {
            return Mathf.Abs(a.r - b.r) < 0.01f && Mathf.Abs(a.g - b.g) < 0.01f && Mathf.Abs(a.b - b.b) < 0.01f;
        }
    }
}
