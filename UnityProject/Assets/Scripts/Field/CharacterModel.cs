using UnityEngine;

namespace ExpeditionCamp
{
    public enum CharacterShape
    {
        Fox,
        Rabbit,
        Wolf,
        Crab,
        Bat,
        Golem
    }

    [ExecuteAlways]
    [DefaultExecutionOrder(-100)]
    public class CharacterModel : MonoBehaviour
    {
        private const string PartPrefix = "Part_";

        public CharacterShape shape = CharacterShape.Fox;
        public Material bodyMaterial;

        [SerializeField] private Mesh partMesh;
        [SerializeField] private Material darkMaterial;
        [SerializeField] private Material lightMaterial;
        [SerializeField] private Material clothMaterial;

        private void Awake()
        {
            if (Application.isPlaying)
            {
                Rebuild();
            }
        }

        private void OnEnable()
        {
            if (!Application.isPlaying)
            {
                Rebuild();
            }
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (Application.isPlaying)
            {
                return;
            }
            UnityEditor.EditorApplication.delayCall += () =>
            {
                if (this != null && isActiveAndEnabled)
                {
                    Rebuild();
                }
            };
        }
#endif

        public void ClearParts()
        {
            for (int index = transform.childCount - 1; index >= 0; index--)
            {
                Transform child = transform.GetChild(index);
                if (!child.name.StartsWith(PartPrefix))
                {
                    continue;
                }
                if (Application.isPlaying)
                {
                    child.SetParent(null);
                    Destroy(child.gameObject);
                }
                else
                {
                    DestroyImmediate(child.gameObject);
                }
            }
        }

        public void Rebuild()
        {
            ClearParts();
            if (partMesh == null)
            {
                return;
            }
            Material body = bodyMaterial != null ? bodyMaterial : lightMaterial;
            switch (shape)
            {
                case CharacterShape.Crab:
                    BuildCrab(body);
                    break;
                case CharacterShape.Bat:
                    BuildBat(body);
                    break;
                case CharacterShape.Golem:
                    BuildGolem(body);
                    break;
                default:
                    BuildFourLegged(body);
                    break;
            }
            ModelMotion motion = GetComponent<ModelMotion>();
            if (motion != null)
            {
                motion.flying = shape == CharacterShape.Bat;
            }
        }

        private void Part(string name, Vector3 position, Vector3 scale, Material material, Vector3 rotation)
        {
            GameObject part = new GameObject(PartPrefix + name);
            if (!Application.isPlaying)
            {
                part.hideFlags = HideFlags.DontSave | HideFlags.NotEditable;
            }
            part.layer = gameObject.layer;
            Transform partTransform = part.transform;
            partTransform.SetParent(transform, false);
            partTransform.localPosition = position;
            partTransform.localScale = scale;
            partTransform.localEulerAngles = rotation;
            part.AddComponent<MeshFilter>().sharedMesh = partMesh;
            part.AddComponent<MeshRenderer>().sharedMaterial = material;
        }

        private void Part(string name, Vector3 position, Vector3 scale, Material material)
        {
            Part(name, position, scale, material, Vector3.zero);
        }

        private void BuildCrab(Material body)
        {
            Part("Shell", new Vector3(0f, 0.65f, 0f), new Vector3(1.5f, 0.7f, 1.15f), body);
            for (int side = -1; side <= 1; side += 2)
            {
                for (int index = 0; index < 3; index++)
                {
                    Part("Leg", new Vector3(side * 0.72f, 0.27f, -0.4f + index * 0.38f), new Vector3(0.85f, 0.17f, 0.18f), body,
                        new Vector3(0f, side * (index - 1) * 22f, side * 23f));
                }
                Part("Claw", new Vector3(side * 0.85f, 0.8f, 0.65f), new Vector3(0.6f, 0.5f, 0.65f), body);
                Part("Eye", new Vector3(side * 0.32f, 1.08f, 0.4f), new Vector3(0.16f, 0.25f, 0.16f), darkMaterial);
            }
        }

