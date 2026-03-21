using System;
using System.Collections.Generic;
using System.IO;
using KompasAPI7;
using KompasDropExport.Domain.Analysis;
using KompasDropExport.Kompas;

namespace KompasDropExport.Services.Analysis
{
    /// <summary>
    /// Обновляет ссылки в документах КОМПАС после переименования файлов.
    ///
    /// Подтверждённые рабочие ветки:
    /// 1) CDW -> 3D model
    ///    IAssociationView.SourceFileName + Update() + RebuildDocument()
    ///
    /// 2) SPW -> attached documents
    ///    ISpecificationDocument.AttachedDocuments
    ///    item.Name -> старый путь
    ///    item.Delete() -> удалить старую ссылку
    ///    AttachedDocuments.Add(newPath, true) -> добавить новую
    ///
    ///    ВАЖНО:
    ///    у SPW может лежать старый абсолютный путь из другой папки,
    ///    поэтому после exact-match по полному пути есть fallback по имени файла,
    ///    но только если имя файла уникально в плане переименования.
    ///
    /// 3) A3D/M3D -> attached documents
    ///    IProductDataManager.ObjectAttachedDocuments(keeper) -> string[]
    ///    IProductDataManager.ObjectAttachedDocuments[keeper] = string[]
    ///
    /// 4) Assembly composition (A3D -> child A3D/M3D)
    ///    IPart7.FileName = newPath
    ///    IModelObject.Update() у part и у topPart
    ///
    /// ВАЖНО:
    /// - composition и attached docs обновляются только при наличии совпадений в renameMap
    /// - документ сохраняется только если были реальные изменения
    /// </summary>
    ///В RenameLinkUpdater заработало обновление ссылок внутри спецификации по пути
    //SPW -> SpecificationDescriptions -> BaseObjects -> AttachedDocuments.
    //Причина неработоспособности была в обходе COM-коллекций через IEnumerable; рабочий вариант — только через Count + Item(...) с поддержкой 0 / 1 - based индексации.
    //Замена выполняется через attachedItem.Delete() + attachedDocs.Add(newPath, true).
    





    public sealed class RenameLinkUpdater
    {
        public sealed class RenameLinkUpdateResult
        {
            public int DocumentsScanned { get; set; }
            public int DocumentsChanged { get; set; }

            public int CdwChanged { get; set; }
            public int SpwChanged { get; set; }
            public int Model3DChanged { get; set; }

            public List<string> ChangedFiles { get; } = new List<string>();
            public List<string> Errors { get; } = new List<string>();
        }

        public RenameLinkUpdateResult UpdateLinks(
            IEnumerable<string> candidateFiles,
            IList<RenameOperation> operations)
        {
            var result = new RenameLinkUpdateResult();

            if (candidateFiles == null)
            {
                result.Errors.Add("Не передан список файлов для обновления ссылок.");
                return result;
            }

            if (operations == null || operations.Count == 0)
                return result;

            var renameMap = BuildRenameMap(operations);
            var renameMapByFileName = BuildRenameMapByFileName(operations);

            if (renameMap.Count == 0 && renameMapByFileName.Count == 0)
                return result;

            using (var host = new KompasHost())
            {
                host.AttachOrStart();

                using (host.SilentScope())
                {
                    foreach (var file in candidateFiles)
                    {
                        if (string.IsNullOrWhiteSpace(file))
                            continue;

                        string ext;
                        try { ext = (Path.GetExtension(file) ?? "").ToLowerInvariant(); }
                        catch { continue; }

                        if (!IsKompasFileExt(ext))
                            continue;

                        if (!File.Exists(file))
                            continue;

                        result.DocumentsScanned++;

                        try
                        {
                            bool changed = false;

                            if (ext == ".cdw" || ext == ".spw")
                            {
                                changed = Update2DDocument(
                                    host,
                                    file,
                                    renameMap,
                                    renameMapByFileName);
                            }
                            else if (ext == ".a3d" || ext == ".m3d")
                            {
                                changed = Update3DDocument(
                                    host,
                                    file,
                                    renameMap);
                            }

                            if (changed)
                            {
                                result.DocumentsChanged++;
                                result.ChangedFiles.Add(file);

                                if (ext == ".cdw")
                                    result.CdwChanged++;
                                else if (ext == ".spw")
                                    result.SpwChanged++;
                                else if (ext == ".a3d" || ext == ".m3d")
                                    result.Model3DChanged++;
                            }
                        }
                        catch (Exception ex)
                        {
                            result.Errors.Add(
                                "Ошибка обновления ссылок в документе:\n" +
                                file + "\n" + ex.Message);
                        }
                    }
                }
            }

            return result;
        }

