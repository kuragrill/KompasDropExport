namespace KompasDropExport.Domain.Analysis
{
    // Ребро графа: связь между двумя узлами
    public sealed class GraphEdge
    {
        public EdgeType EdgeType { get; }

        public int FromId { get; }
        public int ToId { get; }

        // Для состава: сколько раз компонент входит в родителя (пока 1)
        public int Count { get; set; } = 1;

        public bool IsResolved { get; set; }

        public GraphEdge(EdgeType edgeType, int fromId, int toId)
        {
            EdgeType = edgeType;
            FromId = fromId;
            ToId = toId;
        }
    }
}