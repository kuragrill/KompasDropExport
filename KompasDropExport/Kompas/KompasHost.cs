using KompasAPI7;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;

namespace KompasDropExport.Kompas
{
    internal sealed class KompasHost : IDisposable
    {
        public IApplication App7 { get; private set; }
        public dynamic App5 { get; private set; }   // late-binding для HideMessage/SilentMode и т.п.

        public bool IsAttachedToRunning { get; private set; } // true если подцепились к уже открытому
        public bool IsOwnInstance => !IsAttachedToRunning;

        private bool _initialized;



        // Сохранение исходных настроек (чтобы вернуть как было)
        private class AppState
        {
            public int HideMessage;
            public bool SilentMode;
            public bool ApplicationSilentMode;
            public bool Interactive;
            public bool EnablePrompt;
            public bool SuppressUi;

            public bool HasHideMessage;
            public bool HasSilentMode;
            public bool HasApplicationSilentMode;
            public bool HasInteractive;
            public bool HasEnablePrompt;
            public bool HasSuppressUi;
        }

        public void AttachOrStart()
        {
            if (_initialized) return;

            // 1) Пробуем зацепиться к существующему
            try
            {
                App7 = (IApplication)Marshal.GetActiveObject("KOMPAS.Application.7");
                App5 = App7;
                IsAttachedToRunning = true;
                _initialized = true;
                return;
            }
            catch
            {
                // ignored
            }

            // 2) Иначе создаём новый
            var t = Type.GetTypeFromProgID("KOMPAS.Application.7");
            if (t == null) throw new InvalidOperationException("Не найден ProgID KOMPAS.Application.7");

            App5 = Activator.CreateInstance(t);
            App7 = (IApplication)App5;

            // Для своего экземпляра можно безопасно скрыть
            Try(() => App7.Visible = false);

            IsAttachedToRunning = false;
            _initialized = true;
        }

        public dynamic OpenDocument(string path, bool readOnly = true, bool visible = false)
        {
            AttachOrStart();
            try
            {
                // API7: Documents.Open(path, readOnly, visible)
                return App7.Documents.Open(path, readOnly, visible);
            }
            catch
            {
                // запасной вариант через late-binding (редко нужно)
                try { return App5.Documents.Open(path, readOnly, visible); }
                catch { return null; }
            }
        }

        public dynamic OpenDocument7(string path, bool readOnly = true, bool visible = false)
        {
            AttachOrStart();
            return App7.Documents.Open(path, readOnly, visible);
        }

        public List<string> GetOpenDocumentPaths()
        {
            AttachOrStart();

            var result = new List<string>();
            var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            CollectOpenDocumentPaths(TryGetDocumentsCollection(App7), result, seen);
            CollectOpenDocumentPaths(TryGetDocumentsCollection(App5), result, seen);

            return result;
        }

        /// <summary>
        /// Включает "тихий режим" и откатывает обратно при Dispose().
        /// Для чужого КОМПАСа не трогаем Visible.
        /// </summary>
        public IDisposable SilentScope()
        {
            AttachOrStart();

            var st = CaptureState();

            // Включаем максимально аккуратно, не трогаем Visible
            Try(() => App5.HideMessage = 1);              // авто-ОК
            Try(() => App5.SilentMode = true);
            Try(() => App5.ApplicationSilentMode = true);
            Try(() => App5.Interactive = false);
            Try(() => App5.EnablePrompt = false);
            Try(() => App5.SuppressUi = true);

            return new Scope(() => RestoreState(st));
        }

        private AppState CaptureState()
        {
            var st = new AppState();

            // Важно: на разных версиях/состояниях какие-то свойства могут отсутствовать/падать
            Try(() => { st.HideMessage = Convert.ToInt32(App5.HideMessage); st.HasHideMessage = true; });
            Try(() => { st.SilentMode = Convert.ToBoolean(App5.SilentMode); st.HasSilentMode = true; });
            Try(() => { st.ApplicationSilentMode = Convert.ToBoolean(App5.ApplicationSilentMode); st.HasApplicationSilentMode = true; });
            Try(() => { st.Interactive = Convert.ToBoolean(App5.Interactive); st.HasInteractive = true; });
            Try(() => { st.EnablePrompt = Convert.ToBoolean(App5.EnablePrompt); st.HasEnablePrompt = true; });
            Try(() => { st.SuppressUi = Convert.ToBoolean(App5.SuppressUi); st.HasSuppressUi = true; });

            return st;
        }

