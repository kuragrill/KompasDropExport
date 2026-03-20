using KompasDropExport.Domain.Analysis;
using KompasDropExport.Kompas;
using KompasDropExport.Services.Analysis;
using KompasDropExport.UI.Analysis;
using KompasDropExport.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace KompasDropExport.UI.Tabs
{
    public partial class GraphTab : UserControl
    {
        private string _rootAssemblyPath;

        private readonly BindingList<NodeRow> _nodeRows = new BindingList<NodeRow>();
        private readonly BindingList<EdgeRow> _edgeRows = new BindingList<EdgeRow>();

        private BindingList<KompasDropExport.UI.Analysis.RenameTableRow> _renameRows =
    new BindingList<KompasDropExport.UI.Analysis.RenameTableRow>();

        private readonly GraphUiPresenter _presenter = new GraphUiPresenter();

        private AnalysisResult _lastAnalysis;

        public GraphTab()
        {
            InitializeComponent();

            UiStyle.ApplySoftButton(btToTrash);
            UiStyle.ApplySoftButton(btAnalyze);
            UiStyle.ApplySoftButton(btnClear);
            UiStyle.ApplySoftButton(btRename);


            InitRenameGrid();
            ConfigureRenameGrid();

            InitGraphTab();
        }

        private void InitGraphTab()
        {
            // Drag & Drop
            lbDrag.AllowDrop = true;
            lbDrag.DragEnter += LbDrag_DragEnter;
            lbDrag.DragDrop += LbDrag_DragDrop;

            // Таблицы
            dgNodes.AutoGenerateColumns = false;
            dgEdges.AutoGenerateColumns = false;

            dgNodes.DataSource = _nodeRows;
            dgEdges.DataSource = _edgeRows;

            // Прогрессбар
            if (niceProgressBar1 != null)
            {
                niceProgressBar1.Minimum = 0;
                niceProgressBar1.Maximum = 100;
                niceProgressBar1.Value = 0;
            }

            

        }

        private void InitRenameGrid()
        {
            dgRename.DataSource = _renameRows;

            dgRename.CellFormatting += dgRename_CellFormatting;
            dgRename.RowPrePaint += dgRename_RowPrePaint;
            dgRename.CellValueChanged += dgRename_CellValueChanged;
            dgRename.CellEndEdit += dgRename_CellEndEdit;
            dgRename.CellParsing += dgRename_CellParsing;

            dgRename.EditMode = DataGridViewEditMode.EditOnKeystrokeOrF2;
            dgRename.SelectionMode = DataGridViewSelectionMode.CellSelect;
            dgRename.MultiSelect = false;
        }

        private void dgRename_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            try
            {
                var grid = sender as DataGridView;
                if (grid == null) return;
                if (e.RowIndex < 0 || e.ColumnIndex < 0) return;

                var col = grid.Columns[e.ColumnIndex];
                if (col == null) return;

                // Только для колонки имени файла
                if (!string.Equals(col.DataPropertyName, "BaseName", StringComparison.OrdinalIgnoreCase))
                    return;

                var rowObj = grid.Rows[e.RowIndex].DataBoundItem as KompasDropExport.UI.Analysis.RenameTableRow;
                if (rowObj == null) return;

                // Если это текущая редактируемая ячейка — не форматируем,
                // иначе пробелы могут попадать в текст редактора
                if (grid.CurrentCell != null &&
                    grid.IsCurrentCellInEditMode &&
                    grid.CurrentCell.RowIndex == e.RowIndex &&
                    grid.CurrentCell.ColumnIndex == e.ColumnIndex)
                {
                    return;
                }

                string text = Convert.ToString(e.Value) ?? "";

                // Страховка: убираем уже прилипшие слева пробелы
                text = text.TrimStart();

                e.Value = new string(' ', rowObj.Level * 4) + text;
                e.FormattingApplied = true;
            }
            catch
            {
                // UI не валим
            }
        }

        private void dgRename_CellParsing(object sender, DataGridViewCellParsingEventArgs e)
        {
            try
            {
                var grid = sender as DataGridView;
                if (grid == null) return;
                if (e.RowIndex < 0 || e.ColumnIndex < 0) return;

                var col = grid.Columns[e.ColumnIndex];
                if (col == null) return;

                if (!string.Equals(col.DataPropertyName, "BaseName", StringComparison.OrdinalIgnoreCase))
                    return;

                string text = Convert.ToString(e.Value) ?? "";
                e.Value = text.TrimStart();
                e.ParsingApplied = true;
            }
            catch
            {
            }
        }

        private void dgRename_RowPrePaint(object sender, DataGridViewRowPrePaintEventArgs e)
        {
            try
            {
                var grid = sender as DataGridView;
                if (grid == null) return;
                if (e.RowIndex < 0) return;

                var rowObj = grid.Rows[e.RowIndex].DataBoundItem as KompasDropExport.UI.Analysis.RenameTableRow;
                if (rowObj == null) return;

                if (rowObj.IsModified)
                {
                    grid.Rows[e.RowIndex].DefaultCellStyle.BackColor = System.Drawing.Color.LightYellow;
                }
                else
                {
                    grid.Rows[e.RowIndex].DefaultCellStyle.BackColor = System.Drawing.Color.White;
                }
            }
            catch
            {
            }
        }

        private void dgRename_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                if (e.RowIndex < 0) return;
                dgRename.InvalidateRow(e.RowIndex);
            }
            catch
            {
            }
        }

        private void dgRename_CurrentCellDirtyStateChanged(object sender, EventArgs e)
        {
            try
            {
                if (dgRename.IsCurrentCellDirty)
                    dgRename.CommitEdit(DataGridViewDataErrorContexts.Commit);
            }
            catch
            {
            }
        }

        private void dgRename_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                if (e.RowIndex < 0) return;
                dgRename.InvalidateRow(e.RowIndex);
            }
            catch
            {
            }
        }

        private void ConfigureRenameGrid()
        {
            dgRename.AutoGenerateColumns = false;

            dgRename.Columns["Kind"].ReadOnly = true;
            dgRename.Columns["BaseName"].ReadOnly = false;
            dgRename.Columns["Designation"].ReadOnly = true;
            dgRename.Columns["Title"].ReadOnly = true;
            

            dgRename.Columns["FullPath"].Visible = false;
            dgRename.Columns["Level"].Visible = false;
            dgRename.Columns["NodeId"].Visible = false;
            dgRename.Columns["OriginalBaseName"].Visible = false;
            dgRename.Columns["Extension"].Visible = false;
            dgRename.Columns["IsModified"].Visible = false;
        }

        private void LbDrag_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data != null && e.Data.GetDataPresent(DataFormats.FileDrop))
                e.Effect = DragDropEffects.Copy;
            else
                e.Effect = DragDropEffects.None;
        }

        private void LbDrag_DragDrop(object sender, DragEventArgs e)
        {
            var files = (string[])e.Data.GetData(DataFormats.FileDrop);

            if (files == null || files.Length != 1)
            {
                MessageBox.Show("Перетащи один файл сборки (.a3d)");
                return;
            }

            var path = files[0];

            if (!File.Exists(path))
            {
                MessageBox.Show("Файл не существует.");
                return;
            }

            if (Path.GetExtension(path).ToLowerInvariant() != ".a3d")
            {
                MessageBox.Show("Нужен файл сборки .a3d");
                return;
            }

            _rootAssemblyPath = path;
            lbDrag.Text = path;
            lbDrag.BackColor = System.Drawing.Color.DeepSkyBlue;
        }

        private void SetProgress(int value)
        {
            if (niceProgressBar1 == null) return;

            if (value < niceProgressBar1.Minimum) value = niceProgressBar1.Minimum;
            if (value > niceProgressBar1.Maximum) value = niceProgressBar1.Maximum;

            niceProgressBar1.Value = value;
        }

        private void SetStatus(string text)
        {
            if (lblStatus != null) // замени lblStatus на имя своей строки статуса
                lblStatus.Text = text ?? "";
        }

        private async void btAnalyze_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(_rootAssemblyPath))
            {
                MessageBox.Show("Сначала перетащи сборку.");
                return;
            }

            btAnalyze.Enabled = false;
            SetProgress(0);
            SetStatus("Подготовка анализа...");

            try
            {
                var progress = new Progress<AnalysisProgressInfo>(info =>
                {
                    if (info == null) return;
                    SetProgress(info.Percent);
                    SetStatus(info.Status);
                });

                var analysisResult = await Task.Run(() => RunAnalysisCore(progress));

                _lastAnalysis = analysisResult;
                _presenter.Apply(analysisResult, _nodeRows, _edgeRows, lbOrphans);

                var renameRows = KompasDropExport.Services.Analysis.RenameTreeBuilder.Build(_lastAnalysis);

                _renameRows.Clear();
                for (int i = 0; i < renameRows.Count; i++)
                    _renameRows.Add(renameRows[i]);

                

                SetProgress(100);
                SetStatus("Анализ завершён.");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка анализа:\n" + ex.Message);
                SetProgress(0);
                SetStatus("Ошибка анализа.");
            }
            finally
            {
                btAnalyze.Enabled = true;
            }
        }

        /// <summary>
        /// Тяжёлая часть (фон): строим граф, считаем анализ по узлам, health issues и naming.
        /// </summary>
        private AnalysisResult RunAnalysisCore(IProgress<AnalysisProgressInfo> progress)
        {
            Report(progress, 2, "Подготовка анализа...");

            string projectRoot = Path.GetDirectoryName(_rootAssemblyPath);


            var builder = new ProjectGraphBuilder(projectRoot);

            Report(progress, 5, "Построение графа проекта...");

            var tuple = builder.BuildGraph(_rootAssemblyPath, progress);
            var nodes = tuple.Item1;
            var edges = tuple.Item2;
            var nodeDocuments = tuple.Item3;

            Report(progress, 88, "Определение корневого узла...");

            // rootId по пути
            int rootId = 0;
            string rootFull = Path.GetFullPath(_rootAssemblyPath);

            for (int i = 0; i < nodes.Count; i++)
            {
                if (string.Equals(nodes[i].FullPath, rootFull, StringComparison.OrdinalIgnoreCase))
                {
                    rootId = nodes[i].Id;
                    break;
                }
            }

            Report(progress, 91, "Расчёт анализа по узлам...");

            var nodeAnalysis = KompasDropExport.Services.Analysis.GraphNodeAnalysisCalculator
                .Compute(nodes, edges, rootId);

            Report(progress, 94, "Проверка целостности графа...");

            var healthIssues = KompasDropExport.Services.Analysis.GraphHealthChecker
                .Check(nodes, edges, rootId);

            Report(progress, 96, "Проверка полноты документации...");

            var completenessIssues = KompasDropExport.Services.Analysis.GraphCompletenessChecker
                .Check(nodes, nodeAnalysis);

            Report(progress, 98, "Проверка имён...");

            var namingIssues = KompasDropExport.Services.Analysis.FileNameNamingChecker
                .Check(_rootAssemblyPath, nodes, edges, nodeAnalysis);

            Report(progress, 99, "Подготовка результата...");

            return new AnalysisResult(
                nodes,
                edges,
                nodeAnalysis,
                nodeDocuments,
                healthIssues,
                completenessIssues,
                new List<object>(namingIssues)
            );
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
        private void lbOrphans_DoubleClick(object sender, EventArgs e)
        {
            try
            {
                var item = lbOrphans.SelectedItem as KompasDropExport.UI.Analysis.AnalysisListItem;
                if (item == null)
                    return;

                if (!item.CanOpen || string.IsNullOrWhiteSpace(item.PrimaryPath))
                    return;

                OpenInExplorerSmart(item.PrimaryPath);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при открытии:\n" + ex.Message);
            }
        }

        private void OpenInExplorerSmart(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                return;

            // 1) Если это существующий файл — открываем через Shell API
            if (File.Exists(path))
            {
                if (!ExplorerHelper.ShowAndSelectFile(path))
                {
                    // fallback на обычный explorer
                    try
                    {
                        Process.Start("explorer.exe", "/select,\"" + path + "\"");
                    }
                    catch
                    {
                        MessageBox.Show("Не удалось открыть файл в проводнике:\n" + path);
                    }
                }
                return;
            }

            // 2) Если это существующая папка — открыть папку
            if (Directory.Exists(path))
            {
                try
                {
                    Process.Start("explorer.exe", "\"" + path + "\"");
                }
                catch
                {
                    MessageBox.Show("Не удалось открыть папку:\n" + path);
                }
                return;
            }

            // 3) Если файла нет, но есть родительская папка — открыть папку
            string dir = null;
            try { dir = Path.GetDirectoryName(path); } catch { dir = null; }

            if (!string.IsNullOrWhiteSpace(dir) && Directory.Exists(dir))
            {
                try
                {
                    Process.Start("explorer.exe", "\"" + dir + "\"");
                }
                catch
                {
                    MessageBox.Show("Файл не найден, и не удалось открыть папку:\n" + dir);
                }
                return;
            }

            MessageBox.Show("Файл или папка не найдены:\n" + path);
        }



        private async void btToTrash_Click(object sender, EventArgs e)
        {
            if (_lastAnalysis == null)
            {
                MessageBox.Show("Сначала сделай анализ.");
                return;
            }

            // Собираем сироты по новому анализу:
            // внутренние 3D, которые не входят в изделие
            var orphans = new List<GraphNode>();

            if (_lastAnalysis.Nodes != null && _lastAnalysis.NodeAnalysis != null)
            {
                for (int i = 0; i < _lastAnalysis.Nodes.Count; i++)
                {
                    var node = _lastAnalysis.Nodes[i];
                    if (node == null) continue;

                    NodeAnalysisInfo info;
                    if (!_lastAnalysis.NodeAnalysis.TryGetValue(node.Id, out info))
                        continue;

                    if (info != null && info.IsOrphan3D)
                        orphans.Add(node);
                }
            }

            if (orphans.Count == 0)
            {
                MessageBox.Show("Сирот нет — переносить нечего.");
                return;
            }

            string projectRoot = Path.GetDirectoryName(_rootAssemblyPath);
            if (string.IsNullOrEmpty(projectRoot))
            {
                MessageBox.Show("Не удалось определить корень проекта.");
                return;
            }

            btAnalyze.Enabled = false;
            btToTrash.Enabled = false;

            try
            {
                SetProgress(0);

                await Task.Run(() =>
                {
                    TrashMover.EnsureTrashFolder(projectRoot);

                    for (int i = 0; i < orphans.Count; i++)
                    {
                        var n = orphans[i];

                        // страховка: переносим только существующие внутренние файлы
                        if (n == null) continue;
                        if (n.Location != NodeLocation.Internal) continue;
                        if (!File.Exists(n.FullPath)) continue;

                        TrashMover.MoveToTrash(projectRoot, n.FullPath);

                        // прогресс (80..99)
                        int p = 80 + (int)((double)(i + 1) / orphans.Count * 19);

                        this.BeginInvoke((Action)(() => SetProgress(p)));
                    }
                });

                // После переноса — повторяем анализ
            
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка переноса в Trash:\n" + ex.Message);
                SetProgress(0);
            }
            finally
            {
                btAnalyze.Enabled = true;
                btToTrash.Enabled = true;
            }
        }
        private void dgNodes_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            try
            {
                ClearAnalysisUi();
                lbDrag.Text = "Перетащите файл сборки (.a3d) .....";
                lbDrag.BackColor = System.Drawing.Color.SteelBlue;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка сброса:\n" + ex.Message);
            }
        }

        private void ClearAnalysisUi()
        {
            _lastAnalysis = null;
            _rootAssemblyPath = null;

            if (_nodeRows != null)
                _nodeRows.Clear();

            if (_edgeRows != null)
                _edgeRows.Clear();

            if (_renameRows != null)
                _renameRows.Clear();

            if (lbOrphans != null)
                lbOrphans.Items.Clear();

            if (dgNodes != null)
            {
                dgNodes.ClearSelection();
                dgNodes.CurrentCell = null;
            }

            if (dgEdges != null)
            {
                dgEdges.ClearSelection();
                dgEdges.CurrentCell = null;
            }

            if (dgRename != null)
            {
                dgRename.ClearSelection();
                dgRename.CurrentCell = null;
                dgRename.Refresh();
            }

            SetProgress(0);
            SetStatus("");
        }

        private void dgRename_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void btRename_Click(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(_rootAssemblyPath))
                {
                    MessageBox.Show(
                        "Сначала выбери корневую сборку.",
                        "Переименование",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                    return;
                }

                string projectRoot = Path.GetDirectoryName(_rootAssemblyPath);
                if (string.IsNullOrWhiteSpace(projectRoot))
                {
                    MessageBox.Show(
                        "Не удалось определить корень проекта.",
                        "Переименование",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                    return;
                }

                var plan = RenamePlanBuilder.Build(_renameRows);

                if (plan.HasErrors)
                {
                    MessageBox.Show(
                        "Ошибки в плане переименования:\n\n" +
                        string.Join("\n\n", plan.Errors),
                        "Переименование",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                    return;
                }

                if (plan.Operations.Count == 0)
                {
                    MessageBox.Show(
                        "Нет изменённых файлов.",
                        "Переименование",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                    return;
                }

                var lines = new List<string>();
                for (int i = 0; i < plan.Operations.Count; i++)
                {
                    var op = plan.Operations[i];
                    lines.Add(op.OldFileName + "  →  " + op.NewFileName);
                }

                var confirm = MessageBox.Show(
                    "Будут переименованы файлы:\n\n" +
                    string.Join("\n", lines) +
                    "\n\nПродолжить?",
                    "Подтверждение переименования",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (confirm != DialogResult.Yes)
                    return;

                btRename.Enabled = false;
                btAnalyze.Enabled = false;
                btToTrash.Enabled = false;

                // 1) Физический rename файлов
                var exec = RenameExecutor.Execute(plan.Operations);

                if (exec.Errors.Count > 0)
                {
                    MessageBox.Show(
                        "Часть операций завершилась ошибкой:\n\n" +
                        string.Join("\n\n", exec.Errors),
                        "Переименование",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                    return;
                }

                // 2) Обновление ссылок в документах
                var updater = new RenameLinkUpdater();
                var candidateFiles = BuildCandidateFilesForLinkUpdate(plan.Operations);

                var linkResult = updater.UpdateLinks(candidateFiles, plan.Operations);

                if (linkResult.Errors.Count > 0)
                {
                    MessageBox.Show(
                        "Ошибки обновления ссылок:\n\n" +
                        string.Join("\n\n", linkResult.Errors),
                        "Обновление ссылок",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                }

                var infoLines = new List<string>
                    {
                        "Переименовано файлов: " + exec.SuccessCount,
                        "Проверено документов: " + linkResult.DocumentsScanned,
                        "Обновлено документов: " + linkResult.DocumentsChanged,
                        "",
                        "CDW: " + linkResult.CdwChanged,
                        "SPW: " + linkResult.SpwChanged,
                        "3D: " + linkResult.Model3DChanged
                };

                if (linkResult.ChangedFiles.Count > 0)
                {
                    infoLines.Add("");
                    infoLines.Add("Изменены:");
                    for (int i = 0; i < linkResult.ChangedFiles.Count; i++)
                        infoLines.Add("- " + linkResult.ChangedFiles[i]);
                }

                MessageBox.Show(
                    string.Join("\n", infoLines),
                    "Переименование",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                // 3) Повторный анализ
                btAnalyze.PerformClick();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка переименования:\n" + ex.Message);
            }
            finally
            {
                btRename.Enabled = true;
                btAnalyze.Enabled = true;
                btToTrash.Enabled = true;
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            var probe = new KompasDropExport.Services.Analysis.SpwApi7SpecificationProbe();
            probe.Run(@"c:\Users\Vadim\YandexDisk\Alexander_SPb\PROJECTS\9_Shtorka\01 РКД\АШСД.09.20.00 Крышка.spw");
        }

        private List<string> BuildCandidateFilesForLinkUpdate(IList<RenameOperation> operations)
        {
            var result = new List<string>();
            if (_lastAnalysis == null || _lastAnalysis.Nodes == null)
                return result;

            var renameMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            if (operations != null)
            {
                for (int i = 0; i < operations.Count; i++)
                {
                    var op = operations[i];
                    if (op == null) continue;
                    if (string.IsNullOrWhiteSpace(op.OldFullPath)) continue;
                    if (string.IsNullOrWhiteSpace(op.NewFullPath)) continue;

                    try
                    {
                        string oldPath = Path.GetFullPath(op.OldFullPath);
                        string newPath = Path.GetFullPath(op.NewFullPath);

                        if (!renameMap.ContainsKey(oldPath))
                            renameMap.Add(oldPath, newPath);
                    }
                    catch
                    {
                    }
                }
            }

            var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            for (int i = 0; i < _lastAnalysis.Nodes.Count; i++)
            {
                var n = _lastAnalysis.Nodes[i];
                if (n == null) continue;
                if (n.Location != NodeLocation.Internal) continue;
                if (string.IsNullOrWhiteSpace(n.FullPath)) continue;

                string ext;
                try { ext = (Path.GetExtension(n.FullPath) ?? "").ToLowerInvariant(); }
                catch { continue; }

                if (ext != ".a3d" && ext != ".m3d" && ext != ".cdw" && ext != ".spw")
                    continue;

                string path = n.FullPath;

                try
                {
                    path = Path.GetFullPath(path);
                }
                catch
                {
                    continue;
                }

                string remapped;
                if (renameMap.TryGetValue(path, out remapped))
                    path = remapped;

                if (seen.Add(path))
                    result.Add(path);
            }

            return result;
        }
    }
}