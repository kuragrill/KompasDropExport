using System;
using System.Collections.Generic;
using System.IO;
using KompasDropExport.Domain.Analysis;
using KompasDropExport.UI.Analysis;

namespace KompasDropExport.Services.Analysis
{
    /// <summary>
    /// Строит иерархический список строк для таблицы ручного переименования.
    ///
    /// Логика:
    /// - берём только узлы изделия (InProduct)
    /// - только внутренние
    /// - только существующие
    /// - не стандартные
    ///
    /// Порядок:
    /// 1) 3D-узел
    /// 2) его чертежи
    /// 3) его спецификация (если сборка)
    /// 4) его children по Composition
    /// </summary>
    public static class RenameTreeBuilder
    {
        public static List<RenameTableRow> Build(AnalysisResult analysis)
        {
            var rows = new List<RenameTableRow>();
            if (analysis == null || analysis.Nodes == null || analysis.Edges == null || analysis.NodeAnalysis == null)
                return rows;

            var nodeById = new Dictionary<int, GraphNode>();
            for (int i = 0; i < analysis.Nodes.Count; i++)
            {
                var n = analysis.Nodes[i];
                if (n != null)
                    nodeById[n.Id] = n;
            }

            // Ищем root: внутренний 3D узел изделия с DepthMin = 0
            GraphNode root = FindRootNode(analysis);
            if (root == null)
                return rows;

            // Индексы рёбер
            var compositionChildren = new Dictionary<int, List<int>>();
            var modelDrawings = new Dictionary<int, List<int>>();          // 3D -> CDW
            var assemblySpecifications = new Dictionary<int, List<int>>(); // A3D -> SPW
            var specificationDocuments = new Dictionary<int, List<int>>(); // SPW -> CDW

            for (int i = 0; i < analysis.Edges.Count; i++)
            {
                var e = analysis.Edges[i];
                if (e == null) continue;

                if (e.EdgeType == EdgeType.Composition)
                    AddToMap(compositionChildren, e.FromId, e.ToId);

                if (e.EdgeType == EdgeType.ModelToDrawing)
                    AddToMap(modelDrawings, e.FromId, e.ToId);

                if (e.EdgeType == EdgeType.AssemblyToSpecification)
                    AddToMap(assemblySpecifications, e.FromId, e.ToId);

                if (e.EdgeType == EdgeType.SpecificationToDocument)
                    AddToMap(specificationDocuments, e.FromId, e.ToId);
            }

            var emittedNodeIds = new HashSet<int>();

            // Строим дерево от root
            Append3DSubtree(
                root.Id,
                level: 0,
                analysis: analysis,
                nodeById: nodeById,
                compositionChildren: compositionChildren,
                modelDrawings: modelDrawings,
                assemblySpecifications: assemblySpecifications,
                specificationDocuments: specificationDocuments,
                emittedNodeIds: emittedNodeIds,
                rows: rows);

            return rows;
        }

        private static GraphNode FindRootNode(AnalysisResult analysis)
        {
            for (int i = 0; i < analysis.Nodes.Count; i++)
            {
                var n = analysis.Nodes[i];
                if (n == null) continue;

                NodeAnalysisInfo info;
                if (!analysis.NodeAnalysis.TryGetValue(n.Id, out info) || info == null)
                    continue;

                if (!info.InProduct) continue;
                if (!info.IsInternal) continue;
                if (!info.Exists) continue;

                if (n.NodeType == NodeType.ModelAssembly &&
                    info.DepthMin.HasValue &&
                    info.DepthMin.Value == 0)
                {
                    return n;
                }
            }

            return null;
        }