        private void BuildGolem(Material body)
        {
            Part("Torso", new Vector3(0f, 1.1f, 0f), new Vector3(1.4f, 1.4f, 0.9f), body);
            Part("Head", new Vector3(0f, 2f, 0f), new Vector3(0.85f, 0.75f, 0.75f), body);
            for (int side = -1; side <= 1; side += 2)
            {
                Part("Arm", new Vector3(side * 0.9f, 1.05f, 0.1f), new Vector3(0.65f, 1.25f, 0.7f), body);
                Part("Foot", new Vector3(side * 0.4f, 0.28f, 0.1f), new Vector3(0.65f, 0.55f, 0.8f), body);
                Part("Eye", new Vector3(side * 0.19f, 2.07f, 0.34f), new Vector3(0.15f, 0.12f, 0.12f), clothMaterial);
            }
            Part("Heart", new Vector3(0f, 1.25f, 0.47f), new Vector3(0.35f, 0.5f, 0.15f), clothMaterial);
        }

        private void BuildBat(Material body)
        {
            Part("Body", new Vector3(0f, 1.2f, 0f), new Vector3(0.6f, 0.8f, 0.55f), body);
            for (int side = -1; side <= 1; side += 2)
            {
                Part("Wing", new Vector3(side * 0.72f, 1.25f, 0f), new Vector3(1.25f, 0.12f, 0.8f), body, new Vector3(0f, 0f, side * 18f));
                Part("Ear", new Vector3(side * 0.2f, 1.72f, 0f), new Vector3(0.22f, 0.5f, 0.2f), body);
                Part("Eye", new Vector3(side * 0.15f, 1.4f, 0.26f), new Vector3(0.12f, 0.13f, 0.1f), lightMaterial);
            }
        }

        private void BuildFourLegged(Material body)
        {
            bool rabbit = shape == CharacterShape.Rabbit;
            bool wolf = shape == CharacterShape.Wolf;
            Part("Body", new Vector3(0f, 0.72f, 0f), new Vector3(0.82f, 0.95f, 1.05f), body);
            Part("Head", new Vector3(0f, 1.35f, 0.35f), new Vector3(0.95f, 0.85f, 0.85f), body);
            Part("Muzzle", new Vector3(0f, 1.15f, 0.74f), new Vector3(0.62f, 0.42f, 0.45f), lightMaterial);
            Part("Nose", new Vector3(0f, 1.27f, 0.94f), new Vector3(0.19f, 0.14f, 0.14f), darkMaterial);
            float earHeight = rabbit ? 2.0f : 1.83f;
            for (int side = -1; side <= 1; side += 2)
            {
                Vector3 tilt = new Vector3(0f, 0f, side * -14f);
                Part("Ear", new Vector3(side * 0.3f, earHeight, 0.25f), new Vector3(0.32f, rabbit ? 1.05f : 0.62f, 0.28f), body, tilt);
                Part("InnerEar", new Vector3(side * 0.3f, earHeight, 0.38f), new Vector3(0.15f, rabbit ? 0.72f : 0.34f, 0.06f), lightMaterial, tilt);
                Part("Eye", new Vector3(side * 0.28f, 1.44f, 0.69f), new Vector3(0.14f, 0.17f, 0.11f), darkMaterial);
                foreach (float z in new[] { -0.33f, 0.35f })
                {
                    Part("Paw", new Vector3(side * 0.29f, 0.22f, z), new Vector3(0.32f, 0.4f, 0.48f), wolf ? darkMaterial : lightMaterial);
                }
            }
            Part("Tail", new Vector3(0f, 0.85f, -0.78f), new Vector3(0.55f, 0.55f, rabbit ? 0.45f : 1.15f), body, new Vector3(-32f, 0f, 0f));
            if (shape == CharacterShape.Fox)
            {
                Part("Scarf", new Vector3(0f, 1.01f, 0.18f), new Vector3(0.94f, 0.24f, 0.8f), clothMaterial);
                Part("Backpack", new Vector3(0f, 1.04f, -0.42f), new Vector3(0.65f, 0.65f, 0.4f), clothMaterial);
            }
        }
    }
}
