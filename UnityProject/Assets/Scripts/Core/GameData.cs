using System.Collections.Generic;

namespace ExpeditionCamp
{
    public static class GameData
    {
        public static List<Dictionary<string, string>> Monsters { get; private set; }
        public static List<Dictionary<string, string>> Skills { get; private set; }
        public static List<Dictionary<string, string>> Items { get; private set; }
        public static List<Dictionary<string, string>> DropGroups { get; private set; }
        public static List<Dictionary<string, string>> Stages { get; private set; }
        public static List<Dictionary<string, string>> StageWaves { get; private set; }
        public static List<Dictionary<string, string>> LevelExp { get; private set; }
        public static List<Dictionary<string, string>> Shop { get; private set; }
        public static List<Dictionary<string, string>> FieldObjects { get; private set; }

        public static DataLoadError LoadError { get; private set; }
        public static bool IsLoaded { get { return _loaded; } }

        private static Dictionary<string, string> _strings;
        private static Dictionary<string, string> _consts;
        private static Dictionary<int, Dictionary<string, string>> _monsterById;
        private static Dictionary<int, Dictionary<string, string>> _skillById;
        private static Dictionary<int, Dictionary<string, string>> _itemById;
        private static Dictionary<int, Dictionary<string, string>> _stageById;
        private static Dictionary<int, List<Dictionary<string, string>>> _dropRowsByGroup;
        private static bool _loaded;

        public static void Load()
        {
            if (_loaded)
            {
                return;
            }

            LoadError = null;
            Monsters = LoadOne("monster");
            Skills = LoadOne("skill");
            Items = LoadOne("item");
            DropGroups = LoadOne("drop_group");
            Stages = LoadOne("stage");
            StageWaves = LoadOne("stage_wave");
            LevelExp = LoadOne("level_exp");
            Shop = LoadOne("shop");
            FieldObjects = LoadOne("field_object");

            _strings = new Dictionary<string, string>();
            foreach (Dictionary<string, string> row in LoadOne("string"))
            {
                _strings[DataLoader.ReadText(row, "key")] = DataLoader.ReadText(row, "ko");
            }

            _consts = new Dictionary<string, string>();
            foreach (Dictionary<string, string> row in LoadOne("const"))
            {
                _consts[DataLoader.ReadText(row, "key")] = DataLoader.ReadText(row, "value");
            }
            LoadOne("enum");

            _monsterById = BuildIndex(Monsters, "monster_id");
            _skillById = BuildIndex(Skills, "skill_id");
            _itemById = BuildIndex(Items, "item_id");
            _stageById = BuildIndex(Stages, "stage_id");

            _dropRowsByGroup = new Dictionary<int, List<Dictionary<string, string>>>();
            foreach (Dictionary<string, string> row in DropGroups)
            {
                int groupId = DataLoader.ReadInt(row, "drop_group_id", 0);
                if (!_dropRowsByGroup.ContainsKey(groupId))
                {
                    _dropRowsByGroup[groupId] = new List<Dictionary<string, string>>();
                }
                _dropRowsByGroup[groupId].Add(row);
            }

            _loaded = true;
        }

        public static void Reload()
        {
            _loaded = false;
            Load();
        }

        private static List<Dictionary<string, string>> LoadOne(string tableName)
        {
            DataLoadError error;
            List<Dictionary<string, string>> rows = DataLoader.LoadTableChecked(tableName, out error);
            if (error != null && LoadError == null)
            {
                LoadError = error;
            }
            return rows;
        }

        private static Dictionary<int, Dictionary<string, string>> BuildIndex(
            List<Dictionary<string, string>> rows, string keyColumn)
        {
            Dictionary<int, Dictionary<string, string>> index =
                new Dictionary<int, Dictionary<string, string>>();
            foreach (Dictionary<string, string> row in rows)
            {
                int key = DataLoader.ReadInt(row, keyColumn, 0);
                if (!index.ContainsKey(key))
                {
                    index[key] = row;
                }
            }
            return index;
        }

        public static string GetString(string key)
        {
            Load();
            string value;
            if (_strings.TryGetValue(key, out value))
            {
                return value;
            }
            return key;
        }

