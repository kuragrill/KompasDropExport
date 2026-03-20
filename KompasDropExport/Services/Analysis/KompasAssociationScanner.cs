using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using KompasAPI7;
using KompasDropExport.Domain.Analysis;
using KompasDropExport.Kompas;

namespace KompasDropExport.Services.Analysis
{
    /// <summary>
    /// Сканирует связи документов (ассоциации) и читает обозначение/наименование
    /// из уже открытого документа через KompasPropertyReader.
    ///
    /// ВАЖНО:
    /// - Никаких "обратных" рёбер автоматически не добавляем.
    /// - 2D-pass: .spw/.cdw -> ищем ссылки на 3D
    /// - 3D-pass: .a3d/.m3d -> ищем ссылки на 2D/SPW через IProductDataManager.ObjectAttachedDocuments(...)
    /// - свойства документа читаются в этом же проходе, без повторного открытия файла
    /// </summary>
    internal sealed class KompasAssociationScanner
    {
        /// <summary>
        /// Если true — при отсутствии документных ссылок CDW->3D делаем fallback по видам.
        /// </summary>
        private const bool AllowCdwViewFallback = true;

        public DocumentScanResult ScanFrom2D(KompasHost host, string filePath)
        {
            var result = new DocumentScanResult();

            if (!TryPreparePath(filePath, out var fullPath, out var ext))
                return result;

            if (ext != ".spw" && ext != ".cdw")
                return result;

            dynamic doc = host?.OpenDocument(fullPath, true, false);
            if (doc == null)
                return result;

            try
            {
                // Создаём reader один раз на весь проход
                var reader = TryCreatePropertyReader(host);

                // 1) Читаем метаданные из уже открытого документа
                Read2DMetadata(reader, doc, result);

                // 2) Потом связи
                if (ext == ".spw")
                {
                    ScanSpw_AttachedDocuments(fullPath, doc, result.Links);
                    ScanSpw_SpecificationObjectDocuments(fullPath, doc, result.Links);
                   // ScanSpw_DocumentSection(fullPath, doc, result.Links);
                }
                else // .cdw
                {
                    bool gotDocLinks = ScanCdw_AttachedDocuments(fullPath, doc, result.Links);

                    if (AllowCdwViewFallback && !gotDocLinks)
                        ScanCdw_AssociationViews(fullPath, doc, result.Links);
                }

                return result;
            }
            finally
            {
                SafeClose(doc);
            }
        }

        public DocumentScanResult ScanFrom3D(KompasHost host, string filePath)
        {
            var result = new DocumentScanResult();

            if (!TryPreparePath(filePath, out var fullPath, out var ext))
                return result;

            if (ext != ".a3d" && ext != ".m3d")
                return result;

            dynamic doc = host?.OpenDocument(fullPath, true, false);
            if (doc == null)
                return result;

            try
            {
                // Создаём reader один раз на весь проход
                var reader = TryCreatePropertyReader(host);

                // 1) Читаем метаданные из уже открытого документа
                Read3DMetadata(reader, doc, result);

                // 2) Потом связи
                Scan3D_AttachedDocuments(fullPath, doc, result.Links);

                return result;
            }
            finally
            {
                SafeClose(doc);
            }
        }

        // =========================
        // Метаданные документа
        // =========================

        private void Read2DMetadata(KompasPropertyReader reader, dynamic doc, DocumentScanResult result)
        {
            if (reader == null || result == null || doc == null)
            {
                if (result != null) result.MetadataReadSucceeded = false;
                return;
            }

            try
            {
                var nm = reader.Read2D(doc);
                result.Designation = nm?.Marking;
                result.Title = nm?.Name;
                result.MetadataReadSucceeded =
                    !string.IsNullOrWhiteSpace(result.Designation) ||
                    !string.IsNullOrWhiteSpace(result.Title);
            }
            catch
            {
                result.MetadataReadSucceeded = false;
            }
        }

