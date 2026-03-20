using System;
using System.Collections.Generic;
using System.Linq;
using KompasDropExport.Domain.Analysis;

namespace KompasDropExport.Services.Analysis
{
    /// <summary>
    /// Проверки целостности графа:
    /// - отсутствующий источник / цель
    /// - асимметрия парных связей
    /// - множественные цели там, где обычно ожидается одна
    /// - одинаковые FileName внутри проекта
    /// - ссылки на папку Trash
    /// </summary>
    public static class GraphHealthChecker
    {
        public static List<GraphIssue> Check(List<GraphNode> nodes, List<GraphEdge> edges, int rootId)
        {
            var issues = new List<GraphIssue>();
            if (nodes == null || edges == null) return issues;

            var nodeById = new Dictionary<int, GraphNode>();
            for (int i = 0; i < nodes.Count; i++)
            {
                var n = nodes[i];
                if (n == null) continue;
                nodeById[n.Id] = n;
            }

            var edgeSet = new HashSet<EdgeKey>();
            for (int i = 0; i < edges.Count; i++)
            {
                var e = edges[i];
                if (e == null) continue;
                edgeSet.Add(new EdgeKey(e.EdgeType, e.FromId, e.ToId));
            }

            AddMissingIssues(edges, nodeById, issues);

            AddTrashReferenceIssues(edges, nodeById, issues);

            AddAsymmetryIssues(
                edges,
                nodeById,
                edgeSet,
                issues,
                EdgeType.DrawingToModel,
                EdgeType.ModelToDrawing,
                GraphIssueType.Asymmetry_DrawingModel);

            AddAsymmetryIssues(
                edges,
                nodeById,
                edgeSet,
                issues,
                EdgeType.AssemblyToSpecification,
                EdgeType.SpecificationToAssembly,
                GraphIssueType.Asymmetry_AssemblySpec);

            AddMultiTargetIssues(
                edges,
                nodeById,
                issues,
                EdgeType.DrawingToModel,
                GraphIssueType.MultipleTargets_DrawingToModel,
                IssueSeverity.Warning,
                expectedMax: 1);

            AddMultiTargetIssues(
                edges,
                nodeById,
                issues,
                EdgeType.AssemblyToSpecification,
                GraphIssueType.MultipleTargets_AssemblyToSpec,
                IssueSeverity.Warning,
                expectedMax: 1);

            AddDuplicateFileNameIssues(nodes, issues);

            return issues;
        }

        private static void AddMissingIssues(
            List<GraphEdge> edges,
            Dictionary<int, GraphNode> nodeById,
            List<GraphIssue> issues)
        {
            for (int i = 0; i < edges.Count; i++)
            {
                var e = edges[i];
                if (e == null) continue;

                GraphNode from = null;
                GraphNode to = null;

                nodeById.TryGetValue(e.FromId, out from);
                nodeById.TryGetValue(e.ToId, out to);

                string fromName = from != null ? (from.FileName ?? from.FullPath) : ("id=" + e.FromId);
                string toName = to != null ? (to.FileName ?? to.FullPath) : ("id=" + e.ToId);

                if (from != null && !from.Exists)
                {
                    issues.Add(new GraphIssue
                    {
                        Type = GraphIssueType.MissingSource,
                        Severity = IssueSeverity.Warning,
                        FromId = e.FromId,
                        ToId = e.ToId,
                        // Источник отсутствует — как контекст полезнее открыть цель, если она есть
                        PrimaryPath = to != null ? to.FullPath : null,
                        Message = "Отсутствует исходный файл: " + fromName +
                                  "  -> связь: " + EdgeTypeToRu(e.EdgeType) +
                                  " -> цель: " + toName
                    });
                }

                if (to != null && !to.Exists)
                {
                    issues.Add(new GraphIssue
                    {
                        Type = GraphIssueType.MissingTarget,
                        Severity = IssueSeverity.Error,
                        FromId = e.FromId,
                        ToId = e.ToId,
                        // Цель отсутствует — исправлять обычно нужно источник ссылки
                        PrimaryPath = from != null ? from.FullPath : null,
                        Message = "Отсутствует целевой файл: " + toName +
                                  "  <- связь: " + EdgeTypeToRu(e.EdgeType) +
                                  " <- источник: " + fromName
                    });
                }
            }
        }

