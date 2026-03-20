using KompasAPI7;
using KompasDropExport.Domain.Analysis;
using KompasDropExport.Kompas;
using System;
using System.Collections.Generic;

namespace KompasDropExport.Services.Analysis
{
    /// <summary>
    /// Сканирует состав 3D сборки через API7.
    /// Возвращает связи parent -> child.
    ///
    /// ВАЖНО:
    /// - Никаких попыток читать "Тип объекта" через свойства (COM часто не даёт это на вхождении).
    /// - "Стандартность" определяем эвристикой по пути (Libs/PolynomLib/vault).
    /// - Документ обязательно закрываем без сохранения, чтобы не вылезали диалоги.
    /// </summary>
    public sealed class KompasCompositionScanner
    {
        public List<CompositionLink> ScanAssembly(string rootAssemblyPath)
        {
            var result = new List<CompositionLink>();

            using (var host = new KompasHost())
            {
                host.AttachOrStart();

                using (host.SilentScope())
                {
                    dynamic doc = host.OpenDocument(rootAssemblyPath, true, false);
                    if (doc == null)
                        return result;

                    IKompasDocument3D d3 = null;
                    try { d3 = (IKompasDocument3D)doc; }
                    catch
                    {
                        SafeClose(doc);
                        return result;
                    }

                    dynamic topPart = null;
                    try { topPart = d3.TopPart; }
                    catch
                    {
                        SafeClose(doc);
                        return result;
                    }

                    if (topPart != null)
                    {
                        TraversePart(topPart, rootAssemblyPath, result);
                    }

                    SafeClose(doc);
                }
            }

            return result;
        }

        /// <summary>
        /// Рекурсивный обход Part.Parts.
        /// parentPath — путь родителя (сборки/подсборки), childPath — путь компонента.
        /// </summary>
        private void TraversePart(dynamic part, string parentPath, List<CompositionLink> result)
        {
            if (part == null) return;

            dynamic parts = null;
            try { parts = part.Parts; }
            catch { return; }

            if (parts == null) return;

            int count = 0;
            try { count = parts.Count; }
            catch { return; }

            for (int i = 0; i < count; i++)
            {
                dynamic child = null;
                try { child = parts.Item(i); }
                catch { continue; }

                if (child == null) continue;

                // Путь файла компонента
                string childPath = null;
                try { childPath = Convert.ToString(child.FileName); }
                catch { }

                if (string.IsNullOrEmpty(childPath))
                    continue;

                // Эвристика стандартности по пути
                bool isStandard = LooksLikeKompasLibraryPath(childPath);

                result.Add(new CompositionLink
                {
                    ParentPath = parentPath,
                    ChildPath = childPath,
                    IsStandard = isStandard,

                });

                // Спуск дальше (если child — подсборка, у неё тоже будут Parts)
                TraversePart(child, childPath, result);
            }
        }

        /// <summary>
        /// Эвристика: "похоже на библиотеку КОМПАС" => считаем стандартным изделием.
        /// Под твой кейс: Program Files\ASCON\KOMPAS-3D v23\Libs\PolynomLib\...
        /// </summary>
        private static bool LooksLikeKompasLibraryPath(string fullPath)
        {
            if (string.IsNullOrEmpty(fullPath))
                return false;

            string p = fullPath.ToLowerInvariant();

            // Основной признак библиотек КОМПАСа
            if (p.Contains(@"\ascon\") && p.Contains(@"\kompas-3d") && p.Contains(@"\libs\"))
                return true;

            // Под твой конкретный пример
            if (p.Contains(@"\polynomlib\") || p.Contains(@"\data\vault\"))
                return true;

            return false;
        }

        private static void SafeClose(dynamic doc)
        {
            try
            {
                if (doc != null)
                    doc.Close(false); // не сохранять
            }
            catch { }
        }
    }
}