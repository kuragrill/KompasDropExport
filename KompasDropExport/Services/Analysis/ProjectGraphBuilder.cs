using KompasDropExport.Domain.Analysis;
using KompasDropExport.Kompas;
using System;
using System.Collections.Generic;
using System.IO;

namespace KompasDropExport.Services.Analysis
{
    public sealed class ProjectGraphBuilder
    {
        private readonly string _projectRoot;

        private readonly Dictionary<string, GraphNode> _nodesByPath =
            new Dictionary<string, GraphNode>(StringComparer.OrdinalIgnoreCase);

        private readonly List<GraphNode> _nodes = new List<GraphNode>();
        private readonly List<GraphEdge> _edges = new List<GraphEdge>();

        private readonly Dictionary<int, NodeDocumentInfo> _nodeDocumentsById =
            new Dictionary<int, NodeDocumentInfo>();

        private int _nextId = 1;

        public ProjectGraphBuilder(string projectRoot)
        {
            _projectRoot = projectRoot;
        }

        public Tuple<List<GraphNode>, List<GraphEdge>, Dictionary<int, NodeDocumentInfo>> BuildGraph(
            string rootAssemblyPath,
            IProgress<AnalysisProgressInfo> progress)
        {
            Reset();

            Report(progress, 6, "Сканирование папки проекта...");

            // 0) Узлы из папки проекта
            BuildFromFolder();

            Report(progress, 10, "Папка проекта просканирована.");

            // Для агрегации рёбер
            var edgeIndex = new Dictionary<EdgeKey, GraphEdge>();

            int totalAssocDocs = 0;
            for (int i = 0; i < _nodes.Count; i++)
            {
                var n = _nodes[i];
                if (n == null) continue;
                if (n.Location != NodeLocation.Internal) continue;
                if (!n.Exists) continue;

                bool isAssocDoc =
                    n.NodeType == NodeType.Drawing ||
                    n.NodeType == NodeType.Specification ||
                    n.NodeType == NodeType.ModelAssembly ||
                    n.NodeType == NodeType.ModelPart;

                if (isAssocDoc)
                    totalAssocDocs++;
            }

            int processedAssocDocs = 0;

            // 1) Composition
            Report(progress, 12, "Чтение состава изделия...");

            var compScanner = new KompasCompositionScanner();
            var compLinks = compScanner.ScanAssembly(rootAssemblyPath);

            for (int i = 0; i < compLinks.Count; i++)
            {
                var link = compLinks[i];
                if (link == null) continue;

                string parentPath = SafeFullPath(link.ParentPath);
                string childPath = SafeFullPath(link.ChildPath);
                if (string.IsNullOrEmpty(parentPath) || string.IsNullOrEmpty(childPath))
                    continue;

                var parentNode = GetOrCreateNode(parentPath);
                var childNode = GetOrCreateNode(childPath);

                AddOrIncEdge(edgeIndex, EdgeType.Composition, parentNode.Id, childNode.Id, File.Exists(childPath));
            }

            Report(progress, 20, "Состав изделия прочитан.");

            // 2) Ассоциации + чтение обозначения/наименования
            var assocScanner = new KompasAssociationScanner();

            using (var host = new KompasHost())
            {
                host.AttachOrStart();

                using (host.SilentScope())
                {
                    // 2.1) 2D-pass: CDW/SPW -> 3D
                    for (int i = 0; i < _nodes.Count; i++)
                    {
                        var n = _nodes[i];

                        if (n.Location != NodeLocation.Internal) continue;
                        if (!n.Exists) continue;

                        if (n.NodeType != NodeType.Drawing &&
                            n.NodeType != NodeType.Specification)
                            continue;

                        var scan2 = assocScanner.ScanFrom2D(host, n.FullPath);
                        SaveNodeDocumentInfo(n, scan2);
                        AddAssociationLinks(edgeIndex, scan2.Links);

                        processedAssocDocs++;
                        if (totalAssocDocs > 0)
                        {
                            int percent = 20 + (int)((double)processedAssocDocs / totalAssocDocs * 60.0);
                            if (percent > 80) percent = 80;
                            Report(progress, percent, "Чтение связей: " + n.FileName);
                        }
                    }

                    // 2.2) 3D-pass: A3D/M3D -> 2D/SPW
                    for (int i = 0; i < _nodes.Count; i++)
                    {
                        var n = _nodes[i];

                        if (n.Location != NodeLocation.Internal) continue;
                        if (!n.Exists) continue;

                        if (n.NodeType != NodeType.ModelAssembly &&
                            n.NodeType != NodeType.ModelPart)
                            continue;

                        var scan3 = assocScanner.ScanFrom3D(host, n.FullPath);
                        SaveNodeDocumentInfo(n, scan3);
                        AddAssociationLinks(edgeIndex, scan3.Links);

                        processedAssocDocs++;
                        if (totalAssocDocs > 0)
                        {
                            int percent = 20 + (int)((double)processedAssocDocs / totalAssocDocs * 60.0);
                            if (percent > 80) percent = 80;
                            Report(progress, percent, "Чтение связей: " + n.FileName);
                        }
                    }
                }
            }

            Report(progress, 85, "Построение графа завершено.");

            return Tuple.Create(
                _nodes,
                _edges,
                new Dictionary<int, NodeDocumentInfo>(_nodeDocumentsById)
            );
        }