        private void Read3DMetadata(KompasPropertyReader reader, dynamic doc, DocumentScanResult result)
        {
            if (reader == null || result == null || doc == null)
            {
                if (result != null) result.MetadataReadSucceeded = false;
                return;
            }

            try
            {
                var nm = reader.Read3D(doc);
                result.Designation = nm?.Marking;
                result.Title = nm?.Name;
                result.MetadataReadSucceeded =
                    !string.IsNullOrWhiteSpace(result.Designation) ||
                    !string.IsNullOrWhiteSpace(result.Title);
            }
            catch
            {
                result.MetadataReadSucceeded = false;
            }
        }

        private KompasPropertyReader TryCreatePropertyReader(KompasHost host)
        {
            if (host == null) return null;

            // Лучше бы KompasHost сам отдавал IApplication явно.
            // Но пока оставляем безопасный reflection-fallback.
            object appObj = null;

            appObj = TryGetMemberValue(host, "App7");
            if (appObj == null) appObj = TryGetMemberValue(host, "Application");
            if (appObj == null) appObj = TryGetMemberValue(host, "KompasApp");
            if (appObj == null) appObj = TryGetMemberValue(host, "_app7");
            if (appObj == null) appObj = TryGetMemberValue(host, "_application");

            var app7 = appObj as IApplication;
            if (app7 == null) return null;

            try
            {
                return new KompasPropertyReader(app7);
            }
            catch
            {
                return null;
            }
        }

        private static object TryGetMemberValue(object obj, string name)
        {
            if (obj == null || string.IsNullOrWhiteSpace(name))
                return null;

