using KompasAPI7;
using System;

namespace KompasDropExport.Kompas
{
    internal sealed class KompasPropertyWriter
    {
        private readonly IPropertyMng _pm;

        public KompasPropertyWriter(IApplication app7)
        {
            _pm = (IPropertyMng)app7;
        }

        public bool Write3D(dynamic doc, string marking, string name, out string error)
        {
            error = null;
            if (doc == null) { error = "doc == null"; return false; }

            IKompasDocument3D d3 = null;
            try { d3 = (IKompasDocument3D)doc; }
            catch { error = "doc is not IKompasDocument3D"; return false; }

            IPart7 topPart = null;
            try { topPart = d3.TopPart; }
            catch { error = "TopPart недоступен"; return false; }

            var keeper = (IPropertyKeeper)topPart;

            object pMarkObj = null;
            object pNameObj = null;

            try { pMarkObj = _pm.GetProperty((IKompasDocument)doc, "Обозначение"); }
            catch { }

            try { pNameObj = _pm.GetProperty((IKompasDocument)doc, "Наименование"); }
            catch { }

            if (pMarkObj == null) { error = "Не найдено свойство 'Обозначение'"; return false; }
            if (pNameObj == null) { error = "Не найдено свойство 'Наименование'"; return false; }

            try
            {
                // ВАЖНО: третий параметр true — запись в источник
                ((dynamic)keeper).SetPropertyValue(pMarkObj, marking ?? "", true);
                ((dynamic)keeper).SetPropertyValue(pNameObj, name ?? "", true);

                topPart.Update(); // как в примерах
                return true;
            }
            catch (Exception ex)
            {
                error = ex.Message;
                return false;
            }
        }
    }
}