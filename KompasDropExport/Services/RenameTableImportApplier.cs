using KompasDropExport.Domain;
using System;
using System.Collections.Generic;
using System.Linq;

namespace KompasDropExport.Services
{
    internal sealed class ImportApplyResult
    {
        public int Applied;
        public int SkippedNoMatch;
        public List<string> DuplicateNames = new List<string>();
    }

    internal static class RenameTableImportApplier
    {
        public static ImportApplyResult ApplyByFileName(IList<FileRecord> tableRows, IList<CsvRow> imported)
        {
            if (tableRows == null) throw new ArgumentNullException("tableRows");
            if (imported == null) throw new ArgumentNullException("imported");

            var result = new ImportApplyResult();

            // дубли имён в текущей таблице — опасно
            result.DuplicateNames = tableRows
                .GroupBy(r => r.FileName ?? "", StringComparer.OrdinalIgnoreCase)
                .Where(g => g.Count() > 1 && !string.IsNullOrWhiteSpace(g.Key))
                .Select(g => g.Key)
                .ToList();

            if (result.DuplicateNames.Count > 0)
                return result;

            var map = tableRows.ToDictionary(r => r.FileName ?? "", r => r, StringComparer.OrdinalIgnoreCase);

            foreach (var row in imported)
            {
                FileRecord rec;
                if (!map.TryGetValue(row.FileName ?? "", out rec))
                {
                    result.SkippedNoMatch++;
                    continue;
                }

                var newMark = row.Marking ?? "";
                var newName = row.Name ?? "";

                bool changed = false;

                if (!string.Equals(rec.Marking ?? "", newMark, StringComparison.Ordinal))
                {
                    rec.Marking = newMark;
                    rec.MarkingState = CellState.Dirty;
                    changed = true;
                }

                if (!string.Equals(rec.Name ?? "", newName, StringComparison.Ordinal))
                {
                    rec.Name = newName;
                    rec.NameState = CellState.Dirty;
                    changed = true;
                }

                if (changed)
                {
                    rec.Status = "Изменено";
                    result.Applied++;
                }
            }

            return result;
        }
    }
}