        public RenameLinkUpdateResult UpdateLinks(
    string projectRoot,
    IList<RenameOperation> operations)
        {
            if (string.IsNullOrWhiteSpace(projectRoot))
            {
                var bad = new RenameLinkUpdateResult();
                bad.Errors.Add("Не задан корень проекта.");
                return bad;
            }

            return UpdateLinks(
                EnumerateProjectFilesSkippingTrash(projectRoot),
                operations);
        }

        private Dictionary<string, string> BuildRenameMap(IList<RenameOperation> operations)
        {
            var map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            for (int i = 0; i < operations.Count; i++)
            {
                var op = operations[i];
                if (op == null) continue;
                if (string.IsNullOrWhiteSpace(op.OldFullPath)) continue;
                if (string.IsNullOrWhiteSpace(op.NewFullPath)) continue;

                string oldPath;
                string newPath;

                try
                {
                    oldPath = Path.GetFullPath(op.OldFullPath);
                    newPath = Path.GetFullPath(op.NewFullPath);
                }
                catch
                {
                    continue;
                }

                if (!map.ContainsKey(oldPath))
                    map.Add(oldPath, newPath);
            }

            return map;
        }

        /// <summary>
        /// Строит fallback-map по имени файла.
        /// Включаются только уникальные имена, чтобы не было двусмысленности.
        /// Key = OldFileName, Value = NewFullPath
        /// </summary>
        private Dictionary<string, string> BuildRenameMapByFileName(IList<RenameOperation> operations)
        {
            var temp = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);

            for (int i = 0; i < operations.Count; i++)
            {
                var op = operations[i];
                if (op == null) continue;
                if (string.IsNullOrWhiteSpace(op.OldFileName)) continue;
                if (string.IsNullOrWhiteSpace(op.NewFullPath)) continue;

                List<string> list;
                if (!temp.TryGetValue(op.OldFileName, out list))
                {
                    list = new List<string>();
                    temp[op.OldFileName] = list;
                }

                if (!list.Contains(op.NewFullPath))
                    list.Add(op.NewFullPath);
            }

            var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            foreach (var kv in temp)
            {
                if (kv.Value.Count == 1)
                    result[kv.Key] = kv.Value[0];
            }

            return result;
        }

        private bool Update2DDocument(
            KompasHost host,
            string filePath,
            Dictionary<string, string> renameMap,
            Dictionary<string, string> renameMapByFileName)
        {
            string ext = (Path.GetExtension(filePath) ?? "").ToLowerInvariant();

            if (ext == ".cdw")
                return UpdateCdwAssociationViews(host, filePath, renameMap);

            if (ext == ".spw")
            {
                bool changed = false;

                changed |= UpdateSpwAttachedDocuments(
                    host,
                    filePath,
                    renameMap,
                    renameMapByFileName);

                changed |= UpdateSpwSpecificationObjectDocuments(
                    host,
                    filePath,
                    renameMap,
                    renameMapByFileName);

                return changed;
            }


            return false;
        }

