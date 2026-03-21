using KompasDropExport.Domain;
using KompasDropExport.Kompas;
using KompasDropExport.Services;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using KompasDropExport.Utils;

namespace KompasDropExport.UI.Tabs
{
    public partial class ExportTab2 : UserControl
    {
        // Файлы, которые НЕ попали в PDF_archive из-за невалидного обозначения
        private readonly HashSet<string> _pdfArchiveMiss = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        // вместо жёлтого — серо-голубой, очень мягкий
        private static readonly Color SoftWarnBack = Color.FromArgb(145, 170, 220);

        // тонкая “метка” слева (чуть насыщеннее), чтобы даже при выделении было видно
        private static readonly Color SoftWarnStripe = Color.FromArgb(115, 150, 200);

        public ExportTab2()
        {
            InitializeComponent();

            // Drag&Drop
            this.AllowDrop = true;
            listBoxFiles.AllowDrop = true;

            this.DragEnter += OnDragEnter;
            this.DragDrop += OnDragDrop;


            listBoxFiles.DragEnter += OnDragEnter;
            listBoxFiles.DragDrop += OnDragDrop;

            // Включаем owner draw для подсветки строк
            listBoxFiles.DrawMode = DrawMode.OwnerDrawFixed;
            // Вернуть нормальный “шаг” строк (подбери +6/+8 если хочешь больше воздуха)
            listBoxFiles.ItemHeight = Math.Max(listBoxFiles.Font.Height + 6, 18);

            listBoxFiles.DrawItem += ListBoxFiles_DrawItem;

            UiStyle.ApplySoftButton(btnExportPdf);
            UiStyle.ApplySoftButton(btnExportStep);
            UiStyle.ApplySoftButton(btnAddOpenDocs);
            UiStyle.ApplySoftButton(btnClear);


        }

        private void ListBoxFiles_DrawItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index < 0) return;

            string text = listBoxFiles.Items[e.Index]?.ToString() ?? "";
            bool warn = _pdfArchiveMiss.Contains(text);

            bool selected = (e.State & DrawItemState.Selected) == DrawItemState.Selected;
            bool focused = (e.State & DrawItemState.Focus) == DrawItemState.Focus;

            // 1) фон: если выделено — системный фон выделения
            Color backColor = selected ? SystemColors.Highlight : e.BackColor;

            // если не выделено и warn — мягкий фон
            if (!selected && warn) backColor = SoftWarnBack;

            using (var back = new SolidBrush(backColor))
                e.Graphics.FillRectangle(back, e.Bounds);

            // 2) метка слева: тонкая полоска, видна даже при выделении
            if (warn)
            {
                var stripeRect = new Rectangle(e.Bounds.X, e.Bounds.Y, 4, e.Bounds.Height);
                using (var stripe = new SolidBrush(SoftWarnStripe))
                    e.Graphics.FillRectangle(stripe, stripeRect);
            }

            // 3) текст: системный цвет для выделения, иначе обычный
            Color foreColor = selected ? SystemColors.HighlightText : e.ForeColor;

            // небольшая внутренняя “подушка”, чтобы текст не наезжал на полоску
            var textRect = new Rectangle(e.Bounds.X + 6, e.Bounds.Y, e.Bounds.Width - 6, e.Bounds.Height);

            TextRenderer.DrawText(
                e.Graphics,
                text,
                e.Font,
                textRect,
                foreColor,
                TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);

