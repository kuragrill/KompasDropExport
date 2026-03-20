using System;
using System.Collections.Generic;
using System.IO;
using KompasDropExport.Domain.Analysis;

namespace KompasDropExport.Services.Analysis
{
    /// <summary>
    /// Выполняет физическое переименование файлов на диске.
    /// Пока без обновления ссылок КОМПАС.
    ///
    /// ВАЖНО:
    /// - операции сортируются
    /// - используется двухфазное переименование через временные имена
    ///   для защиты от цепочек A->B, B->C и внутренних коллизий
    /// </summary>
    public static class RenameExecutor
    {
        public sealed class RenameExecutionResult
        {
            public List<string> Errors { get; } = new List<string>();
            public int SuccessCount { get; set; }

            public bool HasErrors
            {
                get { return Errors.Count > 0; }
            }
        }

        private sealed class ExecutionItem
        {
            public RenameOperation Operation { get; set; }
            public string TempFullPath { get; set; }
        }

        public static RenameExecutionResult Execute(IList<RenameOperation> operations)
        {
            var result = new RenameExecutionResult();

            if (operations == null || operations.Count == 0)
                return result;

            // 1) Копируем и сортируем операции
            var sorted = new List<RenameOperation>();
            for (int i = 0; i < operations.Count; i++)
            {
                var op = operations[i];
                if (op != null)
                    sorted.Add(op);
            }

            sorted.Sort(CompareOperations);

            // 2) Подготовка и дополнительная валидация
            var items = new List<ExecutionItem>();
            var oldPathSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            for (int i = 0; i < sorted.Count; i++)
            {
                var op = sorted[i];

                if (string.IsNullOrWhiteSpace(op.OldFullPath) ||
                    string.IsNullOrWhiteSpace(op.NewFullPath))
                {
                    result.Errors.Add("Некорректная операция переименования: пустой путь.");
                    continue;
                }

                oldPathSet.Add(op.OldFullPath);
            }

            if (result.Errors.Count > 0)
                return result;

            for (int i = 0; i < sorted.Count; i++)
            {
                var op = sorted[i];

                try
                {
                    if (!File.Exists(op.OldFullPath))
                    {
                        result.Errors.Add("Файл не найден:\n" + op.OldFullPath);
                        continue;
                    }

                    // Если целевой путь уже существует и это не один из старых путей внутри плана,
                    // значит конфликт внешний — останавливаться.
                    if (File.Exists(op.NewFullPath) && !oldPathSet.Contains(op.NewFullPath))
                    {
                        result.Errors.Add("Целевой файл уже существует вне плана переименования:\n" + op.NewFullPath);
                        continue;
                    }

                    items.Add(new ExecutionItem
                    {
                        Operation = op,
                        TempFullPath = BuildTempPath(op.OldFullPath)
                    });
                }
                catch (Exception ex)
                {
                    result.Errors.Add(
                        "Ошибка подготовки операции:\n" +
                        op.OldFullPath + "\n" + ex.Message);
                }
            }

            if (result.Errors.Count > 0)
                return result;

            var movedToTemp = new List<ExecutionItem>();
            var movedToFinal = new List<ExecutionItem>();

            try
            {
                // 3) Фаза 1: все old -> temp
                for (int i = 0; i < items.Count; i++)
                {
                    var item = items[i];

                    File.Move(item.Operation.OldFullPath, item.TempFullPath);
                    movedToTemp.Add(item);
                }

                // 4) Фаза 2: все temp -> new
                for (int i = 0; i < items.Count; i++)
                {
                    var item = items[i];

                    // На этом этапе old-пути уже освобождены, поэтому внутренние цепочки безопасны.
                    if (File.Exists(item.Operation.NewFullPath))
                    {
                        throw new IOException(
                            "Целевой путь всё ещё занят после временного переименования:\n" +
                            item.Operation.NewFullPath);
                    }

                    File.Move(item.TempFullPath, item.Operation.NewFullPath);
                    movedToFinal.Add(item);
                    result.SuccessCount++;
                }

                return result;
            }
            catch (Exception ex)
            {
                result.Errors.Add("Ошибка выполнения переименования:\n" + ex.Message);

                // 5) Best-effort rollback
                TryRollback(movedToFinal, movedToTemp, result);
                result.SuccessCount = 0;
                return result;
            }
        }

        private static void TryRollback(
            List<ExecutionItem> movedToFinal,
            List<ExecutionItem> movedToTemp,
            RenameExecutionResult result)
        {
            // Сначала возвращаем уже финализированные new -> old
            for (int i = movedToFinal.Count - 1; i >= 0; i--)
            {
                var item = movedToFinal[i];
                try
                {
                    if (File.Exists(item.Operation.NewFullPath))
                    {
                        // Если old уже существует, пропускаем чтобы не сделать хуже
                        if (!File.Exists(item.Operation.OldFullPath))
                            File.Move(item.Operation.NewFullPath, item.Operation.OldFullPath);
                    }
                }
                catch (Exception ex)
                {
                    result.Errors.Add(
                        "Не удалось откатить финальное переименование:\n" +
                        item.Operation.NewFullPath + " -> " + item.Operation.OldFullPath +
                        "\n" + ex.Message);
                }
            }

            // Потом возвращаем temp -> old для тех, кто не дошёл до финала
            for (int i = movedToTemp.Count - 1; i >= 0; i--)
            {
                var item = movedToTemp[i];

                // Если temp уже отсутствует, значит этот item либо уже откатили,
                // либо он успел уйти в финал.
                if (!File.Exists(item.TempFullPath))
                    continue;

                try
                {
                    if (!File.Exists(item.Operation.OldFullPath))
                        File.Move(item.TempFullPath, item.Operation.OldFullPath);
                }
                catch (Exception ex)
                {
                    result.Errors.Add(
                        "Не удалось откатить временное переименование:\n" +
                        item.TempFullPath + " -> " + item.Operation.OldFullPath +
                        "\n" + ex.Message);
                }
            }
        }

        private static string BuildTempPath(string oldFullPath)
        {
            string dir = Path.GetDirectoryName(oldFullPath) ?? "";
            string ext = Path.GetExtension(oldFullPath) ?? "";
            string name = Path.GetFileNameWithoutExtension(oldFullPath) ?? "tmp";

            string tempName =
                name +
                ".__rename_tmp__" +
                Guid.NewGuid().ToString("N") +
                ext;

            return Path.Combine(dir, tempName);
        }

        private static int CompareOperations(RenameOperation a, RenameOperation b)
        {
            int ak = GetKindPriority(a != null ? a.Kind : null);
            int bk = GetKindPriority(b != null ? b.Kind : null);

            int c = ak.CompareTo(bk);
            if (c != 0) return c;

            return string.Compare(
                a != null ? a.OldFullPath : null,
                b != null ? b.OldFullPath : null,
                StringComparison.OrdinalIgnoreCase);
        }

        private static int GetKindPriority(string kind)
        {
            if (string.IsNullOrWhiteSpace(kind))
                return 99;

            // Сначала детали, потом чертежи, потом спецификации, потом сборки
            if (string.Equals(kind, "Деталь", StringComparison.OrdinalIgnoreCase))
                return 0;

            if (string.Equals(kind, "Чертёж", StringComparison.OrdinalIgnoreCase))
                return 1;

            if (string.Equals(kind, "Спецификация", StringComparison.OrdinalIgnoreCase))
                return 2;

            if (string.Equals(kind, "Сборка", StringComparison.OrdinalIgnoreCase))
                return 3;

            return 99;
        }
    }
}