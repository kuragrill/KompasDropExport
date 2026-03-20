namespace KompasDropExport.UI.Analysis
{
    /// <summary>
    /// Элемент для ListBox анализа.
    /// Хранит текст, путь для открытия и признак, можно ли реагировать на double click.
    /// </summary>
    public sealed class AnalysisListItem
    {
        public string Text { get; set; }
        public string PrimaryPath { get; set; }
        public bool CanOpen { get; set; }

        public override string ToString()
        {
            return Text ?? "";
        }
    }
}