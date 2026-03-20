using KompasDropExport.Domain.Analysis;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Windows.Forms;

namespace KompasDropExport.UI.Analysis
{
    /// <summary>
    /// Преобразует результат анализа в UI:
    /// - заполняет BindingList для dgNodes/dgEdges
    /// - формирует объектные элементы в lbOrphans
    /// </summary>
    public sealed class GraphUiPresenter
    {
        public void Apply(
            AnalysisResult analysis,
            BindingList<NodeRow> nodeRows,
            BindingList<EdgeRow> edgeRows,
            ListBox lbOrphans)
        {
            if (analysis == null) throw new ArgumentNullException(nameof(analysis));
            if (nodeRows == null) throw new ArgumentNullException(nameof(nodeRows));
            if (edgeRows == null) throw new ArgumentNullException(nameof(edgeRows));
            if (lbOrphans == null) throw new ArgumentNullException(nameof(lbOrphans));

            FillGrids(analysis, nodeRows, edgeRows);
            FillAnalysisList(analysis, lbOrphans);
        }

        private void FillGrids(AnalysisResult analysis, BindingList<NodeRow> nodeRows, BindingList<EdgeRow> edgeRows)
        {
            nodeRows.Clear();
            edgeRows.Clear();

            for (int i = 0; i < analysis.Nodes.Count; i++)
            {
                var n = analysis.Nodes[i];

                nodeRows.Add(new NodeRow
                {
                    Id = n.Id,
                    NodeType = n.NodeType,
                    Location = n.Location,
                    Exists = n.Exists,
                    FileName = n.FileName,
                    RelPath = n.RelPath,
                    Path = n.FullPath,

                    InProduct = false,
                    DepthMin = -1,
                    IncomingComp = 0,
                    HasDrawing = false,
                    HasSpec = false
                });
            }

            for (int i = 0; i < analysis.Edges.Count; i++)
            {
                var e = analysis.Edges[i];

                edgeRows.Add(new EdgeRow
                {
                    EdgeType = e.EdgeType,
                    FromId = e.FromId,
                    ToId = e.ToId,
                    Count = e.Count,
                    IsResolved = e.IsResolved
                });
            }
        }

        private void FillAnalysisList(AnalysisResult analysis, ListBox lb)
        {
            lb.Items.Clear();

            AddHeader(lb, "ВНУТРЕННИЕ 3D-СИРОТЫ");
            AddNodeListOrNo(lb, BuildInternal3DOrphans(analysis), false);
            AddEmpty(lb);

            AddHeader(lb, "ПОТЕРЯННЫЕ ФАЙЛЫ В СОСТАВЕ ИЗДЕЛИЯ");
            AddNodeListOrNo(lb, BuildMissingInProduct(analysis), false);
            AddEmpty(lb);

            AddHeader(lb, "ВНЕШНИЕ ЗАВИСИМОСТИ (НЕ СТАНДАРТНЫЕ)");
            AddExternalDependencies(lb, analysis);
            AddEmpty(lb);

            AddHeader(lb, "ПРОБЛЕМЫ ЦЕЛОСТНОСТИ ГРАФА");
            AddHealthIssues(lb, analysis);
            AddEmpty(lb);

            AddHeader(lb, "ПРОВЕРКИ ПОЛНОТЫ ДОКУМЕНТАЦИИ");
            AddCompletenessIssues(lb, analysis);
            AddEmpty(lb);

            AddHeader(lb, "ПРОВЕРКА ИМЁН (ВЛОЖЕННОСТЬ/КОД)");
            AddNamingIssues(lb, analysis);
        }