            try
            {
                var t = obj.GetType();

                var p = t.GetProperty(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                if (p != null)
                    return p.GetValue(obj, null);

                var f = t.GetField(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                if (f != null)
                    return f.GetValue(obj);
            }
            catch
            {
            }

            return null;
        }

        // =========================
        // 3D: IProductDataManager.ObjectAttachedDocuments(...)
        // =========================

        private void Scan3D_AttachedDocuments(string fromPath, dynamic doc, List<AssociationLink> links)
        {
            IKompasDocument3D d3;
            try { d3 = (IKompasDocument3D)doc; }
            catch { return; }

            if (d3 == null) return;

            IProductDataManager pdm;
            try { pdm = (IProductDataManager)d3; }
            catch { return; }

            if (pdm == null) return;

            IPart7 topPart = null;
            try { topPart = d3.TopPart; }
            catch { topPart = null; }

            // Для A3D часто работает keeper = TopPart,
            // для M3D иногда нужен keeper = Document3D.
            TryAddPdmAttached(fromPath, pdm, topPart as IPropertyKeeper, links);
            TryAddPdmAttached(fromPath, pdm, d3 as IPropertyKeeper, links);
        }

        private void TryAddPdmAttached(string fromPath, IProductDataManager pdm, IPropertyKeeper keeper, List<AssociationLink> links)
        {
            if (pdm == null || keeper == null || links == null)
                return;

            object arrObj = null;

            // Вариант 1: индексатор / property with parameter
            try
            {
                arrObj = pdm.GetType().InvokeMember(
                    "ObjectAttachedDocuments",
                    BindingFlags.GetProperty,
                    null,
                    pdm,
                    new object[] { keeper }
                );
            }
            catch
            {
                arrObj = null;
            }

            // Вариант 2: method-style fallback
            if (arrObj == null)
            {
                try { arrObj = ((dynamic)pdm).ObjectAttachedDocuments(keeper); }
                catch { arrObj = null; }
            }

            AddLinksFromEnumerable(fromPath, arrObj, links);
        }

        // =========================
        // SPW: AttachedDocuments
        // =========================

        private void ScanSpw_AttachedDocuments(string fromPath, dynamic doc, List<AssociationLink> links)
        {
            object attached = null;

            try { attached = ((dynamic)doc).AttachedDocuments; }
            catch { attached = null; }

            AddLinksFromEnumerable(fromPath, attached, links);
        }

        private void ScanSpw_SpecificationObjectDocuments(
    string fromPath,
    dynamic doc,
    List<AssociationLink> links)
        {
            if (string.IsNullOrWhiteSpace(fromPath) || doc == null || links == null)
                return;

            object descriptionsObj = TryGetComProperty(doc, "SpecificationDescriptions");
            if (descriptionsObj == null)
                return;

            var descriptions = descriptionsObj as IEnumerable;
            if (descriptions == null)
                return;

            string fromDir = SafeDir(fromPath);
            var seen = BuildExistingTargetSet(links);

            foreach (var desc in descriptions)
            {
                if (desc == null)
                    continue;

                object baseObjectsObj = TryGetComProperty(desc, "BaseObjects");
                if (baseObjectsObj == null)
                    continue;

                var baseObjects = baseObjectsObj as IEnumerable;
                if (baseObjects == null)
                    continue;

                foreach (var specObj in baseObjects)
                {
                    if (specObj == null)
                        continue;

                    object attachedObj = TryGetComProperty(specObj, "AttachedDocuments");
                    if (attachedObj == null)
                        continue;

                    var attachedDocs = attachedObj as IEnumerable;
                    if (attachedDocs == null)
                        continue;

                    foreach (var attached in attachedDocs)
                    {
                        if (attached == null)
                            continue;

                        string raw = TryGetStringProperty(attached, "Name");
                        if (string.IsNullOrWhiteSpace(raw))
                            continue;

                        raw = raw.Trim();
                        raw = TryCutToKompasPath(raw);

                        string ext = SafeExt(raw);
                        if (ext != ".cdw")
                            continue;

                        string targetPath = ResolveToFullPath(raw, fromDir);
                        if (string.IsNullOrWhiteSpace(targetPath))
                            continue;

                        if (!seen.Add(targetPath))
                            continue;

                        links.Add(new AssociationLink
                        {
                            FromPath = fromPath,
                            ToPath = targetPath,
                            EdgeType = EdgeType.SpecificationToDocument
                        });
                    }
                }
            }
        }

        private static object TryGetComProperty(object obj, string propName)
        {
            if (obj == null || string.IsNullOrWhiteSpace(propName))
                return null;

            try
            {
                return obj.GetType().InvokeMember(
                    propName,
                    BindingFlags.GetProperty,
                    null,
                    obj,
                    new object[0]);
            }
            catch
            {
                return null;
            }
        }


        // =========================
        // CDW: AttachedDocuments (документные связи)
        // =========================

        private bool ScanCdw_AttachedDocuments(string fromPath, dynamic doc, List<AssociationLink> links)
        {
            object attached = null;
            try { attached = ((dynamic)doc).AttachedDocuments; }
            catch { attached = null; }

            int before = links?.Count ?? 0;

            AddLinksFromEnumerable(fromPath, attached, links);

            if (links == null) return false;

            for (int i = before; i < links.Count; i++)
            {
                var l = links[i];
                if (l == null) continue;

                if (l.EdgeType == EdgeType.DrawingToModel)
                    return true;
            }

            return false;
        }

        // =========================
        // CDW fallback: виды и IAssociationView.SourceFileName
        // =========================

        private void ScanCdw_AssociationViews(string fromPath, dynamic doc, List<AssociationLink> links)
        {
            if (links == null) return;

            IKompasDocument2D d2;
            try { d2 = (IKompasDocument2D)doc; }
            catch { return; }

            if (d2 == null) return;

            IViewsAndLayersManager mgr = null;
            try { mgr = (IViewsAndLayersManager)d2.ViewsAndLayersManager; }
            catch { mgr = null; }

            if (mgr == null) return;

            dynamic views = null;
            try { views = mgr.Views; }
            catch { views = null; }

            if (views == null) return;

            int count = 0;
            try { count = views.Count; }
            catch { count = 0; }

            if (count <= 0) return;

            // Учитываем уже добавленные ссылки, чтобы не плодить дубли
            var seen = BuildExistingTargetSet(links);
            string fromDir = SafeDir(fromPath);

            for (int i = 0; i < count; i++)
            {
                dynamic v = null;
                try { v = views.View(i + 1); }
                catch { v = null; }

                if (v == null) continue;

                IAssociationView av = null;
                try { av = (IAssociationView)v; }
                catch { av = null; }

                if (av == null) continue;

                string src = null;
                try { src = av.SourceFileName; }
                catch { src = null; }

                if (string.IsNullOrWhiteSpace(src)) continue;

                string targetPath = ResolveToFullPath(src, fromDir);
                if (string.IsNullOrEmpty(targetPath)) continue;

                if (!Is3D(targetPath)) continue;

                if (!seen.Add(targetPath)) continue;

                links.Add(new AssociationLink
                {
                    FromPath = fromPath,
                    ToPath = targetPath,
                    EdgeType = EdgeType.DrawingToModel
                });
            }
        }

        // =========================
        // Универсальный сбор ссылок
        // =========================

        private void AddLinksFromEnumerable(string fromPath, object enumerableObj, List<AssociationLink> links)
        {
            if (enumerableObj == null || links == null)
                return;

            var en = enumerableObj as IEnumerable;
            if (en == null)
                return;

            // Важно: учитываем уже существующие ссылки
            var seen = BuildExistingTargetSet(links);
            string fromDir = SafeDir(fromPath);

            foreach (var it in en)
            {
                if (it == null) continue;

                string raw = ExtractPathFromComItem(it);
                if (string.IsNullOrWhiteSpace(raw)) continue;

                raw = raw.Trim();
                raw = TryCutToKompasPath(raw);

                string ext = SafeExt(raw);
                if (!IsSupportedLinkExt(ext))
                    continue;

                string targetPath = ResolveToFullPath(raw, fromDir);
                if (string.IsNullOrEmpty(targetPath))
                    continue;

                if (!seen.Add(targetPath))
                    continue;

                if (!TryGuessEdgeTypeByExtensions(fromPath, targetPath, out var et))
                    continue;

                links.Add(new AssociationLink
                {
                    FromPath = fromPath,
                    ToPath = targetPath,
                    EdgeType = et
                });
            }
        }

        private static HashSet<string> BuildExistingTargetSet(List<AssociationLink> links)
        {
            var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            if (links == null)
                return seen;

            foreach (var link in links)
            {
                if (link == null) continue;
                if (string.IsNullOrWhiteSpace(link.ToPath)) continue;

                seen.Add(link.ToPath);
            }

            return seen;
        }

        private static string ExtractPathFromComItem(object it)
        {
            if (it == null) return null;

            if (it is string s && !string.IsNullOrWhiteSpace(s))
                return s;

            string val;

            val = TryGetStringProperty(it, "FullFileName");
            if (!string.IsNullOrWhiteSpace(val)) return val;

            val = TryGetStringProperty(it, "FileName");
            if (!string.IsNullOrWhiteSpace(val)) return val;

            val = TryGetStringProperty(it, "Path");
            if (!string.IsNullOrWhiteSpace(val)) return val;

            val = TryGetStringProperty(it, "SourceFileName");
            if (!string.IsNullOrWhiteSpace(val)) return val;

            val = TryGetStringProperty(it, "Name");
            if (!string.IsNullOrWhiteSpace(val)) return val;

            try { return Convert.ToString(it); }
            catch { return null; }
        }

        private static string TryGetStringProperty(object obj, string propName)
        {
            if (obj == null || string.IsNullOrWhiteSpace(propName))
                return null;

            // Вариант 1: обычное CLR-свойство
            try
            {
                var t = obj.GetType();
                var p = t.GetProperty(propName);
                if (p != null)
                {
                    var v = p.GetValue(obj, null);
                    if (v != null) return Convert.ToString(v);
                }
            }
            catch
            {
            }

            // Вариант 2: COM IDispatch property
            try
            {
                var v = obj.GetType().InvokeMember(
                    propName,
                    BindingFlags.GetProperty,
                    null,
                    obj,
                    new object[0]
                );

                if (v != null)
                    return Convert.ToString(v);
            }
            catch
            {
            }

            return null;
        }

        private static string ResolveToFullPath(string pathOrName, string fromDir)
        {
            if (string.IsNullOrWhiteSpace(pathOrName))
                return null;

            string p = pathOrName.Trim().Trim('"');

            if (p.StartsWith("file:", StringComparison.OrdinalIgnoreCase))
            {
                p = p.Substring(5).TrimStart('/');
                p = p.Replace('/', Path.DirectorySeparatorChar);
            }

            try
            {
                if (Path.IsPathRooted(p))
                    return Path.GetFullPath(p);
            }
            catch
            {
            }

            if (!string.IsNullOrEmpty(fromDir))
            {
                try
                {
                    return Path.GetFullPath(Path.Combine(fromDir, p));
                }
                catch
                {
                }
            }

            try
            {
                return Path.GetFullPath(p);
            }
            catch
            {
                return null;
            }
        }

        private static string TryCutToKompasPath(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw))
                return raw;

