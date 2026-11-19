using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace ExpeditionCamp.EditorTools
{
    // Presentation-only pass: serialized gameplay references and data remain intact.
    public static class VisualRedesign
    {
        static readonly Color Ink = new Color(.065f,.14f,.14f,1);
        static readonly Color Panel = new Color(.105f,.205f,.20f,1);
        static readonly Color Cream = new Color(.96f,.92f,.80f,1);
        static readonly Color Gold = new Color(.79f,.62f,.32f,1);
        static readonly Color Muted = new Color(.64f,.75f,.69f,1);

        [MenuItem("ExpeditionCamp/Rebuild Redesigned Game")]
        public static void Build()
        {
            ProjectSetup.Run();
            PrefabBuilder.BuildAll();
            GameSceneBuilder.BuildAll();
            ApplyScenes();
        }

        static void SkinButton(Button button)
        {
            Image img=button.GetComponent<Image>();
            if(img!=null) { img.sprite=null; img.color=Color.white; }
            ColorBlock c=button.colors;
            c.normalColor=Panel; c.highlightedColor=new Color(.19f,.34f,.31f);
            c.pressedColor=Gold; c.selectedColor=c.highlightedColor;
            c.disabledColor=new Color(.12f,.17f,.17f,.55f); c.fadeDuration=.12f;
            button.colors=c;
            foreach(TMP_Text t in button.GetComponentsInChildren<TMP_Text>(true)) t.color=Cream;
        }

        static Image Box(string name, Transform parent, Vector2 pos, Vector2 size, Color color)
        {
            Image i=EditorUtil.Panel(name,parent,color,null);
            EditorUtil.Place(i.rectTransform,new Vector2(0,1),new Vector2(0,1),pos,size);
            i.raycastTarget=false;
            return i;
        }
        static TMP_Text Label(string name, Transform parent,string value,float size,Vector2 pos,Vector2 area,Color color)
        {
            TMP_Text t=EditorUtil.Text(name,parent,value,size,TextAlignmentOptions.TopLeft);
            EditorUtil.Place(t.rectTransform,new Vector2(0,1),new Vector2(0,1),pos,area);
            t.color=color; return t;
        }
        static Transform Find(Transform root,string name)
        {
            foreach(Transform t in root.GetComponentsInChildren<Transform>(true)) if(t.name==name)return t;
            return null;
        }
        static void Place(Transform root,string name, Vector2 pos,Vector2 size)
        {
            Transform t=Find(root,name);
            if(t!=null) EditorUtil.Place((RectTransform)t,new Vector2(0,1),new Vector2(0,1),pos,size);
        }

        public static void ApplyScenes()
        {
            foreach(string sceneName in GameSceneBuilder.GameScenes)
            {
                var scene=EditorSceneManager.OpenScene(GameSceneBuilder.ScenePath(sceneName));
                Canvas canvas=GameObject.Find("Canvas").GetComponent<Canvas>();
                canvas.GetComponent<CanvasScaler>().matchWidthOrHeight=.5f;
                if(sceneName=="Lobby") Lobby(canvas.transform);
                if(sceneName.StartsWith("Field")) Field(canvas.transform,sceneName=="Field_02");
                foreach(var sceneRoot in scene.GetRootGameObjects())
                    foreach(var component in sceneRoot.GetComponentsInChildren<Component>(true))
                        if(component!=null && PrefabUtility.IsPartOfPrefabInstance(component)) PrefabUtility.RecordPrefabInstancePropertyModifications(component);
                EditorSceneManager.SaveScene(scene);
            }
            // Isolate saved progress from the original teaching project.
            AssetDatabase.SaveAssets();
            EditorSceneManager.OpenScene(GameSceneBuilder.ScenePath("Lobby"));
        }

        public const string LobbyBackdropSprite="Assets/Art/lobby_backdrop.png";

        public static void ApplyLobbyBackdrop(SpriteRenderer background)
        {
            background.sprite=EditorUtil.LoadSprite(LobbyBackdropSprite);
            background.color=Color.white;
            background.transform.position=Vector3.zero;
            background.transform.localScale=Vector3.one;
        }

        static void Lobby(Transform canvas)
        {
            ApplyLobbyBackdrop(GameObject.Find("Background").GetComponent<SpriteRenderer>());
            Transform npcs=GameObject.Find("NPCs").transform;
            string[] npcNames={"NPC_Shop","NPC_Dex","NPC_Board"};
            // 왼쪽 사이드바(620px)를 뺀 영역에 맞춰 놓습니다.
            float[] npcX={0.3f,2.9f,5.5f};
            for(int i=0;i<npcNames.Length;i++)
            {
                Transform npc=npcs.Find(npcNames[i]);
                npc.position=new Vector3(npcX[i],i==1?-2.3f:-2.0f,0f);
                npc.localScale=Vector3.one;
            }
            Image left=Box("JournalSidebar",canvas,Vector2.zero,new Vector2(620,1080),Ink);
            left.transform.SetAsFirstSibling();
            Box("GoldRule",left.transform,new Vector2(48,-62),new Vector2(52,4),Gold);
            Label("Edition",left.transform,"E X P E D I T I O N   /   0 1",20,new Vector2(48,-87),new Vector2(520,35),Gold);
            Label("CampTitle",left.transform,"원정대\n캠프",82,new Vector2(44,-150),new Vector2(540,220),Cream).fontStyle=FontStyles.Bold;
            Label("CampIntro",left.transform,"작은 발걸음으로 시작하는\n새로운 모험의 기록.",27,new Vector2(48,-402),new Vector2(510,100),Muted);
            Box("SidebarRule",left.transform,new Vector2(48,-550),new Vector2(518,1),new Color(.28f,.38f,.33f));
            Label("CompanionCaption",left.transform,"YOUR COMPANION",17,new Vector2(48,-584),new Vector2(460,35),Gold);
            Label("CompanionName",left.transform,"불꽃여우",34,new Vector2(48,-626),new Vector2(500,65),Cream);
            Label("CompanionHint",left.transform,"출정할 스테이지를 선택하고\n필드에서 몬스터를 만나 보세요.",23,new Vector2(48,-699),new Vector2(500,85),Muted);
            Label("Footer",left.transform,"FIELD NOTES   /   CHAPTER 01",16,new Vector2(48,-1000),new Vector2(500,30),Muted);

            Transform top=Find(canvas,"Panel_TopBar");
            top.GetComponent<Image>().color=Ink;
            EditorUtil.Place((RectTransform)top,new Vector2(1,1),new Vector2(1,1),new Vector2(-48,-42),new Vector2(510,78));
            Place(top,"Txt_Level",new Vector2(24,-16),new Vector2(150,48));
            Place(top,"Txt_Gold",new Vector2(196,-16),new Vector2(290,48));
            foreach(TMP_Text t in top.GetComponentsInChildren<TMP_Text>())t.fontSize=28;

            Button departure=EditorUtil.Button("Btn_Departure","Txt_Departure",canvas,"",new Vector2(510,270));
            // Lv · 골드 막대 바로 아래에 같은 너비 · 같은 오른쪽 여백으로 붙입니다.
            EditorUtil.Place((RectTransform)departure.transform,new Vector2(1,1),new Vector2(1,1),new Vector2(-48,-144),new Vector2(510,270));
            SkinButton(departure);
            Box("DepartureAccent",departure.transform,Vector2.zero,new Vector2(510,4),Gold);
            Label("DepartureEyebrow",departure.transform,"CHAPTER 01   /   EXPLORE",18,new Vector2(32,-30),new Vector2(450,40),Gold);
            Label("DepartureTitle",departure.transform,"초원으로 떠나기",39,new Vector2(32,-90),new Vector2(460,65),Cream);
            Label("DepartureDetail",departure.transform,"스테이지 선택     →",25,new Vector2(32,-196),new Vector2(440,45),Muted);
            EditorUtil.AddClick(departure,Find(canvas,"Popup_StageSelect").GetComponent<PopupBase>().Open);
            foreach(string name in new[]{"Btn_Bag","Btn_Dex","Btn_Shop"})
            {
                Transform b=Find(canvas,name);
                int index=name=="Btn_Bag"?0:name=="Btn_Dex"?1:2;
                EditorUtil.Place((RectTransform)b,new Vector2(0,0),new Vector2(0,0),new Vector2(48+index*176,122),new Vector2(160,85));
                b.GetComponentInChildren<TMP_Text>().fontSize=26;
            }
            Transform settings=Find(canvas,"Btn_Settings");
            EditorUtil.Place((RectTransform)settings,new Vector2(1,0),new Vector2(1,0),new Vector2(-48,42),new Vector2(150,62));
            settings.GetComponentInChildren<TMP_Text>().fontSize=23;
            // All modal roots render over the new presentation layers.
            foreach(Transform child in new List<Transform>(Children(canvas)))
                if(child.name.StartsWith("Popup_"))child.SetAsLastSibling();
        }
        static IEnumerable<Transform> Children(Transform t) { foreach(Transform c in t)yield return c; }

        static Material Material(string name,Color color)
        { return EditorUtil.LitMaterial("Assets/Materials/"+name+".mat",color); }
        static GameObject Decor(string name,Transform parent,PrimitiveType type,Vector3 pos,Vector3 scale,Material mat)
        {
            GameObject g=EditorUtil.Primitive(type,name,parent,pos,scale,mat,0);
            Object.DestroyImmediate(g.GetComponent<Collider>());g.isStatic=true;return g;
        }
        // Flat-shaded, eight-sided cone stored as a reusable mesh asset.
        static Mesh CrownMesh()
        {
            const string path="Assets/Art/Canopy.asset";
            Mesh mesh=AssetDatabase.LoadAssetAtPath<Mesh>(path);
            if(mesh!=null)return mesh;
            var vertices=new List<Vector3>(); var tris=new List<int>();
            for(int i=0;i<8;i++)
            {
                float a=i*Mathf.PI/4,b=(i+1)*Mathf.PI/4;
                vertices.Add(new Vector3(Mathf.Cos(a),0,Mathf.Sin(a)));
                vertices.Add(new Vector3(0,1.8f,0));
                vertices.Add(new Vector3(Mathf.Cos(b),0,Mathf.Sin(b)));
                tris.Add(i*3);tris.Add(i*3+1);tris.Add(i*3+2);
            }
            mesh=new Mesh{name="Eight sided forest canopy"};mesh.SetVertices(vertices);mesh.SetTriangles(tris,0);mesh.RecalculateNormals();
            AssetDatabase.CreateAsset(mesh,path);return mesh;
        }
        static Mesh RockMesh()
        {
            const string path="Assets/Art/FacetedRock.asset";
            Mesh mesh=AssetDatabase.LoadAssetAtPath<Mesh>(path);
            if(mesh!=null)return mesh;
            var v=new List<Vector3>();var tr=new List<int>();
            Vector3 top=new Vector3(.08f,.5f,-.06f), bottom=new Vector3(0,-.5f,0);
            for(int i=0;i<7;i++)
            {
                float a=i*Mathf.PI*2/7,b=(i+1)*Mathf.PI*2/7;
                Vector3 p=new Vector3(Mathf.Cos(a)*.54f, .03f+Mathf.Sin(a*2)*.08f,Mathf.Sin(a)*.52f);
                Vector3 q=new Vector3(Mathf.Cos(b)*.54f, .03f+Mathf.Sin(b*2)*.08f,Mathf.Sin(b)*.52f);
                int n=v.Count;v.Add(p);v.Add(top);v.Add(q);v.Add(p);v.Add(q);v.Add(bottom);
                for(int j=0;j<6;j++)tr.Add(n+j);
            }
            mesh=new Mesh{name="Faceted seven sided boulder"};mesh.SetVertices(v);mesh.SetTriangles(tr,0);mesh.RecalculateNormals();
            AssetDatabase.CreateAsset(mesh,path);return mesh;
        }
        static void Field(Transform canvas,bool cave)
        {
            RenderSettings.ambientSkyColor=new Color(.75f,.80f,.73f);
            RenderSettings.ambientEquatorColor=new Color(.45f,.53f,.47f);
            RenderSettings.ambientGroundColor=new Color(.27f,.31f,.25f);
            Camera.main.backgroundColor=new Color(.36f,.47f,.42f);
            Material grass=Material("Ground",new Color(.35f,.46f,.28f));
            Material leaf=Material("TreeLeaves",new Color(.16f,.34f,.26f));
            Material leafLight=Material("CanopyLight",new Color(.30f,.47f,.29f));
            Material stone=Material("Rock",new Color(.51f,.55f,.46f));
            Material path=cave?Material("Trail_Cave",new Color(.44f,.43f,.36f)):Material("Trail_Forest",new Color(.58f,.55f,.37f));
            Material pebble=Material("Pebble",new Color(.51f,.55f,.46f));
            Material wood=Material("Bridge",new Color(.42f,.29f,.17f));
            Material water=Material("River",new Color(.20f,.47f,.49f));
            Transform env=GameObject.Find("Environment").transform;
            Transform detail=new GameObject("Art_Details").transform;detail.SetParent(env,false);
            foreach(Transform rock in env.GetComponentsInChildren<Transform>())
                if(rock.name.StartsWith("Rock_") && rock.GetComponent<MeshFilter>()!=null)
                    rock.GetComponent<MeshFilter>().sharedMesh=RockMesh();
            Transform range=GameObject.Find("Player").transform.Find("AttackRange");
            if(range!=null)range.GetComponent<SpriteRenderer>().color=new Color(.72f,.82f,.62f,.055f);
            if(!cave)
            {
                foreach(Transform t in env.GetComponentsInChildren<Transform>())
                {
                    if(!t.name.EndsWith("_Leaves"))continue;
                    t.GetComponent<MeshFilter>().sharedMesh=CrownMesh();
                    t.localScale=new Vector3(1.8f,1.5f,1.8f);
                    t.localPosition-=Vector3.up*1.6f;
                    var tier=new GameObject(t.parent.name+"_CanopyUpper");tier.transform.SetParent(t.parent,false);
                    tier.transform.localPosition=t.localPosition+Vector3.up*1.4f;
                    tier.transform.localScale=new Vector3(1.25f,1.3f,1.25f);
                    tier.AddComponent<MeshFilter>().sharedMesh=CrownMesh();tier.AddComponent<MeshRenderer>().sharedMaterial=leafLight;
                }
                for(int i=0;i<13;i++)
                    Decor("Bridge_Plank_"+i,detail,PrimitiveType.Cube,new Vector3(0,.26f,3+i*.33f),new Vector3(3.55f,.12f,.28f),wood);
                foreach(float x in new[]{-1.65f,1.65f})
                {
                    string side=x<0?"L":"R";
                    int post=1;
                    foreach(float z in new[]{3f,5f,7f})Decor("Bridge_Post_"+side+(post++),detail,PrimitiveType.Cube,new Vector3(x,.7f,z),new Vector3(.15f,1.3f,.15f),wood);
                    Decor("Bridge_Rail_"+side,detail,PrimitiveType.Cube,new Vector3(x,1.15f,5f),new Vector3(.12f,.14f,4.2f),wood);
                }
            }
            var rng=new System.Random(cave?92:42);
            for(int i=0;i<100;i++)
            {
                float x=(float)(rng.NextDouble()*30-15),z=(float)(rng.NextDouble()*30-15);
                if(!cave && Mathf.Abs(z-5)<2)continue;
                if(Mathf.Abs(x)<2.5f)continue;
                float s=.12f+(float)rng.NextDouble()*.23f;
                Decor("Ground_Pebble_"+i,detail,PrimitiveType.Sphere,new Vector3(x,.04f,z),new Vector3(s*2,.10f,s),pebble);
                if(!cave && i%3==0)
                    Decor("Fern_"+i,detail,PrimitiveType.Capsule,new Vector3(x+.3f,.16f,z),new Vector3(.10f,.22f,.12f),leafLight);
            }
            for(int i=0;i<26;i++)
            {
                float z=-16+i*1.22f;
                if(!cave && Mathf.Abs(z-5)<2.2f)continue;
                Decor("Trail_Stone_"+i,detail,PrimitiveType.Cylinder,new Vector3(Mathf.Sin(i*.7f)*.5f,.025f,z),new Vector3(1.5f,.025f,.85f),path);
            }
            Transform hud=Find(canvas,"Panel_Hud");
            hud.GetComponent<Image>().color=Ink;
            EditorUtil.Place((RectTransform)hud,new Vector2(0,1),new Vector2(0,1),new Vector2(36,-36),new Vector2(470,180));
            Place(hud,"Txt_StageName",new Vector2(24,-18),new Vector2(430,48));
            Place(hud,"Txt_Wave",new Vector2(24,-73),new Vector2(430,38));
            Place(hud,"Bar_PlayerHp",new Vector2(24,-130),new Vector2(422,28));
            Find(hud,"Txt_StageName").GetComponent<TMP_Text>().fontSize=30;
            Find(hud,"Txt_Wave").GetComponent<TMP_Text>().fontSize=23;
            Find(hud,"Txt_PlayerHp").GetComponent<TMP_Text>().fontSize=19;
            Find(hud,"Img_PlayerHpFill").GetComponent<Image>().color=new Color(.38f,.64f,.43f);
            Box("HudAccent",hud,Vector2.zero,new Vector2(4,180),Gold);
            Image hint=Box("ControlLegend",canvas,new Vector2(36,-980),new Vector2(580,60),Ink);
            EditorUtil.Place(hint.rectTransform,new Vector2(0,0),new Vector2(0,0),new Vector2(36,30),new Vector2(580,60));
            Label("ControlHint",hint.transform,"W A S D  이동     /     가까운 적 자동 공격",22,new Vector2(22,-17),new Vector2(550,40),Cream);
            Transform toggle=Find(canvas,"Tgl_AutoBattle");toggle.GetComponent<Image>().color=Ink;
            Find(canvas,"AutoBattleCheck").GetComponent<Image>().color=Gold;
            CombatArt.Lighting(cave);
            Transform result=Find(canvas,"Popup_Result");result.SetAsLastSibling();
        }
        static void Result(Transform root)
        {
            root.GetComponent<Image>().color=new Color(.025f,.055f,.05f,.86f);
            Transform panel=Find(root,"Panel_Result");
            ((RectTransform)panel).sizeDelta=new Vector2(1160,820);
            panel.GetComponent<Image>().color=Ink;
            Box("ResultTopRule",panel,Vector2.zero,new Vector2(1160,6),Gold);
            Label("ResultEyebrow",panel,"E X P E D I T I O N   R E P O R T",19,new Vector2(56,-32),new Vector2(1000,30),Gold);
            Place(panel,"Txt_ResultTitle",new Vector2(56,-80),new Vector2(1048,76));
            var title=Find(panel,"Txt_ResultTitle").GetComponent<TMP_Text>();title.alignment=TextAlignmentOptions.Left;title.fontSize=52;
            Box("GoldCard",panel,new Vector2(56,-180),new Vector2(516,100),Panel).transform.SetAsFirstSibling();
            Box("ExpCard",panel,new Vector2(588,-180),new Vector2(516,100),Panel).transform.SetAsFirstSibling();
            Place(panel,"Txt_ResultGold",new Vector2(80,-210),new Vector2(470,48));
            Place(panel,"Txt_ResultExp",new Vector2(612,-210),new Vector2(470,48));
            Find(panel,"Txt_ResultGold").GetComponent<TMP_Text>().color=Gold;
            Label("RewardHeading",panel,"획득 아이템",24,new Vector2(56,-315),new Vector2(1048,40),Muted);
            var scroll=(RectTransform)Find(panel,"Scroll_Result");EditorUtil.Stretch(scroll,new Vector2(46,166),new Vector2(-46,-370));
            var grid=Find(panel,"List_Result").GetComponent<GridLayoutGroup>();grid.cellSize=new Vector2(190,230);grid.spacing=new Vector2(16,16);
            Place(panel,"Txt_ResultEmpty",new Vector2(56,-430),new Vector2(1048,80));
            Find(panel,"Txt_ResultEmpty").GetComponent<TMP_Text>().fontSize=27;
            var btn=Find(panel,"Btn_ResultConfirm");EditorUtil.Place((RectTransform)btn,new Vector2(.5f,0),new Vector2(.5f,0),new Vector2(0,36),new Vector2(460,84));
            var colors=btn.GetComponent<Button>().colors;colors.normalColor=Gold;colors.highlightedColor=new Color(.89f,.75f,.47f);btn.GetComponent<Button>().colors=colors;
            var label=Find(panel,"Txt_ResultConfirm").GetComponent<TMP_Text>();label.text="보상 받고 캠프로";label.color=Ink;
        }

    }
}
