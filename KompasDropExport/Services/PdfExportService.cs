using KompasDropExport.Domain;
using KompasDropExport.Kompas;
using KompasDropExport.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;

namespace KompasDropExport.Services
{
    internal sealed class PdfExportService
    {
        private readonly ExportPathPlanner _planner = new ExportPathPlanner();



        public ExportResult Export(
            IReadOnlyList<string> files,
            ExportOptions opt,
            Action<string> stage = null,
            Action onFileDone = null,
            Action<string> onPdfArchiveSkippedBadMarking = null
        )
        {
            if (files == null || files.Count == 0) return new ExportResult();

            var result = new ExportResult();

            using (var host = new KompasHost())
            {
                host.AttachOrStart();

                stage?.Invoke("Запущен отдельный экземпляр КОМПАС…");

                var reader = new KompasPropertyReader(host.App7);

                using (host.SilentScope())
                {
                    stage?.Invoke("КОМПАС переведен в тихий режим.");

                    for (int i = 0; i < files.Count; i++)
                    {
                        string path = files[i];
                        string shortName = Path.GetFileName(path) ?? path;

                        bool isCdw = ExportRules.IsCdw(path);
                        bool isSpw = ExportRules.IsSpw(path);

                        // PDF экспортируем только для 2D: cdw и spw
                        if (!isCdw && !isSpw) { result.Skip++; onFileDone?.Invoke(); continue; }

                        // Применяем опции
                        if (isSpw && opt.PdfExcludeSpecs) { result.Skip++; onFileDone?.Invoke(); continue; }
                        if (isCdw && opt.PdfExcludeAssemblyDrawingsByName && ExportRules.LooksLikeAssemblyDrawing(path))
                        { result.Skip++; onFileDone?.Invoke(); continue; }

                        dynamic doc = null;
                        try
                        {
                            stage?.Invoke($"[{i + 1}/{files.Count}] {shortName}: открытие…");
                            doc = host.OpenDocument7(path, readOnly: true, visible: false);
                            if (doc == null) { result.Err++; onFileDone?.Invoke(); continue; }

                            // читаем обозначение/наименование
                            stage?.Invoke($"[{i + 1}/{files.Count}] {shortName}: свойства…");
                            var nm = reader.Read2D(doc);

                            // 1) WORK всегда
                            string workDir = _planner.GetWorkDir(path, isPdf: true);
                            Directory.CreateDirectory(workDir);

                            string workBase = _planner.MakeWorkBaseName(path);
                            string workPdf = Path.Combine(workDir, workBase + ".pdf");

                            stage?.Invoke($"[{i + 1}/{files.Count}] {shortName}: PDF (work)…");
                            bool wOk = ExportToPdf(host, doc, workPdf);

                            // 2) ARCHIVE по условию
                            bool aOk = true;

                            bool archiveEligible = _planner.ShouldWriteArchive(nm.Marking);
                            if (archiveEligible)
                            {
                                string archDir = _planner.GetArchiveDir(path, isPdf: true);
                                Directory.CreateDirectory(archDir);

                                string archBase = _planner.MakeArchiveBaseName(nm.Marking, nm.Name, opt.ActiveNameSeparator);
                                if (!string.IsNullOrWhiteSpace(archBase))
                                {
                                    string archPdf = Path.Combine(archDir, archBase + ".pdf");
                                    stage?.Invoke($"[{i + 1}/{files.Count}] {shortName}: PDF (archive)…");
                                    aOk = ExportToPdf(host, doc, archPdf);
                                }
                            }
                            else
                            {
                                // обозначение не прошло маску => архив не пишем, но сообщаем
                                result.PdfArchiveSkippedBadMarking++;
                                onPdfArchiveSkippedBadMarking?.Invoke(path);
                            }

                            if (wOk && aOk) result.Ok++;
                            else result.Err++;
                        }
                        catch
                        {
                            result.Err++;
                        }
                        finally
                        {
                            SafeCloseAndRelease(doc);
                            onFileDone?.Invoke();
                        }
                    }
                }
            }

            return result;
        }
        private bool ExportToPdf(KompasHost host, dynamic doc, string outPdf)
        {
            _planner.MoveOldIfExists(outPdf);

            // основной путь: PrintJob
            if (TryPrintJob(host, doc, outPdf))
                return File.Exists(outPdf);

            // запасной путь: SaveAs
            try
            {
                doc.SaveAs(outPdf);
                return File.Exists(outPdf);
            }
            catch
            {
                TryDeleteFile(outPdf);
                return false;
            }
        }

        private static bool TryPrintJob(KompasHost host, dynamic doc, string outPdf)
        {
            dynamic pj = null;
            try
            {
                // активируем документ (иногда без этого PrintJob тупит)
                Try(() => doc.Activate());

                pj = host.App5.PrintJob;
                Try(() => pj.Clear());

                try { pj.AddSheets(doc); }
                catch
                {
                    Try(() => pj.Clear());
                    return false;
                }

              

                Try(() => pj.PlotToFile = true);
                Try(() => pj.FileName = outPdf);
                Try(() => pj.OutputFileName = outPdf);

                bool executed = false;
                try { pj.SpecialExecute(outPdf); executed = true; } catch { }

                if (!executed)
                {
                    try { pj.Execute(); executed = true; } catch { }
                }

                Try(() => pj.Clear());

                if (!executed)
                {
                    TryDeleteFile(outPdf);
                    return false;
                }

                return File.Exists(outPdf);


            }
            catch
            {
                TryDeleteFile(outPdf);
                return false;
            }
            finally
            {
                if (pj != null) SafeReleaseCom(pj);
            }
        }

        private static void SafeCloseAndRelease(dynamic doc)
        {
            if (doc == null) return;

            try { doc.Close(false); } catch { }
            SafeReleaseCom(doc);
        }

        private static void SafeReleaseCom(object obj)
        {
            if (obj == null) return;
            try
            {
                if (Marshal.IsComObject(obj))
                    Marshal.FinalReleaseComObject(obj);
            }
            catch { }
        }


        private static void Try(Action a)
        {
            try { a(); } catch { }
        }


        private static void TryDeleteFile(string path)
        {
            if (string.IsNullOrWhiteSpace(path)) return;
            try
            {
                if (File.Exists(path))
                    File.Delete(path);
            }
            catch { }
        }
    }
}