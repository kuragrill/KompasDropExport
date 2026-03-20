using KompasDropExport.Domain.Analysis;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace KompasDropExport.Services.Analysis
{
    /// <summary>
    /// Проверка имён файлов по "коду изделия" и вложенности.
    /// Работает на основе:
    /// - rootAssemblyPath (имя root)
    /// - nodes/edges (граф Composition уже построен)
    /// - nodeAnalysis (результаты анализа узлов: InProduct / IsStandard и т.п.)
    ///
    /// Паттерн кода:
    ///   AAAA.111.222.333 [опционально пробел и текст]
    ///
    /// Правила:
    /// 1) Для всех узлов InProduct (внутренних) первые две группы должны совпадать с root.
    /// 2) Для сборок (ModelAssembly) 4-я группа должна быть нулевая (все символы '0').
    /// 3) Для деталей (ModelPart) 4-я группа НЕ должна быть нулевая.
    /// 4) Для деталей 3-я группа должна совпадать с 3-й группой хотя бы одной родительской сборки, куда деталь входит.
    /// </summary>
    public static class FileNameNamingChecker
    {
        // 4 буквы (латиница/кириллица) + 3 цифровых блока.
        // После 4-го блока допускаем конец строки ИЛИ пробел+что угодно.
        private static readonly Regex CodeRegex = new Regex(
            @"^(?<g1>[A-Za-zА-Яа-я]{4})\.(?<g2>\d+)\.(?<g3>\d+)\.(?<g4>\d+)(?:\s+.*|-.*)?$",
            //@"^(?<g1>[A-Za-zА-Яа-я]{4})\.(?<g2>\d+)\.(?<g3>\d+)\.(?<g4>\d+)(?:\s+.*)?$",
            RegexOptions.Compiled);

        public sealed class NamingIssue
        {
            public int NodeId { get; set; }
            public string PathOrRel { get; set; }
            public string Message { get; set; }

            public override string ToString()
            {
                if (!string.IsNullOrWhiteSpace(PathOrRel) && !string.IsNullOrWhiteSpace(Message))
                    return PathOrRel + " — " + Message;

                if (!string.IsNullOrWhiteSpace(Message))
                    return Message;

                return base.ToString();
            }
        }

        public static List<NamingIssue> Check(
            string rootAssemblyPath,
            List<GraphNode> nodes,
            List<GraphEdge> edges,
            Dictionary<int, NodeAnalysisInfo> nodeAnalysis)
        {
            var issues = new List<NamingIssue>();

            if (string.IsNullOrWhiteSpace(rootAssemblyPath) || nodes == null || edges == null || nodeAnalysis == null)
                return issues;

            string rootName = Path.GetFileNameWithoutExtension(rootAssemblyPath) ?? "";
            CodeParts rootCode;
            if (!TryParseCode(rootName, out rootCode))
            {
                issues.Add(new NamingIssue
                {
                    NodeId = 0,
                    PathOrRel = rootAssemblyPath,
                    Message = "Имя корневой сборки не соответствует шаблону AAAA.123.456.789 [текст]. Проверка именования не выполнена."
                });
                return issues;
            }

            // Индекс узлов по Id
            var byId = new Dictionary<int, GraphNode>();
            for (int i = 0; i < nodes.Count; i++)
            {
                if (nodes[i] == null) continue;
                byId[nodes[i].Id] = nodes[i];
            }

            // Для правила 4 (деталь должна соответствовать третьему блоку хотя бы одной род. сборки)
            // Собираем для каждого узла список родительских сборок (по Composition: parent -> child).
            var parentsByChild = new Dictionary<int, List<int>>();
            for (int i = 0; i < edges.Count; i++)
            {
                var e = edges[i];
                if (e == null) continue;
                if (e.EdgeType != EdgeType.Composition)
                    continue;

                List<int> list;
                if (!parentsByChild.TryGetValue(e.ToId, out list))
                {
                    list = new List<int>();
                    parentsByChild[e.ToId] = list;
                }
                list.Add(e.FromId);
            }

            // Проверяем узлы изделия
            for (int i = 0; i < nodes.Count; i++)
            {
                var n = nodes[i];
                if (n == null) continue;

                NodeAnalysisInfo info;
                if (!nodeAnalysis.TryGetValue(n.Id, out info) || info == null)
                    continue;

                // Проверяем только то, что входит в изделие
                if (!info.InProduct)
                    continue;

                // И только внутренние
                if (!info.IsInternal)
                    continue;

                // Стандартные / библиотечные / покупные пропускаем
                if (info.IsStandard)
                    continue;

                // Отсутствующие файлы не проверяем по имени
                if (info.IsMissing)
                    continue;

                // Пока ограничим 3D
                if (n.NodeType != NodeType.ModelAssembly && n.NodeType != NodeType.ModelPart)
                    continue;

                string name = n.FileName ?? "";
                name = Path.GetFileNameWithoutExtension(name) ?? "";

                // Дополнительный старый словесный пропуск оставим как страховку
                if (ContainsIgnoreCase(name, "ПРОЧИЕ") ||
                    ContainsIgnoreCase(name, "СТАНДАРТ"))
                {
                    continue;
                }

                CodeParts code;
                if (!TryParseCode(name, out code))
                {
                    issues.Add(MakeIssue(n, "Имя не соответствует шаблону AAAA.123.456.789 [текст]."));
                    continue;
                }

                // Правило 1: первые 2 группы совпадают
                if (!StringEquals(code.G1, rootCode.G1) || !StringEquals(code.G2, rootCode.G2))
                {
                    issues.Add(MakeIssue(
                        n,
                        string.Format(
                            "Первые 2 группы не совпадают с root. Ожидается: {0}.{1}.xxxx.xxxx",
                            rootCode.G1,
                            rootCode.G2)));
                }

                // Правило 2: сборки -> 4-й блок нулевой
                if (n.NodeType == NodeType.ModelAssembly)
                {
                    if (!IsAllZeros(code.G4))
                        issues.Add(MakeIssue(n, "Сборка: 4-й блок должен быть нулевым (например, 0000)."));
                }

                // Правило 3+4: детали
                if (n.NodeType == NodeType.ModelPart)
                {
                    if (IsAllZeros(code.G4))
                        issues.Add(MakeIssue(n, "Деталь: 4-й блок не должен быть нулевым."));

                    // Правило 4: 3-й блок детали должен совпадать с 3-м блоком хотя бы одной родительской сборки
                    List<int> parents;
                    if (!parentsByChild.TryGetValue(n.Id, out parents) || parents == null || parents.Count == 0)
                    {
                        // В изделии деталь без родителя — странно, но возможно при битом графе.
                        issues.Add(MakeIssue(n, "Деталь: не найдены родительские сборки для проверки 3-го блока."));
                    }
                    else
                    {
                        bool ok = false;

                        for (int p = 0; p < parents.Count; p++)
                        {
                            GraphNode parentNode;
                            if (!byId.TryGetValue(parents[p], out parentNode))
                                continue;

                            if (parentNode == null || parentNode.NodeType != NodeType.ModelAssembly)
                                continue;

                            string pName = Path.GetFileNameWithoutExtension(parentNode.FileName ?? "") ?? "";
                            CodeParts pCode;
                            if (!TryParseCode(pName, out pCode))
                                continue;

                            if (StringEquals(pCode.G3, code.G3))
                            {
                                ok = true;
                                break;
                            }
                        }

                        if (!ok)
                        {
                            issues.Add(MakeIssue(
                                n,
                                "Деталь: 3-й блок должен совпадать с 3-м блоком хотя бы одной родительской сборки."));
                        }
                    }
                }
            }

            return issues;
        }

        private static NamingIssue MakeIssue(GraphNode n, string msg)
        {
            return new NamingIssue
            {
                NodeId = n.Id,
                PathOrRel = !string.IsNullOrEmpty(n.RelPath) ? n.RelPath : (n.FullPath ?? n.FileName),
                Message = msg
            };
        }

        private struct CodeParts
        {
            public string G1, G2, G3, G4;
        }

        private static bool TryParseCode(string nameWithoutExt, out CodeParts code)
        {
            code = new CodeParts();

            if (string.IsNullOrWhiteSpace(nameWithoutExt))
                return false;

            var m = CodeRegex.Match(nameWithoutExt.Trim());
            if (!m.Success)
                return false;

            code.G1 = m.Groups["g1"].Value;
            code.G2 = m.Groups["g2"].Value;
            code.G3 = m.Groups["g3"].Value;
            code.G4 = m.Groups["g4"].Value;
            return true;
        }

        private static bool IsAllZeros(string s)
        {
            if (string.IsNullOrEmpty(s))
                return false;

            for (int i = 0; i < s.Length; i++)
            {
                if (s[i] != '0')
                    return false;
            }

            return true;
        }

        private static bool StringEquals(string a, string b)
        {
            return string.Equals(a ?? "", b ?? "", StringComparison.OrdinalIgnoreCase);
        }

        private static bool ContainsIgnoreCase(string source, string value)
        {
            if (string.IsNullOrEmpty(source) || string.IsNullOrEmpty(value))
                return false;

            return source.IndexOf(value, StringComparison.OrdinalIgnoreCase) >= 0;
        }
    }
}