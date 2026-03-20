namespace KompasDropExport.Domain.Analysis
{
    // Тип узла (что это за файл/документ)
    public enum NodeType
    {
        ModelAssembly,
        ModelPart,
        Drawing,
        Specification,
        StandardPart,
        Unknown
    }

    // Где находится файл относительно корня проекта
    public enum NodeLocation
    {
        Internal,  // внутри папки проекта
        External,  // вне папки проекта, но на него есть ссылка
        Missing    // ссылка есть, но файла нет
    }

    // Тип ребра (тип связи)
    public enum EdgeType
    {
        Composition = 0,

        // 3D <-> 2D
        ModelToDrawing = 10,
        DrawingToModel = 11,
        
        // 3D <-> Specification
        AssemblyToSpecification = 20,
        SpecificationToAssembly = 21,

        // Specification -> documents from "Документация" section
        SpecificationToDocument = 30
    }
}