        private static void AddHealthIssues(ListBox lb, AnalysisResult analysis)
        {
            if (analysis.HealthIssues == null || analysis.HealthIssues.Count == 0)
            {
                AddText(lb, "Нет", null, false);
                return;
            }

            // Индекс узлов по id
            var byId = new Dictionary<int, GraphNode>();
            for (int i = 0; i < analysis.Nodes.Count; i++)
            {
                var n = analysis.Nodes[i];
                if (n != null)
                    byId[n.Id] = n;
            }

            for (int i = 0; i < analysis.HealthIssues.Count; i++)
            {
                var it = analysis.HealthIssues[i];
                if (it == null) continue;

                // 1) Заголовок проблемы
                AddText(
                    lb,
                    "[" + SeverityToRu(it.Severity) + "] " + BuildHealthIssueTitle(it),
                    null,
                    false);

                // 2) Источник
                GraphNode from = null;
                if (it.FromId != 0)
                    byId.TryGetValue(it.FromId, out from);

                if (from != null)
                {
                    AddText(
                        lb,
                        "   Источник: " + (from.FileName ?? from.FullPath),
                        from.FullPath,
                        !string.IsNullOrWhiteSpace(from.FullPath));
                }

                // 3) Цель
                GraphNode to = null;
                if (it.ToId != 0)
                    byId.TryGetValue(it.ToId, out to);

                if (to != null)
                {
                    string targetText = "   Цель: " + (to.FileName ?? to.FullPath);
                    if (!to.Exists)
                        targetText += " (отсутствует)";

                    AddText(
                        lb,
                        targetText,
                        to.FullPath,
                        !string.IsNullOrWhiteSpace(to.FullPath));
                }

                AddEmpty(lb);
            }
        }

        private static string BuildHealthIssueTitle(GraphIssue issue)
        {
            if (issue == null)
                return "";

            switch (issue.Type)
            {
                case GraphIssueType.MissingTarget:
                    return "Отсутствует целевой файл";

                case GraphIssueType.MissingSource:
                    return "Отсутствует исходный файл";

                case GraphIssueType.Asymmetry_DrawingModel:
                    return "Несимметричная связь чертёж ↔ модель";

                case GraphIssueType.Asymmetry_AssemblySpec:
                    return "Несимметричная связь сборка ↔ спецификация";

                case GraphIssueType.MultipleTargets_DrawingToModel:
                    return "У чертежа несколько целей";

                case GraphIssueType.MultipleTargets_AssemblyToSpec:
                    return "У сборки несколько спецификаций";

                case GraphIssueType.DuplicateFileName_Internal:
                    return issue.Message ?? "Дубликат имени файла";

                case GraphIssueType.ReferenceToTrash:
                    return "Ссылка на папку Trash";

                case GraphIssueType.MissingDrawing:
                    return "Нет чертежа";

                case GraphIssueType.MissingSpecification:
                    return "Нет спецификации";

                case GraphIssueType.DrawingWithoutModel:
                    return "Чертёж не связан с моделью";

                case GraphIssueType.SpecificationWithoutAssembly:
                    return "Спецификация не связана со сборкой";

                default:
                    return issue.Message ?? issue.Type.ToString();
            }
        }

        private static string SeverityToRu(IssueSeverity severity)
        {
            switch (severity)
            {
                case IssueSeverity.Info:
                    return "Инфо";
                case IssueSeverity.Warning:
                    return "Предупреждение";
                case IssueSeverity.Error:
                    return "Ошибка";
                default:
                    return severity.ToString();
            }
        }

        private static void AddExternalDependencies(ListBox lb, AnalysisResult analysis)
        {
            var external = BuildExternalInProductNonStandard(analysis);

            if (external == null || external.Count == 0)
            {
                AddText(lb, "Нет", null, false);
                return;
            }

            // индекс узлов по id
            var byId = new Dictionary<int, GraphNode>();
            for (int i = 0; i < analysis.Nodes.Count; i++)
            {
                var n = analysis.Nodes[i];
                if (n != null)
                    byId[n.Id] = n;
            }

            // для каждого внешнего узла
            for (int i = 0; i < external.Count; i++)
            {
                var ext = external[i];

                AddText(lb, ext.FullPath, ext.FullPath, true);

                // ищем кто на него ссылается
                for (int e = 0; e < analysis.Edges.Count; e++)
                {
                    var edge = analysis.Edges[e];
                    if (edge == null) continue;

                    if (edge.ToId != ext.Id)
                        continue;

                    GraphNode from;
                    if (!byId.TryGetValue(edge.FromId, out from))
                        continue;

                    string name = from.FileName ?? from.FullPath;

                    AddText(
                        lb,
                        "   ← " + name,
                        from.FullPath,
                        !string.IsNullOrWhiteSpace(from.FullPath));
                }

                AddEmpty(lb);
            }
        }

