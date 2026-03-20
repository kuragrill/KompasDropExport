using System;
using System.Collections.Generic;
using System.IO;
using KompasDropExport.Domain.Analysis;
using KompasDropExport.UI.Analysis;

namespace KompasDropExport.Services.Analysis
{
    /// <summary>
    /// Собирает и валидирует план ручного переименования по строкам таблицы.
    /// Пока без применения.
    /// </summary>
    public static class RenamePlanBuilder
    {
        public sealed class RenamePlanResult
        {
            public List<RenameOperation> Operations { get; set; }
            public List<string> Errors { get; set; }

            public bool HasErrors
            {
                get { return Errors != null && Errors.Count > 0; }
            }

            public RenamePlanResult()
            {
                Operations = new List<RenameOperation>();
                Errors = new List<string>();
            }
        }

        public static RenamePlanResult Build(IList<RenameTableRow> rows)
        {
            var result = new RenamePlanResult();
            if (rows == null) return result;

            var modifiedRows = new List<RenameTableRow>();

            // 1) Собираем только изменённые строки
            for (int i = 0; i < rows.Count; i++)
            {
                var row = rows[i];
                if (row == null) continue;
                if (!row.IsModified) continue;

                modifiedRows.Add(row);
            }

            if (modifiedRows.Count == 0)
                return result;

            // 2) Базовая валидация каждой строки
            for (int i = 0; i < modifiedRows.Count; i++)
            {
                var row = modifiedRows[i];

                ValidateRow(row, result.Errors);
            }

            if (result.Errors.Count > 0)
                return result;

            // 3) Конфликты внутри самого плана:
            // два файла не должны хотеть стать одним и тем же путём
            var futurePathMap = new Dictionary<string, RenameTableRow>(StringComparer.OrdinalIgnoreCase);

            for (int i = 0; i < modifiedRows.Count; i++)
            {
                var row = modifiedRows[i];
                string newFullPath = BuildNewFullPath(row);

                RenameTableRow existing;
                if (futurePathMap.TryGetValue(newFullPath, out existing))
                {
                    result.Errors.Add(
                        "Конфликт переименования: два файла хотят получить один и тот же путь:\n" +
                        newFullPath);
                }
                else
                {
                    futurePathMap[newFullPath] = row;
                }
            }

            if (result.Errors.Count > 0)
                return result;

            // 4) Конфликты с уже существующими файлами на диске
            for (int i = 0; i < modifiedRows.Count; i++)
            {
                var row = modifiedRows[i];

                string oldFullPath = row.FullPath ?? "";
                string newFullPath = BuildNewFullPath(row);

                // если новый путь совпадает со старым — это уже не изменение
                if (string.Equals(oldFullPath, newFullPath, StringComparison.OrdinalIgnoreCase))
                    continue;

                if (File.Exists(newFullPath))
                {
                    result.Errors.Add(
                        "Файл с новым именем уже существует:\n" + newFullPath);
                }
            }

            if (result.Errors.Count > 0)
                return result;

            // 5) Строим операции
            for (int i = 0; i < modifiedRows.Count; i++)
            {
                var row = modifiedRows[i];

                string oldFullPath = row.FullPath ?? "";
                string newFullPath = BuildNewFullPath(row);

                if (string.Equals(oldFullPath, newFullPath, StringComparison.OrdinalIgnoreCase))
                    continue;

                result.Operations.Add(new RenameOperation
                {
                    NodeId = row.NodeId,
                    Kind = row.Kind,
                    OldFullPath = oldFullPath,
                    NewFullPath = newFullPath,
                    OldFileName = Path.GetFileName(oldFullPath),
                    NewFileName = Path.GetFileName(newFullPath)
                });
            }

            return result;
        }

        private static void ValidateRow(RenameTableRow row, List<string> errors)
        {
            if (row == null)
            {
                errors.Add("Обнаружена пустая строка в таблице переименования.");
                return;
            }

            string baseName = (row.BaseName ?? "").Trim();

            if (string.IsNullOrWhiteSpace(baseName))
            {
                errors.Add("Пустое имя файла: " + SafeDisplayPath(row));
                return;
            }

            if (baseName == "." || baseName == "..")
            {
                errors.Add("Недопустимое имя файла: " + baseName);
                return;
            }

            char[] invalidChars = Path.GetInvalidFileNameChars();
            for (int i = 0; i < invalidChars.Length; i++)
            {
                if (baseName.IndexOf(invalidChars[i]) >= 0)
                {
                    errors.Add("Имя содержит недопустимый символ '" + invalidChars[i] + "': " + baseName);
                    return;
                }
            }

            if (string.IsNullOrWhiteSpace(row.Extension))
            {
                errors.Add("Не удалось определить расширение файла: " + SafeDisplayPath(row));
                return;
            }

            if (string.IsNullOrWhiteSpace(row.FullPath))
            {
                errors.Add("У строки отсутствует исходный путь: " + (row.FileName ?? "<без имени>"));
                return;
            }

            string dir = null;
            try { dir = Path.GetDirectoryName(row.FullPath); }
            catch { dir = null; }

            if (string.IsNullOrWhiteSpace(dir))
            {
                errors.Add("Не удалось определить папку файла: " + row.FullPath);
                return;
            }

            // Защитимся от случайного включения расширения в BaseName
            if (baseName.EndsWith(row.Extension, StringComparison.OrdinalIgnoreCase))
            {
                errors.Add(
                    "В редактируемом имени не нужно указывать расширение файла: " +
                    baseName + "\nОжидается имя без " + row.Extension);
                return;
            }
        }

        private static string BuildNewFullPath(RenameTableRow row)
        {
            string dir = Path.GetDirectoryName(row.FullPath) ?? "";
            string newFileName = (row.BaseName ?? "") + (row.Extension ?? "");
            return Path.Combine(dir, newFileName);
        }

        private static string SafeDisplayPath(RenameTableRow row)
        {
            if (row == null) return "<null>";
            return !string.IsNullOrWhiteSpace(row.FullPath)
                ? row.FullPath
                : (row.FileName ?? "<без имени>");
        }
    }
}