using System;
using System.Collections.Generic;
using System.IO;

using NPOI.SS.UserModel;

namespace KompasDropExport.Services
{
    internal static class ExcelImportService
    {
        // Ожидаемый порядок колонок:
        // A: Обозначение
        // B: Наименование
        // C: Тип (ИГНОРИРУЕМ)
        // D: Имя файла (ключ для сопоставления)
        public static List<CsvRow> Read(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentNullException(nameof(filePath));
            if (!File.Exists(filePath))
                throw new FileNotFoundException("Excel file not found", filePath);

            var rows = new List<CsvRow>();

            using (var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                IWorkbook wb = WorkbookFactory.Create(fs); // сам определит xls/xlsx
                try
                {
                    var ws = wb.GetSheet("Переименование") ?? wb.GetSheetAt(0);
                    if (ws == null) return rows;

                    // row 0 = header
                    for (int r = 1; r <= ws.LastRowNum; r++)
                    {
                        var rr = ws.GetRow(r);
                        if (rr == null) continue;

                        string marking = GetString(rr.GetCell(0));
                        string name = GetString(rr.GetCell(1));
                        // string kind = GetString(rr.GetCell(2)); // игнор
                        string fileName = GetString(rr.GetCell(3));

                        if (string.IsNullOrWhiteSpace(fileName))
                            break; // стоп по пустому ключу

                        rows.Add(new CsvRow
                        {
                            FileName = fileName.Trim(),
                            Marking = (marking ?? "").Trim(),
                            Name = (name ?? "").Trim()
                        });

                        if (r > 200000) break; // защита
                    }
                }
                finally
                {
                    wb.Close();
                }
            }

            return rows;
        }

        private static string GetString(ICell cell)
        {
            if (cell == null) return "";

            try
            {
                switch (cell.CellType)
                {
                    case CellType.String:
                        return cell.StringCellValue ?? "";

                    case CellType.Numeric:
                        return cell.NumericCellValue.ToString();

                    case CellType.Boolean:
                        return cell.BooleanCellValue ? "TRUE" : "FALSE";

                    case CellType.Formula:
                        // берём кэш результата формулы
                        if (cell.CachedFormulaResultType == CellType.String)
                            return cell.StringCellValue ?? "";
                        if (cell.CachedFormulaResultType == CellType.Numeric)
                            return cell.NumericCellValue.ToString();
                        if (cell.CachedFormulaResultType == CellType.Boolean)
                            return cell.BooleanCellValue ? "TRUE" : "FALSE";
                        return cell.ToString() ?? "";

                    default:
                        return cell.ToString() ?? "";
                }
            }
            catch
            {
                return cell.ToString() ?? "";
            }
        }
    }

    // Если CsvRow у тебя уже есть в другом файле — УДАЛИ этот класс отсюда.
    // Оставил на случай, если ты реально выкинул CSV и класс пропал.

}