namespace KompasDropExport.Domain.Analysis
{
    /// <summary>
    /// Производная информация по узлу, вычисленная после построения графа.
    /// Это НЕ часть "чистого" GraphNode, а результат анализа.
    /// </summary>
    public sealed class NodeAnalysisInfo
    {
        public int NodeId { get; set; }

        // Доступность / положение
        public bool Exists { get; set; }
        public bool IsInternal { get; set; }
        public bool IsExternal { get; set; }
        public bool IsMissing { get; set; }

        // Участие в изделии
        public bool InProduct { get; set; }
        public int? DepthMin { get; set; }
        public int IncomingCompositionCount { get; set; }
        public bool IsOrphan3D { get; set; }

        // Классификация
        public bool IsStandard { get; set; }

        // Ассоциации
        public bool HasDrawing { get; set; }
        public bool HasSpecification { get; set; }
        public bool DrawingHasModel { get; set; }
        public bool SpecificationHasAssembly { get; set; }
    }
}