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

        public ExportResult Export(
            IReadOnlyList<string> files,
            ExportOptions opt,
            Action<string> stage = null,
            Action onFileDone = null)
        {
            if (files == null || files.Count == 0)
                return new ExportResult();

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

                        if (!isM3d && !isA3d)
                        {
                            skip++;
                            onFileDone?.Invoke();
                            continue;
                        }

                        if (isA3d && opt.StepExcludeAssemblies)
                        {
                            skip++;
                            onFileDone?.Invoke();
                            continue;
                        }

                        if (opt.StepExcludeOtherTag && ExportRules.HasOtherTag(path))
                        {
                            skip++;
                            onFileDone?.Invoke();
                            continue;
                        }

                        dynamic doc = null;
                        try
                        {
                            stage?.Invoke($"[{i + 1}/{files.Count}] {shortName}: открытие…");
                            doc = host.OpenDocument7(path, readOnly: true, visible: false);
                            if (doc == null)
                            {
                                err++;
                                continue;
                            }

                            int embodimentCount = TryGetEmbodimentCount(doc);
                            int originalIndex = TryGetCurrentEmbodimentIndex(doc);

                            // Читаем базовые свойства в исходном состоянии документа.
                            var baseNm = reader.Read3D(doc);
                            string baseMarking = ResolveBaseMarking(path, baseNm.Marking);

                            // Если исполнений нет — экспорт как раньше.
                            if (embodimentCount <= 1)
                            {
                                bool singleOk = ExportCurrentState(
                                    doc,
                                    path,
                                    shortName,
                                    baseNm,
                                    baseMarking,
                                    suffix: null,
                                    opt: opt,
                                    stage: stage,
                                    fileIndex: i + 1,
                                    fileCount: files.Count);

                                if (singleOk) ok++;
                                else err++;

                                continue;
                            }

                            bool anyStateExported = false;
                            bool allEmbodimentsOk = true;

                            for (int embIndex = 0; embIndex < embodimentCount; embIndex++)
                            {
                                bool switched = TrySetCurrentEmbodiment(doc, embIndex);
                                if (!switched)
                                    continue;

                                TryRefreshDocumentAfterEmbodimentSwitch(doc);

                                var nm = reader.Read3D(doc);
                                string suffix = NormalizeExecSuffix(nm.Marking);

          

                                bool embOk = ExportCurrentState(
                                    doc,
                                    path,
                                    shortName,
                                    nm,
                                    baseMarking,
                                    suffix,
                                    opt,
                                    stage,
                                    i + 1,
                                    files.Count);

                                anyStateExported = true;
                                if (!embOk)
                                    allEmbodimentsOk = false;
                            }

                            // Возвращаем исходное исполнение.
                            if (originalIndex >= 0)
                                TrySetCurrentEmbodiment(doc, originalIndex);

                            // Если почему-то ни одного suffix-исполнения не нашли — fallback на обычный экспорт.
                            if (!anyStateExported)
                            {
                                TryRefreshDocumentAfterEmbodimentSwitch(doc);

                                var fallbackNm = reader.Read3D(doc);
                                bool singleOk = ExportCurrentState(
                                    doc,
                                    path,
                                    shortName,
                                    fallbackNm,
                                    baseMarking,
                                    suffix: null,
                                    opt: opt,
                                    stage: stage,
                                    fileIndex: i + 1,
                                    fileCount: files.Count);

                                if (singleOk) ok++;
                                else err++;
                            }
                            else
                            {
                                if (allEmbodimentsOk) ok++;
                                else err++;
                            }
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

            return new ExportResult
            {
                Ok = ok,
                Skip = skip,
                Err = err
            };
        }

        private bool ExportCurrentState(
            dynamic doc,
            string sourcePath,
            string shortName,
            NameMark nm,
            string baseMarking,
            string suffix,
            ExportOptions opt,
            Action<string> stage,
            int fileIndex,
            int fileCount)
        {
            string exportLabel = string.IsNullOrWhiteSpace(suffix)
                ? shortName
                : shortName + suffix;

            // 1) WORK — suffix просто в конец имени файла
            string workDir = _planner.GetWorkDir(sourcePath, isPdf: false);
            Directory.CreateDirectory(workDir);

            string workBase = _planner.MakeWorkBaseName(sourcePath, suffix);
            string workStep = Path.Combine(workDir, workBase + ".step");

            stage?.Invoke($"[{fileIndex}/{fileCount}] {exportLabel}: STEP (work)…");
            bool wOk = ExportToStep(doc, workStep);

            // 2) ARCHIVE — suffix между обозначением и наименованием
            bool aOk = true;

            string archiveMarking = string.IsNullOrWhiteSpace(baseMarking)
                ? nm.Marking
                : baseMarking;

            if (_planner.ShouldWriteArchive(archiveMarking))
            {
                string archDir = _planner.GetArchiveDir(sourcePath, isPdf: false);
                Directory.CreateDirectory(archDir);

                string archBase = _planner.MakeArchiveBaseName(
                    archiveMarking,
                    nm.Name,
                    suffix,
                    opt.ActiveNameSeparator);

                if (!string.IsNullOrWhiteSpace(archBase))
                {
                    string archStep = Path.Combine(archDir, archBase + ".step");
                    stage?.Invoke($"[{fileIndex}/{fileCount}] {exportLabel}: STEP (archive)…");
                    aOk = ExportToStep(doc, archStep);
                }
            }

            return wOk && aOk;
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

        private static int TryGetEmbodimentCount(dynamic doc)
        {
            if (doc == null) return 0;

            try { return Convert.ToInt32(doc.EmbodimentCount); }
            catch { return 0; }
        }

        private static int TryGetCurrentEmbodimentIndex(dynamic doc)
        {
            if (doc == null) return -1;

            try { return Convert.ToInt32(doc.CurrentEmbodimentIndex); }
            catch { return -1; }
        }

        private static bool TrySetCurrentEmbodiment(dynamic doc, int index)
        {
            if (doc == null) return false;

            try
            {
                return doc.SetCurrentEmbodiment(index);
            }
            catch
            {
                return false;
            }
        }

        private static void TryRefreshDocumentAfterEmbodimentSwitch(dynamic doc)
        {
            if (doc == null) return;

            try { doc.RebuildDocument(); } catch { }
            try { doc.Update(); } catch { }

            try
            {
                var top = doc.TopPart;
                if (top != null)
                {
                    try { top.Update(); } catch { }
                }
            }
            catch { }
        }

        private static string NormalizeExecSuffix(string marking)
        {
            if (string.IsNullOrWhiteSpace(marking))
                return null;

            string s = marking.Trim();

            // Интересуют именно suffix-исполнения вида -01 / -02 / ...
            if (!s.StartsWith("-"))
                return null;

            foreach (char c in Path.GetInvalidFileNameChars())
                s = s.Replace(c, '_');

            return s;
        }

        private static string ResolveBaseMarking(string sourcePath, string markingFromProperties)
        {
            string m = (markingFromProperties ?? "").Trim();

            // Если из свойств пришло полное обозначение — используем его.
            if (!string.IsNullOrWhiteSpace(m) && !m.StartsWith("-"))
                return m;

            // Иначе fallback: берём обозначение из имени файла до первого пробела.
            string fileBase = Path.GetFileNameWithoutExtension(sourcePath) ?? "";
            int spaceIndex = fileBase.IndexOf(' ');

            if (spaceIndex > 0)
                return fileBase.Substring(0, spaceIndex).Trim();

            return fileBase.Trim();
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