        private bool UpdateCdwAssociationViews(
            KompasHost host,
            string filePath,
            Dictionary<string, string> renameMap)
        {
            if (host == null ||
                string.IsNullOrWhiteSpace(filePath) ||
                renameMap == null ||
                renameMap.Count == 0)
                return false;

            dynamic doc = host.OpenDocument(filePath, false, false);
            if (doc == null)
                return false;

            bool changed = false;

            try
            {
                IKompasDocument2D d2 = null;
                try { d2 = (IKompasDocument2D)doc; }
                catch { d2 = null; }

                if (d2 == null)
                    return false;

                IViewsAndLayersManager mgr = null;
                try { mgr = (IViewsAndLayersManager)d2.ViewsAndLayersManager; }
                catch { mgr = null; }

                if (mgr == null)
                    return false;

                dynamic views = null;
                try { views = mgr.Views; }
                catch { views = null; }

                if (views == null)
                    return false;

                int count = 0;
                try { count = views.Count; }
                catch { count = 0; }

                for (int i = 0; i < count; i++)
                {
                    dynamic view = null;
                    try { view = views.View(i + 1); }
                    catch { view = null; }

                    if (view == null)
                        continue;

                    IAssociationView av = null;
                    try { av = (IAssociationView)view; }
                    catch { av = null; }

                    if (av == null)
                        continue;

                    string oldRaw = null;
                    try { oldRaw = av.SourceFileName; }
                    catch { oldRaw = null; }

                    string oldFullPath = NormalizePathMaybeRelative(oldRaw, filePath);
                    if (string.IsNullOrWhiteSpace(oldFullPath))
                        continue;

                    string newFullPath;
                    if (!renameMap.TryGetValue(oldFullPath, out newFullPath))
                        continue;

                    if (string.Equals(oldFullPath, newFullPath, StringComparison.OrdinalIgnoreCase))
                        continue;

                    try
                    {
                        av.SourceFileName = newFullPath;
                        view.Update();
                        changed = true;
                    }
                    catch
                    {
                        // Не валим весь документ из-за одного вида.
                    }
                }

                if (!changed)
                    return false;

                try
                {
                    var d21 = d2 as IKompasDocument2D1;
                    if (d21 != null)
                        d21.RebuildDocument();
                }
                catch
                {
                }

                try
                {
                    doc.Save();
                }
                catch
                {
                    return false;
                }

                return true;
            }
            finally
            {
                SafeClose(doc);
            }
        }

        private bool UpdateSpwAttachedDocuments(
            KompasHost host,
            string filePath,
            Dictionary<string, string> renameMap,
            Dictionary<string, string> renameMapByFileName)
        {
            if (host == null ||
                string.IsNullOrWhiteSpace(filePath))
                return false;

            if ((renameMap == null || renameMap.Count == 0) &&
                (renameMapByFileName == null || renameMapByFileName.Count == 0))
                return false;

            dynamic doc = host.OpenDocument(filePath, false, false);
            if (doc == null)
                return false;

            bool changed = false;

            try
            {
                ISpecificationDocument specDoc = null;
                try { specDoc = (ISpecificationDocument)doc; }
                catch { specDoc = null; }

                if (specDoc == null)
                    return false;

                dynamic attached = null;
                try { attached = specDoc.AttachedDocuments; }
                catch { attached = null; }

                if (attached == null)
                    return false;

                int count = 0;
                try { count = attached.Count; }
                catch { count = 0; }

                var oldItemsToDelete = new List<object>();
                var newPathsToAdd = new List<string>();
                var newPathSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                var remainingExistingPaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

                for (int i = 0; i < count; i++)
                {
                    object item = null;
                    try { item = attached.Item(i); }
                    catch { item = null; }

                    if (item == null)
                        continue;

                    string oldRaw = TryGetSpwAttachedItemName(item);
                    string oldFullPath = NormalizePathMaybeRelative(oldRaw, filePath);

                    if (string.IsNullOrWhiteSpace(oldFullPath))
                        continue;

                    string newFullPath = null;

                    // 1) Сначала точное совпадение по полному пути
                    if (renameMap != null)
                        renameMap.TryGetValue(oldFullPath, out newFullPath);

                    // 2) Если не нашли — fallback по имени файла
                    if (string.IsNullOrWhiteSpace(newFullPath) && renameMapByFileName != null)
                    {
                        string oldFileName = null;
                        try { oldFileName = Path.GetFileName(oldFullPath); }
                        catch { oldFileName = null; }

                        if (!string.IsNullOrWhiteSpace(oldFileName))
                            renameMapByFileName.TryGetValue(oldFileName, out newFullPath);
                    }

                    if (string.IsNullOrWhiteSpace(newFullPath))
                    {
                        remainingExistingPaths.Add(oldFullPath);
                        continue;
                    }

                    if (string.Equals(oldFullPath, newFullPath, StringComparison.OrdinalIgnoreCase))
                    {
                        remainingExistingPaths.Add(oldFullPath);
                        continue;
                    }

                    oldItemsToDelete.Add(item);

                    if (newPathSet.Add(newFullPath))
                        newPathsToAdd.Add(newFullPath);
                }

                if (oldItemsToDelete.Count == 0)
                    return false;

                for (int i = newPathsToAdd.Count - 1; i >= 0; i--)
                {
                    if (remainingExistingPaths.Contains(newPathsToAdd[i]))
                        newPathsToAdd.RemoveAt(i);
                }

                // 1) Удаляем старые
                for (int i = 0; i < oldItemsToDelete.Count; i++)
                {
                    object item = oldItemsToDelete[i];

                    try
                    {
                        object deleteResult = item.GetType().InvokeMember(
                            "Delete",
                            System.Reflection.BindingFlags.InvokeMethod,
                            null,
                            item,
                            null);

                        bool ok = true;
                        if (deleteResult is bool b)
                            ok = b;

                        if (!ok)
                            return false;

                        changed = true;
                    }
                    catch
                    {
                        return false;
                    }
                }

                // 2) Добавляем новые
                for (int i = 0; i < newPathsToAdd.Count; i++)
                {
                    string newPath = newPathsToAdd[i];

                    try
                    {
                        attached.Add(newPath, true);
                        changed = true;
                    }
                    catch
                    {
                        return false;
                    }
                }

                if (!changed)
                    return false;

                try
                {
                    doc.Save();
                }
                catch
                {
                    return false;
                }

                return true;
            }
            finally
            {
                SafeClose(doc);
            }
        }


