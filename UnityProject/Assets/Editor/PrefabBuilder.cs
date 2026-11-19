using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace ExpeditionCamp.EditorTools
{
    public static class PrefabBuilder
    {
        public const string PrefabFolder = "Assets/Prefabs";
        public const string UiFolder = PrefabFolder + "/UI";
        public const string NpcFolder = PrefabFolder + "/NPC";
        public const string MonsterFolder = "Assets/Resources/Monsters";
        public const string MaterialFolder = "Assets/Materials";

        public const string PlayerSprite = "Assets/Resources/Icons/Monster/mon_100001.png";
        public const string ShadowSprite = "Assets/Art/shadow.png";
        public const string CircleSprite = "Assets/Art/circle.png";
        public const float GolemHpBarHeight = 2.8f;
        public const string SolidBarSprite = "Assets/Art/solid_bar.png";

        public struct VariantSpec
        {
            public string Name;
            public int MonsterId;
            public float MoveSpeed;
            public float VisualScale;

            public VariantSpec(string name, int monsterId, float moveSpeed, float visualScale)
            {
                Name = name;
                MonsterId = monsterId;
                MoveSpeed = moveSpeed;
                VisualScale = visualScale;
            }
        }

        public static readonly VariantSpec[] Variants =
        {
            new VariantSpec("Monster_Crab", 100002, 1.8f, 0.75f),
            new VariantSpec("Monster_Rabbit", 100003, 1.5f, 0.75f),
            new VariantSpec("Monster_Bat", 100010, 3.2f, 0.75f),
            new VariantSpec("Monster_Golem", 100014, 1.2f, 1.05f),
            new VariantSpec("Monster_Wolf", 100016, 3.0f, 0.8f),
        };

        [MenuItem("ExpeditionCamp/Rebuild Prefabs")]
        public static void BuildAll()
        {
            BuildMaterials();
            BuildPlayer();
            BuildMonsterBase();
            BuildVariants();
            BuildDropItem();
            BuildSpawnPoint();
            BuildUiParts();
            BuildPopups();
            UiTheme.ApplyToUiPrefabs();
            BuildNpcs();
            AssetDatabase.SaveAssets();
            Debug.Log("[PrefabBuilder] 프리팹을 만들었습니다");
        }

        public static Material Mat(string name)
        {
            return AssetDatabase.LoadAssetAtPath<Material>(MaterialFolder + "/" + name + ".mat");
        }

        private static void BuildMaterials()
        {
            EditorUtil.LitMaterial(MaterialFolder + "/Ground.mat", new Color(0.42f, 0.62f, 0.33f));
            EditorUtil.LitMaterial(MaterialFolder + "/Rock.mat", new Color(0.55f, 0.55f, 0.58f));
            EditorUtil.LitMaterial(MaterialFolder + "/TreeTrunk.mat", new Color(0.45f, 0.30f, 0.18f));
            EditorUtil.LitMaterial(MaterialFolder + "/TreeLeaves.mat", new Color(0.20f, 0.48f, 0.22f));
            EditorUtil.LitMaterial(MaterialFolder + "/Bridge.mat", new Color(0.62f, 0.45f, 0.26f));
            EditorUtil.LitMaterial(MaterialFolder + "/River.mat", new Color(0.25f, 0.50f, 0.85f));
            EditorUtil.LitMaterial(MaterialFolder + "/CaveGround.mat", new Color(0.40f, 0.36f, 0.32f));
            EditorUtil.LitMaterial(MaterialFolder + "/CaveWall.mat", new Color(0.30f, 0.27f, 0.25f));
            EditorUtil.LitMaterial(MaterialFolder + "/Pillar.mat", new Color(0.50f, 0.44f, 0.38f));
        }

        private static GameObject CharacterRoot(string name, string tag, string layer, out CharacterController controller)
        {
            GameObject root = new GameObject(name);
            root.tag = tag;
            controller = root.AddComponent<CharacterController>();
            controller.center = new Vector3(0f, 0.9f, 0f);
            controller.height = 1.8f;
            controller.radius = 0.45f;
            controller.skinWidth = 0.05f;
            controller.minMoveDistance = 0f;
            return root;
        }

        private static HitFeedback AddVisual(GameObject root, CharacterShape shape)
        {
            return CombatArt.Build(root, shape);
        }

        private static CharacterShape ShapeOf(string variantName)
        {
            if (variantName.Contains("Rabbit")) return CharacterShape.Rabbit;
            if (variantName.Contains("Bat")) return CharacterShape.Bat;
            if (variantName.Contains("Golem")) return CharacterShape.Golem;
            if (variantName.Contains("Wolf")) return CharacterShape.Wolf;
            if (variantName.Contains("Crab")) return CharacterShape.Crab;
            return CharacterShape.Fox;
        }

        private static void AddShadow(GameObject root, float scale)
        {
            SpriteRenderer shadow = EditorUtil.SpriteObject("Shadow", root.transform, EditorUtil.LoadSprite(ShadowSprite),
                new Vector3(0f, 0.02f, 0f), scale, -1, 0);
            shadow.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
        }

        private static void BuildPlayer()
        {
            CharacterController controller;
            GameObject root = CharacterRoot("Player", "Player", "Player", out controller);
            PlayerController player = root.AddComponent<PlayerController>();
            player.moveSpeed = 5f;
            player.attackInterval = 0.6f;
            player.attackRange = 2.2f;

            HitFeedback feedback = AddVisual(root, CharacterShape.Fox);
            AddShadow(root, 1.6f);
            SpriteRenderer range = EditorUtil.SpriteObject("AttackRange", root.transform, EditorUtil.LoadSprite(CircleSprite),
                new Vector3(0f, 0.03f, 0f), 1f, -2, 0);
            range.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            range.color = new Color(0.30f, 0.60f, 1f, 0.18f);
            range.transform.localScale = new Vector3(player.attackRange * 2f / 1.28f, player.attackRange * 2f / 1.28f, 1f);

            EditorUtil.SetField(player, "visual", feedback);
            EditorUtil.SetField(player, "attackRangeView", range.transform);
            EditorUtil.SetLayerRecursive(root, LayerMask.NameToLayer("Player"));
            EditorUtil.SavePrefab(root, PrefabFolder + "/Player.prefab");
        }

        private static void BuildMonsterBase()
        {
            CharacterController controller;
            GameObject root = CharacterRoot("Monster_Base", "Monster", "Monster", out controller);
            Monster monster = root.AddComponent<Monster>();
            monster.moveSpeed = 2f;
            monster.attackRange = 1.6f;
            monster.attackInterval = 1.5f;
            monster.hpBarHeight = 2.4f;

            HitFeedback feedback = AddVisual(root, CharacterShape.Crab);
            AddShadow(root, 1.5f);

            GameObject barObject = new GameObject("HPBar", typeof(RectTransform));
            barObject.transform.SetParent(root.transform, false);
            Canvas canvas = barObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            RectTransform barRect = barObject.GetComponent<RectTransform>();
            barRect.sizeDelta = new Vector2(120f, 16f);
            barRect.localScale = Vector3.one * 0.01f;
            barRect.localPosition = new Vector3(0f, monster.hpBarHeight, 0f);
            barObject.AddComponent<Billboard>();

            Image back = EditorUtil.Panel("HPBack", barObject.transform, new Color(0f, 0f, 0f, 0.65f), null);
            EditorUtil.Stretch(back.rectTransform, Vector2.zero, Vector2.zero);
            Image fill = EditorUtil.Panel("HPFill", barObject.transform, new Color(0.90f, 0.22f, 0.22f, 1f), null);
            EditorUtil.Stretch(fill.rectTransform, new Vector2(2f, 2f), new Vector2(-2f, -2f));
            fill.sprite = EditorUtil.LoadSprite(SolidBarSprite);
            fill.type = Image.Type.Filled;
            fill.fillMethod = Image.FillMethod.Horizontal;
            fill.fillAmount = 1f;

            EditorUtil.SetField(monster, "visual", feedback);
            EditorUtil.SetField(monster, "hpBar", barObject.transform);
            EditorUtil.SetField(monster, "hpFill", fill);
            EditorUtil.SetString(monster, "prefabKey", "");
            EditorUtil.SetLayerRecursive(root, LayerMask.NameToLayer("Monster"));
            EditorUtil.SavePrefab(root, PrefabFolder + "/Monster_Base.prefab");
        }

        private static void BuildVariants()
        {
            GameObject basePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabFolder + "/Monster_Base.prefab");
            foreach (VariantSpec spec in Variants)
            {
                GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(basePrefab);
                instance.name = spec.Name;
                Monster monster = instance.GetComponent<Monster>();
                monster.moveSpeed = spec.MoveSpeed;
                EditorUtil.SetString(monster, "prefabKey", spec.Name);

                CharacterModel model = instance.transform.Find("Visual").GetComponent<CharacterModel>();
                CharacterShape shape = ShapeOf(spec.Name);
                if (model.shape != shape)
                {
                    model.shape = shape;
                }
                Material body = CombatArt.BodyMaterial(shape);
                if (model.bodyMaterial != body)
                {
                    model.bodyMaterial = body;
                }
                model.ClearParts();
                PrefabUtility.RecordPrefabInstancePropertyModifications(model);
                if (shape == CharacterShape.Golem)
                {
                    monster.hpBarHeight = GolemHpBarHeight;
                }

                EditorUtil.SavePrefab(instance, MonsterFolder + "/" + spec.Name + ".prefab");
            }
        }

        private static void BuildDropItem()
        {
            GameObject root = new GameObject("DropItem");
            root.tag = "DropItem";
            SphereCollider sphere = root.AddComponent<SphereCollider>();
            sphere.isTrigger = true;
            sphere.radius = 0.45f;
            sphere.center = new Vector3(0f, 0.5f, 0f);
            Rigidbody body = root.AddComponent<Rigidbody>();
            body.isKinematic = true;
            body.useGravity = false;
            DropItem drop = root.AddComponent<DropItem>();

            GameObject bob = EditorUtil.Empty("Bob", root.transform, new Vector3(0f, 0.6f, 0f));
            SpriteRenderer icon = EditorUtil.SpriteObject("Icon", bob.transform,
                EditorUtil.LoadSprite("Assets/Resources/Icons/Item/item_201001.png"), Vector3.zero, 0.55f, 1, 0);
            icon.enabled = false;
            CombatArt.Drop(bob.transform);
            AddShadow(root, 0.7f);

            EditorUtil.SetField(drop, "icon", icon);
            EditorUtil.SetField(drop, "bobRoot", bob.transform);
            EditorUtil.SetLayerRecursive(root, LayerMask.NameToLayer("DropItem"));
            EditorUtil.SavePrefab(root, PrefabFolder + "/DropItem.prefab");
        }

        private static void BuildSpawnPoint()
        {
            GameObject root = new GameObject("SpawnPoint");
            SpawnPoint spawnPoint = root.AddComponent<SpawnPoint>();
            spawnPoint.spawnRadius = 1.5f;
            EditorUtil.SavePrefab(root, PrefabFolder + "/SpawnPoint.prefab");
        }

        private static void BuildUiParts()
        {
            RectTransform slot = UiRoot("UI_ItemSlot", new Vector2(200f, 240f));
            Image slotFrame = slot.gameObject.AddComponent<Image>();
            slotFrame.color = new Color(0.16f, 0.18f, 0.25f, 0.95f);
            Image slotIcon = EditorUtil.IconImage("Icon", slot, new Vector2(140f, 140f));
            EditorUtil.Place(slotIcon.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -14f), new Vector2(140f, 140f));
            TMP_Text slotName = EditorUtil.Text("Name", slot, "아이템", 26f, TextAlignmentOptions.Center);
            EditorUtil.Place(slotName.rectTransform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 40f), new Vector2(190f, 50f));
            TMP_Text slotCount = EditorUtil.Text("Count", slot, "x0", 28f, TextAlignmentOptions.Center);
            EditorUtil.Place(slotCount.rectTransform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 6f), new Vector2(190f, 36f));
            ItemSlotView slotView = slot.gameObject.AddComponent<ItemSlotView>();
            EditorUtil.SetField(slotView, "icon", slotIcon);
            EditorUtil.SetField(slotView, "nameText", slotName);
            EditorUtil.SetField(slotView, "countText", slotCount);
            EditorUtil.SavePrefab(slot.gameObject, UiFolder + "/UI_ItemSlot.prefab");

            RectTransform card = UiRoot("UI_DexCard", new Vector2(190f, 230f));
            Image cardFrame = card.gameObject.AddComponent<Image>();
            cardFrame.color = new Color(0.6f, 0.6f, 0.65f, 1f);
            Image cardInner = EditorUtil.Panel("Inner", card, new Color(0.12f, 0.13f, 0.18f, 1f), null);
            EditorUtil.Stretch(cardInner.rectTransform, new Vector2(6f, 6f), new Vector2(-6f, -6f));
            cardInner.raycastTarget = false;
            Image cardIcon = EditorUtil.IconImage("Icon", card, new Vector2(150f, 150f));
            EditorUtil.Place(cardIcon.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -14f), new Vector2(150f, 150f));
            TMP_Text cardName = EditorUtil.Text("Name", card, "???", 26f, TextAlignmentOptions.Center);
            EditorUtil.Place(cardName.rectTransform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 12f), new Vector2(180f, 50f));
            DexCardView cardView = card.gameObject.AddComponent<DexCardView>();
            EditorUtil.SetField(cardView, "icon", cardIcon);
            EditorUtil.SetField(cardView, "nameText", cardName);
            EditorUtil.SetField(cardView, "frame", cardFrame);
            EditorUtil.SavePrefab(card.gameObject, UiFolder + "/UI_DexCard.prefab");

            RectTransform shopRow = UiRoot("UI_ShopSlot", new Vector2(1040f, 110f));
            Image shopFrame = shopRow.gameObject.AddComponent<Image>();
            shopFrame.color = new Color(0.16f, 0.18f, 0.25f, 0.95f);
            Image shopIcon = EditorUtil.IconImage("Icon", shopRow, new Vector2(90f, 90f));
            EditorUtil.Place(shopIcon.rectTransform, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(16f, 0f), new Vector2(90f, 90f));
            TMP_Text shopName = EditorUtil.Text("Name", shopRow, "아이템", 32f, TextAlignmentOptions.Left);
            EditorUtil.Place(shopName.rectTransform, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(130f, 0f), new Vector2(480f, 60f));
            TMP_Text shopPrice = EditorUtil.Text("Price", shopRow, "0", 32f, TextAlignmentOptions.Right);
            EditorUtil.Place(shopPrice.rectTransform, new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(-230f, 0f), new Vector2(200f, 60f));
            Button buy = EditorUtil.Button("Btn_Buy", "Txt_Buy", shopRow, "구매", new Vector2(180f, 80f));
            EditorUtil.Place((RectTransform)buy.transform, new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(-20f, 0f), new Vector2(180f, 80f));
            ShopSlotView shopView = shopRow.gameObject.AddComponent<ShopSlotView>();
            EditorUtil.SetField(shopView, "icon", shopIcon);
            EditorUtil.SetField(shopView, "nameText", shopName);
            EditorUtil.SetField(shopView, "priceText", shopPrice);
            EditorUtil.SetField(shopView, "buyButton", buy);
            EditorUtil.SavePrefab(shopRow.gameObject, UiFolder + "/UI_ShopSlot.prefab");

            RectTransform stageButtonRoot = UiRoot("UI_StageButton", new Vector2(520f, 96f));
            Button stageButton = BuildButtonOn(stageButtonRoot, "Label", "1-1");
            EditorUtil.SavePrefab(stageButton.gameObject, UiFolder + "/UI_StageButton.prefab");
        }

        private static Button BuildButtonOn(RectTransform root, string labelName, string label)
        {
            Image image = root.gameObject.AddComponent<Image>();
            Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/ThirdParty/Kenney/Resources/Kenney/button_blue_rectangle_depth_gloss.png");
            image.sprite = sprite;
            image.type = Image.Type.Sliced;
            Button button = root.gameObject.AddComponent<Button>();
            button.targetGraphic = image;
            TMP_Text text = EditorUtil.Text(labelName, root, label, 32f, TextAlignmentOptions.Center);
            EditorUtil.Stretch(text.rectTransform, new Vector2(10f, 8f), new Vector2(-10f, -4f));
            return button;
        }

        private static RectTransform UiRoot(string name, Vector2 size)
        {
            GameObject root = new GameObject(name, typeof(RectTransform));
            root.layer = LayerMask.NameToLayer("UI");
            RectTransform rect = root.GetComponent<RectTransform>();
            rect.sizeDelta = size;
            return rect;
        }

        private class PopupParts
        {
            public RectTransform Root;
            public RectTransform Panel;
            public Button Close;
        }

        private static PopupParts PopupFrame(string key, string title, Vector2 panelSize, string background, bool withClose)
        {
            PopupParts parts = new PopupParts();
            parts.Root = UiRoot("Popup_" + key, Vector2.zero);
            EditorUtil.Stretch(parts.Root, Vector2.zero, Vector2.zero);
            Image dim = parts.Root.gameObject.AddComponent<Image>();
            dim.color = new Color(0f, 0f, 0f, 0.6f);

            Image panel = EditorUtil.Panel("Panel_" + key, parts.Root, EditorUtil.DarkPanel, "panel_grey_square_depth_flat");
            if (!string.IsNullOrEmpty(background))
            {
                panel.sprite = EditorUtil.LoadSprite(background);
                panel.type = Image.Type.Simple;
                panel.color = new Color(0.55f, 0.55f, 0.6f, 1f);
            }
            else
            {
                panel.color = new Color(0.22f, 0.24f, 0.32f, 1f);
            }
            parts.Panel = panel.rectTransform;
            EditorUtil.Place(parts.Panel, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, panelSize);

            TMP_Text titleText = EditorUtil.Text("Txt_" + key + "Title", parts.Panel, title, 48f, TextAlignmentOptions.Center);
            EditorUtil.Place(titleText.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -20f), new Vector2(panelSize.x - 200f, 70f));
            titleText.fontStyle = FontStyles.Bold;

            if (withClose)
            {
                parts.Close = EditorUtil.Button("Btn_" + key + "Close", "Txt_" + key + "Close", parts.Panel, "닫기", new Vector2(160f, 80f));
                EditorUtil.Place((RectTransform)parts.Close.transform, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-20f, -20f), new Vector2(160f, 80f));
            }
            return parts;
        }

        private static RectTransform ScrollList(string key, RectTransform panel, Vector2 offsetMin, Vector2 offsetMax,
            Vector2 cellSize, int columns, bool vertical)
        {
            RectTransform scrollRect = EditorUtil.Rect("Scroll_" + key, panel);
            EditorUtil.Stretch(scrollRect, offsetMin, offsetMax);
            ScrollRect scroll = scrollRect.gameObject.AddComponent<ScrollRect>();
            scroll.horizontal = false;
            scroll.vertical = true;
            scroll.movementType = ScrollRect.MovementType.Clamped;
            scroll.scrollSensitivity = 40f;

            RectTransform viewport = EditorUtil.Rect("Viewport_" + key, scrollRect);
            EditorUtil.Stretch(viewport, Vector2.zero, Vector2.zero);
            Image viewportImage = viewport.gameObject.AddComponent<Image>();
            viewportImage.color = new Color(1f, 1f, 1f, 0.01f);
            viewport.gameObject.AddComponent<RectMask2D>();

            RectTransform content = EditorUtil.Rect("List_" + key, viewport);
            content.anchorMin = new Vector2(0f, 1f);
            content.anchorMax = new Vector2(1f, 1f);
            content.pivot = new Vector2(0.5f, 1f);
            content.anchoredPosition = Vector2.zero;
            content.sizeDelta = new Vector2(0f, 0f);
            GridLayoutGroup grid = content.gameObject.AddComponent<GridLayoutGroup>();
            grid.cellSize = cellSize;
            grid.spacing = new Vector2(16f, 16f);
            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            grid.constraintCount = columns;
            grid.childAlignment = TextAnchor.UpperCenter;
            grid.padding = new RectOffset(10, 10, 10, 10);
            ContentSizeFitter fitter = content.gameObject.AddComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            scroll.viewport = viewport;
            scroll.content = content;
            return content;
        }

        private static void BuildPopups()
        {
            ItemSlotView itemSlot = AssetDatabase.LoadAssetAtPath<ItemSlotView>(UiFolder + "/UI_ItemSlot.prefab");
            DexCardView dexCard = AssetDatabase.LoadAssetAtPath<DexCardView>(UiFolder + "/UI_DexCard.prefab");
            ShopSlotView shopSlot = AssetDatabase.LoadAssetAtPath<ShopSlotView>(UiFolder + "/UI_ShopSlot.prefab");
            Button stageButton = AssetDatabase.LoadAssetAtPath<Button>(UiFolder + "/UI_StageButton.prefab");

            PopupParts bag = PopupFrame("Bag", "가방", new Vector2(1400f, 860f), null, true);
            RectTransform bagList = ScrollList("Bag", bag.Panel, new Vector2(40f, 40f), new Vector2(-40f, -120f), new Vector2(200f, 240f), 6, true);
            TMP_Text bagEmpty = EditorUtil.Text("Txt_BagEmpty", bag.Panel, "가방이 비어 있습니다", 36f, TextAlignmentOptions.Center);
            EditorUtil.Place(bagEmpty.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(800f, 80f));
            BagPopup bagPopup = bag.Root.gameObject.AddComponent<BagPopup>();
            EditorUtil.SetField(bagPopup, "closeButton", bag.Close);
            EditorUtil.SetField(bagPopup, "listRoot", bagList);
            EditorUtil.SetField(bagPopup, "slotPrefab", itemSlot);
            EditorUtil.SetField(bagPopup, "emptyText", bagEmpty);
            EditorUtil.SavePrefab(bag.Root.gameObject, UiFolder + "/UI_Popup_Bag.prefab");

            PopupParts dex = PopupFrame("Dex", "도감", new Vector2(1500f, 900f), null, true);
            RectTransform dexList = ScrollList("Dex", dex.Panel, new Vector2(40f, 40f), new Vector2(-40f, -150f), new Vector2(190f, 230f), 7, true);
            TMP_Text dexCount = EditorUtil.Text("Txt_DexCount", dex.Panel, "등록 0 / 0", 32f, TextAlignmentOptions.Left);
            EditorUtil.Place(dexCount.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(40f, -96f), new Vector2(500f, 50f));
            DexPopup dexPopup = dex.Root.gameObject.AddComponent<DexPopup>();
            EditorUtil.SetField(dexPopup, "closeButton", dex.Close);
            EditorUtil.SetField(dexPopup, "listRoot", dexList);
            EditorUtil.SetField(dexPopup, "cardPrefab", dexCard);
            EditorUtil.SetField(dexPopup, "countText", dexCount);
            EditorUtil.SavePrefab(dex.Root.gameObject, UiFolder + "/UI_Popup_Dex.prefab");

            PopupParts shop = PopupFrame("Shop", "상점", new Vector2(1200f, 900f), null, true);
            RectTransform shopList = ScrollList("Shop", shop.Panel, new Vector2(40f, 90f), new Vector2(-40f, -150f), new Vector2(1080f, 110f), 1, true);
            TMP_Text shopGold = EditorUtil.Text("Txt_ShopGold", shop.Panel, "보유 골드 0", 32f, TextAlignmentOptions.Left);
            EditorUtil.Place(shopGold.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(40f, -96f), new Vector2(600f, 50f));
            TMP_Text shopMessage = EditorUtil.Text("Txt_ShopMessage", shop.Panel, "", 30f, TextAlignmentOptions.Center);
            EditorUtil.Place(shopMessage.rectTransform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 24f), new Vector2(1000f, 50f));
            ShopPopup shopPopup = shop.Root.gameObject.AddComponent<ShopPopup>();
            EditorUtil.SetField(shopPopup, "closeButton", shop.Close);
            EditorUtil.SetField(shopPopup, "listRoot", shopList);
            EditorUtil.SetField(shopPopup, "slotPrefab", shopSlot);
            EditorUtil.SetField(shopPopup, "goldText", shopGold);
            EditorUtil.SetField(shopPopup, "messageText", shopMessage);
            EditorUtil.SavePrefab(shop.Root.gameObject, UiFolder + "/UI_Popup_Shop.prefab");

            PopupParts stage = PopupFrame("StageSelect", "출정 게시판 · 1챕터", new Vector2(1240f, 820f), null, true);
            RectTransform stageList = ScrollList("StageSelect", stage.Panel, new Vector2(40f, 40f), new Vector2(-40f, -120f), new Vector2(540f, 100f), 2, true);
            StageSelectPopup stagePopup = stage.Root.gameObject.AddComponent<StageSelectPopup>();
            EditorUtil.SetField(stagePopup, "closeButton", stage.Close);
            EditorUtil.SetField(stagePopup, "listRoot", stageList);
            EditorUtil.SetField(stagePopup, "stageButtonPrefab", stageButton);
            EditorUtil.SavePrefab(stage.Root.gameObject, UiFolder + "/UI_Popup_StageSelect.prefab");

            PopupParts settings = PopupFrame("Settings", "설정", new Vector2(1000f, 560f), null, true);
            TMP_Text settingsInfo = EditorUtil.Text("Txt_SettingsInfo", settings.Panel, "", 30f, TextAlignmentOptions.Center);
            EditorUtil.Stretch(settingsInfo.rectTransform, new Vector2(40f, 40f), new Vector2(-40f, -120f));
            SettingsPopup settingsPopup = settings.Root.gameObject.AddComponent<SettingsPopup>();
            EditorUtil.SetField(settingsPopup, "closeButton", settings.Close);
            EditorUtil.SetField(settingsPopup, "infoText", settingsInfo);
            EditorUtil.SavePrefab(settings.Root.gameObject, UiFolder + "/UI_Popup_Settings.prefab");

            PopupParts result = PopupFrame("Result", "스테이지 클리어", new Vector2(1300f, 820f), null, false);
            TMP_Text resultGold = EditorUtil.Text("Txt_ResultGold", result.Panel, "골드 +0", 36f, TextAlignmentOptions.Left);
            EditorUtil.Place(resultGold.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(60f, -110f), new Vector2(500f, 56f));
            TMP_Text resultExp = EditorUtil.Text("Txt_ResultExp", result.Panel, "경험치 +0", 36f, TextAlignmentOptions.Left);
            EditorUtil.Place(resultExp.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(600f, -110f), new Vector2(500f, 56f));
            RectTransform resultList = ScrollList("Result", result.Panel, new Vector2(40f, 140f), new Vector2(-40f, -190f), new Vector2(200f, 240f), 5, true);
            TMP_Text resultEmpty = EditorUtil.Text("Txt_ResultEmpty", result.Panel, "획득한 아이템이 없습니다", 34f, TextAlignmentOptions.Center);
            EditorUtil.Place(resultEmpty.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(900f, 80f));
            Button confirm = EditorUtil.Button("Btn_ResultConfirm", "Txt_ResultConfirm", result.Panel, "확인", new Vector2(320f, 100f));
            EditorUtil.Place((RectTransform)confirm.transform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 24f), new Vector2(320f, 100f));
            ResultPopup resultPopup = result.Root.gameObject.AddComponent<ResultPopup>();
            EditorUtil.SetField(resultPopup, "titleText", result.Panel.Find("Txt_ResultTitle").GetComponent<TMP_Text>());
            EditorUtil.SetField(resultPopup, "goldText", resultGold);
            EditorUtil.SetField(resultPopup, "expText", resultExp);
            EditorUtil.SetField(resultPopup, "emptyText", resultEmpty);
            EditorUtil.SetField(resultPopup, "listRoot", resultList);
            EditorUtil.SetField(resultPopup, "slotPrefab", itemSlot);
            EditorUtil.SetField(resultPopup, "confirmButton", confirm);
            ResultLayout(result.Root);
            EditorUtil.SavePrefab(result.Root.gameObject, UiFolder + "/UI_Popup_Result.prefab");
        }


        public static void ResultLayout(Transform root)
        {
            root.GetComponent<Image>().color = new Color(0.025f, 0.055f, 0.05f, 0.86f);
            RectTransform panel = (RectTransform)root.Find("Panel_Result");
            Image panelImage = panel.GetComponent<Image>();
            panelImage.sprite = null;
            panelImage.type = Image.Type.Simple;
            panelImage.color = UiTheme.Ink;
            float width = panel.sizeDelta.x;
            float cardWidth = (width - 136f) / 2f;

            ResultBox("ResultReward", panel, Vector2.zero, Vector2.zero, UiTheme.Panel);
            EditorUtil.Stretch((RectTransform)panel.Find("ResultReward"), new Vector2(50f, 166f), new Vector2(-50f, -370f));
            ResultBox("ResultExpCard", panel, new Vector2(76f + cardWidth, -180f), new Vector2(cardWidth, 100f), UiTheme.Panel);
            ResultBox("ResultGoldCard", panel, new Vector2(60f, -180f), new Vector2(cardWidth, 100f), UiTheme.Panel);
            ResultBox("ResultTopRule", panel, Vector2.zero, new Vector2(width, 6f), UiTheme.Gold);
            TMP_Text eyebrow = ResultLabel("Txt_ResultEyebrow", panel, "EXPEDITION REPORT", 19f, UiTheme.Gold);
            eyebrow.characterSpacing = 30f;
            ResultPlace(eyebrow.transform, new Vector2(60f, -32f), new Vector2(width - 120f, 30f));
            TMP_Text heading = ResultLabel("Txt_ResultHeading", panel, "획득 아이템", 24f, UiTheme.Muted);
            ResultPlace(heading.transform, new Vector2(60f, -315f), new Vector2(width - 120f, 40f));

            TMP_Text title = panel.Find("Txt_ResultTitle").GetComponent<TMP_Text>();
            ResultPlace(title.transform, new Vector2(60f, -80f), new Vector2(width - 120f, 76f));
            title.alignment = TextAlignmentOptions.Left;
            title.fontSize = 52f;
            title.color = UiTheme.Cream;
            TMP_Text gold = panel.Find("Txt_ResultGold").GetComponent<TMP_Text>();
            ResultPlace(gold.transform, new Vector2(84f, -206f), new Vector2(cardWidth - 48f, 48f));
            gold.color = UiTheme.Gold;
            TMP_Text exp = panel.Find("Txt_ResultExp").GetComponent<TMP_Text>();
            ResultPlace(exp.transform, new Vector2(100f + cardWidth, -206f), new Vector2(cardWidth - 48f, 48f));
            exp.color = UiTheme.Cream;

            EditorUtil.Stretch((RectTransform)panel.Find("Scroll_Result"), new Vector2(50f, 166f), new Vector2(-50f, -370f));
            GridLayoutGroup grid = panel.Find("Scroll_Result/Viewport_Result/List_Result").GetComponent<GridLayoutGroup>();
            grid.cellSize = new Vector2(190f, 230f);
            grid.spacing = new Vector2(16f, 16f);
            TMP_Text empty = panel.Find("Txt_ResultEmpty").GetComponent<TMP_Text>();
            ResultPlace(empty.transform, new Vector2(60f, -430f), new Vector2(width - 120f, 80f));
            empty.fontSize = 27f;
            empty.color = UiTheme.Cream;

            Button confirm = panel.Find("Btn_ResultConfirm").GetComponent<Button>();
            EditorUtil.Place((RectTransform)confirm.transform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 36f), new Vector2(460f, 84f));
            Image confirmImage = confirm.GetComponent<Image>();
            confirmImage.sprite = null;
            confirmImage.type = Image.Type.Simple;
            confirmImage.color = Color.white;
            ColorBlock colors = confirm.colors;
            colors.normalColor = UiTheme.Gold;
            colors.highlightedColor = new Color(0.89f, 0.75f, 0.47f, 1f);
            colors.selectedColor = colors.highlightedColor;
            colors.pressedColor = UiTheme.Cream;
            colors.fadeDuration = 0.12f;
            confirm.colors = colors;
            TMP_Text confirmText = confirm.transform.Find("Txt_ResultConfirm").GetComponent<TMP_Text>();
            confirmText.text = "보상 받고 캠프로";
            confirmText.color = UiTheme.Ink;

            ResultPopup popup = root.GetComponent<ResultPopup>();
            EditorUtil.SetField(popup, "panel", panel);
            EditorUtil.SetField(popup, "confirmText", confirmText);
        }

        private static void ResultBox(string name, RectTransform panel, Vector2 position, Vector2 size, Color color)
        {
            Transform found = panel.Find(name);
            Image box = found != null ? found.GetComponent<Image>() : EditorUtil.Panel(name, panel, color, null);
            box.color = color;
            box.raycastTarget = false;
            box.transform.SetAsFirstSibling();
            ResultPlace(box.transform, position, size);
        }

        private static TMP_Text ResultLabel(string name, RectTransform panel, string content, float size, Color color)
        {
            Transform found = panel.Find(name);
            TMP_Text text = found != null ? found.GetComponent<TMP_Text>() : EditorUtil.Text(name, panel, content, size, TextAlignmentOptions.TopLeft);
            text.text = content;
            text.fontSize = size;
            text.color = color;
            return text;
        }

        private static void ResultPlace(Transform target, Vector2 position, Vector2 size)
        {
            EditorUtil.Place((RectTransform)target, new Vector2(0f, 1f), new Vector2(0f, 1f), position, size);
        }

        private static void BuildNpcs()
        {
            BuildNpc("NPC_Shop", "Txt_NpcShop", "상점 주인", "Assets/Sprites/NPC/npc_shop.png");
            BuildNpc("NPC_Dex", "Txt_NpcDex", "도감 관리인", "Assets/Sprites/NPC/npc_dex.png");
            BuildNpc("NPC_Board", "Txt_NpcBoard", "출정 게시판", "Assets/Sprites/NPC/npc_board.png");
        }

        private static void BuildNpc(string name, string labelName, string label, string spritePath)
        {
            GameObject root = new GameObject(name);
            SpriteRenderer renderer = root.AddComponent<SpriteRenderer>();
            renderer.sprite = EditorUtil.LoadSprite(spritePath);
            renderer.sortingOrder = 5;
            BoxCollider2D box = root.AddComponent<BoxCollider2D>();
            box.size = new Vector2(1.8f, 2.3f);
            box.offset = new Vector2(0f, -0.05f);
            root.AddComponent<NpcClick>();

            GameObject labelObject = new GameObject(labelName);
            labelObject.transform.SetParent(root.transform, false);
            labelObject.transform.localPosition = new Vector3(0f, -1.45f, 0f);
            TextMeshPro text = labelObject.AddComponent<TextMeshPro>();
            text.font = EditorUtil.Font;
            text.text = label;
            text.fontSize = 4.2f;
            text.alignment = TextAlignmentOptions.Center;
            text.color = Color.white;
            text.fontStyle = FontStyles.Bold;
            text.rectTransform.sizeDelta = new Vector2(4f, 1f);
            text.sortingOrder = 6;

            EditorUtil.SavePrefab(root, NpcFolder + "/" + name + ".prefab");
        }
    }
}
