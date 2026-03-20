using System.Collections.Generic;
using System.IO;

namespace KompasDropExport.Utils
{
    internal static class FileCollector
    {
        public static IEnumerable<string> Collect3DFiles(IEnumerable<string> paths)
        {
            if (paths == null) yield break;

            foreach (var p in paths)
            {
                if (File.Exists(p))
                {
                    if (Is3D(p)) yield return p;
                    continue;
                }

                if (Directory.Exists(p))
                {
                    foreach (var f in Directory.GetFiles(p, "*.*", SearchOption.AllDirectories))
                        if (Is3D(f)) yield return f;
                }
            }
        }

        private static bool Is3D(string path)
        {
            var ext = Path.GetExtension(path)?.ToLowerInvariant();
            return ext == ".m3d" || ext == ".a3d"; // только детали/сборки
        }
    }
}