#if QA_BRIDGE
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace ExpeditionCamp.QA
{
    public class BridgeRequest
    {
        public string Method;
        public string Path;
        public string RawUrl;
        public string Body;
        public Dictionary<string, string> Query = new Dictionary<string, string>();

        public int Status = 200;
        public string Json = "{}";

        public readonly TaskCompletionSource<bool> Completion = new TaskCompletionSource<bool>();
        public HttpListenerContext Context;

        private int _abandoned;

        public bool Abandoned
        {
            get { return Volatile.Read(ref _abandoned) == 1; }
        }

        public void MarkAbandoned()
        {
            Volatile.Write(ref _abandoned, 1);
        }

        public AsyncOperation LoadOperation;
        public string ScreenshotPath;
        public float Deadline;
    }

    public class QABridge : MonoBehaviour
    {
        public const int DefaultPort = 7700;
        private const int MainThreadTimeoutMillisecond = 5000;
        private const float ScreenshotTimeoutSecond = 10f;

        private static QABridge _instance;

        private HttpListener _listener;
        private Thread _listenThread;
        private int _port = DefaultPort;
        private volatile bool _running;

        private readonly ConcurrentQueue<BridgeRequest> _incoming = new ConcurrentQueue<BridgeRequest>();
        private readonly List<BridgeRequest> _pending = new List<BridgeRequest>();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void CreateBridge()
        {
            if (_instance != null)
            {
                return;
            }
            GameObject host = new GameObject("QABridge");
            _instance = host.AddComponent<QABridge>();
            DontDestroyOnLoad(host);
        }

        private void Awake()
        {
            _port = ReadPortArgument(DefaultPort);
            StartListener();
        }

        private static int ReadPortArgument(int fallback)
        {
            string[] arguments = Environment.GetCommandLineArgs();
            for (int index = 0; index < arguments.Length - 1; index++)
            {
                if (arguments[index] != "-qaPort")
                {
                    continue;
                }
                int value;
                if (int.TryParse(arguments[index + 1], out value))
                {
                    return value;
                }
            }
            return fallback;
        }

        private void StartListener()
        {
            string prefix = "http://127.0.0.1:" + _port + "/";
            try
            {
                _listener = new HttpListener();
                _listener.Prefixes.Add(prefix);
                _listener.Start();
            }
            catch (Exception error)
            {
                Debug.LogError("[QABridge] " + prefix + " 주소를 열지 못했습니다: " + error.Message);
                _listener = null;
                return;
            }

            _running = true;
            _listenThread = new Thread(ListenLoop);
            _listenThread.IsBackground = true;
            _listenThread.Start();
            Debug.Log("[QABridge] " + prefix + " 에서 요청을 기다립니다");
        }

        private void ListenLoop()
        {
            while (_running)
            {
                HttpListenerContext context;
                try
                {
                    context = _listener.GetContext();
                }
                catch (Exception)
                {
                    return;
                }

                BridgeRequest request = ReadRequest(context);
                _incoming.Enqueue(request);

                bool completed = request.Completion.Task.Wait(MainThreadTimeoutMillisecond);
                if (!completed)
                {
                    request.MarkAbandoned();
                    request.Status = 504;
                    request.Json = "{\"error\":\"main thread timeout\"}";
                }
                WriteResponse(request);
            }
        }

        private static BridgeRequest ReadRequest(HttpListenerContext context)
        {
            BridgeRequest request = new BridgeRequest();
            request.Context = context;
            request.Method = context.Request.HttpMethod;
            request.Path = context.Request.Url.AbsolutePath;
            request.RawUrl = context.Request.RawUrl;

            foreach (string key in context.Request.QueryString.AllKeys)
            {
                if (key == null)
                {
                    continue;
                }
                request.Query[key] = context.Request.QueryString[key];
            }

            if (context.Request.HasEntityBody)
            {
                using (StreamReader reader = new StreamReader(
                    context.Request.InputStream, context.Request.ContentEncoding))
                {
                    request.Body = reader.ReadToEnd();
                }
            }
            else
            {
                request.Body = "";
            }
            return request;
        }

        private static void WriteResponse(BridgeRequest request)
        {
            try
            {
                byte[] payload = Encoding.UTF8.GetBytes(request.Json);
                request.Context.Response.StatusCode = request.Status;
                request.Context.Response.ContentType = "application/json; charset=utf-8";
                request.Context.Response.ContentLength64 = payload.Length;
                request.Context.Response.OutputStream.Write(payload, 0, payload.Length);
                request.Context.Response.OutputStream.Close();
            }
            catch (Exception error)
            {
                Debug.LogWarning("[QABridge] 응답을 보내지 못했습니다: " + error.Message);
            }
        }

        private void Update()
        {
            BridgeRequest incoming;
            while (_incoming.TryDequeue(out incoming))
            {
                _pending.Add(incoming);
            }

            for (int index = _pending.Count - 1; index >= 0; index--)
            {
                BridgeRequest request = _pending[index];
                if (request.Abandoned)
                {
                    _pending.RemoveAt(index);
                    continue;
                }

                bool done = Handle(request);
                if (!done)
                {
                    continue;
                }

                _pending.RemoveAt(index);
                Debug.Log("[QABridge] " + request.Method + " " + request.RawUrl + " -> " + request.Status);
                request.Completion.TrySetResult(true);
            }
        }

        private bool Handle(BridgeRequest request)
        {
            if (request.Path == "/health")
            {
                return HandleHealth(request);
            }
            if (request.Path == "/hierarchy")
            {
                return HandleHierarchy(request);
            }
            if (request.Path == "/object")
            {
                return HandleObject(request);
            }
            if (request.Path == "/text")
            {
                return HandleText(request);
            }
            if (request.Path == "/click")
            {
                return HandleClick(request);
            }
            if (request.Path == "/reset")
            {
                return HandleReset(request);
            }
            if (request.Path == "/screenshot")
            {
                return HandleScreenshot(request);
            }
            if (request.Path == "/scene")
            {
                return HandleScene(request);
            }
            if (request.Path == "/count")
            {
                return HandleCount(request);
            }
            if (request.Path == "/cheat")
            {
                return HandleCheat(request);
            }
            if (request.Path == "/inventory")
            {
                return HandleInventory(request);
            }
            if (request.Path == "/last_drop")
            {
                return HandleLastDrop(request);
            }
            if (request.Path == "/field")
            {
                return HandleField(request);
            }

            SetError(request, 404, "unknown path", request.Path);
            return true;
        }

        private bool HandleHealth(BridgeRequest request)
        {
            HealthResponse response = new HealthResponse();
            response.qaBridge = true;
            response.scene = SceneManager.GetActiveScene().name;
            response.port = _port;
            response.batchMode = Application.isBatchMode;
            SetJson(request, 200, JsonUtility.ToJson(response));
            return true;
        }

        private bool HandleHierarchy(BridgeRequest request)
        {
            List<GameObject> found = CollectUiObjects();
            List<ObjectInfo> infos = new List<ObjectInfo>();
            foreach (GameObject target in found)
            {
                infos.Add(DescribeObject(target));
            }

            HierarchyResponse response = new HierarchyResponse();
            response.scene = SceneManager.GetActiveScene().name;
            response.count = infos.Count;
            response.objects = infos.ToArray();
            SetJson(request, 200, JsonUtility.ToJson(response));
            return true;
        }

        private bool HandleObject(BridgeRequest request)
        {
            string name;
            if (!request.Query.TryGetValue("name", out name) || string.IsNullOrEmpty(name))
            {
                SetError(request, 400, "name is required", "/object?name=이름 형태로 요청해 주세요");
                return true;
            }

            List<GameObject> matches = FindUiObjects(name);
            if (matches.Count == 0)
            {
                SetError(request, 404, "object not found", name);
                return true;
            }
            if (matches.Count > 1)
            {
                SetDuplicate(request, name, matches);
                return true;
            }

            ObjectInfo info = DescribeObject(matches[0]);
            ObjectResponse response = new ObjectResponse();
            response.found = true;
            response.active = info.active;
            response.interactable = info.interactable;
            response.text = info.text;
            response.path = info.path;
            SetJson(request, 200, JsonUtility.ToJson(response));
            return true;
        }

        private bool HandleText(BridgeRequest request)
        {
            string name;
            if (!request.Query.TryGetValue("name", out name) || string.IsNullOrEmpty(name))
            {
                SetError(request, 400, "name is required", "/text?name=이름 형태로 요청해 주세요");
                return true;
            }

            List<GameObject> matches = FindUiObjects(name);
            if (matches.Count == 0)
            {
                SetError(request, 404, "object not found", name);
                return true;
            }
            if (matches.Count > 1)
            {
                SetDuplicate(request, name, matches);
                return true;
            }

            string text = ReadTextValue(matches[0]);
            if (text == null)
            {
                SetError(request, 422, "text component not found", name);
                return true;
            }

            TextResponse response = new TextResponse();
            response.name = name;
            response.text = text;
            SetJson(request, 200, JsonUtility.ToJson(response));
            return true;
        }

        private bool HandleClick(BridgeRequest request)
        {
            NameRequest body = ParseBody<NameRequest>(request.Body);
            if (body == null || string.IsNullOrEmpty(body.name))
            {
                SetError(request, 400, "name is required", "{\"name\": \"오브젝트 이름\"} 형태로 보내 주세요");
                return true;
            }

            List<GameObject> matches = FindUiObjects(body.name);
            if (matches.Count == 0)
            {
                SetError(request, 404, "object not found", body.name);
                return true;
            }
            if (matches.Count > 1)
            {
                SetDuplicate(request, body.name, matches);
                return true;
            }

            GameObject target = matches[0];
            if (!target.activeInHierarchy)
            {
                SetError(request, 409, "object is not active", body.name);
                return true;
            }

            Selectable selectable = target.GetComponent<Selectable>();
            if (selectable != null && !selectable.IsInteractable())
            {
                SetError(request, 409, "object is not interactable", body.name);
                return true;
            }

            if (EventSystem.current == null)
            {
                SetError(request, 409, "event system not found", SceneManager.GetActiveScene().name);
                return true;
            }

            PointerEventData pointerEventData = new PointerEventData(EventSystem.current);
            pointerEventData.button = PointerEventData.InputButton.Left;
            bool handled = ExecuteEvents.Execute(target, pointerEventData, ExecuteEvents.pointerClickHandler);
            if (!handled)
            {
                SetError(request, 409, "click handler not found", body.name);
                return true;
            }

            ClickResponse response = new ClickResponse();
            response.clicked = true;
            response.name = body.name;
            response.scene = SceneManager.GetActiveScene().name;
            SetJson(request, 200, JsonUtility.ToJson(response));
            return true;
        }

        private bool HandleReset(BridgeRequest request)
        {
            if (request.LoadOperation == null)
            {
                SaveSystem.ResetAndSave();
                GameSession.ResetSession();
                DropLog.Clear();
                request.LoadOperation = SceneManager.LoadSceneAsync(GameSession.LobbyScene, LoadSceneMode.Single);
                return false;
            }

            if (!request.LoadOperation.isDone)
            {
                return false;
            }

            ResetResponse response = new ResetResponse();
            response.ok = true;
            response.scene = SceneManager.GetActiveScene().name;
            SetJson(request, 200, JsonUtility.ToJson(response));
            return true;
        }

        private bool HandleScene(BridgeRequest request)
        {
            if (request.Method == "GET")
            {
                SceneResponse current = new SceneResponse();
                current.scene = SceneManager.GetActiveScene().name;
                current.stage_id = GameSession.CurrentStageId;
                SetJson(request, 200, JsonUtility.ToJson(current));
                return true;
            }

            if (request.LoadOperation == null)
            {
                StageRequest body = ParseBody<StageRequest>(request.Body);
                if (body == null || body.stage_id == 0)
                {
                    SetError(request, 400, "stage_id is required", "{\"stage_id\": 401001} 형태로 보내 주세요");
                    return true;
                }
                StageMoveResult check = CheatCommands.CheckStage(body.stage_id);
                if (check == StageMoveResult.NotFound)
                {
                    SetError(request, 404, "stage not found", body.stage_id.ToString());
                    return true;
                }
                if (check == StageMoveResult.Locked)
                {
                    SetError(request, 409, "stage is locked", body.stage_id.ToString());
                    return true;
                }
                if (check == StageMoveResult.SceneNotInBuild)
                {
                    SetError(request, 500, "scene is not in build list", GameSession.GetSceneName(body.stage_id));
                    return true;
                }
                request.LoadOperation = CheatCommands.MoveToStage(body.stage_id, false);
                return false;
            }

            if (!request.LoadOperation.isDone)
            {
                return false;
            }

            SceneResponse response = new SceneResponse();
            response.scene = SceneManager.GetActiveScene().name;
            response.stage_id = GameSession.CurrentStageId;
            SetJson(request, 200, JsonUtility.ToJson(response));
            return true;
        }

        private bool HandleCount(BridgeRequest request)
        {
            string tag;
            if (!request.Query.TryGetValue("tag", out tag) || string.IsNullOrEmpty(tag))
            {
                SetError(request, 400, "tag is required", "/count?tag=Monster 형태로 요청해 주세요");
                return true;
            }
            GameObject[] found;
            try
            {
                found = GameObject.FindGameObjectsWithTag(tag);
            }
            catch (UnityException)
            {
                SetError(request, 404, "tag not defined", tag);
                return true;
            }
            CountResponse response = new CountResponse();
            response.tag = tag;
            response.count = found.Length;
            SetJson(request, 200, JsonUtility.ToJson(response));
            return true;
        }

        private bool HandleCheat(BridgeRequest request)
        {
            CheatRequest body = ParseBody<CheatRequest>(request.Body);
            if (body == null || string.IsNullOrEmpty(body.name))
            {
                SetError(request, 400, "name is required", "{\"name\": \"kill_all\"} 형태로 보내 주세요");
                return true;
            }

            int affected;
            if (body.name == "kill_all")
            {
                affected = CheatCommands.KillAll();
            }
            else if (body.name == "add_gold")
            {
                affected = ParseInt(body.value, 1000);
                CheatCommands.AddGold(affected);
            }
            else if (body.name == "add_item")
            {
                affected = body.count > 0 ? body.count : 1;
                CheatCommands.AddItem(body.item_id, affected);
            }
            else if (body.name == "invincible")
            {
                bool value = body.value != "false";
                CheatCommands.SetInvincible(value);
                affected = value ? 1 : 0;
            }
            else if (body.name == "speed")
            {
                float multiplier;
                if (!float.TryParse(body.value, System.Globalization.NumberStyles.Float,
                    System.Globalization.CultureInfo.InvariantCulture, out multiplier) || multiplier <= 0f)
                {
                    SetError(request, 400, "value must be a positive number", body.value);
                    return true;
                }
                CheatCommands.SetSpeed(multiplier);
                affected = 1;
            }
            else if (body.name == "reset_save")
            {
                CheatCommands.ResetSave();
                affected = 1;
            }
            else
            {
                SetError(request, 404, "unknown cheat", body.name);
                return true;
            }

            CheatResponse response = new CheatResponse();
            response.ok = true;
            response.name = body.name;
            response.affected = affected;
            response.scene = SceneManager.GetActiveScene().name;
            SetJson(request, 200, JsonUtility.ToJson(response));
            return true;
        }

        private static int ParseInt(string text, int fallback)
        {
            int value;
            if (int.TryParse(text, out value))
            {
                return value;
            }
            return fallback;
        }

        private bool HandleInventory(BridgeRequest request)
        {
            SaveSystem.EnsureLoaded();
            List<int> itemIds = Inventory.GetItemIds();
            InventoryItem[] items = new InventoryItem[itemIds.Count];
            for (int index = 0; index < itemIds.Count; index++)
            {
                InventoryItem item = new InventoryItem();
                item.item_id = itemIds[index];
                item.count = Inventory.GetCount(itemIds[index]);
                items[index] = item;
            }
            InventoryResponse response = new InventoryResponse();
            response.gold = SaveSystem.Data.gold;
            response.level = SaveSystem.Data.level;
            response.exp = SaveSystem.Data.exp;
            response.items = items;
            SetJson(request, 200, JsonUtility.ToJson(response));
            return true;
        }

        private bool HandleField(BridgeRequest request)
        {
            FieldManager field = FieldManager.Current;
            if (field == null)
            {
                SetError(request, 404, "field not loaded", SceneManager.GetActiveScene().name);
                return true;
            }

            List<FieldMonster> monsters = new List<FieldMonster>();
            Monster[] found = FindObjectsByType<Monster>(FindObjectsInactive.Exclude, FindObjectsSortMode.InstanceID);
            foreach (Monster monster in found)
            {
                if (!monster.IsSpawned || monster.IsDead)
                {
                    continue;
                }
                FieldMonster item = new FieldMonster();
                item.name = monster.gameObject.name;
                item.tag = monster.gameObject.tag;
                item.x = monster.transform.position.x;
                item.z = monster.transform.position.z;
                item.move_speed = monster.moveSpeed;
                item.attack_range = monster.attackRange;
                item.hp = monster.Hp;
                monsters.Add(item);
            }

            FieldResponse response = new FieldResponse();
            response.scene = SceneManager.GetActiveScene().name;
            response.stage_id = field.StageId;
            response.wave = field.CurrentWave;
            response.total_waves = field.TotalWaves;
            response.finished = field.IsFinished;
            response.cleared = field.IsCleared;
            response.player_hp = field.Player.Hp;
            response.player_max_hp = field.Player.MaxHp;
            response.player_x = field.Player.transform.position.x;
            response.player_z = field.Player.transform.position.z;
            response.monsters = monsters.ToArray();
            SetJson(request, 200, JsonUtility.ToJson(response));
            return true;
        }

        private bool HandleLastDrop(BridgeRequest request)
        {
            DropRecord record = DropLog.Last;
            if (record == null)
            {
                record = new DropRecord();
            }
            SetJson(request, 200, JsonUtility.ToJson(record));
            return true;
        }

        private bool HandleScreenshot(BridgeRequest request)
        {
            if (request.ScreenshotPath == null)
            {
                if (Application.isBatchMode)
                {
                    SetError(request, 503, "screenshot is not available in batch mode",
                        "화면이 없는 실행에서는 화면을 저장할 수 없습니다");
                    return true;
                }

                PathRequest body = ParseBody<PathRequest>(request.Body);
                if (body == null || string.IsNullOrEmpty(body.path))
                {
                    SetError(request, 400, "path is required", "{\"path\": \"저장 경로\"} 형태로 보내 주세요");
                    return true;
                }

                string directory = Path.GetDirectoryName(body.path);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                if (File.Exists(body.path))
                {
                    try
                    {
                        File.Delete(body.path);
                    }
                    catch (IOException error)
                    {
                        SetError(request, 500, "existing screenshot file could not be deleted", error.Message);
                        return true;
                    }
                }

                ScreenCapture.CaptureScreenshot(body.path);
                request.ScreenshotPath = body.path;
                request.Deadline = Time.realtimeSinceStartup + ScreenshotTimeoutSecond;
                return false;
            }

            FileInfo screenshotFile = new FileInfo(request.ScreenshotPath);
            if (screenshotFile.Exists && screenshotFile.Length > 0)
            {
                ScreenshotResponse response = new ScreenshotResponse();
                response.path = request.ScreenshotPath;
                SetJson(request, 200, JsonUtility.ToJson(response));
                return true;
            }

            if (Time.realtimeSinceStartup > request.Deadline)
            {
                SetError(request, 500, "screenshot file was not created", request.ScreenshotPath);
                return true;
            }
            return false;
        }

        private static T ParseBody<T>(string body) where T : class
        {
            if (string.IsNullOrEmpty(body))
            {
                return null;
            }
            try
            {
                return JsonUtility.FromJson<T>(body);
            }
            catch (Exception)
            {
                return null;
            }
        }

        private static void SetJson(BridgeRequest request, int status, string json)
        {
            request.Status = status;
            request.Json = json;
        }

        private static void SetError(BridgeRequest request, int status, string error, string detail)
        {
            ErrorResponse response = new ErrorResponse();
            response.error = error;
            response.detail = detail;
            SetJson(request, status, JsonUtility.ToJson(response));
        }

        private static void SetDuplicate(BridgeRequest request, string name, List<GameObject> matches)
        {
            string[] paths = new string[matches.Count];
            for (int index = 0; index < matches.Count; index++)
            {
                paths[index] = BuildPath(matches[index]);
            }

            DuplicateResponse response = new DuplicateResponse();
            response.error = "duplicated object name";
            response.name = name;
            response.count = matches.Count;
            response.paths = paths;
            SetJson(request, 409, JsonUtility.ToJson(response));
        }

        private static List<GameObject> CollectUiObjects()
        {
            List<GameObject> found = new List<GameObject>();
            for (int sceneIndex = 0; sceneIndex < SceneManager.sceneCount; sceneIndex++)
            {
                Scene scene = SceneManager.GetSceneAt(sceneIndex);
                if (!scene.isLoaded)
                {
                    continue;
                }
                foreach (GameObject root in scene.GetRootGameObjects())
                {
                    CollectFrom(root.transform, found);
                }
            }
            return found;
        }

        private static void CollectFrom(Transform parent, List<GameObject> found)
        {
            GameObject target = parent.gameObject;
            bool isUi = target.GetComponent<RectTransform>() != null && target.GetComponent<TMP_SubMeshUI>() == null;
            bool is2dClickable = target.GetComponent<Collider2D>() != null;
            if (target.activeInHierarchy && (isUi || is2dClickable))
            {
                found.Add(target);
            }
            for (int index = 0; index < parent.childCount; index++)
            {
                CollectFrom(parent.GetChild(index), found);
            }
        }

        private static List<GameObject> FindUiObjects(string name)
        {
            List<GameObject> matches = new List<GameObject>();
            foreach (GameObject target in CollectUiObjects())
            {
                if (target.name == name)
                {
                    matches.Add(target);
                }
            }
            return matches;
        }

        private static ObjectInfo DescribeObject(GameObject target)
        {
            ObjectInfo info = new ObjectInfo();
            info.name = target.name;
            info.path = BuildPath(target);
            info.active = target.activeInHierarchy;

            Selectable selectable = target.GetComponent<Selectable>();
            info.interactable = selectable != null && selectable.IsInteractable();

            string text = ReadTextValue(target);
            if (text == null)
            {
                text = "";
            }
            info.text = text;
            return info;
        }

        private static string ReadTextValue(GameObject target)
        {
            TMP_Text tmpText = target.GetComponent<TMP_Text>();
            if (tmpText != null)
            {
                return tmpText.text;
            }
            Text uiText = target.GetComponent<Text>();
            if (uiText != null)
            {
                return uiText.text;
            }

            TMP_Text childTmpText = target.GetComponentInChildren<TMP_Text>();
            if (childTmpText != null)
            {
                return childTmpText.text;
            }
            Text childUiText = target.GetComponentInChildren<Text>();
            if (childUiText != null)
            {
                return childUiText.text;
            }
            return null;
        }

        private static string BuildPath(GameObject target)
        {
            StringBuilder builder = new StringBuilder(target.name);
            Transform parent = target.transform.parent;
            while (parent != null)
            {
                builder.Insert(0, parent.name + "/");
                parent = parent.parent;
            }
            return builder.ToString();
        }

        private void OnApplicationQuit()
        {
            StopListener();
        }

        private void OnDestroy()
        {
            StopListener();
        }

        private void StopListener()
        {
            if (!_running)
            {
                return;
            }
            _running = false;
            if (_listener != null)
            {
                _listener.Stop();
                _listener.Close();
                _listener = null;
            }
            Debug.Log("[QABridge] 요청 받기를 종료했습니다");
        }
    }
}
#endif