            string ext = SafeExt(raw);
            if (IsSupportedLinkExt(ext))
                return raw;

            int bestEnd = -1;
            string[] exts = { ".a3d", ".m3d", ".cdw", ".spw" };

            string lower = raw.ToLowerInvariant();

            foreach (string e in exts)
            {
                int idx = lower.IndexOf(e, StringComparison.OrdinalIgnoreCase);
                if (idx < 0) continue;

                int end = idx + e.Length;

                // Берём самое раннее вхождение валидного расширения
                if (bestEnd < 0 || end < bestEnd)
                    bestEnd = end;
            }

            if (bestEnd > 0 && bestEnd <= raw.Length)
                return raw.Substring(0, bestEnd).Trim().Trim('"');

            return raw;
        }

        private static bool TryGuessEdgeTypeByExtensions(string from, string to, out EdgeType et)
        {
            et = EdgeType.Composition;

            string e1 = (Path.GetExtension(from) ?? string.Empty).ToLowerInvariant();
            string e2 = (Path.GetExtension(to) ?? string.Empty).ToLowerInvariant();

            bool is3D1 = e1 == ".a3d" || e1 == ".m3d";
            bool is3D2 = e2 == ".a3d" || e2 == ".m3d";

            bool isCdw1 = e1 == ".cdw";
            bool isCdw2 = e2 == ".cdw";

            bool isSpw1 = e1 == ".spw";
            bool isSpw2 = e2 == ".spw";

            if (is3D1 && isCdw2)
            {
                et = EdgeType.ModelToDrawing;
                return true;
            }

            if (isCdw1 && is3D2)
            {
                et = EdgeType.DrawingToModel;
                return true;
            }

            if (e1 == ".a3d" && isSpw2)
            {
                et = EdgeType.AssemblyToSpecification;
                return true;
            }

            if (isSpw1 && e2 == ".a3d")
            {
                et = EdgeType.SpecificationToAssembly;
                return true;
            }

            return false;
        }

        private static bool TryPreparePath(string filePath, out string fullPath, out string ext)
        {
            fullPath = null;
            ext = null;

            if (string.IsNullOrWhiteSpace(filePath))
                return false;

            try
            {
                fullPath = Path.GetFullPath(filePath);
                ext = (Path.GetExtension(fullPath) ?? string.Empty).ToLowerInvariant();
                return true;
            }
            catch
            {
                return false;
            }
        }

        private static bool Is3D(string path)
        {
            string e = (Path.GetExtension(path) ?? string.Empty).ToLowerInvariant();
            return e == ".a3d" || e == ".m3d";
        }

        private static bool IsSupportedLinkExt(string ext)
        {
            return ext == ".a3d" || ext == ".m3d" || ext == ".cdw" || ext == ".spw";
        }

        private static string SafeExt(string s)
        {
            try { return (Path.GetExtension(s) ?? string.Empty).ToLowerInvariant(); }
            catch { return string.Empty; }
        }

        private static string SafeDir(string fullPath)
        {
            try { return Path.GetDirectoryName(fullPath); }
            catch { return null; }
        }

        private static void SafeClose(dynamic doc)
        {
            try
            {
                if (doc != null)
                    doc.Close(false);
            }
            catch
            {
            }
        }
    }
}