        private void RestoreState(AppState st)
        {
            if (st == null) return;

            if (st.HasHideMessage) Try(() => App5.HideMessage = st.HideMessage);
            if (st.HasSilentMode) Try(() => App5.SilentMode = st.SilentMode);
            if (st.HasApplicationSilentMode) Try(() => App5.ApplicationSilentMode = st.ApplicationSilentMode);
            if (st.HasInteractive) Try(() => App5.Interactive = st.Interactive);
            if (st.HasEnablePrompt) Try(() => App5.EnablePrompt = st.EnablePrompt);
            if (st.HasSuppressUi) Try(() => App5.SuppressUi = st.SuppressUi);
        }

        public void Dispose()
        {
            // закрываем только свой экземпляр
            if (IsOwnInstance && App5 != null)
            {
                Try(() => App5.Quit()); // это dynamic-вызов, но он внутри Try, пусть падает и гасится
            }

            // ВАЖНО: убрать dynamic на этапе передачи аргументов
            object a5 = (object)App5;
            object a7 = (object)App7;

            ReleaseCom(a5);
            ReleaseCom(a7);

            App5 = null;
            App7 = null;

            _initialized = false;
        }

        private static void Try(Action a)
        {
            try { a(); } catch { }
        }

        private static object TryGetDocumentsCollection(object app)
        {
            if (app == null) return null;

            try
            {
                return app.GetType().InvokeMember(
                    "Documents",
                    BindingFlags.GetProperty | BindingFlags.Public | BindingFlags.Instance,
                    null,
                    app,
                    null);
            }
            catch
            {
                return null;
            }
        }

        private static void CollectOpenDocumentPaths(object documents, List<string> result, HashSet<string> seen)
        {
            if (documents == null || result == null || seen == null)
                return;

            foreach (var doc in EnumerateComCollection(documents))
            {
                var path = TryGetDocumentPath(doc);
                if (string.IsNullOrWhiteSpace(path))
                    continue;

                if (!File.Exists(path))
                    continue;

                if (seen.Add(path))
                    result.Add(path);
            }
        }

        private static IEnumerable<object> EnumerateComCollection(object collection)
        {
            if (collection == null)
                yield break;

            var enumerable = collection as IEnumerable;
            if (enumerable != null)
            {
                foreach (var item in enumerable)
                    yield return item;

                yield break;
            }

            int count = TryGetCollectionCount(collection);
            for (int i = 0; i < count; i++)
            {
                object item = TryGetCollectionItem(collection, i) ?? TryGetCollectionItem(collection, i + 1);
                if (item != null)
                    yield return item;
            }
        }

        private static int TryGetCollectionCount(object collection)
        {
            if (collection == null) return 0;

            foreach (var propName in new[] { "Count", "DocumentCount" })
            {
                try
                {
                    object value = collection.GetType().InvokeMember(
                        propName,
                        BindingFlags.GetProperty | BindingFlags.Public | BindingFlags.Instance,
                        null,
                        collection,
                        null);

                    if (value != null)
                        return Convert.ToInt32(value);
                }
                catch
                {
                }
            }

            return 0;
        }

        private static object TryGetCollectionItem(object collection, int index)
        {
            if (collection == null) return null;

            try
            {
                return collection.GetType().InvokeMember(
                    "Item",
                    BindingFlags.GetProperty | BindingFlags.Public | BindingFlags.Instance,
                    null,
                    collection,
                    new object[] { index });
            }
            catch
            {
                return null;
            }
        }

        private static string TryGetDocumentPath(object doc)
        {
            if (doc == null)
                return null;

            foreach (var propName in new[] { "PathName", "FullName", "FullFileName", "FileName" })
            {
                try
                {
                    object value = doc.GetType().InvokeMember(
                        propName,
                        BindingFlags.GetProperty | BindingFlags.Public | BindingFlags.Instance,
                        null,
                        doc,
                        null);

                    string path = Convert.ToString(value);
                    if (!string.IsNullOrWhiteSpace(path))
                        return path;
                }
                catch
                {
                }
            }

            return null;
        }

        private static void ReleaseCom(object obj)
        {
            if (obj == null) return;

            try
            {
                if (!Marshal.IsComObject(obj)) return;

                try
                {
                    while (Marshal.ReleaseComObject(obj) > 0) { }
                }
                catch (COMException) { }
                catch { }
            }
            catch { }
        }

        private sealed class Scope : IDisposable
        {
            private readonly Action _onDispose;
            public Scope(Action onDispose) => _onDispose = onDispose;
            public void Dispose() => _onDispose?.Invoke();
        }
    }
}