        private void SaveNodeDocumentInfo(GraphNode node, DocumentScanResult scan)
        {
            if (node == null || scan == null)
                return;

            _nodeDocumentsById[node.Id] = new NodeDocumentInfo
            {
                NodeId = node.Id,
                Designation = scan.Designation,
                Title = scan.Title,
                ReadSucceeded = scan.MetadataReadSucceeded
            };
        }

        private void AddAssociationLinks(Dictionary<EdgeKey, GraphEdge> edgeIndex, List<AssociationLink> links)
        {
            if (links == null) return;

            for (int i = 0; i < links.Count; i++)
            {
                var l = links[i];
                if (l == null) continue;

                string fromPath = SafeFullPath(l.FromPath);
                string toPath = SafeFullPath(l.ToPath);
                if (string.IsNullOrEmpty(fromPath) || string.IsNullOrEmpty(toPath))
                    continue;

                var fromNode = GetOrCreateNode(fromPath);
                var toNode = GetOrCreateNode(toPath);

                bool resolved = File.Exists(toNode.FullPath);

                AddOrIncEdge(edgeIndex, l.EdgeType, fromNode.Id, toNode.Id, resolved);
            }
        }

        private void AddOrIncEdge(Dictionary<EdgeKey, GraphEdge> edgeIndex, EdgeType type, int fromId, int toId, bool isResolved)
        {
            var key = new EdgeKey(type, fromId, toId);

            GraphEdge e;
            if (edgeIndex.TryGetValue(key, out e))
            {
                e.Count += 1;
                if (isResolved) e.IsResolved = true;
                return;
            }

            e = new GraphEdge(type, fromId, toId);
            e.Count = 1;
            e.IsResolved = isResolved;

            edgeIndex[key] = e;
            _edges.Add(e);
        }

        private void BuildFromFolder()
        {
            foreach (var file in EnumerateProjectFilesSkippingTrash(_projectRoot))
            {
                string ext = Path.GetExtension(file);
                if (!IsKompasFileExt(ext))
                    continue;

                GetOrCreateNode(file);
            }
        }

        private IEnumerable<string> EnumerateProjectFilesSkippingTrash(string root)
        {
            var stack = new Stack<string>();
            stack.Push(root);

            while (stack.Count > 0)
            {
                string dir = stack.Pop();

                if (IsTrashFolder(dir))
                    continue;

                IEnumerable<string> files = null;
                try { files = Directory.EnumerateFiles(dir); } catch { files = null; }

                if (files != null)
                {
                    foreach (var f in files)
                        yield return f;
                }

                IEnumerable<string> subDirs = null;
                try { subDirs = Directory.EnumerateDirectories(dir); } catch { subDirs = null; }

                if (subDirs != null)
                {
                    foreach (var sd in subDirs)
                        stack.Push(sd);
                }
            }
        }

