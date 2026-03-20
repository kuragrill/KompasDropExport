using KompasAPI7;
using System;

namespace KompasDropExport.Kompas
{
    internal readonly struct NameMark
    {
        public readonly string Marking;
        public readonly string Name;

        public NameMark(string marking, string name)
        {
            Marking = marking;
            Name = name;
        }
    }

    internal sealed class KompasPropertyReader
    {
        private readonly IPropertyMng _propMng;

        // Id'шники КОМПАСа (как у тебя было)
        private const int PROP_MARKING = 4; // Обозначение
        private const int PROP_NAME = 5;    // Наименование


        public KompasPropertyReader(IApplication app7)
        {
            if (app7 == null) throw new ArgumentNullException(nameof(app7));
            _propMng = (IPropertyMng)app7;
        }

        /// <summary>
        /// Читает "Обозначение/Наименование" из 2D документа (.cdw/.spw) через IPropertyKeeper.
        /// </summary>
        public NameMark Read2D(dynamic doc)
        {
            if (doc == null) return default;

            IPropertyKeeper keeper = null;
            try { keeper = (IPropertyKeeper)doc; } catch { }

            // минимальный fallback как в старом коде
            if (keeper == null)
            {
                try
                {
                    var d2 = (IKompasDocument2D)doc;
                    keeper = (IPropertyKeeper)d2;
                }
                catch { }
            }

            if (keeper == null) return default;

            try
            {
                object arrObj = _propMng.GetProperties(keeper);
                if (!(arrObj is Array arr)) return default;

                IProperty pMark = null;
                IProperty pName = null;

                foreach (var it in arr)
                {
                    if (!(it is IProperty p)) continue;

                    double id = double.NaN;
                    string pn = null;

                    try { id = p.Id; } catch { }
                    try { pn = p.Name; } catch { }

                    // сначала по Id
                    if (pMark == null && id == 4) pMark = p;
                    if (pName == null && id == 5) pName = p;

                    // если Id не сработали — добиваем по имени
                    if (pMark == null && string.Equals(pn, "Обозначение", StringComparison.OrdinalIgnoreCase)) pMark = p;
                    if (pName == null && string.Equals(pn, "Наименование", StringComparison.OrdinalIgnoreCase)) pName = p;

                    if (pMark != null && pName != null) break;
                }

                string marking = ReadKeeperPropByProperty(keeper, pMark);
                string name = ReadKeeperPropByProperty(keeper, pName);

                return new NameMark(Clean(marking), Clean(name));
            }
            catch
            {
                return default;
            }
        }

        private string ReadKeeperPropByProperty(IPropertyKeeper keeper, IProperty prop)
        {
            if (keeper == null || prop == null) return null;
            try
            {
                object val;
                bool fromSource;
                bool ok = ((dynamic)keeper).GetPropertyValue(prop, out val, true, out fromSource);
                if (!ok || val == null) return null;
                return val.ToString();
            }
            catch
            {
                return null;
            }
        }

        private string ReadKeeperProp(IPropertyKeeper keeper, int id)
        {
            try
            {
                object propObj = _propMng.GetProperty(keeper, id);
                if (propObj == null) return null;

                object val;
                bool fromSource;
                bool ok = ((dynamic)keeper).GetPropertyValue(propObj, out val, true, out fromSource);
                if (!ok || val == null) return null;

                return val.ToString();
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Читает "marking/name" из 3D документа через Part (TopPart).
        /// </summary>
        public NameMark Read3D(dynamic doc)
        {
            if (doc == null) return default;

            IKompasDocument3D d3 = null;
            try { d3 = (IKompasDocument3D)doc; } catch { }

            if (d3 == null) return default;

            object topPart = null;
            try { topPart = d3.TopPart; } catch { }

            if (topPart == null) return default;

            string marking = null;
            string name = null;

            // минимально: читаем свойства через dynamic
            try { marking = Convert.ToString(((dynamic)topPart).marking); } catch { }
            try { name = Convert.ToString(((dynamic)topPart).name); } catch { }

            return new NameMark(Clean(marking), Clean(name));
        }

        private static string Clean(string s)
        {
            if (string.IsNullOrWhiteSpace(s)) return null;

            s = s.Trim();

            if (s == "-" || s.Equals("string", StringComparison.OrdinalIgnoreCase))
                return null;

            // минимальная чистка: убираем мусорные пробелы
            while (s.Contains("  "))
                s = s.Replace("  ", " ");

            return s;
        }
    }
}