using System.Collections.Generic;

namespace KompasDropExport.Domain.Analysis
{
    public sealed class AnalysisResult
    {
        public List<GraphNode> Nodes { get; }
        public List<GraphEdge> Edges { get; }

        /// <summary>
        /// Производный анализ по каждому узлу: NodeId -> info
        /// </summary>
        public Dictionary<int, NodeAnalysisInfo> NodeAnalysis { get; }

        /// <summary>
        /// Метаданные документов: обозначение / наименование
        /// </summary>
        public Dictionary<int, NodeDocumentInfo> NodeDocuments { get; }

        /// <summary>
        /// Проблемы целостности графа
        /// </summary>
        public List<GraphIssue> HealthIssues { get; }

        /// <summary>
        /// Проблемы полноты комплекта документации
        /// </summary>
        public List<GraphIssue> CompletenessIssues { get; }

        /// <summary>
        /// Проблемы нейминга
        /// </summary>
        public List<object> NamingIssues { get; }

        public AnalysisResult(
            List<GraphNode> nodes,
            List<GraphEdge> edges,
            Dictionary<int, NodeAnalysisInfo> nodeAnalysis,
            Dictionary<int, NodeDocumentInfo> nodeDocuments,
            List<GraphIssue> healthIssues,
            List<GraphIssue> completenessIssues,
            List<object> namingIssues)
        {
            Nodes = nodes ?? new List<GraphNode>();
            Edges = edges ?? new List<GraphEdge>();
            NodeAnalysis = nodeAnalysis ?? new Dictionary<int, NodeAnalysisInfo>();
            NodeDocuments = nodeDocuments ?? new Dictionary<int, NodeDocumentInfo>();
            HealthIssues = healthIssues ?? new List<GraphIssue>();
            CompletenessIssues = completenessIssues ?? new List<GraphIssue>();
            NamingIssues = namingIssues ?? new List<object>();
        }
    }
}