        private static void AddAsymmetryIssues(
            List<GraphEdge> edges,
            Dictionary<int, GraphNode> nodeById,
            HashSet<EdgeKey> edgeSet,
            List<GraphIssue> issues,
            EdgeType forward,
            EdgeType backward,
            GraphIssueType issueType)
        {
            for (int i = 0; i < edges.Count; i++)
            {
                var e = edges[i];
                if (e == null) continue;
                if (e.EdgeType != forward) continue;

                var reverseKey = new EdgeKey(backward, e.ToId, e.FromId);
                if (edgeSet.Contains(reverseKey))
                    continue;

                GraphNode from = null;
                GraphNode to = null;

                nodeById.TryGetValue(e.FromId, out from);
                nodeById.TryGetValue(e.ToId, out to);

                string fromName = from != null ? from.FileName : ("id=" + e.FromId);
                string toName = to != null ? to.FileName : ("id=" + e.ToId);

                // Для асимметрии логичнее открывать узел, у которого не хватает обратной связи,
                // то есть target исходного ребра. Если его нет — открываем source.
                string primaryPath = null;
                if (to != null && !string.IsNullOrWhiteSpace(to.FullPath))
                    primaryPath = to.FullPath;
                else if (from != null)
                    primaryPath = from.FullPath;

                issues.Add(new GraphIssue
                {
                    Type = issueType,
                    Severity = IssueSeverity.Warning,
                    FromId = e.FromId,
                    ToId = e.ToId,
                    PrimaryPath = primaryPath,
                    Message = "Несимметричная связь: есть " + EdgeTypeToRu(forward) +
                              " (" + fromName + " -> " + toName + "), но нет обратной связи " +
                              EdgeTypeToRu(backward)
                });
            }
        }

        private static void AddMultiTargetIssues(
            List<GraphEdge> edges,
            Dictionary<int, GraphNode> nodeById,
            List<GraphIssue> issues,
            EdgeType type,
            GraphIssueType issueType,
            IssueSeverity severity,
            int expectedMax)
        {
            var map = new Dictionary<int, List<int>>();

            for (int i = 0; i < edges.Count; i++)
            {
                var e = edges[i];
                if (e == null) continue;
                if (e.EdgeType != type) continue;

                List<int> list;
                if (!map.TryGetValue(e.FromId, out list))
                {
                    list = new List<int>();
                    map[e.FromId] = list;
                }

                list.Add(e.ToId);
            }

            foreach (var kv in map)
            {
                if (kv.Value.Count <= expectedMax)
                    continue;

                GraphNode from = null;
                nodeById.TryGetValue(kv.Key, out from);

                issues.Add(new GraphIssue
                {
                    Type = issueType,
                    Severity = severity,
                    FromId = kv.Key,
                    ToId = 0,
                    PrimaryPath = from != null ? from.FullPath : null,
                    Message = "Несколько целей у связи " + EdgeTypeToRu(type) + ": " +
                              (from != null ? from.FileName : ("id=" + kv.Key)) +
                              " -> " + kv.Value.Count + " шт. (ожидалось не более " + expectedMax + ")"
                });
            }
        }

        private static void AddDuplicateFileNameIssues(
            List<GraphNode> nodes,
            List<GraphIssue> issues)
        {
            var groups = nodes
                .Where(n => n != null
                            && n.Location == NodeLocation.Internal
                            && n.Exists
                            && !string.IsNullOrWhiteSpace(n.FileName))
                .GroupBy(n => n.FileName, StringComparer.OrdinalIgnoreCase)
                .Where(g => g.Count() > 1);

            foreach (var g in groups)
            {
                var first = g.FirstOrDefault();

                issues.Add(new GraphIssue
                {
                    Type = GraphIssueType.DuplicateFileName_Internal,
                    Severity = IssueSeverity.Warning,
                    FromId = 0,
                    ToId = 0,
                    PrimaryPath = first != null ? first.FullPath : null,
                    Message = "Дубликат имени файла внутри проекта: " + g.Key + "  (количество: " + g.Count() + ")"
                });
            }
        }

