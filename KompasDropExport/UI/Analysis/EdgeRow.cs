using KompasDropExport.Domain.Analysis;

namespace KompasDropExport.UI.Analysis
{
    public sealed class EdgeRow
    {
        public EdgeType EdgeType { get; set; }
        public int FromId { get; set; }
        public int ToId { get; set; }
        public int Count { get; set; }
        public bool IsResolved { get; set; }

        // Эти обычно скрывают, но удобно иметь
        public string FromPath { get; set; } = "";
        public string ToPath { get; set; } = "";
    }
}