            if (focused) e.DrawFocusRectangle();
        }


        private List<string> GetFilesFromListBox()
        {
            var files = new List<string>();

            foreach (var it in listBoxFiles.Items)
            {
                var s = it?.ToString();
                if (!string.IsNullOrWhiteSpace(s))
                    files.Add(s);
            }

            return files;
        }

        private void OnDragEnter(object sender, DragEventArgs e)
        {
            if (e.Data != null && e.Data.GetDataPresent(DataFormats.FileDrop))
                e.Effect = DragDropEffects.Copy;
            else
                e.Effect = DragDropEffects.None;
        }

        private void OnDragDrop(object sender, DragEventArgs e)
        {
            if (e.Data == null || !e.Data.GetDataPresent(DataFormats.FileDrop)) return;

            var dropped = (string[])e.Data.GetData(DataFormats.FileDrop);
            if (dropped == null || dropped.Length == 0) return;

            AddPathsToList(dropped);
        }

        private int AddPathsToList(IEnumerable<string> paths)
        {
            int before = listBoxFiles.Items.Count;

            foreach (var p in paths)
            {
                if (File.Exists(p))
                {
                    AddFile(p);
                }
                else if (Directory.Exists(p))
                {
                    foreach (var f in Directory.EnumerateFiles(p, "*.*", SearchOption.AllDirectories))
                        AddFile(f);
                }
            }

            UpdateQueueLabel();
            return listBoxFiles.Items.Count - before;
        }

        private void RemoveSelectedFilesFromList()
        {
            if (listBoxFiles.SelectedIndices.Count == 0)
                return;

            var selectedPaths = listBoxFiles.SelectedItems
                .Cast<object>()
                .Select(it => it?.ToString())
                .Where(path => !string.IsNullOrWhiteSpace(path))
                .ToList();

            foreach (var path in selectedPaths)
                _pdfArchiveMiss.Remove(path);

            for (int i = listBoxFiles.SelectedIndices.Count - 1; i >= 0; i--)
                listBoxFiles.Items.RemoveAt(listBoxFiles.SelectedIndices[i]);

            listBoxFiles.Invalidate();
            UpdateQueueLabel();
            lblStatus.Text = $"Удалено из очереди: {selectedPaths.Count}.";
        }

        private void RemoveSelectedFilesFromList()
        {
            if (listBoxFiles.SelectedIndices.Count == 0)
                return;

            var selectedPaths = listBoxFiles.SelectedItems
                .Cast<object>()
                .Select(it => it?.ToString())
                .Where(path => !string.IsNullOrWhiteSpace(path))
                .ToList();

            foreach (var path in selectedPaths)
                _pdfArchiveMiss.Remove(path);

            for (int i = listBoxFiles.SelectedIndices.Count - 1; i >= 0; i--)
                listBoxFiles.Items.RemoveAt(listBoxFiles.SelectedIndices[i]);

            listBoxFiles.Invalidate();
            UpdateQueueLabel();
            lblStatus.Text = $"Удалено из очереди: {selectedPaths.Count}.";
        }

        private void AddFile(string path)
        {
            if (!IsSupportedKompasFile(path))
                return;

            for (int i = 0; i < listBoxFiles.Items.Count; i++)
            {
                if (string.Equals(listBoxFiles.Items[i]?.ToString(), path, StringComparison.OrdinalIgnoreCase))
                    return;
            }

            listBoxFiles.Items.Add(path);
        }

        private static bool IsSupportedKompasFile(string path)
        {
            return ExportRules.IsCdw(path) ||
                   ExportRules.IsSpw(path) ||
                   ExportRules.IsM3d(path) ||
                   ExportRules.IsA3d(path);
        }

        private void UpdateQueueLabel()
        {
            int cdw = 0, spw = 0, m3d = 0, a3d = 0;

            foreach (var it in listBoxFiles.Items)
            {
                var path = it?.ToString();
                if (string.IsNullOrWhiteSpace(path)) continue;

                var ext = Path.GetExtension(path)?.ToLowerInvariant();

                switch (ext)
                {
                    case ".cdw": cdw++; break;
                    case ".spw": spw++; break;
                    case ".m3d": m3d++; break;
                    case ".a3d": a3d++; break;
                }
            }

            int total = listBoxFiles.Items.Count;

            lblStatus.Text = "Готов к работе.";

            lblQueue.Text = $"Всего: {total} | CDW: {cdw} | SPW: {spw} | M3D: {m3d} | A3D: {a3d}";
        }

        private NameSeparator GetSeparatorFromUi()
        {
            if (rbDash.Checked) return NameSeparator.Dash;
            if (rbUnderscore.Checked) return NameSeparator.Underscore;
            return NameSeparator.Space;
        }

        private ExportOptions ReadOptionsFromUi()
        {
            return new ExportOptions
            {
                PdfExcludeAssemblyDrawingsByName = cbPdfExcludeAsm.Checked,
                PdfExcludeSpecs = cbPdfExcludeSpecs.Checked,
                StepExcludeAssemblies = cbStepExcludeAsm.Checked,
                StepExcludeOtherTag = cbStepExcludeOther.Checked,
                ActiveNameSeparator = GetSeparatorFromUi()
            };
        }

        private void ToggleUi(bool enable)
        {
            btnExportPdf.Enabled = enable;
            btnExportStep.Enabled = enable;
            btnAddOpenDocs.Enabled = enable;
            btnClear.Enabled = enable;

            cbPdfExcludeAsm.Enabled = enable;
            cbPdfExcludeSpecs.Enabled = enable;

            cbStepExcludeAsm.Enabled = enable;
            cbStepExcludeOther.Enabled = enable;

            rbDash.Enabled = enable;
            rbUnderscore.Enabled = enable;
            rbSpace.Enabled = enable;

            listBoxFiles.Enabled = enable;
        }

        private void InitProgress(int total)
        {
            if (total < 0) total = 0;
            progress.Minimum = 0;
            progress.Maximum = total;
            progress.Value = 0;
        }

        private void StepProgress()
        {
            int v = progress.Value + 1;
            if (v > progress.Maximum) v = progress.Maximum;
            progress.Value = v;
        }

        private void btnExportStep_Click(object sender, EventArgs e)
        {
            var files = GetFilesFromListBox();
            if (files.Count == 0)
            {
                MessageBox.Show("Очередь пустая.");
                return;
            }

            lblStatus.Text = "Ищем КОМПАС...";

            var opt = ReadOptionsFromUi();
            var svc = new StepExportService();

            ToggleUi(false);
            try
            {
                InitProgress(files.Count);

                var result = svc.Export(
                    files,
                    opt,
                    stage: s =>
                    {
                        lblStatus.Text = s;
                        Application.DoEvents();
                    },
                    onFileDone: () =>
                    {
                        StepProgress();
                        Application.DoEvents();
                    });

                MessageBox.Show($"STEP готово.\nOK: {result.Ok}\nПропуск: {result.Skip}\nОшибок: {result.Err}");
            }
            finally
            {
                ToggleUi(true);
                lblStatus.Text = "Готово.";
            }
        }

        private void button_clear_Click(object sender, EventArgs e)
        {
            listBoxFiles.Items.Clear();
            _pdfArchiveMiss.Clear();
            listBoxFiles.Invalidate();

            UpdateQueueLabel();
            lblStatus.Text = "Очередь очищена.";
            progress.Value = 0;
        }

        private void btnAddOpenDocs_Click(object sender, EventArgs e)
        {
            try
            {
                using (var host = new KompasHost())
                {
                    if (!host.TryAttachToRunning())
                    {
                        lblStatus.Text = "Открытый КОМПАС не найден.";
                        return;
                    }

                    lblStatus.Text = "Получаем открытые документы КОМПАС...";
                    var openPaths = host.GetOpenDocumentPaths();

                    if (openPaths.Count == 0)
                    {
                        lblStatus.Text = "Нет сохранённых открытых документов.";
                        return;
                    }

                    int supportedCount = openPaths.Count(IsSupportedKompasFile);
                    int added = AddPathsToList(openPaths);

                    if (supportedCount == 0)
                    {
                        lblStatus.Text = "Среди открытых документов нет файлов .cdw/.spw/.m3d/.a3d.";
                        return;
                    }

                    int skippedDuplicates = supportedCount - added;
                    lblStatus.Text = skippedDuplicates > 0
                        ? $"Добавлено открытых документов: {added}. Уже были в очереди: {skippedDuplicates}."
                        : $"Добавлено открытых документов: {added}.";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Не удалось получить список открытых документов КОМПАС:\n" + ex.Message);
            }
        }

        private void btnExportPdf_Click(object sender, EventArgs e)
        {
            var files = GetFilesFromListBox();
            if (files.Count == 0)
            {
                MessageBox.Show("Очередь пустая.");
                return;
            }

            // очищаем подсветку перед новым прогоном
            _pdfArchiveMiss.Clear();
            listBoxFiles.Invalidate();

            lblStatus.Text = "Ищем КОМПАС...";

            var opt = ReadOptionsFromUi();
            var svc = new PdfExportService();

            ToggleUi(false);
            try
            {
                InitProgress(files.Count);

                var result = svc.Export(
                    files,
                    opt,
                    stage: s =>
                    {
                        lblStatus.Text = s;
                        Application.DoEvents();
                    },
                    onFileDone: () =>
                    {
                        StepProgress();
                        Application.DoEvents();
                    },
                    onPdfArchiveSkippedBadMarking: path =>
                    {
                        // подсветить в listbox
                        _pdfArchiveMiss.Add(path);
                        listBoxFiles.Invalidate();
                    });

                MessageBox.Show(
                    "PDF готово.\n" +
                    $"OK: {result.Ok}\n" +
                    $"Пропуск: {result.Skip}\n" +
                    $"Ошибок: {result.Err}\n" +
                    $"Не попали в Архив: {result.PdfArchiveSkippedBadMarking}"
                );
            }
            finally
            {
                ToggleUi(true);
                lblStatus.Text = "Готово.";
            }
        }

        // Ниже — твои пустые хендлеры, оставляю как было (можно потом почистить)
        private void tableLayoutPanel1_Paint(object sender, PaintEventArgs e) { }
        private void listBox1_SelectedIndexChanged(object sender, EventArgs e) { }
        private void splitContainer1_Panel1_Paint(object sender, PaintEventArgs e) { }
        private void splitContainer2_Panel2_Paint(object sender, PaintEventArgs e) { }
        private void splitContainer2_Panel1_Paint(object sender, PaintEventArgs e) { }
        private void tableLayoutPanel2_Paint(object sender, PaintEventArgs e) { }
        private void checkedListBox1_SelectedIndexChanged(object sender, EventArgs e) { }
        private void checkedListBox1_SelectedIndexChanged_1(object sender, EventArgs e) { }
        private void tableLayoutPanel3_Paint(object sender, PaintEventArgs e) { }
        private void checkedListBox1_SelectedIndexChanged_2(object sender, EventArgs e) { }
        private void textBox2_TextChanged(object sender, EventArgs e) { }
        private void textBox3_TextChanged(object sender, EventArgs e) { }
        private void richTextBox1_TextChanged(object sender, EventArgs e) { }
        private void textBox4_TextChanged(object sender, EventArgs e) { }
        private void checkedListBox4_SelectedIndexChanged(object sender, EventArgs e) { }
        private void panel1_Paint(object sender, PaintEventArgs e) { }
        private void radioButton7_CheckedChanged(object sender, EventArgs e) { }
        private void progress_Click(object sender, EventArgs e) { }

        private void listBoxFiles_DoubleClick(object sender, EventArgs e)
        {
            var path = listBoxFiles.SelectedItem?.ToString();
            if (string.IsNullOrWhiteSpace(path)) return;

            // Пытаемся открыть проводник и выделить файл
            if (!ExplorerHelper.ShowAndSelectFile(path))
            {
                // Если файл исчез/путь битый — мягко сообщим
                MessageBox.Show("Файл не найден:\n" + path);
            }
        }

        private void listBoxFiles_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.Delete)
                return;

            RemoveSelectedFilesFromList();

            e.Handled = true;
            e.SuppressKeyPress = true;
        }
    }
}
