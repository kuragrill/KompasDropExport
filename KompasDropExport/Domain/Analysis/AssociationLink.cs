namespace KompasDropExport.Domain.Analysis
{
    public sealed class AssociationLink
    {
        public string FromPath { get; set; }
        public string ToPath { get; set; }
        public EdgeType EdgeType { get; set; }
    }
}