using KompasDropExport.Domain;
using KompasDropExport.Kompas;
using KompasDropExport.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;

namespace KompasDropExport.Services
{
    internal sealed class StepExportService
    {
        private readonly ExportPathPlanner _planner = new ExportPathPlanner();

        public ExportResult Export(IReadOnlyList<string> files, ExportOptions opt, Action<string> stage = null, Action onFileDone = null)
        {
            if (files == null || files.Count == 0) return new ExportResult();

            int ok = 0, skip = 0, err = 0;

            using (var host = new KompasHost())
            {
                host.AttachOrStart();
                var reader = new KompasPropertyReader(host.App7);

                using (host.SilentScope())
                {
                    for (int i = 0; i < files.Count; i++)
                    {
                        string path = files[i];
                        string ext = (Path.GetExtension(path) ?? "").ToLowerInvariant();
                        string shortName = Path.GetFileName(path) ?? path;

                        bool isM3d = ext == ".m3d";
                        bool isA3d = ext == ".a3d";

                        if (!isM3d && !isA3d) { skip++; onFileDone?.Invoke(); continue; }

                        // опции
                        if (isA3d && opt.StepExcludeAssemblies) { skip++; onFileDone?.Invoke(); continue; }
                        if (opt.StepExcludeOtherTag && ExportRules.HasOtherTag(path)) { skip++; onFileDone?.Invoke(); continue; }

                        dynamic doc = null;
                        try
                        {
                            stage?.Invoke($"[{i + 1}/{files.Count}] {shortName}: открытие…");
                            doc = host.OpenDocument7(path, readOnly: true, visible: false);
                            if (doc == null) { err++; onFileDone?.Invoke(); continue; }

                            stage?.Invoke($"[{i + 1}/{files.Count}] {shortName}: свойства…");
                            var nm = reader.Read3D(doc); // marking/name из 3D

                            // 1) WORK всегда
                            string workDir = _planner.GetWorkDir(path, isPdf: false);
                            Directory.CreateDirectory(workDir);

                            string workBase = _planner.MakeWorkBaseName(path);
                            string workStep = Path.Combine(workDir, workBase + ".step");

                            stage?.Invoke($"[{i + 1}/{files.Count}] {shortName}: STEP (work)…");
                            bool wOk = ExportToStep(doc, workStep);

                            // 2) ARCHIVE по условию (по обозначению)
                            bool aOk = true;
                            if (_planner.ShouldWriteArchive(nm.Marking))
                            {
                                string archDir = _planner.GetArchiveDir(path, isPdf: false);
                                Directory.CreateDirectory(archDir);

                                string archBase = _planner.MakeArchiveBaseName(nm.Marking, nm.Name, opt.ActiveNameSeparator);
                                if (!string.IsNullOrWhiteSpace(archBase))
                                {
                                    string archStep = Path.Combine(archDir, archBase + ".step");
                                    stage?.Invoke($"[{i + 1}/{files.Count}] {shortName}: STEP (archive)…");
                                    aOk = ExportToStep(doc, archStep);
                                }
                            }

                            if (wOk && aOk) ok++;
                            else err++;
                        }
                        catch
                        {
                            err++;
                        }
                        finally
                        {
                            SafeCloseAndRelease(doc);
                            onFileDone?.Invoke();
                        }
                    }
                }
            }

            return new ExportResult { Ok = ok, Skip = skip, Err = err };
        }

        private bool ExportToStep(dynamic doc, string outStep)
        {
            _planner.MoveOldIfExists(outStep);

            try
            {
                Try(() => doc.Activate());
                doc.SaveAs(outStep);
                return File.Exists(outStep);
            }
            catch
            {
                return false;
            }
        }

        private static void SafeCloseAndRelease(dynamic doc)
        {
            if (doc == null) return;

            try { doc.Close(false); } catch { }
            SafeReleaseCom((object)doc);
        }

        private static void SafeReleaseCom(object obj)
        {
            if (obj == null) return;
            try
            {
                if (Marshal.IsComObject(obj))
                    Marshal.FinalReleaseComObject(obj);
            }
            catch (COMException) { }
            catch { }
        }

        private static void Try(Action a)
        {
            try { a(); } catch { }
        }
    }
}