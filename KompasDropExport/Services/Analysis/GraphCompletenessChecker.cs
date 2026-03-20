using System;
using System.Collections.Generic;
using KompasDropExport.Domain.Analysis;

namespace KompasDropExport.Services.Analysis
{
    /// <summary>
    /// Проверки полноты комплекта документации.
    /// Это НЕ ошибки целостности графа, а проверки "хватает ли документов".
    /// </summary>
    public static class GraphCompletenessChecker
    {
        public static List<GraphIssue> Check(
            List<GraphNode> nodes,
            Dictionary<int, NodeAnalysisInfo> nodeAnalysis)
        {
            var issues = new List<GraphIssue>();

            if (nodes == null || nodeAnalysis == null)
                return issues;

            for (int i = 0; i < nodes.Count; i++)
            {
                var n = nodes[i];
                if (n == null) continue;

                NodeAnalysisInfo info;
                if (!nodeAnalysis.TryGetValue(n.Id, out info) || info == null)
                    continue;

                // Общие фильтры:
                // - только внутренние
                // - только существующие
                // - только входящие в изделие
                // - не стандартные
                if (!info.IsInternal) continue;
                if (!info.Exists) continue;
                if (!info.InProduct) continue;
                if (info.IsStandard) continue;

                // Дополнительный словесный пропуск:
                // прочие / стандарт / подобные служебные позиции не требуем комплектовать
                if (ShouldSkipByName(n))
                    continue;

                // 1) Модель / сборка без чертежа
                if ((n.NodeType == NodeType.ModelAssembly || n.NodeType == NodeType.ModelPart) &&
                    !info.HasDrawing)
                {
                    issues.Add(new GraphIssue
                    {
                        Type = GraphIssueType.MissingDrawing,
                        Severity = IssueSeverity.Warning,
                        FromId = n.Id,
                        ToId = 0,
                        PrimaryPath = n.FullPath,
                        Message = "Нет чертежа: " + (n.FileName ?? n.FullPath)
                    });
                }

                // 2) Сборка без спецификации
                if (n.NodeType == NodeType.ModelAssembly && !info.HasSpecification)
                {
                    issues.Add(new GraphIssue
                    {
                        Type = GraphIssueType.MissingSpecification,
                        Severity = IssueSeverity.Warning,
                        FromId = n.Id,
                        ToId = 0,
                        PrimaryPath = n.FullPath,
                        Message = "Нет спецификации: " + (n.FileName ?? n.FullPath)
                    });
                }

                // 3) Чертёж без модели
                if (n.NodeType == NodeType.Drawing && !info.DrawingHasModel)
                {
                    issues.Add(new GraphIssue
                    {
                        Type = GraphIssueType.DrawingWithoutModel,
                        Severity = IssueSeverity.Warning,
                        FromId = n.Id,
                        ToId = 0,
                        PrimaryPath = n.FullPath,
                        Message = "Чертёж не связан с моделью: " + (n.FileName ?? n.FullPath)
                    });
                }

                // 4) Спецификация без сборки
                if (n.NodeType == NodeType.Specification && !info.SpecificationHasAssembly)
                {
                    issues.Add(new GraphIssue
                    {
                        Type = GraphIssueType.SpecificationWithoutAssembly,
                        Severity = IssueSeverity.Warning,
                        FromId = n.Id,
                        ToId = 0,
                        PrimaryPath = n.FullPath,
                        Message = "Спецификация не связана со сборкой: " + (n.FileName ?? n.FullPath)
                    });
                }
            }

            return issues;
        }

        private static bool ShouldSkipByName(GraphNode n)
        {
            if (n == null)
                return false;

            string name = n.FileName ?? n.FullPath ?? "";
            if (string.IsNullOrWhiteSpace(name))
                return false;

            if (ContainsIgnoreCase(name, "ПРОЧИЕ") ||
                ContainsIgnoreCase(name, "СТАНДАРТ"))
            {
                return true;
            }

            return false;
        }

        private static bool ContainsIgnoreCase(string text, string value)
        {
            if (string.IsNullOrEmpty(text) || string.IsNullOrEmpty(value))
                return false;

            return text.IndexOf(value, StringComparison.OrdinalIgnoreCase) >= 0;
        }
    }
}