        private bool UpdateSpwSpecificationObjectDocuments(
            KompasHost host,
            string filePath,
            Dictionary<string, string> renameMap,
            Dictionary<string, string> renameMapByFileName)
        {
            if (host == null || string.IsNullOrWhiteSpace(filePath))
                return false;

            if ((renameMap == null || renameMap.Count == 0) &&
                (renameMapByFileName == null || renameMapByFileName.Count == 0))
                return false;

            dynamic doc = host.OpenDocument(filePath, false, false);
            if (doc == null)
                return false;

            bool changed = false;

            try
            {
                object descriptionsObj = TryGetComProperty(doc, "SpecificationDescriptions");
                if (descriptionsObj == null)
                    return false;

                int descCount = TryGetComCount(descriptionsObj);
                if (descCount <= 0)
                    return false;

                for (int di = 0; di < descCount; di++)
                {
                    object desc = TryGetComItem(descriptionsObj, di);
                    if (desc == null)
                        continue;

                    object baseObjectsObj = TryGetComProperty(desc, "BaseObjects");
                    if (baseObjectsObj == null)
                        continue;

                    int baseCount = TryGetComCount(baseObjectsObj);
                    if (baseCount <= 0)
                        continue;

                    bool descChanged = false;

                    for (int bi = 0; bi < baseCount; bi++)
                    {
                        object specObj = TryGetComItem(baseObjectsObj, bi);
                        if (specObj == null)
                            continue;

                        object attachedObj = TryGetComProperty(specObj, "AttachedDocuments");
                        if (attachedObj == null)
                            continue;

                        bool oneChanged = UpdateSingleSpecificationObjectAttachedDocuments(
                            attachedObj,
                            filePath,
                            renameMap,
                            renameMapByFileName);

                        if (oneChanged)
                        {
                            descChanged = true;
                            changed = true;

                            // Иногда COM-объекту нужно явное обновление
                            TryInvokeComMethod(specObj, "Update");
                        }
                    }

                    if (descChanged)
                    {
                        TryInvokeComMethod(desc, "Update");
                    }
                }

                if (!changed)
                    return false;

                try
                {
                    doc.Save();
                }
                catch
                {
                    return false;
                }

                return true;
            }
            finally
            {
                SafeClose(doc);
            }
        }

