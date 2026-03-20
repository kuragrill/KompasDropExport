using System;
using System.IO;
using System.Text.RegularExpressions;

namespace KompasDropExport.Utils
{
    internal static class ExportRules
    {
        public static bool IsCdw(string path) => HasExt(path, ".cdw");
        public static bool IsSpw(string path) => HasExt(path, ".spw");
        public static bool IsM3d(string path) => HasExt(path, ".m3d");
        public static bool IsA3d(string path) => HasExt(path, ".a3d");

        private static bool HasExt(string path, string ext) =>
            string.Equals(Path.GetExtension(path), ext, StringComparison.OrdinalIgnoreCase);

        public static bool LooksLikeAssemblyDrawing(string path)
        {
            var up = (Path.GetFileNameWithoutExtension(path) ?? "").ToUpperInvariant();
            return up.Contains(" СБ") || up.Contains("_СБ") || up.Contains("[СБ]") || up.EndsWith("СБ");
        }

        public static bool HasOtherTag(string path)
        {
            var nm = Path.GetFileNameWithoutExtension(path) ?? "";
            return nm.IndexOf("[ПРОЧИЕ]", StringComparison.OrdinalIgnoreCase) >= 0;
        }


        public static bool IsArchiveEligible(string marking)
        {
            if (string.IsNullOrWhiteSpace(marking)) return false;

            string m = marking.Trim();

            // База: АШСД.556545.132
            // Суффикс (необязательный): СБ / Э5 / А1 / ... (1-3 буквы + 0-2 цифры), с пробелом или без
            var rx = new Regex(
                   @"^[А-ЯA-Z]{3,10}\.\d{6}\.\d{3}" +   // база
                   @"(?:-\d{2})?" +                     // -01
                   @"(?:\s?[А-ЯA-Z]{1,3}\d{0,2})?$",    // СБ / Э5 / А1
                   RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

            return rx.IsMatch(m);
        }

    }
}