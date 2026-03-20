namespace KompasDropExport.Domain.Analysis
{
    /// <summary>
    /// Одна операция переименования файла.
    /// Пока только план, без применения.
    /// </summary>
    public sealed class RenameOperation
    {
        public int NodeId { get; set; }

        public string Kind { get; set; }

        public string OldFullPath { get; set; }
        public string NewFullPath { get; set; }

        public string OldFileName { get; set; }
        public string NewFileName { get; set; }
    }
}