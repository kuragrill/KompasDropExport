namespace KompasDropExport.Domain.Analysis
{
    // Узел графа: 1 узел = 1 файл (уникальный путь)
    public sealed class GraphNode
    {
        public int Id { get; }
        public string FullPath { get; }

        public NodeType NodeType { get; set; }
        public NodeLocation Location { get; set; }

        public bool Exists { get; set; }
        public string RelPath { get; set; } = "";
        public string FileName { get; set; } = "";

        // Производные поля (посчитаем позже, пока можно оставить)
        public bool InProduct { get; set; }
        public int DepthMin { get; set; } = -1;
        public int IncomingComp { get; set; }
        public bool HasDrawing { get; set; }
        public bool HasSpec { get; set; }
        public bool IsStandard { get; set; }

        public GraphNode(int id, string fullPath)
        {
            Id = id;
            FullPath = fullPath;
        }
    }
}