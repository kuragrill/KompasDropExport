using System;
using System.Collections.Generic;
using System.IO;

using KompasDropExport.Domain;

using NPOI.SS.UserModel;
using NPOI.SS.Util;
using NPOI.XSSF.UserModel;

namespace KompasDropExport.Services
{
    internal static class ExcelExportService
    {
        // Excel columns:
        // A: Обозначение
        // B: Наименование
        // C: Тип (Деталь/Сборочная единица)
        // D: Имя файла
        public static void ExportRenameTable(string filePath, IEnumerable<FileRecord> rows)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentNullException(nameof(filePath));
            if (rows == null)
                throw new ArgumentNullException(nameof(rows));

            var dir = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrWhiteSpace(dir))
                Directory.CreateDirectory(dir);

            IWorkbook wb = new XSSFWorkbook();
            try
            {
                var ws = wb.CreateSheet("Переименование");

                // --- styles ---
                var headerFont = wb.CreateFont();
                headerFont.IsBold = true;

                var headerStyle = wb.CreateCellStyle();
                headerStyle.SetFont(headerFont);

                // --- header ---
                var headerRow = ws.CreateRow(0);
                CreateCell(headerRow, 0, "Обозначение", headerStyle);
                CreateCell(headerRow, 1, "Наименование", headerStyle);
                CreateCell(headerRow, 2, "Тип", headerStyle);
                CreateCell(headerRow, 3, "Имя файла", headerStyle);

                ws.CreateFreezePane(0, 1);

                // --- data ---
                int r = 1;
                foreach (var row in rows)
                {
                    var rr = ws.CreateRow(r);

                    string kind = DetectKind(row.FullPath);

                    CreateCell(rr, 0, row.Marking ?? "");
                    CreateCell(rr, 1, row.Name ?? "");
                    CreateCell(rr, 2, kind);
                    CreateCell(rr, 3, row.FileName ?? "");

                    r++;
                }

                // AutoFilter A1:D(last)
                int lastRow = Math.Max(1, r); // header always exists
                ws.SetAutoFilter(new CellRangeAddress(0, lastRow - 1, 0, 3));

                // ширина = символы * 256
                ws.SetColumnWidth(0, 22 * 256); // Обозначение
                ws.SetColumnWidth(1, 40 * 256); // Наименование
                ws.SetColumnWidth(2, 20 * 256); // Тип
                ws.SetColumnWidth(3, 45 * 256); // Имя файла

                using (var fs = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.Read))
                {
                    wb.Write(fs);
                }
            }
            finally
            {
                wb.Close();
            }
        }

        private static void CreateCell(IRow row, int col, string value, ICellStyle style = null)
        {
            var cell = row.CreateCell(col, CellType.String);
            cell.SetCellValue(value ?? "");
            if (style != null) cell.CellStyle = style;
        }

        private static string DetectKind(string fullPath)
        {
            var ext = (Path.GetExtension(fullPath) ?? "").ToLowerInvariant();
            if (ext == ".m3d") return "Деталь";
            if (ext == ".a3d") return "Сборочная единица";
            return "";
        }
    }
}