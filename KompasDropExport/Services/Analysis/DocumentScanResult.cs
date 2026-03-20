using KompasDropExport.Domain.Analysis;
using System.Collections.Generic;

namespace KompasDropExport.Services.Analysis
{
    internal sealed class DocumentScanResult
    {
        public List<AssociationLink> Links { get; set; }

        public string Designation { get; set; }
        public string Title { get; set; }

        public bool MetadataReadSucceeded { get; set; }

        public DocumentScanResult()
        {
            Links = new List<AssociationLink>();
        }
    }
}