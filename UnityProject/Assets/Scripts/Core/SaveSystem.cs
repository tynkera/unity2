using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace ExpeditionCamp
{
    [Serializable]
    public class SaveItem
    {
        public int item_id;
        public int count;
    }

    [Serializable]
    public class SaveData
    {
        public int version = 1;
        public int gold;
        public int level = 1;
        public int exp;
        public List<SaveItem> bag = new List<SaveItem>();
        public List<int> dex = new List<int>();
        public List<int> cleared_stages = new List<int>();
        public List<int> unlocked_stages = new List<int>();
    }

    public static class SaveSystem
    {
        public const string FileName = "save.json";
        public const string BackupFileName = "save.bak.json";

        public static SaveData Data { get; private set; }
        public static bool IsLoaded { get { return Data != null; } }

        public static string SavePath
        {
            get { return Path.Combine(Application.persistentDataPath, FileName); }
        }

        public static string BackupPath
        {
            get { return Path.Combine(Application.persistentDataPath, BackupFileName); }
        }

        public static void EnsureLoaded()
        {
            if (Data == null)
            {
                Load();
            }
        }

        public static void Load()
        {
            string path = SavePath;
            if (!File.Exists(path))
            {
                StartNew();
                Debug.Log("[Save] 세이브 파일이 없어서 새 세이브로 시작합니다: " + path);
                return;
            }

            string text = File.ReadAllText(path);
            SaveData loaded = null;
            try
            {
                loaded = JsonUtility.FromJson<SaveData>(text);
            }
            catch (Exception)
            {
                loaded = null;
            }

            if (loaded == null || loaded.bag == null || loaded.dex == null || loaded.cleared_stages == null)
            {
                File.Copy(path, BackupPath, true);
                Debug.LogWarning("[Save] save.json 을 읽지 못했습니다. 백업 " + BackupPath + " 을 만들고 새 세이브로 시작합니다");
                StartNew();
                Save();
                return;
            }

            Data = loaded;
            ApplyToInventory();
            Debug.Log("[Save] 세이브를 불러왔습니다: " + path);
        }

        public static void StartNew()
        {
            Data = new SaveData();
            Inventory.Clear();
            RefreshDerived();
        }

        public static void ResetAndSave()
        {
            StartNew();
            Save();
        }

        public static void Save()
        {
            EnsureLoaded();
            CollectFromInventory();
            RefreshDerived();
            string path = SavePath;
            string directory = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
            File.WriteAllText(path, JsonUtility.ToJson(Data, true));
            Debug.Log("[Save] 저장했습니다: gold " + Data.gold + " · level " + Data.level + " · bag " + Data.bag.Count + "종");
        }

        public static void AddExp(int amount)
        {
            EnsureLoaded();
            Data.exp += amount;
            RefreshDerived();
        }

        public static void RegisterDex(int monsterId)
        {
            EnsureLoaded();
            if (!Data.dex.Contains(monsterId))
            {
                Data.dex.Add(monsterId);
            }
        }

        public static void MarkCleared(int stageId)
        {
            EnsureLoaded();
            if (!Data.cleared_stages.Contains(stageId))
            {
                Data.cleared_stages.Add(stageId);
            }
            RefreshDerived();
        }

        public static bool IsStageUnlocked(int stageId)
        {
            EnsureLoaded();
            HashSet<int> cleared = new HashSet<int>(Data.cleared_stages);
            return StageUnlock.IsUnlocked(stageId, cleared, GameData.Stages);
        }

        private static void RefreshDerived()
        {
            if (GameData.LevelExp != null && GameData.LevelExp.Count > 0)
            {
                Data.level = LevelCalculator.GetLevel(Data.exp, GameData.LevelExp);
            }
            Data.unlocked_stages = new List<int>();
            if (GameData.Stages == null)
            {
                return;
            }
            HashSet<int> cleared = new HashSet<int>(Data.cleared_stages);
            foreach (Dictionary<string, string> row in GameData.Stages)
            {
                int stageId = DataLoader.ReadInt(row, "stage_id", 0);
                if (StageUnlock.IsUnlocked(stageId, cleared, GameData.Stages))
                {
                    Data.unlocked_stages.Add(stageId);
                }
            }
        }

        private static void ApplyToInventory()
        {
            Inventory.Clear();
            foreach (SaveItem item in Data.bag)
            {
                Inventory.AddItem(item.item_id, item.count);
            }
        }

        private static void CollectFromInventory()
        {
            Data.bag = new List<SaveItem>();
            foreach (int itemId in Inventory.GetItemIds())
            {
                SaveItem item = new SaveItem();
                item.item_id = itemId;
                item.count = Inventory.GetCount(itemId);
                Data.bag.Add(item);
            }
        }

        public static void ClearForTest()
        {
            Data = null;
            Inventory.Clear();
        }
    }
}