        private static void Append3DSubtree(
            int nodeId,
            int level,
            AnalysisResult analysis,
            Dictionary<int, GraphNode> nodeById,
            Dictionary<int, List<int>> compositionChildren,
            Dictionary<int, List<int>> modelDrawings,
            Dictionary<int, List<int>> assemblySpecifications,
            Dictionary<int, List<int>> specificationDocuments,
            HashSet<int> emittedNodeIds,
            List<RenameTableRow> rows)
        {
            GraphNode node;
            if (!nodeById.TryGetValue(nodeId, out node) || node == null)
                return;

            if (!ShouldInclude3DNode(node, analysis))
                return;

            if (!emittedNodeIds.Contains(node.Id))
            {
                rows.Add(MakeRow(node, level, analysis));
                emittedNodeIds.Add(node.Id);
            }

            // 1) Чертежи этого 3D-узла
            List<int> drawingIds;
            if (modelDrawings.TryGetValue(node.Id, out drawingIds) && drawingIds != null)
            {
                drawingIds.Sort((a, b) => CompareNodesByPath(a, b, nodeById));

                for (int i = 0; i < drawingIds.Count; i++)
                {
                    GraphNode drw;
                    if (!nodeById.TryGetValue(drawingIds[i], out drw) || drw == null)
                        continue;

                    if (!ShouldIncludeAttachedDocument(drw, analysis))
                        continue;

                    if (!emittedNodeIds.Contains(drw.Id))
                    {
                        rows.Add(MakeRow(drw, level, analysis));
                        emittedNodeIds.Add(drw.Id);
                    }
                }
            }

            // 2) Спецификация этой сборки
           if (node.NodeType == NodeType.ModelAssembly)
            {
                List<int> specIds;
                if (assemblySpecifications.TryGetValue(node.Id, out specIds) && specIds != null)
                {
                    specIds.Sort((a, b) => CompareNodesByPath(a, b, nodeById));

                    for (int i = 0; i < specIds.Count; i++)
                    {
                        GraphNode spw;
                        if (!nodeById.TryGetValue(specIds[i], out spw) || spw == null)
                            continue;

                        if (!ShouldIncludeAttachedDocument(spw, analysis))
                            continue;

                        if (!emittedNodeIds.Contains(spw.Id))
                        {
                            rows.Add(MakeRow(spw, level, analysis));
                            emittedNodeIds.Add(spw.Id);
                        }

                        // ДОКУМЕНТЫ СПЕЦИФИКАЦИИ
                        List<int> docIds;
                        if (specificationDocuments.TryGetValue(spw.Id, out docIds) && docIds != null)
                        {
                            docIds.Sort((a, b) => CompareNodesByPath(a, b, nodeById));

                            for (int j = 0; j < docIds.Count; j++)
                            {
                                GraphNode docNode;
                                if (!nodeById.TryGetValue(docIds[j], out docNode) || docNode == null)
                                    continue;

                                if (!ShouldIncludeAttachedDocument(docNode, analysis))
                                    continue;

                                if (!emittedNodeIds.Contains(docNode.Id))
                                {
                                    rows.Add(MakeRow(docNode, level + 1, analysis));
                                    emittedNodeIds.Add(docNode.Id);
                                }
                            }
                        }
                    }
                }
            }

            // 3) Дети по составу
            List<int> childIds;
            if (compositionChildren.TryGetValue(node.Id, out childIds) && childIds != null)
            {
                childIds.Sort((a, b) => CompareChildrenForTree(a, b, nodeById));

                for (int i = 0; i < childIds.Count; i++)
                {
                    Append3DSubtree(
                        childIds[i],
                        level + 1,
                        analysis,
                        nodeById,
                        compositionChildren,
                        modelDrawings,
                        assemblySpecifications,
                        specificationDocuments,
                        emittedNodeIds,
                        rows);
                }
            }
        }

        private static bool ShouldInclude3DNode(GraphNode node, AnalysisResult analysis)
        {
            if (node == null) return false;

            NodeAnalysisInfo info;
            if (!analysis.NodeAnalysis.TryGetValue(node.Id, out info) || info == null)
                return false;

            if (!info.InProduct) return false;
            if (!info.IsInternal) return false;
            if (!info.Exists) return false;
            if (info.IsStandard) return false;

            return node.NodeType == NodeType.ModelAssembly ||
                   node.NodeType == NodeType.ModelPart;
        }

