using System;

namespace KompasDropExport.Domain.Analysis
{
    /// <summary>
    /// Серьёзность проблемы.
    /// </summary>
    public enum IssueSeverity
    {
        Info = 0,
        Warning = 1,
        Error = 2
    }

    /// <summary>
    /// Тип проблемы графа.
    /// </summary>
    public enum GraphIssueType
    {
        MissingTarget,
        MissingSource,

        Asymmetry_DrawingModel,
        Asymmetry_AssemblySpec,

        MultipleTargets_DrawingToModel,
        MultipleTargets_AssemblyToSpec,

        DuplicateFileName_Internal,

        ReferenceToTrash,

        MissingDrawing,
        MissingSpecification,
        DrawingWithoutModel,
        SpecificationWithoutAssembly
    }
    /// <summary>
    /// Описание найденной проблемы в графе.
    /// </summary>
    public sealed class GraphIssue
    {
        public GraphIssueType Type { get; set; }

        public IssueSeverity Severity { get; set; }

        /// <summary>
        /// Id исходного узла (если проблема связана с ребром)
        /// </summary>
        public int FromId { get; set; }

        /// <summary>
        /// Id целевого узла (если проблема связана с ребром)
        /// </summary>
        public int ToId { get; set; }

        /// <summary>
        /// Путь, который удобно открыть по double-click в UI
        /// </summary>
        public string PrimaryPath { get; set; }

        /// <summary>
        /// Текст сообщения для UI
        /// </summary>
        public string Message { get; set; }

        public override string ToString()
        {
            if (!string.IsNullOrEmpty(Message))
                return Message;

            return Type.ToString();
        }
    }
}