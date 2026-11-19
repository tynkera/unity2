#if QA_BRIDGE
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace ExpeditionCamp.QA
{
    public class QATools : MonoBehaviour
    {
        private static QATools _instance;

        private bool _showCheat;
        private bool _showDebug;
        private string _itemId = "202001";
        private string _itemCount = "10";
        private string _stageId = "401001";
        private float _fps;
        private float _fpsTimer;
        private int _frames;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Create()
        {
            if (_instance != null)
            {
                return;
            }
            GameObject host = new GameObject("QATools");
            _instance = host.AddComponent<QATools>();
            DontDestroyOnLoad(host);
        }

        private void Start()
        {
            if (GameContext.QA != null)
            {
                _showDebug = GameContext.QA.showDebugOnStart;
            }
        }

        private void Update()
        {
            Keyboard keyboard = Keyboard.current;
            if (keyboard != null)
            {
                if (keyboard.f1Key.wasPressedThisFrame)
                {
                    _showCheat = !_showCheat;
                }
                if (keyboard.f2Key.wasPressedThisFrame)
                {
                    _showDebug = !_showDebug;
                }
            }

            _frames++;
            _fpsTimer += Time.unscaledDeltaTime;
            if (_fpsTimer >= 0.5f)
            {
                _fps = _frames / _fpsTimer;
                _frames = 0;
                _fpsTimer = 0f;
            }
        }

        private void OnGUI()
        {
            GUI.depth = -100;
            float scale = Screen.height / 720f;
            GUI.matrix = Matrix4x4.Scale(new Vector3(scale, scale, 1f));
            if (_showCheat)
            {
                GUILayout.Window(7701, new Rect(10f, 60f, 300f, 420f), DrawCheatWindow, "Cheat (F1)");
            }
            if (_showDebug)
            {
                DrawDebug();
            }
        }

        private void DrawCheatWindow(int windowId)
        {
            if (GUILayout.Button("골드 +1000"))
            {
                CheatCommands.AddGold(1000);
            }
            GUILayout.BeginHorizontal();
            _itemId = GUILayout.TextField(_itemId, GUILayout.Width(90f));
            _itemCount = GUILayout.TextField(_itemCount, GUILayout.Width(50f));
            if (GUILayout.Button("아이템 추가"))
            {
                int itemId;
                int count;
                if (int.TryParse(_itemId, out itemId) && int.TryParse(_itemCount, out count))
                {
                    CheatCommands.AddItem(itemId, count);
                }
            }
            GUILayout.EndHorizontal();
            if (GUILayout.Button("필드 몬스터 전부 처치"))
            {
                CheatCommands.KillAll();
            }
            if (GUILayout.Button("무적 " + (GameSession.Invincible ? "켜짐" : "꺼짐")))
            {
                CheatCommands.SetInvincible(!GameSession.Invincible);
            }
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("배속 1")) CheatCommands.SetSpeed(1f);
            if (GUILayout.Button("배속 2")) CheatCommands.SetSpeed(2f);
            if (GUILayout.Button("배속 5")) CheatCommands.SetSpeed(5f);
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            _stageId = GUILayout.TextField(_stageId, GUILayout.Width(90f));
            if (GUILayout.Button("스테이지 이동"))
            {
                int stageId;
                if (int.TryParse(_stageId, out stageId))
                {
                    CheatCommands.MoveToStage(stageId, true);
                }
            }
            GUILayout.EndHorizontal();
            if (GUILayout.Button("세이브 초기화"))
            {
                CheatCommands.ResetSave();
            }
        }

        private void DrawDebug()
        {
            int monsterCount = GameObject.FindGameObjectsWithTag("Monster").Length;
            int dropCount = GameObject.FindGameObjectsWithTag("DropItem").Length;
            string lastDrop = "-";
            DropRecord record = DropLog.Last;
            if (record != null)
            {
                lastDrop = "group " + record.group_id + " · roll " + record.roll + " · items " + string.Join(",", record.item_ids);
            }
            string text =
                "FPS " + _fps.ToString("0") + "\n" +
                "Scene " + SceneManager.GetActiveScene().name + "\n" +
                "Monster " + monsterCount + " · DropItem " + dropCount + "\n" +
                "Seed " + GameRandom.Seed + (GameRandom.IsFixed ? " (고정)" : "") + " · TimeScale " + Time.timeScale.ToString("0.##") + "\n" +
                "Last drop " + lastDrop;
            GUI.Box(new Rect(Screen.width / (Screen.height / 720f) - 430f, 60f, 420f, 110f), "");
            GUI.Label(new Rect(Screen.width / (Screen.height / 720f) - 420f, 66f, 400f, 100f), text);
        }
    }
}
#endif