        private static bool ShouldIncludeAttachedDocument(GraphNode node, AnalysisResult analysis)
        {
            if (node == null) return false;

            NodeAnalysisInfo info;
            if (!analysis.NodeAnalysis.TryGetValue(node.Id, out info) || info == null)
                return false;

            if (!info.IsInternal) return false;
            if (!info.Exists) return false;
            if (info.IsStandard) return false;

            return node.NodeType == NodeType.Drawing ||
                   node.NodeType == NodeType.Specification;
        }

        private static RenameTableRow MakeRow(GraphNode node, int level, AnalysisResult analysis)
        {
            string fileName = node.FileName ?? "";
            string baseName = Path.GetFileNameWithoutExtension(fileName) ?? fileName;
            string ext = Path.GetExtension(fileName) ?? "";

            string designation = "";
            string title = "";

            if (analysis != null && analysis.NodeDocuments != null)
            {
                NodeDocumentInfo docInfo;
                if (analysis.NodeDocuments.TryGetValue(node.Id, out docInfo) && docInfo != null)
                {
                    designation = docInfo.Designation ?? "";
                    title = docInfo.Title ?? "";
                }
            }

            return new RenameTableRow
            {
                NodeId = node.Id,
                Level = level,

                OriginalBaseName = baseName,
                BaseName = baseName,
                Extension = ext,

                Designation = designation,
                Title = title,

                Kind = KindToRu(node.NodeType),
                FullPath = node.FullPath
            };
        }

        private static void AddToMap(Dictionary<int, List<int>> map, int key, int value)
        {
            List<int> list;
            if (!map.TryGetValue(key, out list))
            {
                list = new List<int>();
                map[key] = list;
            }

            if (!list.Contains(value))
                list.Add(value);
        }

        private static int CompareChildrenForTree(int aId, int bId, Dictionary<int, GraphNode> nodeById)
        {
            GraphNode a = null;
            GraphNode b = null;
            nodeById.TryGetValue(aId, out a);
            nodeById.TryGetValue(bId, out b);

            if (a == null && b == null) return 0;
            if (a == null) return 1;
            if (b == null) return -1;

            // Сначала детали, потом сборки, потом по пути
            int ak = GetChildSortKind(a);
            int bk = GetChildSortKind(b);

            int c = ak.CompareTo(bk);
            if (c != 0) return c;

            return string.Compare(
                a.RelPath ?? a.FullPath,
                b.RelPath ?? b.FullPath,
                StringComparison.OrdinalIgnoreCase);
        }

        private static int GetChildSortKind(GraphNode node)
        {
            if (node == null) return 99;

            if (node.NodeType == NodeType.ModelPart)
                return 0;

            if (node.NodeType == NodeType.ModelAssembly)
                return 1;

            if (node.NodeType == NodeType.Drawing)
                return 2;

            if (node.NodeType == NodeType.Specification)
                return 3;

            return 99;
        }

        private static int CompareNodesByPath(int aId, int bId, Dictionary<int, GraphNode> nodeById)
        {
            GraphNode a = null;
            GraphNode b = null;
            nodeById.TryGetValue(aId, out a);
            nodeById.TryGetValue(bId, out b);

            if (a == null && b == null) return 0;
            if (a == null) return 1;
            if (b == null) return -1;

            return string.Compare(
                a.RelPath ?? a.FullPath,
                b.RelPath ?? b.FullPath,
                StringComparison.OrdinalIgnoreCase);
        }

        private static string KindToRu(NodeType nodeType)
        {
            switch (nodeType)
            {
                case NodeType.ModelAssembly:
                    return "Сборка";

                case NodeType.ModelPart:
                    return "Деталь";

                case NodeType.Drawing:
                    return "Чертёж";

                case NodeType.Specification:
                    return "Спецификация";

                default:
                    return nodeType.ToString();
            }
        }
    }
}