        private static void AddNamingIssues(ListBox lb, AnalysisResult analysis)
        {
            if (analysis.NamingIssues == null || analysis.NamingIssues.Count == 0)
            {
                AddText(lb, "Нет", null, false);
                return;
            }

            for (int i = 0; i < analysis.NamingIssues.Count; i++)
            {
                var item = analysis.NamingIssues[i];
                if (item == null) continue;

                string text = FormatNamingIssue(item);
                string path = ExtractNamingIssueFullPath(item, analysis);

                AddText(lb, text, path, !string.IsNullOrWhiteSpace(path));
            }
        }

        private static void AddCompletenessIssues(ListBox lb, AnalysisResult analysis)
        {
            if (analysis.CompletenessIssues == null || analysis.CompletenessIssues.Count == 0)
            {
                AddText(lb, "Нет", null, false);
                return;
            }

            for (int i = 0; i < analysis.CompletenessIssues.Count; i++)
            {
                var it = analysis.CompletenessIssues[i];
                if (it == null) continue;

                AddText(
                    lb,
                    "[" + SeverityToRu(it.Severity) + "] " + it.Message,
                    it.PrimaryPath,
                    !string.IsNullOrWhiteSpace(it.PrimaryPath));
            }
        }

        private static List<GraphNode> BuildInternal3DOrphans(AnalysisResult analysis)
        {
            var result = new List<GraphNode>();

            for (int i = 0; i < analysis.Nodes.Count; i++)
            {
                var node = analysis.Nodes[i];
                if (node == null) continue;

                NodeAnalysisInfo info;
                if (!analysis.NodeAnalysis.TryGetValue(node.Id, out info) || info == null)
                    continue;

                if (info.IsOrphan3D)
                    result.Add(node);
            }

            return result;
        }

        private static List<GraphNode> BuildMissingInProduct(AnalysisResult analysis)
        {
            var result = new List<GraphNode>();

            for (int i = 0; i < analysis.Nodes.Count; i++)
            {
                var node = analysis.Nodes[i];
                if (node == null) continue;

                NodeAnalysisInfo info;
                if (!analysis.NodeAnalysis.TryGetValue(node.Id, out info) || info == null)
                    continue;

                if (info.InProduct && info.IsMissing)
                    result.Add(node);
            }

            return result;
        }

        private static List<GraphNode> BuildExternalInProductNonStandard(AnalysisResult analysis)
        {
            var result = new List<GraphNode>();

            for (int i = 0; i < analysis.Nodes.Count; i++)
            {
                var node = analysis.Nodes[i];
                if (node == null) continue;

                NodeAnalysisInfo info;
                if (!analysis.NodeAnalysis.TryGetValue(node.Id, out info) || info == null)
                    continue;

                if (info.InProduct && info.IsExternal && !info.IsStandard)
                    result.Add(node);
            }

            return result;
        }

        private static string FormatNamingIssue(object item)
        {
            if (item == null) return "";

            string pathOrRel = TryReadMemberAsString(item, "PathOrRel");
            string message = TryReadMemberAsString(item, "Message");

            if (!string.IsNullOrWhiteSpace(pathOrRel) && !string.IsNullOrWhiteSpace(message))
                return pathOrRel + " — " + message;

            if (!string.IsNullOrWhiteSpace(message))
                return message;

            return item.ToString();
        }

