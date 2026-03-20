using System;
using System.IO;

namespace KompasDropExport.Utils
{
    internal static class TrashMover
    {
        public static string EnsureTrashFolder(string projectRoot)
        {
            string trashDir = Path.Combine(projectRoot, "Trash");
            if (!Directory.Exists(trashDir))
                Directory.CreateDirectory(trashDir);
            return trashDir;
        }

        /// <summary>
        /// Переносит файл в Trash, избегая конфликтов имён (добавляет индекс).
        /// Возвращает итоговый путь назначения.
        /// </summary>
        public static string MoveToTrash(string projectRoot, string sourceFullPath)
        {
            if (string.IsNullOrWhiteSpace(projectRoot))
                throw new ArgumentNullException(nameof(projectRoot));

            if (string.IsNullOrWhiteSpace(sourceFullPath))
                throw new ArgumentNullException(nameof(sourceFullPath));

            if (!File.Exists(sourceFullPath))
                throw new FileNotFoundException("Файл не найден", sourceFullPath);

            string trashDir = EnsureTrashFolder(projectRoot);

            string fileName = Path.GetFileName(sourceFullPath);
            string destPath = GetUniqueDestinationPath(trashDir, fileName);

            // ВАЖНО: File.Move работает в пределах одного диска.
            // Если вдруг будет другой диск — надо копировать+удалять, но у нас trash внутри проекта, значит один диск.
            File.Move(sourceFullPath, destPath);

            return destPath;
        }

        private static string GetUniqueDestinationPath(string trashDir, string fileName)
        {
            string name = Path.GetFileNameWithoutExtension(fileName);
            string ext = Path.GetExtension(fileName);

            string candidate = Path.Combine(trashDir, fileName);
            if (!File.Exists(candidate))
                return candidate;

            int i = 1;
            while (true)
            {
                string indexed = string.Format("{0} ({1}){2}", name, i, ext);
                candidate = Path.Combine(trashDir, indexed);

                if (!File.Exists(candidate))
                    return candidate;

                i++;
            }
        }
    }
}