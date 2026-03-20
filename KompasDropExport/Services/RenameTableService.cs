using KompasDropExport.Domain;
using KompasDropExport.Kompas;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace KompasDropExport.Services
{
    internal sealed class RenameTableService
    {
        /// <summary>
        /// Читает свойства для всех строк (только 3D), одним батчем в SilentScope.
        /// </summary>
        public void ReadAllProps(IList<FileRecord> rows, Action<int, int> progress)
        {
            if (rows == null || rows.Count == 0) return;

            using (var host = new KompasHost())
            {
                host.AttachOrStart();

                using (host.SilentScope())
                {
                    var reader = new KompasPropertyReader(host.App7);

                    int total = rows.Count;
                    for (int i = 0; i < total; i++)
                    {
                        var r = rows[i];

                        if (!Is3D(r.FullPath))
                        {
                            r.Status = "Пропуск (не 3D)";
                            if (progress != null) progress(i + 1, total);
                            continue;
                        }

                        dynamic doc = null;
                        try
                        {
                            doc = host.OpenDocument(r.FullPath, readOnly: true, visible: false);
                            if (doc == null)
                            {
                                r.Status = "Не удалось открыть";
                                if (progress != null) progress(i + 1, total);
                                continue;
                            }

                            var nm = reader.Read3D(doc);

                            r.Marking = nm.Marking ?? "";
                            r.Name = nm.Name ?? "";

                            r.MarkingState = CellState.Clean;
                            r.NameState = CellState.Clean;
                            r.Status = "Прочитано";
                        }
                        catch (Exception ex)
                        {
                            r.Status = "Ошибка чтения: " + ex.Message;
                        }
                        finally
                        {
                            TryClose(doc, false);
                        }

                        if (progress != null) progress(i + 1, total);
                    }
                }
            }
        }

        /// <summary>
        /// Записывает только изменённые marking/name (только 3D), одним батчем в SilentScope.
        /// </summary>
        public void WriteChanged(IList<FileRecord> rows, Action<int, int> progress)
        {
            if (rows == null || rows.Count == 0) return;

            var changed = rows
                .Where(r => Is3D(r.FullPath) &&
                            (r.MarkingState == CellState.Dirty || r.NameState == CellState.Dirty))
                .ToList();

            if (changed.Count == 0) return;

            using (var host = new KompasHost())
            {
                host.AttachOrStart();

                using (host.SilentScope())
                {
                    int total = changed.Count;
                    for (int i = 0; i < total; i++)
                    {
                        var r = changed[i];

                        dynamic doc = null;
                        try
                        {
                            doc = host.OpenDocument(r.FullPath, readOnly: false, visible: false);
                            if (doc == null)
                            {
                                MarkWriteError(r, "Не удалось открыть");
                                if (progress != null) progress(i + 1, total);
                                continue;
                            }

                            dynamic d3 = doc;
                            object topPart = null;
                            try { topPart = d3.TopPart; } catch { }

                            if (topPart == null)
                            {
                                MarkWriteError(r, "TopPart недоступен");
                                if (progress != null) progress(i + 1, total);
                                continue;
                            }

                            // пишем только то, что реально Dirty
                            if (r.MarkingState == CellState.Dirty)
                            {
                                try { ((dynamic)topPart).marking = r.Marking ?? ""; }
                                catch { MarkWriteError(r, "Не удалось записать обозначение"); }
                            }

                            if (r.NameState == CellState.Dirty)
                            {
                                try { ((dynamic)topPart).name = r.Name ?? ""; }
                                catch { MarkWriteError(r, "Не удалось записать наименование"); }
                            }

                            TrySave(doc);

                            // успех
                            if (r.MarkingState == CellState.Dirty) r.MarkingState = CellState.WrittenOk;
                            if (r.NameState == CellState.Dirty) r.NameState = CellState.WrittenOk;
                            r.Status = "Записано";
                        }
                        catch (Exception ex)
                        {
                            MarkWriteError(r, ex.Message);
                        }
                        finally
                        {
                            TryClose(doc, true);
                        }

                        if (progress != null) progress(i + 1, total);
                    }
                }
            }
        }

        private static bool Is3D(string path)
        {
            var ext = Path.GetExtension(path);
            if (ext == null) return false;
            ext = ext.ToLowerInvariant();
            return ext == ".m3d" || ext == ".a3d";
        }

        private static void TrySave(dynamic doc)
        {
            if (doc == null) return;
            try { doc.Save(); } catch { }
        }

        private static void TryClose(dynamic doc, bool save)
        {
            if (doc == null) return;
            try { doc.Close(save); }
            catch { try { doc.Close(); } catch { } }
        }

        private static void MarkWriteError(FileRecord r, string msg)
        {
            if (r.MarkingState == CellState.Dirty) r.MarkingState = CellState.WriteError;
            if (r.NameState == CellState.Dirty) r.NameState = CellState.WriteError;
            r.Status = "Ошибка: " + msg;
        }
    }
}