        private static void AddTrashReferenceIssues(
            List<GraphEdge> edges,
            Dictionary<int, GraphNode> nodeById,
            List<GraphIssue> issues)
        {
            for (int i = 0; i < edges.Count; i++)
            {
                var e = edges[i];
                if (e == null) continue;

                GraphNode from = null;
                GraphNode to = null;

                nodeById.TryGetValue(e.FromId, out from);
                nodeById.TryGetValue(e.ToId, out to);

                bool fromTrash = from != null && IsUnderTrash(from.FullPath);
                bool toTrash = to != null && IsUnderTrash(to.FullPath);

                if (!fromTrash && !toTrash)
                    continue;

                string fromName = from != null ? (from.FileName ?? from.FullPath) : ("id=" + e.FromId);
                string toName = to != null ? (to.FileName ?? to.FullPath) : ("id=" + e.ToId);

                // Предпочтительно открывать "живой" контекст, а не сам объект в Trash
                string primaryPath = null;

                if (!fromTrash && from != null && !string.IsNullOrWhiteSpace(from.FullPath))
                    primaryPath = from.FullPath;
                else if (!toTrash && to != null && !string.IsNullOrWhiteSpace(to.FullPath))
                    primaryPath = to.FullPath;
                else if (from != null && !string.IsNullOrWhiteSpace(from.FullPath))
                    primaryPath = from.FullPath;
                else if (to != null && !string.IsNullOrWhiteSpace(to.FullPath))
                    primaryPath = to.FullPath;

                string msg;
                if (fromTrash && toTrash)
                {
                    msg = "Ссылка на Trash: и источник, и цель находятся в папке Trash. " +
                          "связь: " + EdgeTypeToRu(e.EdgeType) +
                          " источник: " + fromName +
                          " цель: " + toName;
                }
                else if (toTrash)
                {
                    msg = "Ссылка на Trash: целевой файл находится в папке Trash. " +
                          "связь: " + EdgeTypeToRu(e.EdgeType) +
                          " источник: " + fromName +
                          " цель: " + toName;
                }
                else
                {
                    msg = "Ссылка на Trash: исходный файл находится в папке Trash. " +
                          "связь: " + EdgeTypeToRu(e.EdgeType) +
                          " источник: " + fromName +
                          " цель: " + toName;
                }

                issues.Add(new GraphIssue
                {
                    Type = GraphIssueType.ReferenceToTrash,
                    Severity = IssueSeverity.Warning,
                    FromId = e.FromId,
                    ToId = e.ToId,
                    PrimaryPath = primaryPath,
                    Message = msg
                });
            }
        }

        private static bool IsUnderTrash(string fullPath)
        {
            if (string.IsNullOrWhiteSpace(fullPath))
                return false;

            try
            {
                string p = fullPath.Replace('/', '\\').ToLowerInvariant();
                return p.Contains("\\trash\\") || p.EndsWith("\\trash");
            }
            catch
            {
                return false;
            }
        }

        private static string EdgeTypeToRu(EdgeType edgeType)
        {
            switch (edgeType)
            {
                case EdgeType.Composition:
                    return "Состав";

                case EdgeType.DrawingToModel:
                    return "Чертёж -> Модель";

                case EdgeType.ModelToDrawing:
                    return "Модель -> Чертёж";

                case EdgeType.AssemblyToSpecification:
                    return "Сборка -> Спецификация";

                case EdgeType.SpecificationToAssembly:
                    return "Спецификация -> Сборка";

                default:
                    return edgeType.ToString();
            }
        }

        private struct EdgeKey : IEquatable<EdgeKey>
        {
            private readonly EdgeType _type;
            private readonly int _from;
            private readonly int _to;

            public EdgeKey(EdgeType type, int from, int to)
            {
                _type = type;
                _from = from;
                _to = to;
            }

            public bool Equals(EdgeKey other)
            {
                return _type == other._type
                    && _from == other._from
                    && _to == other._to;
            }

            public override bool Equals(object obj)
            {
                if (!(obj is EdgeKey)) return false;
                return Equals((EdgeKey)obj);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    int h = 17;
                    h = h * 31 + (int)_type;
                    h = h * 31 + _from;
                    h = h * 31 + _to;
                    return h;
                }
            }
        }
    }
}