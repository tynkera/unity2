using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ExpeditionCamp
{
    public class FieldManager : MonoBehaviour
    {
        [SerializeField] private int editorStageId = 401001;
        [SerializeField] private GameConfig config;
        [SerializeField] private PlayerController player;
        [SerializeField] private Transform playerStart;
        [SerializeField] private CameraFollow cameraFollow;
        [SerializeField] private FieldHud hud;
        [SerializeField] private ResultPopup resultPopup;
        [SerializeField] private DropItem dropItemPrefab;

        public static FieldManager Current { get; private set; }

        public PlayerController Player
        {
            get { return player; }
        }

        public int StageId { get; private set; }
        public int CurrentWave { get; private set; }
        public int TotalWaves { get; private set; }
        public bool IsFinished { get; private set; }
        public bool IsCleared { get; private set; }
        public bool IsResultConfirmed { get; private set; }
        public FieldLoot Loot { get; private set; }

        private readonly List<DropItem> _drops = new List<DropItem>();
        private int _plannedInWave;
        private int _killedInWave;

        public int DropCount
        {
            get { return _drops.Count; }
        }

        private void Awake()
        {
            Current = this;
            Loot = new FieldLoot();
        }

        private void OnDestroy()
        {
            if (Current == this)
            {
                Current = null;
            }
        }

        private IEnumerator Start()
        {
            StageId = GameSession.CurrentStageId != 0 ? GameSession.CurrentStageId : editorStageId;
            GameSession.CurrentStageId = StageId;

            player.Bind(this);
            player.Teleport(playerStart.position);
            cameraFollow.SetTarget(player.transform);
            hud.Bind(player);

            Dictionary<string, string> stageRow = GameData.GetStage(StageId);
            string stageName = GameData.GetString(DataLoader.ReadText(stageRow, "name_key"));
            hud.SetStage(DataLoader.ReadInt(stageRow, "chapter", 0) + "-" + DataLoader.ReadInt(stageRow, "stage_no", 0) + " " + stageName);

            List<Dictionary<string, string>> rows = GameData.GetFieldRows(StageId);
            TotalWaves = 0;
            foreach (Dictionary<string, string> row in rows)
            {
                TotalWaves = Mathf.Max(TotalWaves, DataLoader.ReadInt(row, "wave_no", 0));
            }
            Debug.Log("[Field] " + StageId + " 스테이지 시작 · 웨이브 " + TotalWaves + "개");

            for (int wave = 1; wave <= TotalWaves; wave++)
            {
                CurrentWave = wave;
                hud.SetWave(wave, TotalWaves);
                SpawnWave(wave, rows);
                while (_killedInWave < _plannedInWave && !player.IsDead)
                {
                    yield return null;
                }
                if (player.IsDead)
                {
                    break;
                }
                Debug.Log("[Field] 웨이브 " + wave + " 완료");
                yield return CollectDrops();
                if (wave < TotalWaves)
                {
                    yield return new WaitForSeconds(config.waveInterval);
                }
            }

            Finish(!player.IsDead);
        }

        private void SpawnWave(int wave, List<Dictionary<string, string>> rows)
        {
            _plannedInWave = 0;
            _killedInWave = 0;
            foreach (Dictionary<string, string> row in rows)
            {
                if (DataLoader.ReadInt(row, "wave_no", 0) != wave)
                {
                    continue;
                }
                string spawnKey = DataLoader.ReadText(row, "spawn_point_key");
                int monsterId = DataLoader.ReadInt(row, "monster_id", 0);
                int count = DataLoader.ReadInt(row, "count", 0);
                _plannedInWave += count;

                SpawnPoint spawnPoint = FindSpawnPoint(spawnKey);
                if (spawnPoint == null)
                {
                    Debug.LogWarning("[Field] spawn point not found: " + spawnKey);
                    continue;
                }
                for (int index = 0; index < count; index++)
                {
                    SpawnMonster(spawnPoint, monsterId);
                }
            }
            Debug.Log("[Field] 웨이브 " + wave + " 시작 · 몬스터 " + _plannedInWave + "마리");
        }

        private static SpawnPoint FindSpawnPoint(string spawnKey)
        {
            SpawnPoint[] spawnPoints = FindObjectsByType<SpawnPoint>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
            foreach (SpawnPoint spawnPoint in spawnPoints)
            {
                if (spawnPoint.gameObject.name == spawnKey)
                {
                    return spawnPoint;
                }
            }
            return null;
        }

        private void SpawnMonster(SpawnPoint spawnPoint, int monsterId)
        {
            string prefabKey = DataLoader.ReadText(GameData.GetMonster(monsterId), "prefab_key");
            Monster template = spawnPoint.FindTemplate(prefabKey);
            GameObject source = template != null ? template.gameObject : Resources.Load<GameObject>("Monsters/" + prefabKey);
            if (source == null)
            {
                Debug.LogError("[Field] monster prefab not found: Monsters/" + prefabKey);
                return;
            }

            GameObject spawned = Instantiate(source, spawnPoint.PickPosition(), Quaternion.identity);
            spawned.SetActive(true);
            Monster monster = spawned.GetComponent<Monster>();
            monster.Init(monsterId, this);
        }

        public void OnMonsterKilled(Monster monster)
        {
            _killedInWave++;
            Loot.Kills++;
            Loot.Exp += GameData.GetConstInt("EXP_PER_KILL", 0);
            SaveSystem.EnsureLoaded();
            if (!SaveSystem.Data.dex.Contains(monster.MonsterId) && !Loot.NewDex.Contains(monster.MonsterId))
            {
                Loot.NewDex.Add(monster.MonsterId);
            }

            List<DropResult> results = DropLog.Roll(monster.DropGroupId);
            DropRecord record = DropLog.Last;
            Debug.Log("[Drop] group " + record.group_id + " · roll " + record.roll + " · items " + string.Join(",", record.item_ids));
            foreach (DropResult result in results)
            {
                SpawnDrop(monster.transform.position, result.ItemId, result.Count);
            }
        }

        private void SpawnDrop(Vector3 origin, int itemId, int count)
        {
            Vector3 offset = new Vector3(GameRandom.SpawnRange(-0.8f, 0.8f), 0f, GameRandom.SpawnRange(-0.8f, 0.8f));
            Vector3 position = new Vector3(origin.x, 0f, origin.z) + offset;
            DropItem drop = Instantiate(dropItemPrefab, position, Quaternion.identity);
            drop.Init(itemId, count, this, config.dropItemLifetime);
            _drops.Add(drop);
        }

        public void Collect(DropItem drop)
        {
            _drops.Remove(drop);
            Loot.AddItem(drop.ItemId, drop.Count);
            Debug.Log("[Drop] 획득 " + drop.ItemId + " x" + drop.Count);
        }

        public void OnDropExpired(DropItem drop)
        {
            _drops.Remove(drop);
            Debug.Log("[Drop] 유지 시간이 지나 사라졌습니다 " + drop.ItemId + " x" + drop.Count);
        }

        private IEnumerator CollectDrops()
        {
            foreach (DropItem drop in _drops)
            {
                drop.StartMagnet(player.transform, config.dropMagnetSpeed);
            }
            float elapsed = 0f;
            while (_drops.Count > 0 && elapsed < 3f)
            {
                elapsed += Time.deltaTime;
                yield return null;
            }
            if (_drops.Count > 0)
            {
                Debug.LogWarning("[Field] 획득하지 못한 드롭 아이템 " + _drops.Count + "개");
            }
        }

        public int KillAll()
        {
            int killed = 0;
            Monster[] monsters = FindObjectsByType<Monster>(FindObjectsInactive.Exclude, FindObjectsSortMode.InstanceID);
            foreach (Monster monster in monsters)
            {
                if (monster.IsDead || !monster.IsSpawned)
                {
                    continue;
                }
                monster.Die();
                killed++;
            }
            return killed;
        }

        private void Finish(bool cleared)
        {
            IsFinished = true;
            IsCleared = cleared;
            Debug.Log("[Field] " + StageId + " 스테이지 " + (cleared ? "클리어" : "실패") +
                " · 처치 " + Loot.Kills + " · 골드 " + Loot.Gold + " · 아이템 " + Loot.ItemOrder.Count + "종");
            resultPopup.Show(cleared, Loot, ConfirmResult);
        }

        public void ConfirmResult()
        {
            if (IsResultConfirmed)
            {
                return;
            }
            IsResultConfirmed = true;
            if (IsCleared)
            {
                SaveSystem.EnsureLoaded();
                SaveSystem.Data.gold += Loot.Gold;
                foreach (int itemId in Loot.ItemOrder)
                {
                    Inventory.AddItem(itemId, Loot.GetCount(itemId));
                }
                SaveSystem.AddExp(Loot.Exp);
                foreach (int monsterId in Loot.NewDex)
                {
                    SaveSystem.RegisterDex(monsterId);
                }
                SaveSystem.MarkCleared(StageId);
                SaveSystem.Save();
            }
            SceneManager.LoadScene(GameSession.LobbyScene);
        }
    }
}