        private bool UpdateSingleSpecificationObjectAttachedDocuments(
            object attachedObj,
            string ownerFilePath,
            Dictionary<string, string> renameMap,
            Dictionary<string, string> renameMapByFileName)
        {
            if (attachedObj == null)
                return false;

            int count = TryGetComCount(attachedObj);
            if (count <= 0)
                return false;

            var oldItemsToDelete = new List<object>();
            var newPathsToAdd = new List<string>();
            var newPathSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var remainingExistingPaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            for (int i = 0; i < count; i++)
            {
                object item = TryGetComItem(attachedObj, i);
                if (item == null)
                    continue;

                string oldRaw = TryGetSpecificationObjectAttachedItemName(item);
                string oldFullPath = NormalizePathMaybeRelative(oldRaw, ownerFilePath);

                if (string.IsNullOrWhiteSpace(oldFullPath))
                    continue;

                string ext = "";
                try { ext = (Path.GetExtension(oldFullPath) ?? "").ToLowerInvariant(); }
                catch { ext = ""; }

                // Здесь обновляем только чертежи из спецификации
                if (ext != ".cdw")
                {
                    remainingExistingPaths.Add(oldFullPath);
                    continue;
                }

                string newFullPath = null;

                if (renameMap != null)
                    renameMap.TryGetValue(oldFullPath, out newFullPath);

                if (string.IsNullOrWhiteSpace(newFullPath) && renameMapByFileName != null)
                {
                    string oldFileName = null;
                    try { oldFileName = Path.GetFileName(oldFullPath); }
                    catch { oldFileName = null; }

                    if (!string.IsNullOrWhiteSpace(oldFileName))
                        renameMapByFileName.TryGetValue(oldFileName, out newFullPath);
                }

                if (string.IsNullOrWhiteSpace(newFullPath))
                {
                    remainingExistingPaths.Add(oldFullPath);
                    continue;
                }

                if (string.Equals(oldFullPath, newFullPath, StringComparison.OrdinalIgnoreCase))
                {
                    remainingExistingPaths.Add(oldFullPath);
                    continue;
                }

                oldItemsToDelete.Add(item);

                if (newPathSet.Add(newFullPath))
                    newPathsToAdd.Add(newFullPath);
            }

            if (oldItemsToDelete.Count == 0)
                return false;

            for (int i = newPathsToAdd.Count - 1; i >= 0; i--)
            {
                if (remainingExistingPaths.Contains(newPathsToAdd[i]))
                    newPathsToAdd.RemoveAt(i);
            }

            bool changed = false;
            dynamic attached = attachedObj;

            // 1) удаляем старые
            for (int i = 0; i < oldItemsToDelete.Count; i++)
            {
                object item = oldItemsToDelete[i];

                try
                {
                    object deleteResult = item.GetType().InvokeMember(
                        "Delete",
                        System.Reflection.BindingFlags.InvokeMethod,
                        null,
                        item,
                        null);

                    bool ok = true;
                    if (deleteResult is bool b)
                        ok = b;

                    if (!ok)
                        return false;

                    changed = true;
                }
                catch
                {
                    return false;
                }
            }

            // 2) добавляем новые
            for (int i = 0; i < newPathsToAdd.Count; i++)
            {
                string newPath = newPathsToAdd[i];

                try
                {
                    attached.Add(newPath, true);
                    changed = true;
                }
                catch
                {
                    // иногда сигнатура отличается
                    try
                    {
                        attached.Add(newPath);
                        changed = true;
                    }
                    catch
                    {
                        return false;
                    }
                }
            }

            return changed;
        }

        private static int TryGetComCount(object obj)
        {
            if (obj == null)
                return 0;

            try
            {
                var value = obj.GetType().InvokeMember(
                    "Count",
                    System.Reflection.BindingFlags.GetProperty,
                    null,
                    obj,
                    new object[0]);

                if (value == null)
                    return 0;

                return Convert.ToInt32(value);
            }
            catch
            {
                return 0;
            }
        }

        private static object TryGetComItem(object collection, int index)
        {
            if (collection == null)
                return null;

            // Сначала 0-based
            try
            {
                return collection.GetType().InvokeMember(
                    "Item",
                    System.Reflection.BindingFlags.InvokeMethod,
                    null,
                    collection,
                    new object[] { index });
            }
            catch
            {
            }

            // Потом 1-based
            try
            {
                return collection.GetType().InvokeMember(
                    "Item",
                    System.Reflection.BindingFlags.InvokeMethod,
                    null,
                    collection,
                    new object[] { index + 1 });
            }
            catch
            {
            }

            return null;
        }

