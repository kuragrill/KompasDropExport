using System;
using System.IO;
using System.Runtime.InteropServices;

namespace KompasDropExport.Utils
{
    /// <summary>
    /// Открытие проводника Windows с выделением файла.
    /// Использует Shell API, обычно корректно скроллит и фокусирует.
    /// </summary>
    internal static class ExplorerHelper
    {
        // Открывает папку и выделяет элемент(ы)
        [DllImport("shell32.dll", CharSet = CharSet.Unicode)]
        private static extern int SHOpenFolderAndSelectItems(
            IntPtr pidlFolder,
            uint cidl,
            IntPtr[] apidl,
            uint dwFlags);

        // Создаёт PIDL по пути
        [DllImport("shell32.dll", CharSet = CharSet.Unicode)]
        private static extern IntPtr ILCreateFromPath(string pszPath);

        // Освобождает PIDL
        [DllImport("shell32.dll")]
        private static extern void ILFree(IntPtr pidl);

        /// <summary>
        /// Открывает проводник и выделяет файл.
        /// Возвращает false, если путь некорректный/файл не существует.
        /// </summary>
        public static bool ShowAndSelectFile(string fullPath)
        {
            if (string.IsNullOrWhiteSpace(fullPath))
                return false;

            if (!File.Exists(fullPath))
                return false;

            IntPtr pidl = ILCreateFromPath(fullPath);
            if (pidl == IntPtr.Zero)
                return false;

            try
            {
                // ВАЖНО: используем pidl элемента, Shell сам откроет папку и выделит файл.
                SHOpenFolderAndSelectItems(pidl, 0, null, 0);
                return true;
            }
            finally
            {
                ILFree(pidl);
            }
        }
    }
}