using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

namespace ExpeditionCamp
{
    public static class DataLoader
    {
        public static List<string> ParseLine(string line)
        {
            List<string> values = new List<string>();
            StringBuilder field = new StringBuilder();
            bool inQuotes = false;
            int index = 0;

            while (index < line.Length)
            {
                char letter = line[index];

                if (inQuotes)
                {
                    if (letter == '"')
                    {
                        if (index + 1 < line.Length && line[index + 1] == '"')
                        {
                            field.Append('"');
                            index += 2;
                            continue;
                        }
                        inQuotes = false;
                        index += 1;
                        continue;
                    }
                    field.Append(letter);
                    index += 1;
                    continue;
                }

                if (letter == '"')
                {
                    inQuotes = true;
                    index += 1;
                    continue;
                }
                if (letter == ',')
                {
                    values.Add(field.ToString());
                    field.Length = 0;
                    index += 1;
                    continue;
                }
                field.Append(letter);
                index += 1;
            }

            values.Add(field.ToString());
            return values;
        }

        public static List<Dictionary<string, string>> ParseCsv(string text)
        {
            List<Dictionary<string, string>> rows = new List<Dictionary<string, string>>();
            if (string.IsNullOrEmpty(text))
            {
                return rows;
            }

            string[] lines = text.Replace("\r\n", "\n").Replace("\r", "\n").Split('\n');
            if (lines.Length == 0)
            {
                return rows;
            }

            List<string> header = ParseLine(lines[0]);
            for (int lineIndex = 1; lineIndex < lines.Length; lineIndex++)
            {
                string line = lines[lineIndex];
                if (line.Length == 0)
                {
                    continue;
                }
                List<string> values = ParseLine(line);
                Dictionary<string, string> row = new Dictionary<string, string>();
                for (int columnIndex = 0; columnIndex < header.Count; columnIndex++)
                {
                    string value = "";
                    if (columnIndex < values.Count)
                    {
                        value = values[columnIndex];
                    }
                    row[header[columnIndex]] = value;
                }
                rows.Add(row);
            }
            return rows;
        }

        public static string DataDirectory()
        {
            return Path.Combine(Application.streamingAssetsPath, "data");
        }

        public static List<Dictionary<string, string>> LoadTable(string tableName)
        {
            DataLoadError error;
            List<Dictionary<string, string>> rows = LoadTableChecked(tableName, out error);
            if (error != null)
            {
                Debug.LogError("[Data] " + error.ToMessage());
            }
            return rows;
        }

        public static List<Dictionary<string, string>> LoadTableChecked(string tableName, out DataLoadError error)
        {
            error = null;
            string path = Path.Combine(DataDirectory(), tableName + ".csv");
            if (!File.Exists(path))
            {
                error = new DataLoadError(tableName, 0, "", "CSV 파일을 찾을 수 없습니다: " + path);
                return new List<Dictionary<string, string>>();
            }

            byte[] bytes = File.ReadAllBytes(path);
            string text;
            try
            {
                text = StrictUtf8.GetString(bytes);
            }
            catch (DecoderFallbackException)
            {
                error = FindEncodingError(tableName, bytes);
                return new List<Dictionary<string, string>>();
            }

            if (text.Length > 0 && text[0] == '\uFEFF')
            {
                text = text.Substring(1);
            }

            List<Dictionary<string, string>> rows = ParseCsv(text);
            error = DataSchema.Validate(tableName, text, rows);
            return rows;
        }

        private static readonly UTF8Encoding StrictUtf8 = new UTF8Encoding(false, true);

        private static DataLoadError FindEncodingError(string tableName, byte[] bytes)
        {
            string loose = Encoding.UTF8.GetString(bytes);
            string[] lines = loose.Replace("\r\n", "\n").Split('\n');
            List<string> header = lines.Length > 0 ? ParseLine(lines[0]) : new List<string>();
            for (int lineIndex = 0; lineIndex < lines.Length; lineIndex++)
            {
                if (lines[lineIndex].IndexOf('\uFFFD') < 0)
                {
                    continue;
                }
                List<string> values = ParseLine(lines[lineIndex]);
                string column = "";
                for (int columnIndex = 0; columnIndex < values.Count; columnIndex++)
                {
                    if (values[columnIndex].IndexOf('\uFFFD') >= 0 && columnIndex < header.Count)
                    {
                        column = header[columnIndex];
                        break;
                    }
                }
                return new DataLoadError(tableName, lineIndex + 1, column,
                    "UTF-8 로 읽을 수 없는 글자가 있습니다 (엑셀 · 메모장에서 다른 인코딩으로 저장한 파일일 수 있습니다)");
            }
            return new DataLoadError(tableName, 0, "", "UTF-8 로 읽을 수 없는 파일입니다");
        }

        public static int ReadInt(Dictionary<string, string> row, string column, int fallback)
        {
            string text;
            if (!row.TryGetValue(column, out text))
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

        public static string ReadText(Dictionary<string, string> row, string column)
        {
            string text;
            if (!row.TryGetValue(column, out text))
            {
                return "";
            }
            return text;
        }

        public static bool ReadBool(Dictionary<string, string> row, string column)
        {
            return ReadText(row, column) == "TRUE";
        }
    }
}
