using System.IO;
using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace ExpeditionCamp.EditorTools
{
    public static class EditorUtil
    {
        public const string FontAssetPath = "Assets/Fonts/NotoSansKR SDF.asset";
        public const string LitShaderName = "Universal Render Pipeline/Lit";
        public static readonly Vector2 ReferenceResolution = new Vector2(1920f, 1080f);
        public static readonly Color TextColor = new Color(0.96f, 0.96f, 0.98f, 1f);
        public static readonly Color DarkPanel = new Color(0.10f, 0.11f, 0.16f, 0.94f);

        private static TMP_FontAsset _font;

        public static TMP_FontAsset Font
        {
            get
            {
                if (_font == null)
                {
                    _font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FontAssetPath);
                }
                return _font;
            }
        }

        public static void EnsureFolder(string folder)
        {
            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
                AssetDatabase.Refresh();
            }
        }

        public static Material LitMaterial(string path, Color color)
        {
            Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
            Shader shader = Shader.Find(LitShaderName);
            if (material == null)
            {
                EnsureFolder(Path.GetDirectoryName(path));
                material = new Material(shader);
                AssetDatabase.CreateAsset(material, path);
            }
            material.shader = shader;
            material.SetColor("_BaseColor", color);
            material.SetFloat("_Smoothness", 0.15f);
            EditorUtility.SetDirty(material);
            return material;
        }

        public static Material ShaderMaterial(string path, string shaderName, Color color)
        {
            Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
            Shader shader = Shader.Find(shaderName);
            if (material == null)
            {
                EnsureFolder(Path.GetDirectoryName(path));
                material = new Material(shader);
                AssetDatabase.CreateAsset(material, path);
            }
            material.shader = shader;
            material.color = color;
            EditorUtility.SetDirty(material);
            return material;
        }

        public static Sprite LoadSprite(string path)
        {
            Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if (sprite == null)
            {
                Debug.LogError("[Builder] 스프라이트를 찾을 수 없습니다: " + path);
            }
            return sprite;
        }

        public static GameObject Primitive(PrimitiveType type, string name, Transform parent, Vector3 position,
            Vector3 scale, Material material, int layer)
        {
            GameObject target = GameObject.CreatePrimitive(type);
            target.name = name;
            if (parent != null)
            {
                target.transform.SetParent(parent, false);
            }
            target.transform.localPosition = position;
            target.transform.localScale = scale;
            target.GetComponent<MeshRenderer>().sharedMaterial = material;
            target.layer = layer;
            return target;
        }

        public static GameObject Empty(string name, Transform parent, Vector3 position)
        {
            GameObject target = new GameObject(name);
            if (parent != null)
            {
                target.transform.SetParent(parent, false);
            }
            target.transform.localPosition = position;
            return target;
        }

        public static SpriteRenderer SpriteObject(string name, Transform parent, Sprite sprite, Vector3 position,
            float scale, int order, int layer)
        {
            GameObject target = Empty(name, parent, position);
            target.layer = layer;
            target.transform.localScale = Vector3.one * scale;
            SpriteRenderer renderer = target.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.sortingOrder = order;
            return renderer;
        }

        public static void SetLayerRecursive(GameObject root, int layer)
        {
            root.layer = layer;
            foreach (Transform child in root.transform)
            {
                SetLayerRecursive(child.gameObject, layer);
            }
        }

        public static Canvas ScreenCanvas(string name, Transform parent, int sortingOrder, float match)
        {
            GameObject canvasObject = new GameObject(name, typeof(RectTransform));
            if (parent != null)
            {
                canvasObject.transform.SetParent(parent, false);
            }
            canvasObject.layer = LayerMask.NameToLayer("UI");
            Canvas canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = sortingOrder;
            CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = ReferenceResolution;
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = match;
            canvasObject.AddComponent<GraphicRaycaster>();
            return canvas;
        }

        public static GameObject EventSystemObject()
        {
            GameObject eventSystem = new GameObject("EventSystem");
            eventSystem.AddComponent<EventSystem>();
            eventSystem.AddComponent<InputSystemUIInputModule>();
            return eventSystem;
        }

        public static RectTransform Rect(string name, Transform parent)
        {
            GameObject target = new GameObject(name, typeof(RectTransform));
            target.transform.SetParent(parent, false);
            target.layer = LayerMask.NameToLayer("UI");
            return target.GetComponent<RectTransform>();
        }

        public static void Place(RectTransform rect, Vector2 anchor, Vector2 pivot, Vector2 position, Vector2 size)
        {
            rect.anchorMin = anchor;
            rect.anchorMax = anchor;
            rect.pivot = pivot;
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
        }

        public static void Stretch(RectTransform rect, Vector2 offsetMin, Vector2 offsetMax)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.offsetMin = offsetMin;
            rect.offsetMax = offsetMax;
        }

        public static Image Panel(string name, Transform parent, Color color, string kenneySprite)
        {
            RectTransform rect = Rect(name, parent);
            Image image = rect.gameObject.AddComponent<Image>();
            image.color = color;
            if (!string.IsNullOrEmpty(kenneySprite))
            {
                Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/ThirdParty/Kenney/Resources/Kenney/" + kenneySprite + ".png");
                if (sprite != null)
                {
                    image.sprite = sprite;
                    image.type = Image.Type.Sliced;
                }
            }
            return image;
        }

        public static TMP_Text Text(string name, Transform parent, string content, float size, TextAlignmentOptions alignment)
        {
            RectTransform rect = Rect(name, parent);
            TextMeshProUGUI text = rect.gameObject.AddComponent<TextMeshProUGUI>();
            text.font = Font;
            text.text = content;
            text.fontSize = size;
            text.color = TextColor;
            text.alignment = alignment;
            text.textWrappingMode = TextWrappingModes.Normal;
            text.raycastTarget = false;
            return text;
        }

        public static Button Button(string name, string labelName, Transform parent, string label, Vector2 size)
        {
            RectTransform rect = Rect(name, parent);
            rect.sizeDelta = size;
            Image image = rect.gameObject.AddComponent<Image>();
            Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/ThirdParty/Kenney/Resources/Kenney/button_blue_rectangle_depth_gloss.png");
            if (sprite != null)
            {
                image.sprite = sprite;
                image.type = Image.Type.Sliced;
            }
            else
            {
                image.color = new Color(0.25f, 0.38f, 0.62f, 1f);
            }
            Button button = rect.gameObject.AddComponent<Button>();
            button.targetGraphic = image;
            TMP_Text text = Text(labelName, rect, label, 34f, TextAlignmentOptions.Center);
            Stretch(text.rectTransform, new Vector2(8f, 8f), new Vector2(-8f, -4f));
            return button;
        }

        public static Image IconImage(string name, Transform parent, Vector2 size)
        {
            RectTransform rect = Rect(name, parent);
            rect.sizeDelta = size;
            Image image = rect.gameObject.AddComponent<Image>();
            image.preserveAspect = true;
            image.raycastTarget = false;
            return image;
        }

        public static void AddClick(Button button, UnityAction action)
        {
            UnityEventTools.AddPersistentListener(button.onClick, action);
        }

        public static void SetField(Object target, string fieldName, Object value)
        {
            SerializedObject serialized = new SerializedObject(target);
            SerializedProperty property = serialized.FindProperty(fieldName);
            if (property == null)
            {
                Debug.LogError("[Builder] " + target.GetType().Name + " 에 " + fieldName + " 칸이 없습니다");
                return;
            }
            property.objectReferenceValue = value;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        public static void SetFloat(Object target, string fieldName, float value)
        {
            SerializedObject serialized = new SerializedObject(target);
            serialized.FindProperty(fieldName).floatValue = value;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        public static void SetInt(Object target, string fieldName, int value)
        {
            SerializedObject serialized = new SerializedObject(target);
            serialized.FindProperty(fieldName).intValue = value;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        public static void SetString(Object target, string fieldName, string value)
        {
            SerializedObject serialized = new SerializedObject(target);
            serialized.FindProperty(fieldName).stringValue = value;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        public static GameObject SavePrefab(GameObject root, string path)
        {
            EnsureFolder(Path.GetDirectoryName(path));
            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(root, path);
            Object.DestroyImmediate(root);
            return prefab;
        }

        public static GameObject Instance(GameObject prefab, string name, Transform parent)
        {
            GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            instance.name = name;
            if (parent != null)
            {
                instance.transform.SetParent(parent, false);
            }
            return instance;
        }
    }
}
