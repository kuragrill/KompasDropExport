using KompasDropExport.Domain;
using KompasDropExport.Utils;
using System;
using System.IO;

namespace KompasDropExport.Services
{
    internal sealed class ExportPathPlanner
    {
        public string GetWorkDir(string srcPath, bool isPdf)
        {
            var dir = Path.GetDirectoryName(srcPath) ?? "";
            return Path.Combine(dir, isPdf ? "PDF" : "STEP");
        }

        public string GetArchiveDir(string srcPath, bool isPdf)
        {
            var dir = Path.GetDirectoryName(srcPath) ?? "";
            return Path.Combine(dir, isPdf ? "PDF_archive" : "STEP_archive");
        }

        public string MakeWorkBaseName(string srcPath)
        {
            return Path.GetFileNameWithoutExtension(srcPath) ?? "noname";
        }

        public bool ShouldWriteArchive(string marking)
        {
            return ExportRules.IsArchiveEligible(marking);
        }

        public string MakeArchiveBaseName(string marking, string name, NameSeparator sep)
        {
            string m = (marking ?? "").Trim();
            if (string.IsNullOrWhiteSpace(m)) return null;

            string n = (name ?? "").Trim();

            string s = (m + Sep(sep) + n).Trim();
            return SanitizeFileName(s);
        }

        private static string SanitizeFileName(string s)
        {
            if (string.IsNullOrWhiteSpace(s)) return "noname";

            foreach (var ch in Path.GetInvalidFileNameChars())
                s = s.Replace(ch.ToString(), " ");

            s = s.Replace("@", "");

            while (s.Contains("  "))
                s = s.Replace("  ", " ");

            return s.Trim();
        }

        public void MoveOldIfExists(string outPath)
        {
            if (!File.Exists(outPath)) return;

            string destDir = Path.GetDirectoryName(outPath) ?? "";
            string oldDir = Path.Combine(destDir, "old");
            Directory.CreateDirectory(oldDir);

            string name = Path.GetFileNameWithoutExtension(outPath);
            string ext = Path.GetExtension(outPath);
            string stamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");

            string moved = Path.Combine(oldDir, $"{name}_{stamp}{ext}");
            File.Move(outPath, moved);
        }

        private static string Sep(NameSeparator s)
        {
            switch (s)
            {
                case NameSeparator.Dash: return " - ";
                case NameSeparator.Underscore: return "_";
                default: return " ";
            }
        }
    }
}