namespace KompasDropExport.Domain.Analysis
{
    /// <summary>
    /// Связь состава: parent -> child (вхождение компонента в сборку).
    /// IsStandard — эвристика: библиотечный компонент (например KOMPAS Libs).
    /// </summary>
    public class CompositionLink
    {
        public string ParentPath { get; set; }
        public string ChildPath { get; set; }

        // Пока определяем "стандартность" по пути (эвристика)
        public bool IsStandard { get; set; }
    }
}