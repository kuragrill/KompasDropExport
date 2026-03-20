namespace KompasDropExport.Domain
{

    internal enum NameSeparator
    {
        Space = 0,
        Dash = 1,
        Underscore = 2
    }
    internal sealed class ExportOptions
    {

        // PDF
        public bool PdfExcludeAssemblyDrawingsByName { get; set; } // чекбокс "Исключить сборки (СБ)"
        public bool PdfExcludeSpecs { get; set; }                  // чекбокс "Исключить .spw"

        // STEP
        public bool StepExcludeAssemblies { get; set; }            // чекбокс "Исключить .a3d"
        public bool StepExcludeOtherTag { get; set; }              // чекбокс "Исключить [ПРОЧИЕ]"

        public NameSeparator ActiveNameSeparator { get; set; } = NameSeparator.Space;
    }

    internal sealed class ExportResult
    {
        public int Ok { get; set; }
        public int Skip { get; set; }
        public int Err { get; set; }
        public int PdfArchiveWritten { get; set; }                 // сколько реально ушло в PDF_archive
        public int PdfArchiveSkippedBadMarking { get; set; }       // сколько НЕ ушло в архив из-за невалидного обозначения
    }
}