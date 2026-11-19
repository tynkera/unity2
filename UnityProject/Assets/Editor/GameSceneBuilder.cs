using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace ExpeditionCamp.EditorTools
{
    public static class GameSceneBuilder
    {
        public const string ScenesFolder = "Assets/Scenes";
        public static readonly string[] GameScenes = { "Boot", "Lobby", "Field_01", "Field_02" };
        public static readonly string[] FieldCameraLayers =
        {
            "Default", "UI", "Monster", "Player", "DropItem", "Ground", "Obstacle"
        };

        [MenuItem("ExpeditionCamp/Rebuild Game Scenes")]
        public static void BuildAll()
        {
            EditorUtil.EnsureFolder(ScenesFolder);
            BuildBoot();
            BuildLobby();
            BuildField01();
            BuildField02();
            RegisterBuildScenes();
            AssetDatabase.SaveAssets();
            Debug.Log("[SceneBuilder] 게임 씬 4개를 만들었습니다");
        }

        public static string ScenePath(string sceneName)
        {
            return ScenesFolder + "/" + sceneName + ".unity";
        }

        public static void RegisterBuildScenes()
        {
            List<EditorBuildSettingsScene> scenes = new List<EditorBuildSettingsScene>();
            foreach (string sceneName in GameScenes)
            {
                scenes.Add(new EditorBuildSettingsScene(ScenePath(sceneName), true));
            }
            EditorBuildSettings.scenes = scenes.ToArray();
        }

        public static int LayerMaskOf(string[] layerNames)
        {
            int mask = 0;
            foreach (string layerName in layerNames)
            {
                mask |= 1 << LayerMask.NameToLayer(layerName);
            }
            return mask;
        }

        private static Scene NewScene()
        {
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            RenderSettings.ambientMode = AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = new Color(0.55f, 0.58f, 0.66f);
            RenderSettings.ambientEquatorColor = new Color(0.40f, 0.40f, 0.40f);
            RenderSettings.ambientGroundColor = new Color(0.22f, 0.20f, 0.18f);
            return scene;
        }

        private static void Save(Scene scene, string sceneName)
        {
            UiTheme.ApplyToScene(scene);
            EditorSceneManager.SaveScene(scene, ScenePath(sceneName));
        }

        private static GameContext Systems(string name)
        {
            GameObject systems = new GameObject(name);
            GameContext context = systems.AddComponent<GameContext>();
            EditorUtil.SetField(context, "gameConfig", ProjectSetup.GameConfigAsset);
            EditorUtil.SetField(context, "qaConfig", ProjectSetup.QAConfigAsset);
            return context;
        }

        private static Camera UiOnlyCamera()
        {
            GameObject cameraObject = new GameObject("Main Camera");
            cameraObject.tag = "MainCamera";
            Camera camera = cameraObject.AddComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.08f, 0.09f, 0.12f);
            camera.orthographic = true;
            camera.cullingMask = LayerMaskOf(new[] { "Default", "UI" });
            cameraObject.transform.position = new Vector3(0f, 0f, -10f);
            cameraObject.AddComponent<AudioListener>();
            return camera;
        }

        // Boot

        private static void BuildBoot()
        {
            Scene scene = NewScene();
            UiOnlyCamera();
            Canvas canvas = EditorUtil.ScreenCanvas("Canvas", null, 0, 1f);

            Image background = EditorUtil.Panel("Img_Background", canvas.transform, Color.white, null);
            background.sprite = EditorUtil.LoadSprite(VisualRedesign.LobbyBackdropSprite);
            EditorUtil.Stretch(background.rectTransform, Vector2.zero, Vector2.zero);
            AspectRatioFitter fitter = background.gameObject.AddComponent<AspectRatioFitter>();
            fitter.aspectMode = AspectRatioFitter.AspectMode.EnvelopeParent;
            fitter.aspectRatio = 16f / 9f;

            TMP_Text title = EditorUtil.Text("Txt_Title", canvas.transform, "원정대 캠프", 120f, TextAlignmentOptions.Center);
            title.fontStyle = FontStyles.Bold;
            EditorUtil.Place(title.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 120f), new Vector2(1200f, 180f));
            TMP_Text loading = EditorUtil.Text("Txt_Loading", canvas.transform, "데이터를 불러오는 중입니다", 44f, TextAlignmentOptions.Center);
            EditorUtil.Place(loading.rectTransform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 120f), new Vector2(1200f, 80f));

            RectTransform popup = EditorUtil.Rect("Popup_DataError", canvas.transform);
            EditorUtil.Stretch(popup, Vector2.zero, Vector2.zero);
            Image dim = popup.gameObject.AddComponent<Image>();
            dim.color = new Color(0f, 0f, 0f, 0.75f);
            Image panel = EditorUtil.Panel("Panel_DataError", popup, new Color(0.35f, 0.12f, 0.14f, 1f), "panel_grey_square_depth_flat");
            EditorUtil.Place(panel.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(1300f, 640f));
            TMP_Text errorTitle = EditorUtil.Text("Txt_DataErrorTitle", panel.transform, "데이터 오류로 게임을 시작할 수 없습니다", 50f, TextAlignmentOptions.Center);
            errorTitle.fontStyle = FontStyles.Bold;
            EditorUtil.Place(errorTitle.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -40f), new Vector2(1200f, 80f));
            TMP_Text errorDetail = EditorUtil.Text("Txt_DataErrorDetail", panel.transform, "", 40f, TextAlignmentOptions.TopLeft);
            EditorUtil.Stretch(errorDetail.rectTransform, new Vector2(80f, 60f), new Vector2(-80f, -150f));
            popup.gameObject.SetActive(false);

            GameContext context = Systems("Systems");
            BootLoader boot = context.gameObject.AddComponent<BootLoader>();
            EditorUtil.SetField(boot, "loadingText", loading);
            EditorUtil.SetField(boot, "dataErrorPopup", popup.gameObject);
            EditorUtil.SetField(boot, "dataErrorDetail", errorDetail);
            Save(scene, "Boot");
        }

        // Lobby

        private static void BuildLobby()
        {
            Scene scene = NewScene();
            Camera camera = UiOnlyCamera();
            camera.orthographicSize = 5.4f;
            camera.gameObject.AddComponent<Physics2DRaycaster>();

            SpriteRenderer background = EditorUtil.SpriteObject("Background", null,
                EditorUtil.LoadSprite(VisualRedesign.LobbyBackdropSprite), new Vector3(0f, 0f, 0f), 1f, 0, 0);

            Canvas canvas = EditorUtil.ScreenCanvas("Canvas", null, 0, 1f);

            Image topBar = EditorUtil.Panel("Panel_TopBar", canvas.transform, new Color(0.10f, 0.11f, 0.16f, 0.85f), "panel_grey_square_depth_flat");
            topBar.color = new Color(0.25f, 0.27f, 0.35f, 0.92f);
            EditorUtil.Place(topBar.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(30f, -30f), new Vector2(620f, 110f));
            TMP_Text level = EditorUtil.Text("Txt_Level", topBar.transform, "Lv.1", 44f, TextAlignmentOptions.Left);
            EditorUtil.Place(level.rectTransform, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(30f, 0f), new Vector2(200f, 70f));
            TMP_Text gold = EditorUtil.Text("Txt_Gold", topBar.transform, "골드 0", 44f, TextAlignmentOptions.Left);
            EditorUtil.Place(gold.rectTransform, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(240f, 0f), new Vector2(360f, 70f));

            Button bag = BottomButton("Btn_Bag", "Txt_Bag", canvas.transform, "가방", new Vector2(0f, 0f), new Vector2(40f, 40f));
            Button dex = BottomButton("Btn_Dex", "Txt_Dex", canvas.transform, "도감", new Vector2(0f, 0f), new Vector2(260f, 40f));
            Button shop = BottomButton("Btn_Shop", "Txt_Shop", canvas.transform, "상점", new Vector2(0f, 0f), new Vector2(480f, 40f));
            Button settings = BottomButton("Btn_Settings", "Txt_Settings", canvas.transform, "설정", new Vector2(1f, 0f), new Vector2(-40f, 40f));

            PopupBase bagPopup = PopupInstance("UI_Popup_Bag", "Popup_Bag", canvas.transform);
            PopupBase dexPopup = PopupInstance("UI_Popup_Dex", "Popup_Dex", canvas.transform);
            PopupBase shopPopup = PopupInstance("UI_Popup_Shop", "Popup_Shop", canvas.transform);
            PopupBase stagePopup = PopupInstance("UI_Popup_StageSelect", "Popup_StageSelect", canvas.transform);
            PopupBase settingsPopup = PopupInstance("UI_Popup_Settings", "Popup_Settings", canvas.transform);

            RectTransform loadingPopup = EditorUtil.Rect("Popup_Loading", canvas.transform);
            EditorUtil.Stretch(loadingPopup, Vector2.zero, Vector2.zero);
            Image loadingDim = loadingPopup.gameObject.AddComponent<Image>();
            loadingDim.color = new Color(0f, 0f, 0f, 0.85f);
            TMP_Text loadingText = EditorUtil.Text("Txt_StageLoading", loadingPopup, "스테이지를 불러오는 중입니다", 52f, TextAlignmentOptions.Center);
            EditorUtil.Place(loadingText.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(1200f, 100f));
            loadingPopup.gameObject.SetActive(false);

            GameObject npcRoot = new GameObject("NPCs");
            NpcInstance("NPC_Shop", npcRoot.transform, new Vector3(-6.2f, -0.6f, 0f), shopPopup);
            NpcInstance("NPC_Dex", npcRoot.transform, new Vector3(-1.8f, -0.9f, 0f), dexPopup);
            NpcInstance("NPC_Board", npcRoot.transform, new Vector3(5.6f, -0.6f, 0f), stagePopup);

            EditorUtil.EventSystemObject();

            GameContext context = Systems("Systems");
            LobbyController lobby = context.gameObject.AddComponent<LobbyController>();
            EditorUtil.SetField(lobby, "goldText", gold);
            EditorUtil.SetField(lobby, "levelText", level);
            EditorUtil.SetField(lobby, "bagButton", bag);
            EditorUtil.SetField(lobby, "dexButton", dex);
            EditorUtil.SetField(lobby, "shopButton", shop);
            EditorUtil.SetField(lobby, "settingsButton", settings);
            EditorUtil.SetField(lobby, "bagPopup", bagPopup);
            EditorUtil.SetField(lobby, "dexPopup", dexPopup);
            EditorUtil.SetField(lobby, "shopPopup", shopPopup);
            EditorUtil.SetField(lobby, "settingsPopup", settingsPopup);
            EditorUtil.SetField(lobby, "loadingPopup", loadingPopup.gameObject);
            EditorUtil.SetField(lobby, "loadingText", loadingText);
            EditorUtil.SetField(shopPopup, "lobby", lobby);
            EditorUtil.SetField(stagePopup, "lobby", lobby);

            Save(scene, "Lobby");
        }

        private static Button BottomButton(string name, string labelName, Transform parent, string label, Vector2 anchor, Vector2 position)
        {
            Button button = EditorUtil.Button(name, labelName, parent, label, new Vector2(180f, 110f));
            EditorUtil.Place((RectTransform)button.transform, anchor, anchor, position, new Vector2(180f, 110f));
            return button;
        }

        private static PopupBase PopupInstance(string prefabName, string instanceName, Transform parent)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabBuilder.UiFolder + "/" + prefabName + ".prefab");
            GameObject instance = EditorUtil.Instance(prefab, instanceName, parent);
            instance.SetActive(false);
            return instance.GetComponent<PopupBase>();
        }

        private static void NpcInstance(string prefabName, Transform parent, Vector3 position, PopupBase popup)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabBuilder.NpcFolder + "/" + prefabName + ".prefab");
            GameObject instance = EditorUtil.Instance(prefab, prefabName, parent);
            instance.transform.localPosition = position;
            instance.transform.localScale = Vector3.one * 1.3f;
            EditorUtil.SetField(instance.GetComponent<NpcClick>(), "targetPopup", popup);
        }

        // Field 공통

        private class FieldParts
        {
            public Transform Environment;
            public Transform SpawnRoot;
            public Transform PlayerStart;
        }

        private static FieldParts FieldBase(Color lightColor, float lightIntensity, Vector3 playerStart)
        {
            FieldParts parts = new FieldParts();

            GameObject lightObject = new GameObject("Directional Light");
            Light light = lightObject.AddComponent<Light>();
            light.type = LightType.Directional;
            light.color = lightColor;
            light.intensity = lightIntensity;
            light.shadows = LightShadows.Soft;
            lightObject.transform.rotation = Quaternion.Euler(50f, -30f, 0f);

            parts.Environment = new GameObject("Environment").transform;
            parts.SpawnRoot = new GameObject("SpawnPoints").transform;
            parts.PlayerStart = EditorUtil.Empty("PlayerStart", null, playerStart).transform;
            return parts;
        }

        private static void Boundary(Transform parent, float half)
        {
            int obstacle = LayerMask.NameToLayer("Obstacle");
            Transform root = EditorUtil.Empty("Boundary", parent, Vector3.zero).transform;
            BoundaryWall("Boundary_North", root, new Vector3(0f, 1f, half), new Vector3(half * 2f, 2f, 1f), obstacle);
            BoundaryWall("Boundary_South", root, new Vector3(0f, 1f, -half), new Vector3(half * 2f, 2f, 1f), obstacle);
            BoundaryWall("Boundary_East", root, new Vector3(half, 1f, 0f), new Vector3(1f, 2f, half * 2f), obstacle);
            BoundaryWall("Boundary_West", root, new Vector3(-half, 1f, 0f), new Vector3(1f, 2f, half * 2f), obstacle);
        }

        private static void BoundaryWall(string name, Transform parent, Vector3 position, Vector3 size, int layer)
        {
            GameObject wall = EditorUtil.Empty(name, parent, position);
            wall.layer = layer;
            BoxCollider box = wall.AddComponent<BoxCollider>();
            box.size = size;
        }

        private static GameObject Rock(string name, Transform parent, PrimitiveType type, Vector3 position, Vector3 scale, float yaw)
        {
            GameObject rock = EditorUtil.Primitive(type, name, parent, position, scale, PrefabBuilder.Mat("Rock"), LayerMask.NameToLayer("Obstacle"));
            rock.transform.localRotation = Quaternion.Euler(0f, yaw, 0f);
            rock.isStatic = true;
            return rock;
        }

        private static void Tree(string name, Transform parent, Vector3 position, float height)
        {
            int obstacle = LayerMask.NameToLayer("Obstacle");
            GameObject tree = EditorUtil.Empty(name, parent, position);
            tree.layer = obstacle;
            GameObject trunk = EditorUtil.Primitive(PrimitiveType.Cylinder, name + "_Trunk", tree.transform,
                new Vector3(0f, height * 0.5f, 0f), new Vector3(0.45f, height * 0.5f, 0.45f), PrefabBuilder.Mat("TreeTrunk"), obstacle);
            trunk.isStatic = true;
            GameObject leaves = EditorUtil.Primitive(PrimitiveType.Sphere, name + "_Leaves", tree.transform,
                new Vector3(0f, height + 0.6f, 0f), new Vector3(2.2f, 2f, 2.2f), PrefabBuilder.Mat("TreeLeaves"), obstacle);
            Object.DestroyImmediate(leaves.GetComponent<Collider>());
            leaves.isStatic = true;
        }

        private static void Spawn(string name, Transform parent, Vector3 position, float radius)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabBuilder.PrefabFolder + "/SpawnPoint.prefab");
            GameObject instance = EditorUtil.Instance(prefab, name, parent);
            instance.transform.localPosition = position;
            instance.GetComponent<SpawnPoint>().spawnRadius = radius;
        }

        private static void FieldCommon(FieldParts parts, string stageLabel, int editorStageId)
        {
            GameConfig config = ProjectSetup.GameConfigAsset;

            GameObject playerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabBuilder.PrefabFolder + "/Player.prefab");
            GameObject player = EditorUtil.Instance(playerPrefab, "Player", null);
            player.transform.position = parts.PlayerStart.position;

            GameObject cameraObject = new GameObject("Main Camera");
            cameraObject.tag = "MainCamera";
            Camera camera = cameraObject.AddComponent<Camera>();
            camera.fieldOfView = 45f;
            camera.nearClipPlane = 0.3f;
            camera.farClipPlane = 200f;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.55f, 0.72f, 0.90f);
            camera.cullingMask = LayerMaskOf(FieldCameraLayers);
            cameraObject.AddComponent<AudioListener>();
            Vector3 focus = parts.PlayerStart.position + Vector3.up * config.cameraLookHeight;
            cameraObject.transform.position = CameraFollow.CalculatePosition(focus, config);
            cameraObject.transform.LookAt(focus);
            CameraFollow follow = cameraObject.AddComponent<CameraFollow>();
            EditorUtil.SetField(follow, "config", config);
            EditorUtil.SetField(follow, "target", player.transform);

            Canvas canvas = EditorUtil.ScreenCanvas("Canvas", null, 0, 1f);
            Image hudPanel = EditorUtil.Panel("Panel_Hud", canvas.transform, new Color(0.10f, 0.11f, 0.16f, 0.8f), null);
            EditorUtil.Place(hudPanel.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(30f, -30f), new Vector2(640f, 220f));
            TMP_Text stageText = EditorUtil.Text("Txt_StageName", hudPanel.transform, stageLabel, 40f, TextAlignmentOptions.Left);
            EditorUtil.Place(stageText.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(24f, -14f), new Vector2(600f, 60f));
            TMP_Text waveText = EditorUtil.Text("Txt_Wave", hudPanel.transform, "웨이브 1 / 3", 36f, TextAlignmentOptions.Left);
            EditorUtil.Place(waveText.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(24f, -78f), new Vector2(600f, 54f));
            Image hpBack = EditorUtil.Panel("Bar_PlayerHp", hudPanel.transform, new Color(0f, 0f, 0f, 0.6f), null);
            EditorUtil.Place(hpBack.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(24f, -150f), new Vector2(592f, 46f));
            Image hpFill = EditorUtil.Panel("Img_PlayerHpFill", hpBack.transform, new Color(0.28f, 0.80f, 0.40f, 1f), null);
            EditorUtil.Stretch(hpFill.rectTransform, new Vector2(3f, 3f), new Vector2(-3f, -3f));
            hpFill.sprite = EditorUtil.LoadSprite(PrefabBuilder.SolidBarSprite);
            hpFill.type = Image.Type.Filled;
            hpFill.fillMethod = Image.FillMethod.Horizontal;
            TMP_Text hpText = EditorUtil.Text("Txt_PlayerHp", hpBack.transform, "HP 1000 / 1000", 30f, TextAlignmentOptions.Center);
            EditorUtil.Stretch(hpText.rectTransform, Vector2.zero, Vector2.zero);

            Toggle toggle = AutoBattleToggle(canvas.transform);

            GameObject resultPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabBuilder.UiFolder + "/UI_Popup_Result.prefab");
            GameObject result = EditorUtil.Instance(resultPrefab, "Popup_Result", canvas.transform);
            result.SetActive(false);

            EditorUtil.EventSystemObject();
            GameContext context = Systems("Systems");

            GameObject hudObject = canvas.gameObject;
            FieldHud hud = hudObject.AddComponent<FieldHud>();
            EditorUtil.SetField(hud, "stageText", stageText);
            EditorUtil.SetField(hud, "waveText", waveText);
            EditorUtil.SetField(hud, "hpText", hpText);
            EditorUtil.SetField(hud, "hpFill", hpFill);
            EditorUtil.SetField(hud, "autoBattleToggle", toggle);

            GameObject managerObject = new GameObject("FieldManager");
            FieldManager manager = managerObject.AddComponent<FieldManager>();
            EditorUtil.SetInt(manager, "editorStageId", editorStageId);
            EditorUtil.SetField(manager, "config", config);
            EditorUtil.SetField(manager, "player", player.GetComponent<PlayerController>());
            EditorUtil.SetField(manager, "playerStart", parts.PlayerStart);
            EditorUtil.SetField(manager, "cameraFollow", follow);
            EditorUtil.SetField(manager, "hud", hud);
            EditorUtil.SetField(manager, "resultPopup", result.GetComponent<ResultPopup>());
            EditorUtil.SetField(manager, "dropItemPrefab",
                AssetDatabase.LoadAssetAtPath<DropItem>(PrefabBuilder.PrefabFolder + "/DropItem.prefab"));
        }

        private static Toggle AutoBattleToggle(Transform parent)
        {
            RectTransform root = EditorUtil.Rect("Tgl_AutoBattle", parent);
            EditorUtil.Place(root, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-30f, -30f), new Vector2(320f, 90f));
            Image background = root.gameObject.AddComponent<Image>();
            background.color = new Color(0.10f, 0.11f, 0.16f, 0.8f);
            Toggle toggle = root.gameObject.AddComponent<Toggle>();

            Image box = EditorUtil.Panel("AutoBattleBox", root, Color.white, null);
            EditorUtil.Place(box.rectTransform, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(20f, 0f), new Vector2(56f, 56f));
            Image check = EditorUtil.Panel("AutoBattleCheck", box.transform, new Color(0.20f, 0.65f, 0.30f, 1f), null);
            EditorUtil.Stretch(check.rectTransform, new Vector2(8f, 8f), new Vector2(-8f, -8f));
            TMP_Text label = EditorUtil.Text("Txt_AutoBattle", root, "자동 전투", 36f, TextAlignmentOptions.Left);
            EditorUtil.Place(label.rectTransform, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(96f, 0f), new Vector2(220f, 60f));

            toggle.targetGraphic = box;
            toggle.graphic = check;
            toggle.isOn = true;
            return toggle;
        }

        // Field_01

        private static void BuildField01()
        {
            Scene scene = NewScene();
            FieldParts parts = FieldBase(new Color(1f, 0.96f, 0.88f), 1.0f, new Vector3(0f, 0f, -8f));
            int ground = LayerMask.NameToLayer("Ground");

            GameObject floor = EditorUtil.Primitive(PrimitiveType.Cube, "Ground", parts.Environment,
                new Vector3(0f, -0.5f, 0f), new Vector3(44f, 1f, 44f), PrefabBuilder.Mat("Ground"), ground);
            floor.isStatic = true;
            GameObject river = EditorUtil.Primitive(PrimitiveType.Cube, "River", parts.Environment,
                new Vector3(0f, 0.01f, 5f), new Vector3(44f, 0.02f, 3.2f), PrefabBuilder.Mat("River"), 0);
            Object.DestroyImmediate(river.GetComponent<Collider>());
            river.isStatic = true;
            GameObject bridge = EditorUtil.Primitive(PrimitiveType.Cube, "Bridge_01", parts.Environment,
                new Vector3(0f, 0.12f, 5f), new Vector3(3.4f, 0.2f, 4.6f), PrefabBuilder.Mat("Bridge"), ground);
            bridge.isStatic = true;

            Transform rocks = EditorUtil.Empty("Rocks", parts.Environment, Vector3.zero).transform;
            Rock("Rock_01", rocks, PrimitiveType.Sphere, new Vector3(-9f, 0.6f, -4f), new Vector3(2.4f, 1.6f, 2.2f), 0f);
            Rock("Rock_02", rocks, PrimitiveType.Cube, new Vector3(-4f, 0.7f, 1.5f), new Vector3(1.6f, 1.4f, 1.8f), 25f);
            Rock("Rock_03", rocks, PrimitiveType.Sphere, new Vector3(5f, 0.5f, -2f), new Vector3(1.8f, 1.2f, 1.6f), 0f);
            Rock("Rock_04", rocks, PrimitiveType.Cube, new Vector3(9.5f, 0.9f, 7.5f), new Vector3(2.2f, 1.8f, 2f), -15f);
            Rock("Rock_05", rocks, PrimitiveType.Sphere, new Vector3(-7f, 0.8f, 10f), new Vector3(3f, 2f, 2.6f), 0f);
            Rock("Rock_06", rocks, PrimitiveType.Cube, new Vector3(3.5f, 0.6f, 12f), new Vector3(1.4f, 1.2f, 1.4f), 40f);

            Transform trees = EditorUtil.Empty("Trees", parts.Environment, Vector3.zero).transform;
            Tree("Tree_01", trees, new Vector3(-15f, 0f, -12f), 2.4f);
            Tree("Tree_02", trees, new Vector3(-12f, 0f, 0f), 2.8f);
            Tree("Tree_03", trees, new Vector3(-16f, 0f, 9f), 2.6f);
            Tree("Tree_04", trees, new Vector3(13f, 0f, -11f), 2.5f);
            Tree("Tree_05", trees, new Vector3(15f, 0f, 1f), 3.0f);
            Tree("Tree_06", trees, new Vector3(12f, 0f, 13f), 2.7f);
            Tree("Tree_07", trees, new Vector3(-3f, 0f, 16f), 2.9f);
            Tree("Tree_08", trees, new Vector3(7f, 0f, -14f), 2.4f);
            Boundary(parts.Environment, 21f);

            Spawn("Spawn_01", parts.SpawnRoot, new Vector3(-8f, 0f, 2f), 1.5f);
            Spawn("Spawn_02", parts.SpawnRoot, new Vector3(0f, 0f, 9.5f), 1.2f);
            Spawn("Spawn_03", parts.SpawnRoot, new Vector3(8.5f, 0f, 2.5f), 1.5f);
            Spawn("Spawn_04", parts.SpawnRoot, new Vector3(-3f, 0f, 13f), 1.8f);

            GameObject crabPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabBuilder.MonsterFolder + "/Monster_Crab.prefab");
            Transform spawn02 = parts.SpawnRoot.Find("Spawn_02");
            GameObject placed = EditorUtil.Instance(crabPrefab, "Monster_Crab_Bridge", spawn02);
            placed.transform.localPosition = Vector3.zero;
            placed.SetActive(false);

            FieldCommon(parts, "1-1 초원 입구", 401001);
            Save(scene, "Field_01");
        }

        // Field_02

        private static void BuildField02()
        {
            Scene scene = NewScene();
            FieldParts parts = FieldBase(new Color(0.95f, 0.85f, 0.75f), 0.8f, new Vector3(0f, 0f, -10f));
            int ground = LayerMask.NameToLayer("Ground");
            int obstacle = LayerMask.NameToLayer("Obstacle");

            GameObject floor = EditorUtil.Primitive(PrimitiveType.Cube, "Ground", parts.Environment,
                new Vector3(0f, -0.5f, 0f), new Vector3(36f, 1f, 36f), PrefabBuilder.Mat("CaveGround"), ground);
            floor.isStatic = true;

            Transform cave = EditorUtil.Empty("CaveEntrance", parts.Environment, Vector3.zero).transform;
            EditorUtil.Primitive(PrimitiveType.Cube, "CaveEntrance_Left", cave, new Vector3(-5f, 2.5f, 8f), new Vector3(6f, 5f, 3f), PrefabBuilder.Mat("CaveWall"), obstacle).isStatic = true;
            EditorUtil.Primitive(PrimitiveType.Cube, "CaveEntrance_Right", cave, new Vector3(5f, 2.5f, 8f), new Vector3(6f, 5f, 3f), PrefabBuilder.Mat("CaveWall"), obstacle).isStatic = true;
            EditorUtil.Primitive(PrimitiveType.Cube, "CaveEntrance_Top", cave, new Vector3(0f, 5.5f, 8f), new Vector3(16f, 1.2f, 3f), PrefabBuilder.Mat("CaveWall"), obstacle).isStatic = true;

            Transform corridor = EditorUtil.Empty("Corridor", parts.Environment, Vector3.zero).transform;
            EditorUtil.Primitive(PrimitiveType.Cube, "Corridor_WallLeft", corridor, new Vector3(-2.2f, 1.2f, -1f), new Vector3(1f, 2.4f, 9f), PrefabBuilder.Mat("CaveWall"), obstacle).isStatic = true;
            EditorUtil.Primitive(PrimitiveType.Cube, "Corridor_WallRight", corridor, new Vector3(2.2f, 1.2f, -1f), new Vector3(1f, 2.4f, 9f), PrefabBuilder.Mat("CaveWall"), obstacle).isStatic = true;

            Transform pillars = EditorUtil.Empty("Pillars", parts.Environment, Vector3.zero).transform;
            Vector3[] pillarPositions =
            {
                new Vector3(-9f, 1.6f, -6f), new Vector3(9f, 1.6f, -6f), new Vector3(-10f, 1.6f, 3f), new Vector3(10f, 1.6f, 3f)
            };
            for (int index = 0; index < pillarPositions.Length; index++)
            {
                EditorUtil.Primitive(PrimitiveType.Cylinder, "Pillar_0" + (index + 1), pillars, pillarPositions[index],
                    new Vector3(1.4f, 1.6f, 1.4f), PrefabBuilder.Mat("Pillar"), obstacle).isStatic = true;
            }
            Boundary(parts.Environment, 17f);

            Spawn("Spawn_01", parts.SpawnRoot, new Vector3(-7f, 0f, 1f), 1.5f);
            Spawn("Spawn_02", parts.SpawnRoot, new Vector3(7f, 0f, 1f), 1.5f);
            Spawn("Spawn_03", parts.SpawnRoot, new Vector3(0f, 0f, 12f), 1.5f);

            FieldCommon(parts, "1-6 돌무지 길", 401006);
            Save(scene, "Field_02");
        }
    }
}
