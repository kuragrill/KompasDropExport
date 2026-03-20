using System;
using System.Collections.Generic;
using KompasDropExport.Domain.Analysis;

namespace KompasDropExport.Services.Analysis
{
    /// <summary>
    /// Считает производную информацию по узлам после построения графа.
    /// Ничего не записывает в GraphNode — возвращает отдельный словарь NodeId -> NodeAnalysisInfo.
    /// </summary>
    public static class GraphNodeAnalysisCalculator
    {
        public static Dictionary<int, NodeAnalysisInfo> Compute(
            List<GraphNode> nodes,
            List<GraphEdge> edges,
            int rootId)
        {
            var result = new Dictionary<int, NodeAnalysisInfo>();
            if (nodes == null) return result;
            if (edges == null) edges = new List<GraphEdge>();

            // Индекс узлов
            var nodeById = new Dictionary<int, GraphNode>();
            for (int i = 0; i < nodes.Count; i++)
            {
                var n = nodes[i];
                if (n == null) continue;

                nodeById[n.Id] = n;

                var info = new NodeAnalysisInfo();
                info.NodeId = n.Id;

                // Базовые derived-признаки по самому узлу
                info.Exists = n.Exists;
                info.IsInternal = n.Location == NodeLocation.Internal;
                info.IsExternal = n.Location == NodeLocation.External;
                info.IsMissing = n.Location == NodeLocation.Missing || !n.Exists;

                result[n.Id] = info;
            }

            // Индексы по composition-графу
            var compOutgoing = new Dictionary<int, List<int>>();
            var compIncomingCount = new Dictionary<int, int>();

            // Ассоциативные признаки
            for (int i = 0; i < edges.Count; i++)
            {
                var e = edges[i];
                if (e == null) continue;

                NodeAnalysisInfo fromInfo;
                NodeAnalysisInfo toInfo;

                result.TryGetValue(e.FromId, out fromInfo);
                result.TryGetValue(e.ToId, out toInfo);

                // -------- Composition --------
                if (e.EdgeType == EdgeType.Composition)
                {
                    List<int> lst;
                    if (!compOutgoing.TryGetValue(e.FromId, out lst))
                    {
                        lst = new List<int>();
                        compOutgoing[e.FromId] = lst;
                    }
                    lst.Add(e.ToId);

                    int inc;
                    compIncomingCount.TryGetValue(e.ToId, out inc);
                    compIncomingCount[e.ToId] = inc + 1;
                }

                // -------- Ассоциации --------
                // 3D -> CDW
                if (e.EdgeType == EdgeType.ModelToDrawing)
                {
                    if (fromInfo != null) fromInfo.HasDrawing = true;
                }

                // A3D -> SPW
                if (e.EdgeType == EdgeType.AssemblyToSpecification)
                {
                    if (fromInfo != null) fromInfo.HasSpecification = true;
                }

                // CDW -> 3D
                if (e.EdgeType == EdgeType.DrawingToModel)
                {
                    if (fromInfo != null) fromInfo.DrawingHasModel = true;
                }

                // SPW -> A3D
                if (e.EdgeType == EdgeType.SpecificationToAssembly)
                {
                    if (fromInfo != null) fromInfo.SpecificationHasAssembly = true;
                }
            }

            // IncomingCompositionCount
            foreach (var kv in result)
            {
                int cnt;
                compIncomingCount.TryGetValue(kv.Key, out cnt);
                kv.Value.IncomingCompositionCount = cnt;
            }

            // InProduct + DepthMin по composition-графу от rootId
            if (rootId != 0 && result.ContainsKey(rootId))
            {
                ComputeReachabilityAndDepth(rootId, compOutgoing, result);
            }

            // IsOrphan3D
            foreach (var kv in result)
            {
                GraphNode node;
                if (!nodeById.TryGetValue(kv.Key, out node)) continue;

                bool is3D = node.NodeType == NodeType.ModelAssembly || node.NodeType == NodeType.ModelPart;
                kv.Value.IsOrphan3D = is3D && kv.Value.IsInternal && !kv.Value.InProduct;
            }

            // IsStandard
            // Пока оставляем простую эвристику:
            // стандартной считаем внутреннюю/внешнюю 3D-деталь, у которой есть входящие composition,
            // но путь/имя выглядит как стандарт/покупное/прочее.
            // Если у тебя уже есть более надёжная логика — потом вынесем сюда.
            foreach (var kv in result)
            {
                GraphNode node;
                if (!nodeById.TryGetValue(kv.Key, out node)) continue;

                kv.Value.IsStandard = GuessIsStandard(node);
            }

            return result;
        }

        private static void ComputeReachabilityAndDepth(
            int rootId,
            Dictionary<int, List<int>> compOutgoing,
            Dictionary<int, NodeAnalysisInfo> result)
        {
            var q = new Queue<int>();

            NodeAnalysisInfo rootInfo;
            if (!result.TryGetValue(rootId, out rootInfo)) return;

            rootInfo.InProduct = true;
            rootInfo.DepthMin = 0;
            q.Enqueue(rootId);

            while (q.Count > 0)
            {
                int cur = q.Dequeue();

                List<int> children;
                if (!compOutgoing.TryGetValue(cur, out children) || children == null)
                    continue;

                NodeAnalysisInfo curInfo;
                if (!result.TryGetValue(cur, out curInfo) || !curInfo.DepthMin.HasValue)
                    continue;

                int nextDepth = curInfo.DepthMin.Value + 1;

                for (int i = 0; i < children.Count; i++)
                {
                    int childId = children[i];

                    NodeAnalysisInfo childInfo;
                    if (!result.TryGetValue(childId, out childInfo))
                        continue;

                    bool shouldEnqueue = false;

                    if (!childInfo.InProduct)
                    {
                        childInfo.InProduct = true;
                        shouldEnqueue = true;
                    }

                    if (!childInfo.DepthMin.HasValue || nextDepth < childInfo.DepthMin.Value)
                    {
                        childInfo.DepthMin = nextDepth;
                        shouldEnqueue = true;
                    }

                    if (shouldEnqueue)
                        q.Enqueue(childId);
                }
            }
        }

        private static bool GuessIsStandard(GraphNode node)
        {
            if (node == null) return false;

            // Пока максимально простая и безопасная эвристика.
            // Потом можно заменить на чтение свойства/классификатора.
            string s = (node.FullPath ?? "") + " " + (node.FileName ?? "");
            s = s.ToLowerInvariant();

            if (s.Contains("libs")) return true;


            return false;
        }
    }
}