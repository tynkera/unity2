using System.Collections.Generic;

namespace ExpeditionCamp
{
    public class DataLoadError
    {
        public string Sheet;
        public int Row;
        public string Column;
        public string Reason;

        public DataLoadError(string sheet, int row, string column, string reason)
        {
            Sheet = sheet;
            Row = row;
            Column = column;
            Reason = reason;
        }

        public string ToMessage()
        {
            return "시트 " + Sheet + " · " + Row + "행 · 열 " + (string.IsNullOrEmpty(Column) ? "-" : Column) + " · " + Reason;
        }
    }

    public static class DataSchema
    {
        private class TableRule
        {
            public string[] Required;
            public string[] IntColumns;
            public string[] BoolColumns;

            public TableRule(string[] required, string[] intColumns, string[] boolColumns)
            {
                Required = required;
                IntColumns = intColumns;
                BoolColumns = boolColumns;
            }
        }

        public static readonly string[] TableNames =
        {
            "monster", "skill", "item", "drop_group", "stage", "stage_wave",
            "level_exp", "const", "enum", "string", "shop", "field_object"
        };

        private static readonly Dictionary<string, TableRule> Rules = new Dictionary<string, TableRule>
        {
            {
                "monster", new TableRule(
                    new[] { "monster_id", "name_key", "base_hp", "base_atk", "drop_group_id", "icon_path", "is_dex_visible", "prefab_key" },
                    new[] { "monster_id", "base_hp", "base_atk", "base_def", "drop_group_id" },
                    new[] { "is_dex_visible" })
            },
            {
                "skill", new TableRule(
                    new[] { "skill_id", "name_key" },
                    new[] { "skill_id" },
                    new string[0])
            },
            {
                "item", new TableRule(
                    new[] { "item_id", "name_key", "item_type", "max_stack", "icon_path" },
                    new[] { "item_id", "max_stack", "sell_price" },
                    new string[0])
            },
            {
                "drop_group", new TableRule(
                    new[] { "drop_group_id", "seq", "drop_type", "item_id", "min_count", "max_count", "prob" },
                    new[] { "drop_group_id", "seq", "item_id", "min_count", "max_count", "prob" },
                    new string[0])
            },
            {
                "stage", new TableRule(
                    new[] { "stage_id", "chapter", "stage_no", "name_key", "prev_stage_id", "scene_name" },
                    new[] { "stage_id", "chapter", "stage_no", "prev_stage_id" },
                    new string[0])
            },
            {
                "stage_wave", new TableRule(
                    new[] { "stage_id", "wave_no", "slot", "monster_id", "monster_level" },
                    new[] { "stage_id", "wave_no", "slot", "monster_id", "monster_level" },
                    new string[0])
            },
            {
                "level_exp", new TableRule(
                    new[] { "level", "required_exp" },
                    new[] { "level", "required_exp" },
                    new string[0])
            },
            {
                "const", new TableRule(
                    new[] { "key", "value" },
                    new string[0],
                    new string[0])
            },
            {
                "enum", new TableRule(
                    new[] { "enum_name", "value" },
                    new string[0],
                    new string[0])
            },
            {
                "string", new TableRule(
                    new[] { "key", "ko" },
                    new string[0],
                    new string[0])
            },
            {
                "shop", new TableRule(
                    new[] { "shop_slot_id", "item_id", "price", "currency_item_id", "sort_order", "is_visible" },
                    new[] { "shop_slot_id", "item_id", "price", "currency_item_id", "sort_order" },
                    new[] { "is_visible" })
            },
            {
                "field_object", new TableRule(
                    new[] { "stage_id", "spawn_point_key", "wave_no", "monster_id", "count" },
                    new[] { "stage_id", "wave_no", "monster_id", "count" },
                    new string[0])
            },
        };

        public static DataLoadError Validate(string tableName, string text, List<Dictionary<string, string>> rows)
        {
            TableRule rule;
            if (!Rules.TryGetValue(tableName, out rule))
            {
                return null;
            }

            string firstLine = text;
            int lineEnd = text.IndexOf('\n');
            if (lineEnd >= 0)
            {
                firstLine = text.Substring(0, lineEnd);
            }
            List<string> header = DataLoader.ParseLine(firstLine.TrimEnd('\r'));
            foreach (string column in rule.Required)
            {
                if (!header.Contains(column))
                {
                    return new DataLoadError(tableName, 1, column, "필요한 열이 없습니다");
                }
            }

            for (int rowIndex = 0; rowIndex < rows.Count; rowIndex++)
            {
                Dictionary<string, string> row = rows[rowIndex];
                int csvRow = rowIndex + 2;
                foreach (string column in rule.IntColumns)
                {
                    string value = DataLoader.ReadText(row, column);
                    int parsed;
                    if (!int.TryParse(value, out parsed))
                    {
                        return new DataLoadError(tableName, csvRow, column, "정수가 아닌 값입니다: \"" + value + "\"");
                    }
                }
                foreach (string column in rule.BoolColumns)
                {
                    string value = DataLoader.ReadText(row, column);
                    if (value != "TRUE" && value != "FALSE")
                    {
                        return new DataLoadError(tableName, csvRow, column, "TRUE 또는 FALSE 가 아닌 값입니다: \"" + value + "\"");
                    }
                }
            }
            return null;
        }
    }
}