        private bool IsTrashFolder(string dir)
        {
            try
            {
                var name = new DirectoryInfo(dir).Name;
                return string.Equals(name, "Trash", StringComparison.OrdinalIgnoreCase);
            }
            catch { return false; }
        }

        private static bool IsKompasFileExt(string ext)
        {
            if (string.IsNullOrEmpty(ext)) return false;
            ext = ext.ToLowerInvariant();
            return ext == ".a3d" || ext == ".m3d" || ext == ".cdw" || ext == ".spw";
        }

        private GraphNode GetOrCreateNode(string path)
        {
            string fullPath = SafeFullPath(path);

            GraphNode existing;
            if (_nodesByPath.TryGetValue(fullPath, out existing))
                return existing;

            var node = new GraphNode(_nextId++, fullPath);

            node.Exists = File.Exists(fullPath);
            node.FileName = Path.GetFileName(fullPath);

            if (IsUnderRoot(fullPath, _projectRoot))
            {
                node.Location = NodeLocation.Internal;
                node.RelPath = GetRelativePathSafe(_projectRoot, fullPath);
            }
            else
            {
                node.Location = node.Exists ? NodeLocation.External : NodeLocation.Missing;
                node.RelPath = "";
            }

            node.NodeType = GuessTypeByExtension(fullPath);

            _nodesByPath[fullPath] = node;
            _nodes.Add(node);

            return node;
        }

        private static NodeType GuessTypeByExtension(string fullPath)
        {
            string ext = Path.GetExtension(fullPath);
            if (string.IsNullOrEmpty(ext)) return NodeType.Unknown;
            ext = ext.ToLowerInvariant();

            if (ext == ".a3d") return NodeType.ModelAssembly;
            if (ext == ".m3d") return NodeType.ModelPart;
            if (ext == ".cdw") return NodeType.Drawing;
            if (ext == ".spw") return NodeType.Specification;

            return NodeType.Unknown;
        }

        private static bool IsUnderRoot(string fullPath, string root)
        {
            string normalizedFullPath =
                Path.GetFullPath(fullPath)
                    .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
                + Path.DirectorySeparatorChar;

            string normalizedRoot =
                Path.GetFullPath(root)
                    .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
                + Path.DirectorySeparatorChar;

            return normalizedFullPath.StartsWith(normalizedRoot, StringComparison.OrdinalIgnoreCase);
        }

        private static string GetRelativePathSafe(string root, string fullPath)
        {
            Uri rootUri = new Uri(AppendDirectorySeparator(root));
            Uri fileUri = new Uri(fullPath);

            Uri relativeUri = rootUri.MakeRelativeUri(fileUri);
            string relativePath = Uri.UnescapeDataString(relativeUri.ToString());

            return relativePath.Replace('/', Path.DirectorySeparatorChar);
        }

        private static string AppendDirectorySeparator(string path)
        {
            if (!path.EndsWith(Path.DirectorySeparatorChar.ToString()))
                return path + Path.DirectorySeparatorChar;

            return path;
        }

        private static string SafeFullPath(string path)
        {
            if (string.IsNullOrEmpty(path)) return null;
            try { return Path.GetFullPath(path); }
            catch { return null; }
        }

        private void Reset()
        {
            _nodesByPath.Clear();
            _nodes.Clear();
            _edges.Clear();
            _nodeDocumentsById.Clear();
            _nextId = 1;
        }

        private static void Report(IProgress<AnalysisProgressInfo> progress, int percent, string status)
        {
            if (progress == null) return;

            progress.Report(new AnalysisProgressInfo
            {
                Percent = percent,
                Status = status
            });
        }

        private struct EdgeKey
        {
            private readonly EdgeType _type;
            private readonly int _from;
            private readonly int _to;

            public EdgeKey(EdgeType type, int from, int to)
            {
                _type = type;
                _from = from;
                _to = to;
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    int h = 17;
                    h = h * 31 + (int)_type;
                    h = h * 31 + _from;
                    h = h * 31 + _to;
                    return h;
                }
            }

            public override bool Equals(object obj)
            {
                if (!(obj is EdgeKey)) return false;
                var o = (EdgeKey)obj;
                return o._type == _type && o._from == _from && o._to == _to;
            }
        }
    }
}