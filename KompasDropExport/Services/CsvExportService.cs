using KompasDropExport.Domain;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace KompasDropExport.Services
{
    internal static class CsvExportService
    {
        public static void ExportRenameTable(string filePath, IEnumerable<FileRecord> rows)
        {
            if (rows == null) throw new ArgumentNullException(nameof(rows));

            // UTF-8 with BOM, чтобы Excel нормально открыл кириллицу
            using (var sw = new StreamWriter(filePath, false, new UTF8Encoding(true)))
            {
                sw.WriteLine("Имя файла;Обозначение;Наименование");

                foreach (var r in rows)
                {
                    sw.WriteLine(
                        Escape(r.FileName) + ";" +
                        Escape(r.Marking) + ";" +
                        Escape(r.Name));
                }
            }
        }

        private static string Escape(string s)
        {
            if (string.IsNullOrEmpty(s)) return "";
            // CSV правила: если есть ; " \n — берём в кавычки, кавычки удваиваем
            bool needQuotes = s.IndexOfAny(new[] { ';', '"', '\n', '\r' }) >= 0;
            if (!needQuotes) return s;
            return "\"" + s.Replace("\"", "\"\"") + "\"";
        }
    }
}