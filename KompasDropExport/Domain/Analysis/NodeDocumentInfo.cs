namespace KompasDropExport.Domain.Analysis
{
    /// <summary>
    /// Метаданные документа, прочитанные из файла КОМПАС.
    /// Это не часть GraphNode, а отдельный слой данных.
    /// </summary>
    public sealed class NodeDocumentInfo
    {
        public int NodeId { get; set; }

        public string Designation { get; set; }
        public string Title { get; set; }

        public bool ReadSucceeded { get; set; }
    }
}