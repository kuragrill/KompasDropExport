using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace KompasDropExport.Services
{
    internal sealed class CsvRow
    {
        public string FileName;
        public string Marking;
        public string Name;
    }

    internal static class CsvImportService
    {
        // Формат: Имя файла;Обозначение;Наименование (UTF-8 with BOM)
        public static List<CsvRow> Read(string filePath)
        {
            if (string.IsNullOrEmpty(filePath)) throw new ArgumentNullException("filePath");

            var rows = new List<CsvRow>();

            using (var sr = new StreamReader(filePath, Encoding.UTF8, true))
            {
                bool first = true;
                string line;

                while ((line = sr.ReadLine()) != null)
                {
                    if (first)
                    {
                        first = false;
                        // пропускаем заголовок (если он есть)
                        // если заголовка нет — всё равно ок, просто попадёт как строка,
                        // но мы ниже отфильтруем по пустому имени файла
                        continue;
                    }

                    if (string.IsNullOrWhiteSpace(line)) continue;

                    var parts = ParseCsvLineSemicolon(line);
                    if (parts.Count < 1) continue;

                    var fn = SafeGet(parts, 0);
                    if (string.IsNullOrWhiteSpace(fn)) continue;

                    rows.Add(new CsvRow
                    {
                        FileName = fn,
                        Marking = SafeGet(parts, 1),
                        Name = SafeGet(parts, 2),
                    });
                }
            }

            return rows;
        }

        private static string SafeGet(List<string> parts, int index)
        {
            if (index < 0 || index >= parts.Count) return "";
            return parts[index] ?? "";
        }

        // Парсер CSV с разделителем ';' и кавычками "
        private static List<string> ParseCsvLineSemicolon(string line)
        {
            var res = new List<string>();
            if (line == null) { res.Add(""); return res; }

            var sb = new StringBuilder();
            bool inQuotes = false;

            for (int i = 0; i < line.Length; i++)
            {
                char c = line[i];

                if (inQuotes)
                {
                    if (c == '"')
                    {
                        // "" -> "
                        if (i + 1 < line.Length && line[i + 1] == '"')
                        {
                            sb.Append('"');
                            i++;
                        }
                        else
                        {
                            inQuotes = false;
                        }
                    }
                    else
                    {
                        sb.Append(c);
                    }
                }
                else
                {
                    if (c == ';')
                    {
                        res.Add(sb.ToString());
                        sb.Length = 0;
                    }
                    else if (c == '"')
                    {
                        inQuotes = true;
                    }
                    else
                    {
                        sb.Append(c);
                    }
                }
            }

            res.Add(sb.ToString());
            return res;
        }
    }
}