        private static bool TryInvokeComMethod(object obj, string methodName)
        {
            if (obj == null || string.IsNullOrWhiteSpace(methodName))
                return false;

            try
            {
                obj.GetType().InvokeMember(
                    methodName,
                    System.Reflection.BindingFlags.InvokeMethod,
                    null,
                    obj,
                    null);

                return true;
            }
            catch
            {
                return false;
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
                    System.Reflection.BindingFlags.GetProperty,
                    null,
                    obj,
                    new object[0]);
            }
            catch
            {
                return null;
            }
        }

        private static string TryGetSpecificationObjectAttachedItemName(object item)
        {
            if (item == null)
                return null;

            try
            {
                var value = item.GetType().InvokeMember(
                    "Name",
                    System.Reflection.BindingFlags.GetProperty,
                    null,
                    item,
                    new object[0]);

                return value != null ? Convert.ToString(value) : null;
            }
            catch
            {
                return null;
            }
        }

        private bool Update3DDocument(
            KompasHost host,
            string filePath,
            Dictionary<string, string> renameMap)
        {
            string ext = (Path.GetExtension(filePath) ?? "").ToLowerInvariant();
            if (ext != ".a3d" && ext != ".m3d")
                return false;

            if (host == null ||
                string.IsNullOrWhiteSpace(filePath) ||
                renameMap == null ||
                renameMap.Count == 0)
                return false;

            dynamic doc = host.OpenDocument(filePath, false, false);
            if (doc == null)
                return false;

            bool changed = false;

            try
            {
                IKompasDocument3D d3 = null;
                try { d3 = (IKompasDocument3D)doc; }
                catch { d3 = null; }

                if (d3 == null)
                    return false;

                IProductDataManager pdm = null;
                try { pdm = (IProductDataManager)d3; }
                catch { pdm = null; }

                if (pdm == null)
                    return false;

                IPart7 topPart = null;
                try { topPart = d3.TopPart; }
                catch { topPart = null; }

                // 1) Attached documents
                changed |= Update3DAttachedDocumentsForKeeper(
                    pdm,
                    topPart as IPropertyKeeper,
                    filePath,
                    renameMap);

                changed |= Update3DAttachedDocumentsForKeeper(
                    pdm,
                    d3 as IPropertyKeeper,
                    filePath,
                    renameMap);

                // 2) Composition links
                changed |= Update3DCompositionLinks(
                    topPart,
                    filePath,
                    renameMap);

                if (!changed)
                    return false;

                // Обновляем модель сборки целиком
                try
                {
                    var topModel = topPart as IModelObject;
                    if (topModel != null)
                        topModel.Update();
                }
                catch
                {
                }

                try
                {
                    doc.Save();
                }
                catch
                {
                    return false;
                }

                return true;
            }
            finally
            {
                SafeClose(doc);
            }
        }

        private bool Update3DAttachedDocumentsForKeeper(
            IProductDataManager pdm,
            IPropertyKeeper keeper,
            string ownerFilePath,
            Dictionary<string, string> renameMap)
        {
            if (pdm == null || keeper == null)
                return false;

            string[] oldArr = null;

            try
            {
                oldArr = ((dynamic)pdm).ObjectAttachedDocuments(keeper);
            }
            catch
            {
                oldArr = null;
            }

            if (oldArr == null || oldArr.Length == 0)
                return false;

            bool changed = false;
            var newArr = new string[oldArr.Length];

            for (int i = 0; i < oldArr.Length; i++)
            {
                string oldRaw = oldArr[i];
                string oldFullPath = NormalizePathMaybeRelative(oldRaw, ownerFilePath);

                if (string.IsNullOrWhiteSpace(oldFullPath))
                {
                    newArr[i] = oldRaw;
                    continue;
                }

                string newFullPath;
                if (renameMap.TryGetValue(oldFullPath, out newFullPath) &&
                    !string.Equals(oldFullPath, newFullPath, StringComparison.OrdinalIgnoreCase))
                {
                    newArr[i] = newFullPath;
                    changed = true;
                }
                else
                {
                    newArr[i] = oldRaw;
                }
            }

            if (!changed)
                return false;

            newArr = DeduplicatePathsPreserveOrder(newArr, ownerFilePath);

            try
            {
                ((dynamic)pdm).ObjectAttachedDocuments[keeper] = newArr;
                return true;
            }
            catch
            {
                return false;
            }
        }

        private bool Update3DCompositionLinks(
            IPart7 topPart,
            string ownerFilePath,
            Dictionary<string, string> renameMap)
        {
            if (topPart == null || renameMap == null || renameMap.Count == 0)
                return false;

            var allParts = new List<IPart7>();
            CollectPartsRecursive(topPart, allParts);

            bool changed = false;

            for (int i = 0; i < allParts.Count; i++)
            {
                IPart7 part = allParts[i];
                if (part == null)
                    continue;

                string oldRaw = TryGetPartFileName(part);
                string oldFullPath = NormalizePathMaybeRelative(oldRaw, ownerFilePath);

                if (string.IsNullOrWhiteSpace(oldFullPath))
                    continue;

                string newFullPath;
                if (!renameMap.TryGetValue(oldFullPath, out newFullPath))
                    continue;

                if (string.Equals(oldFullPath, newFullPath, StringComparison.OrdinalIgnoreCase))
                    continue;

                try
                {
                    part.FileName = newFullPath;

                    var modelObj = part as IModelObject;
                    if (modelObj != null)
                    {
                        bool updated = false;
                        try { updated = modelObj.Update(); }
                        catch { updated = false; }

                        if (!updated)
                            continue;
                    }

                    string afterRaw = TryGetPartFileName(part);
                    string afterFullPath = NormalizePathMaybeRelative(afterRaw, ownerFilePath);

                    if (string.Equals(afterFullPath, newFullPath, StringComparison.OrdinalIgnoreCase))
                        changed = true;
                }
                catch
                {
                    // Не валим весь документ из-за одного компонента.
                }
            }

            return changed;
        }

        private static void CollectPartsRecursive(IPart7 parent, List<IPart7> result)
        {
            if (parent == null || result == null)
                return;

            IParts7 parts = null;
            try { parts = parent.Parts; }
            catch { parts = null; }

            if (parts == null)
                return;

            int count = 0;
            try { count = parts.Count; }
            catch { count = 0; }

            for (int i = 0; i < count; i++)
            {
                IPart7 child = null;

                try { child = parts.Part[i]; }
                catch
                {
                    try { child = parts.Part[i + 1]; }
                    catch { child = null; }
                }

                if (child == null)
                    continue;

                result.Add(child);
                CollectPartsRecursive(child, result);
            }
        }

        private IEnumerable<string> EnumerateProjectFilesSkippingTrash(string root)
        {
            var stack = new Stack<string>();
            stack.Push(root);

            while (stack.Count > 0)
            {
                string dir = stack.Pop();

                if (IsTrashFolder(dir))
                    continue;

                IEnumerable<string> files = null;
                try { files = Directory.EnumerateFiles(dir); } catch { }

                if (files != null)
                {
                    foreach (var f in files)
                        yield return f;
                }

                IEnumerable<string> subDirs = null;
                try { subDirs = Directory.EnumerateDirectories(dir); } catch { }

                if (subDirs != null)
                {
                    foreach (var sd in subDirs)
                        stack.Push(sd);
                }
            }
        }

        private bool IsTrashFolder(string dir)
        {
            try
            {
                return string.Equals(
                    new DirectoryInfo(dir).Name,
                    "Trash",
                    StringComparison.OrdinalIgnoreCase);
            }
            catch
            {
                return false;
            }
        }

        private static bool IsKompasFileExt(string ext)
        {
            return ext == ".a3d" || ext == ".m3d" || ext == ".cdw" || ext == ".spw";
        }

        private static string NormalizePathMaybeRelative(string rawPath, string ownerDocumentPath)
        {
            if (string.IsNullOrWhiteSpace(rawPath))
                return null;

            string p = rawPath.Trim().Trim('"');

            try
            {
                if (Path.IsPathRooted(p))
                    return Path.GetFullPath(p);
            }
            catch
            {
            }

            try
            {
                string dir = Path.GetDirectoryName(ownerDocumentPath);
                if (!string.IsNullOrWhiteSpace(dir))
                    return Path.GetFullPath(Path.Combine(dir, p));
            }
            catch
            {
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

        private static string[] DeduplicatePathsPreserveOrder(string[] paths, string ownerDocumentPath)
        {
            if (paths == null || paths.Length == 0)
                return paths ?? Array.Empty<string>();

            var result = new List<string>();
            var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            for (int i = 0; i < paths.Length; i++)
            {
                string raw = paths[i];
                if (string.IsNullOrWhiteSpace(raw))
                    continue;

                string normalized = NormalizePathMaybeRelative(raw, ownerDocumentPath);
                string key = string.IsNullOrWhiteSpace(normalized) ? raw.Trim() : normalized;

                if (seen.Add(key))
                    result.Add(raw);
            }

            return result.ToArray();
        }

        private static string TryGetSpwAttachedItemName(object item)
        {
            if (item == null)
                return null;

            try
            {
                var value = item.GetType().InvokeMember(
                    "Name",
                    System.Reflection.BindingFlags.GetProperty,
                    null,
                    item,
                    new object[0]);

                return value != null ? Convert.ToString(value) : null;
            }
            catch
            {
                return null;
            }
        }

        private static string TryGetPartFileName(IPart7 part)
        {
            if (part == null)
                return null;

            try
            {
                return Convert.ToString(part.FileName);
            }
            catch
            {
                return null;
            }
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