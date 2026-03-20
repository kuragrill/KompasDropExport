using KompasDropExport.Domain.Analysis;

namespace KompasDropExport.UI.Analysis
{
    // DTO для таблицы dgNodes: именно эти свойства должны совпасть с DataPropertyName колонок
    public sealed class NodeRow
    {
        public int Id { get; set; }
        public NodeType NodeType { get; set; }
        public NodeLocation Location { get; set; }

        public bool InProduct { get; set; }
        public int DepthMin { get; set; }
        public int IncomingComp { get; set; }

        public bool HasDrawing { get; set; }
        public bool HasSpec { get; set; }

        public string RelPath { get; set; } = "";
        public string FileName { get; set; } = "";

        // Полный путь — часто скрытый столбец
        public string Path { get; set; } = "";

        public bool Exists { get; set; }
    }
}