using System;
using System.Collections;
using System.Reflection;
using System.Runtime.InteropServices;
using KompasDropExport.Kompas;

namespace KompasDropExport.Services.Analysis
{
    internal sealed class SpwApi7SpecificationProbe
    {
        public void Run(string spwPath)
        {
            using (var host = new KompasHost())
            {
                host.AttachOrStart();

                using (host.SilentScope())
                {
                    dynamic doc = null;

                    try
                    {
                        doc = host.OpenDocument(spwPath, false, false);
                        if (doc == null)
                        {
                            Console.WriteLine("Не удалось открыть SPW.");
                            return;
                        }

                        Console.WriteLine("SPW opened: " + spwPath);
                        DumpObject("doc", doc);

                        TryDumpProperty(doc, "PathName");
                        TryDumpProperty(doc, "Name");
                        TryDumpProperty(doc, "DocumentType");
                        TryDumpProperty(doc, "AttachedDocuments");

                        // Самые вероятные входы в описание спецификации
                        ProbeSpecificationEntry(doc, "SpecificationDescriptions");
                        ProbeSpecificationEntry(doc, "SpecificationDescription");
                        ProbeSpecificationEntry(doc, "Descriptions");
                        ProbeSpecificationEntry(doc, "Description");
                    }
                    finally
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
        }

        private void ProbeSpecificationEntry(object owner, string memberName)
        {
            object entry = TryGetProperty(owner, memberName);
            if (entry == null)
            {
                Console.WriteLine(memberName + " = <null>");
                return;
            }

            Console.WriteLine();
            Console.WriteLine("================================================");
            Console.WriteLine("ENTRY: " + memberName);
            Console.WriteLine("================================================");

            DumpObject(memberName, entry);

            // Если это коллекция описаний — обходим
            var en = entry as IEnumerable;
            if (en != null && !(entry is string))
            {
                int i = 0;
                foreach (var item in en)
                {
                    Console.WriteLine();
                    Console.WriteLine("---- " + memberName + "[" + i + "] ----");
                    DumpObject(memberName + "[" + i + "]", item);

                    ProbeSpecificationDescription(item);

                    i++;
                    if (i >= 20)
                    {
                        Console.WriteLine("... truncated after 20 items");
                        break;
                    }
                }
            }
            else
            {
                ProbeSpecificationDescription(entry);
            }
        }

        private void ProbeSpecificationDescription(object desc)
        {
            if (desc == null)
                return;

            Console.WriteLine();
            Console.WriteLine("******** PROBE SPECIFICATION DESCRIPTION ********");

            TryDumpProperty(desc, "Name");
            TryDumpProperty(desc, "Style");
            TryDumpProperty(desc, "Type");
            TryDumpProperty(desc, "BaseObjects");
            TryDumpProperty(desc, "Objects");

            // 1) Базовые объекты
            object baseObjects = TryGetProperty(desc, "BaseObjects");
            if (baseObjects != null)
            {
                Console.WriteLine();
                Console.WriteLine("=== BaseObjects ===");
                DumpObject("BaseObjects", baseObjects);
                DumpSpecificationObjectsEnumerable(baseObjects, "BaseObjects");
            }

            // 2) Все объекты
            object objects = TryGetProperty(desc, "Objects");
            if (objects != null)
            {
                Console.WriteLine();
                Console.WriteLine("=== Objects ===");
                DumpObject("Objects", objects);
                DumpSpecificationObjectsEnumerable(objects, "Objects");
            }
        }

        private void DumpSpecificationObjectsEnumerable(object collection, string caption)
        {
            var en = collection as IEnumerable;
            if (en == null)
            {
                Console.WriteLine(caption + " is not IEnumerable");
                return;
            }

            int i = 0;
            foreach (var obj in en)
            {
                Console.WriteLine();
                Console.WriteLine("---- " + caption + "[" + i + "] ----");
                DumpObject(caption + "[" + i + "]", obj);

                ProbeSpecificationObject(obj);

                i++;
                if (i >= 100)
                {
                    Console.WriteLine("... truncated after 100 objects");
                    break;
                }
            }
        }

        private void ProbeSpecificationObject(object obj)
        {
            if (obj == null)
                return;

            TryDumpProperty(obj, "Name");
            TryDumpProperty(obj, "ObjectType");
            TryDumpProperty(obj, "Section");
            TryDumpProperty(obj, "Subsection");
            TryDumpProperty(obj, "Number");
            TryDumpProperty(obj, "Text");
            TryDumpProperty(obj, "Designation");
            TryDumpProperty(obj, "Title");
            TryDumpProperty(obj, "Format");
            TryDumpProperty(obj, "AttachedDocuments");

            object attachedDocs = TryGetProperty(obj, "AttachedDocuments");
            if (attachedDocs == null)
                return;

            Console.WriteLine(">>>> AttachedDocuments FOUND on specification object");
            DumpObject("AttachedDocuments", attachedDocs);

            // Попробуем Count / Item
            TryDumpProperty(attachedDocs, "Count");

            int count = 0;
            try
            {
                var countObj = TryGetProperty(attachedDocs, "Count");
                if (countObj != null)
                    count = Convert.ToInt32(countObj);
            }
            catch
            {
                count = 0;
            }

            // Сначала через Item(i)
            for (int i = 0; i < count + 2; i++)
            {
                object item = TryInvokeIndexerLike(attachedDocs, "Item", i);
                if (item == null)
                    continue;

                Console.WriteLine("AttachedDocuments.Item(" + i + "):");
                DumpObject("AttachedDocument[" + i + "]", item);
                ProbeAttachedDocument(item);
            }

            // Потом, если коллекция enumerable — через foreach
            var en = attachedDocs as IEnumerable;
            if (en != null)
            {
                int i = 0;
                foreach (var item in en)
                {
                    Console.WriteLine("AttachedDocuments foreach [" + i + "]:");
                    DumpObject("AttachedDocumentForeach[" + i + "]", item);
                    ProbeAttachedDocument(item);

                    i++;
                    if (i >= 20)
                    {
                        Console.WriteLine("... truncated after 20 attached docs");
                        break;
                    }
                }
            }
        }

        private void ProbeAttachedDocument(object item)
        {
            if (item == null)
                return;

            TryDumpProperty(item, "Name");
            TryDumpProperty(item, "Transmit");
            TryDumpProperty(item, "FileName");
            TryDumpProperty(item, "FullFileName");
            TryDumpProperty(item, "Path");
            TryDumpProperty(item, "SourceFileName");
        }

        private static void DumpObject(string caption, object obj)
        {
            Console.WriteLine();
            Console.WriteLine("[" + caption + "]");

            if (obj == null)
            {
                Console.WriteLine("<null>");
                return;
            }

            Type t = obj.GetType();
            Console.WriteLine("runtime type: " + t.FullName);
            Console.WriteLine("is COM object: " + Marshal.IsComObject(obj));

            try
            {
                var props = t.GetProperties(BindingFlags.Public | BindingFlags.Instance);
                Console.WriteLine("properties:");
                if (props.Length == 0)
                {
                    Console.WriteLine("  <none>");
                }
                else
                {
                    for (int i = 0; i < props.Length; i++)
                    {
                        var p = props[i];
                        Console.WriteLine("  " + p.Name + " : " + p.PropertyType.FullName);
                    }
                }
            }
            catch
            {
            }

            try
            {
                var methods = t.GetMethods(BindingFlags.Public | BindingFlags.Instance);
                Console.WriteLine("methods:");
                int printed = 0;
                for (int i = 0; i < methods.Length; i++)
                {
                    var m = methods[i];
                    if (m.IsSpecialName) continue;

                    Console.WriteLine("  " + m.Name);
                    printed++;
                    if (printed >= 40)
                    {
                        Console.WriteLine("  ... truncated after 40 methods");
                        break;
                    }
                }

                if (printed == 0)
                    Console.WriteLine("  <none>");
            }
            catch
            {
            }
        }

        private static void TryDumpProperty(object obj, string propName)
        {
            try
            {
                var value = TryGetProperty(obj, propName);
                Console.WriteLine(propName + " = " + DescribeValue(value));
            }
            catch (Exception ex)
            {
                Console.WriteLine(propName + " FAIL: " + ex.Message);
            }
        }

        private static object TryGetProperty(object obj, string propName)
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

        private static object TryInvokeIndexerLike(object obj, string methodName, object arg)
        {
            if (obj == null || string.IsNullOrWhiteSpace(methodName))
                return null;

            try
            {
                return obj.GetType().InvokeMember(
                    methodName,
                    BindingFlags.InvokeMethod,
                    null,
                    obj,
                    new object[] { arg });
            }
            catch
            {
                return null;
            }
        }

        private static string DescribeValue(object value)
        {
            if (value == null)
                return "<null>";

            try
            {
                return value + " <" + value.GetType().FullName + ">";
            }
            catch
            {
                return "<unprintable>";
            }
        }
    }
}