        public static int GetConstInt(string key, int fallback)
        {
            Load();
            string text;
            if (!_consts.TryGetValue(key, out text))
            {
                return fallback;
            }
            int value;
            if (!int.TryParse(text, out value))
            {
                return fallback;
            }
            return value;
        }

        public static bool GetConstBool(string key, bool fallback)
        {
            Load();
            string text;
            if (!_consts.TryGetValue(key, out text))
            {
                return fallback;
            }
            return text == "TRUE";
        }

        public static Dictionary<string, string> GetMonster(int monsterId)
        {
            Load();
            Dictionary<string, string> row;
            if (_monsterById.TryGetValue(monsterId, out row))
            {
                return row;
            }
            return null;
        }

        public static Dictionary<string, string> GetMonsterByPrefabKey(string prefabKey)
        {
            Load();
            if (string.IsNullOrEmpty(prefabKey))
            {
                return null;
            }
            foreach (Dictionary<string, string> row in Monsters)
            {
                if (DataLoader.ReadText(row, "prefab_key") == prefabKey)
                {
                    return row;
                }
            }
            return null;
        }

        public static Dictionary<string, string> GetSkill(int skillId)
        {
            Load();
            Dictionary<string, string> row;
            if (_skillById.TryGetValue(skillId, out row))
            {
                return row;
            }
            return null;
        }

        public static Dictionary<string, string> GetItem(int itemId)
        {
            Load();
            Dictionary<string, string> row;
            if (_itemById.TryGetValue(itemId, out row))
            {
                return row;
            }
            return null;
        }

        public static Dictionary<string, string> GetStage(int stageId)
        {
            Load();
            Dictionary<string, string> row;
            if (_stageById.TryGetValue(stageId, out row))
            {
                return row;
            }
            return null;
        }

        public static List<Dictionary<string, string>> GetDropRows(int dropGroupId)
        {
            Load();
            List<Dictionary<string, string>> rows;
            if (_dropRowsByGroup.TryGetValue(dropGroupId, out rows))
            {
                return rows;
            }
            return new List<Dictionary<string, string>>();
        }

        public static List<Dictionary<string, string>> GetFieldRows(int stageId)
        {
            Load();
            List<Dictionary<string, string>> rows = new List<Dictionary<string, string>>();
            foreach (Dictionary<string, string> row in FieldObjects)
            {
                if (DataLoader.ReadInt(row, "stage_id", 0) == stageId)
                {
                    rows.Add(row);
                }
            }
            return rows;
        }

        public static bool IsPlayableStage(int stageId)
        {
            return GetFieldRows(stageId).Count > 0;
        }

        public static List<Dictionary<string, string>> GetChapterStages(int chapter)
        {
            Load();
            List<Dictionary<string, string>> rows = new List<Dictionary<string, string>>();
            foreach (Dictionary<string, string> row in Stages)
            {
                if (DataLoader.ReadInt(row, "chapter", 0) == chapter)
                {
                    rows.Add(row);
                }
            }
            rows.Sort((left, right) =>
                DataLoader.ReadInt(left, "stage_no", 0).CompareTo(DataLoader.ReadInt(right, "stage_no", 0)));
            return rows;
        }

        public static List<Dictionary<string, string>> GetVisibleShopRows()
        {
            Load();
            List<Dictionary<string, string>> rows = new List<Dictionary<string, string>>();
            foreach (Dictionary<string, string> row in Shop)
            {
                if (DataLoader.ReadBool(row, "is_visible"))
                {
                    rows.Add(row);
                }
            }
            rows.Sort((left, right) =>
                DataLoader.ReadInt(left, "sort_order", 0).CompareTo(DataLoader.ReadInt(right, "sort_order", 0)));
            return rows;
        }

        public static List<Dictionary<string, string>> GetDexMonsters()
        {
            Load();
            bool showUnreleased = GetConstBool("DEX_SHOW_UNRELEASED", false);
            List<Dictionary<string, string>> visible = new List<Dictionary<string, string>>();
            foreach (Dictionary<string, string> row in Monsters)
            {
                if (showUnreleased || DataLoader.ReadBool(row, "is_dex_visible"))
                {
                    visible.Add(row);
                }
            }
            return visible;
        }

        public static int GoldItemId
        {
            get { return GetConstInt("GOLD_ITEM_ID", 203001); }
        }
    }
}