        private static string ExtractNamingIssueFullPath(object item, AnalysisResult analysis)
        {
            if (item == null || analysis == null || analysis.Nodes == null)
                return null;

            // 1) Сначала пробуем взять NodeId и найти реальный узел
            int? nodeId = TryReadMemberAsInt(item, "NodeId");
            if (nodeId.HasValue)
            {
                for (int i = 0; i < analysis.Nodes.Count; i++)
                {
                    var n = analysis.Nodes[i];
                    if (n == null) continue;

                    if (n.Id == nodeId.Value)
                        return n.FullPath;
                }
            }

            // 2) Fallback: если вдруг в issue уже лежит полный путь
            string pathOrRel = TryReadMemberAsString(item, "PathOrRel");
            if (!string.IsNullOrWhiteSpace(pathOrRel))
                return pathOrRel;

            return null;
        }

        private static int? TryReadMemberAsInt(object obj, string name)
        {
            if (obj == null) return null;

            try
            {
                var p = obj.GetType().GetProperty(name, BindingFlags.Instance | BindingFlags.Public);
                if (p != null)
                {
                    var v = p.GetValue(obj, null);
                    if (v != null) return Convert.ToInt32(v);
                }
            }
            catch { }

            try
            {
                var f = obj.GetType().GetField(name, BindingFlags.Instance | BindingFlags.Public);
                if (f != null)
                {
                    var v = f.GetValue(obj);
                    if (v != null) return Convert.ToInt32(v);
                }
            }
            catch { }

            return null;
        }

        private static string TryReadMemberAsString(object obj, string name)
        {
            if (obj == null) return null;

            try
            {
                var p = obj.GetType().GetProperty(name, BindingFlags.Instance | BindingFlags.Public);
                if (p != null)
                {
                    var v = p.GetValue(obj, null);
                    if (v != null) return Convert.ToString(v);
                }
            }
            catch { }

            try
            {
                var f = obj.GetType().GetField(name, BindingFlags.Instance | BindingFlags.Public);
                if (f != null)
                {
                    var v = f.GetValue(obj);
                    if (v != null) return Convert.ToString(v);
                }
            }
            catch { }

            return null;
        }

        private static void AddHeader(ListBox lb, string title)
        {
            lb.Items.Add(new AnalysisListItem
            {
                Text = "=== " + title + " ===",
                PrimaryPath = null,
                CanOpen = false
            });
        }

        private static void AddEmpty(ListBox lb)
        {
            lb.Items.Add(new AnalysisListItem
            {
                Text = "",
                PrimaryPath = null,
                CanOpen = false
            });
        }

        private static void AddText(ListBox lb, string text, string primaryPath, bool canOpen)
        {
            lb.Items.Add(new AnalysisListItem
            {
                Text = text ?? "",
                PrimaryPath = primaryPath,
                CanOpen = canOpen
            });
        }

        private static void AddNodeListOrNo(ListBox lb, List<GraphNode> nodes, bool showFullPath)
        {
            if (nodes == null || nodes.Count == 0)
            {
                AddText(lb, "Нет", null, false);
                return;
            }

            nodes.Sort(delegate (GraphNode a, GraphNode b)
            {
                int ta = (a.NodeType == NodeType.ModelAssembly) ? 0 : 1;
                int tb = (b.NodeType == NodeType.ModelAssembly) ? 0 : 1;
                int c = ta.CompareTo(tb);
                if (c != 0) return c;

                return string.Compare(a.RelPath, b.RelPath, StringComparison.OrdinalIgnoreCase);
            });

            for (int i = 0; i < nodes.Count; i++)
            {
                var n = nodes[i];
                string text;

                if (showFullPath)
                {
                    text = n.FullPath;
                }
                else
                {
                    if (!string.IsNullOrEmpty(n.RelPath))
                        text = n.RelPath;
                    else
                        text = n.FileName ?? n.FullPath;
                }

                AddText(lb, text, n.FullPath, !string.IsNullOrWhiteSpace(n.FullPath));
            }
        }
    }
}