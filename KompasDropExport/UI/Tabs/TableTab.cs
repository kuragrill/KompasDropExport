using KompasDropExport.Domain;
using KompasDropExport.Kompas;
using KompasDropExport.Services;
using KompasDropExport.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;

namespace KompasDropExport.UI.Tabs
{
    public partial class TableTab : UserControl
    {
        private readonly BindingList<FileRecord> _rows = new BindingList<FileRecord>();

        private bool _inited;
        private string _editOldValue = "";

        public TableTab()
        {
            InitializeComponent();

            UiStyle.ApplySoftButton(btnWriteProps);
            UiStyle.ApplySoftButton(btnReadProps);
            UiStyle.ApplySoftButton(btnExport);
            UiStyle.ApplySoftButton(btnImport);
            UiStyle.ApplySoftButton(btnClear);

            InitOnce();
        }

        private void InitOnce()
        {
            if (_inited) return;
            _inited = true;

            InitGridBinding();
            ApplyGridStyleFix();
            InitDnD();
            InitGridEvents();

            dataGridFiles.DoubleBuffered(true);
        }

        private void InitGridBinding()
        {
            dataGridFiles.AutoGenerateColumns = false;

            colFileName.DataPropertyName = "FileName";
            colDesignation.DataPropertyName = "Marking";
            colTitle.DataPropertyName = "Name";
            colPath.DataPropertyName = "FullPath";

            colFileName.ReadOnly = true;
            colPath.ReadOnly = true;
            colPath.Visible = false;

            colDesignation.ReadOnly = false;
            colTitle.ReadOnly = false;

            dataGridFiles.DataSource = _rows;
        }

        private void ApplyGridStyleFix()
        {
            dataGridFiles.EnableHeadersVisualStyles = false;

            dataGridFiles.DefaultCellStyle.BackColor = System.Drawing.SystemColors.Window;
            dataGridFiles.DefaultCellStyle.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridFiles.DefaultCellStyle.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridFiles.DefaultCellStyle.SelectionForeColor = System.Drawing.SystemColors.HighlightText;

            dataGridFiles.RowsDefaultCellStyle.BackColor = System.Drawing.SystemColors.Window;
            dataGridFiles.RowsDefaultCellStyle.ForeColor = System.Drawing.SystemColors.WindowText;

            dataGridFiles.AlternatingRowsDefaultCellStyle.BackColor = System.Drawing.SystemColors.Window;
            dataGridFiles.AlternatingRowsDefaultCellStyle.ForeColor = System.Drawing.SystemColors.WindowText;

            colDesignation.DefaultCellStyle.BackColor = System.Drawing.SystemColors.Window;
            colDesignation.DefaultCellStyle.ForeColor = System.Drawing.SystemColors.WindowText;
            colDesignation.DefaultCellStyle.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            colDesignation.DefaultCellStyle.SelectionForeColor = System.Drawing.SystemColors.HighlightText;

            colTitle.DefaultCellStyle.BackColor = System.Drawing.SystemColors.Window;
            colTitle.DefaultCellStyle.ForeColor = System.Drawing.SystemColors.WindowText;
            colTitle.DefaultCellStyle.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            colTitle.DefaultCellStyle.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
        }

        private void InitDnD()
        {
            dataGridFiles.AllowDrop = true;

            dataGridFiles.DragEnter -= DataGridFiles_DragEnter;
            dataGridFiles.DragDrop -= DataGridFiles_DragDrop;

            dataGridFiles.DragEnter += DataGridFiles_DragEnter;
            dataGridFiles.DragDrop += DataGridFiles_DragDrop;
        }

        private void InitGridEvents()
        {
            dataGridFiles.CellBeginEdit -= DataGridFiles_CellBeginEdit;
            dataGridFiles.CellEndEdit -= DataGridFiles_CellEndEdit;
            dataGridFiles.CellFormatting -= DataGridFiles_CellFormatting;

            dataGridFiles.CellBeginEdit += DataGridFiles_CellBeginEdit;
            dataGridFiles.CellEndEdit += DataGridFiles_CellEndEdit;
            dataGridFiles.CellFormatting += DataGridFiles_CellFormatting;
        }

        // -------------------- Drag&Drop --------------------

        private void DataGridFiles_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data != null && e.Data.GetDataPresent(DataFormats.FileDrop))
                e.Effect = DragDropEffects.Copy;
            else
                e.Effect = DragDropEffects.None;
        }

        private void DataGridFiles_DragDrop(object sender, DragEventArgs e)
        {
            if (e.Data == null || !e.Data.GetDataPresent(DataFormats.FileDrop)) return;

            var paths = (string[])e.Data.GetData(DataFormats.FileDrop);

            foreach (var f in FileCollector.Collect3DFiles(paths).Distinct(StringComparer.OrdinalIgnoreCase))
            {
                if (cbStepExcludeTrash.Checked &&
                f.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
                 .Any(part => string.Equals(part, "TRASH", StringComparison.OrdinalIgnoreCase)))
                    continue;

                var fileName = Path.GetFileName(f);

                if (ShouldSkipByName(fileName))
                    continue;

                if (_rows.Any(r => string.Equals(r.FullPath, f, StringComparison.OrdinalIgnoreCase)))
                    continue;

                _rows.Add(new FileRecord(f, Path.GetFileName(f)) { Status = "В очереди" });
            }
        }

        // -------------------- Editing (Dirty tracking) --------------------

        private void DataGridFiles_CellBeginEdit(object sender, DataGridViewCellCancelEventArgs e)
        {
            _editOldValue = Convert.ToString(dataGridFiles.Rows[e.RowIndex].Cells[e.ColumnIndex].Value) ?? "";
        }

        private void DataGridFiles_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;

            var rec = dataGridFiles.Rows[e.RowIndex].DataBoundItem as FileRecord;
            if (rec == null) return;

            string colName = dataGridFiles.Columns[e.ColumnIndex].Name;
            string newValue = Convert.ToString(dataGridFiles.Rows[e.RowIndex].Cells[e.ColumnIndex].Value) ?? "";

            if (string.Equals(_editOldValue ?? "", newValue ?? "", StringComparison.Ordinal))
                return;

            if (colName == "colDesignation")
            {
                rec.MarkingState = CellState.Dirty;
                rec.Status = "Изменено";
            }
            else if (colName == "colTitle")
            {
                rec.NameState = CellState.Dirty;
                rec.Status = "Изменено";
            }

            dataGridFiles.InvalidateRow(e.RowIndex);
        }

        // -------------------- Coloring --------------------

        private void DataGridFiles_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex < 0) return;

            var rec = dataGridFiles.Rows[e.RowIndex].DataBoundItem as FileRecord;
            if (rec == null) return;

            string colName = dataGridFiles.Columns[e.ColumnIndex].Name;

            if (colName == "colDesignation")
                ApplyCellStateColor(e, rec.MarkingState);

            if (colName == "colTitle")
                ApplyCellStateColor(e, rec.NameState);
        }

        private void ApplyCellStateColor(DataGridViewCellFormattingEventArgs e, CellState st)
        {
            e.CellStyle.ForeColor = System.Drawing.SystemColors.WindowText;
            e.CellStyle.BackColor = System.Drawing.SystemColors.Window;

            if (st == CellState.Dirty)
                e.CellStyle.BackColor = System.Drawing.Color.LightYellow;
            else if (st == CellState.WrittenOk)
                e.CellStyle.BackColor = System.Drawing.Color.LightGreen;
            else if (st == CellState.WriteError)
                e.CellStyle.BackColor = System.Drawing.Color.MistyRose;

            e.CellStyle.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            e.CellStyle.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
        }

        // -------------------- Buttons --------------------

        private void btnClear_Click(object sender, EventArgs e)
        {
            _rows.Clear();
            ProgressStart(0);

        }

        private void btnReadProps_Click(object sender, EventArgs e)
        {
            if (_rows.Count == 0) return;

            ToggleButtons(false);

            var list = _rows.ToList();

            ProgressStart(list.Count);

            var t = new Thread(() =>
            {
                using (var host = new KompasHost())
                {
                    host.AttachOrStart();

                    using (host.SilentScope())
                    {
                        var reader = new KompasPropertyReader(host.App7);

                        for (int i = 0; i < list.Count; i++)
                        {
                            var rec = list[i];
                            if (!Is3D(rec.FullPath)) continue;

                            dynamic doc = null;
                            NameMark nm = default(NameMark);
                            string err = null;

                            try
                            {
                                doc = host.OpenDocument(rec.FullPath, readOnly: true, visible: false);
                                if (doc == null) err = "Не удалось открыть";
                                else nm = reader.Read3D(doc);
                            }
                            catch (Exception ex)
                            {
                                err = ex.Message;
                            }
                            finally
                            {
                                TryClose(doc, false);
                            }

                            BeginInvoke(new Action(() =>
                            {
                                if (err == null)
                                {
                                    rec.Marking = nm.Marking ?? "";
                                    rec.Name = nm.Name ?? "";

                                    rec.MarkingState = CellState.Clean;
                                    rec.NameState = CellState.Clean;
                                    rec.Status = "Прочитано";
                                }
                                else
                                {
                                    rec.Status = "Ошибка: " + err;
                                }

                                ProgressStep(i);
                                dataGridFiles.Invalidate();


                            }));
                        }
                    }
                }

                BeginInvoke(new Action(() =>
                {
                    ToggleButtons(true);
                    ProgressDone();
                    dataGridFiles.Invalidate();
                }));
            });

            t.SetApartmentState(ApartmentState.STA);
            t.IsBackground = true;
            t.Start();
        }

        private void btnWriteProps_Click(object sender, EventArgs e)
        {
            if (_rows.Count == 0) return;

            var changed = _rows
                .Where(r => Is3D(r.FullPath) &&
                            (r.MarkingState == CellState.Dirty || r.NameState == CellState.Dirty))
                .ToList();

            if (changed.Count == 0) return;

            ProgressStart(changed.Count);

            ToggleButtons(false);

            var t = new Thread(() =>
            {
                using (var host = new KompasHost())
                {
                    host.AttachOrStart();

                    using (host.SilentScope())
                    {
                        var writer = new KompasPropertyWriter(host.App7);

                        for (int i = 0; i < changed.Count; i++)
                        {
                            var rec = changed[i];

                            dynamic doc = null;
                            bool ok = true;
                            string err = null;

                            try
                            {
                                doc = host.OpenDocument(rec.FullPath, readOnly: false, visible: false);
                                if (doc == null)
                                {
                                    ok = false;
                                    err = "Не удалось открыть";
                                }
                                else
                                {
                                    string werr;
                                    bool wok = writer.Write3D(doc, rec.Marking, rec.Name, out werr);
                                    if (!wok)
                                    {
                                        ok = false;
                                        err = werr ?? "Write3D вернул false";
                                    }
                                    else
                                    {
                                        // Сохраняем явно
                                        try { doc.Save(); }
                                        catch (Exception ex) { ok = false; err = "Save() failed: " + ex.Message; }
                                    }

                                    // Закрываем без bool, чтобы не упереться в сигнатуру
                                    try { doc.Close(); } catch { }
                                    doc = null;
                                }
                            }
                            catch (Exception ex)
                            {
                                ok = false;
                                err = ex.Message;
                            }
                            finally
                            {
                                TryClose(doc, false);
                            }

                            BeginInvoke(new Action(() =>
                            {
                                if (ok)
                                {
                                    if (rec.MarkingState == CellState.Dirty) rec.MarkingState = CellState.WrittenOk;
                                    if (rec.NameState == CellState.Dirty) rec.NameState = CellState.WrittenOk;
                                    rec.Status = "Записано";
                                }
                                else
                                {
                                    if (rec.MarkingState == CellState.Dirty) rec.MarkingState = CellState.WriteError;
                                    if (rec.NameState == CellState.Dirty) rec.NameState = CellState.WriteError;
                                    rec.Status = "Ошибка: " + (err ?? "");
                                }

                                ProgressStep(i);
                                dataGridFiles.Invalidate();
                            }));
                        }
                    }
                }

                BeginInvoke(new Action(() =>
                {
                    ToggleButtons(true);
                    ProgressDone();
                    dataGridFiles.Invalidate();
                }));
            });

            t.SetApartmentState(ApartmentState.STA);
            t.IsBackground = true;
            t.Start();
        }

        // пока не трогаем


        private void btnImport_Click(object sender, EventArgs e)
        {
            if (_rows.Count == 0)
            {
                MessageBox.Show("Таблица пуста. Сначала добавь файлы.");
                return;
            }

            using (var ofd = new OpenFileDialog())
            {
                ofd.Filter = "Excel (*.xlsx)|*.xlsx";
                ofd.Multiselect = false;

                var dir = Path.GetDirectoryName(_rows[0].FullPath);
                if (!string.IsNullOrEmpty(dir) && Directory.Exists(dir))
                    ofd.InitialDirectory = dir;

                if (ofd.ShowDialog() != DialogResult.OK) return;

                List<CsvRow> imported;
                try
                {
                    imported = ExcelImportService.Read(ofd.FileName);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка чтения Excel :\n" + ex.Message);
                    return;
                }

                if (imported.Count == 0)
                {
                    MessageBox.Show("В Excel  нет строк для импорта.");
                    return;
                }

                var res = RenameTableImportApplier.ApplyByFileName(_rows.ToList(), imported);

                if (res.DuplicateNames.Count > 0)
                {
                    MessageBox.Show(
                        "Импорт по имени файла небезопасен: в таблице есть дубли:\n" +
                        string.Join("\n", res.DuplicateNames.Take(10)) +
                        (res.DuplicateNames.Count > 10 ? "\n..." : "")
                    );
                    return;
                }

                dataGridFiles.Invalidate();

                MessageBox.Show(
                    "Импорт завершён.\n" +
                    "Применено: " + res.Applied + "\n" +
                    "Пропущено (нет такого имени файла): " + res.SkippedNoMatch
                );
            }

        }

        // -------------------- Helpers --------------------

        private void ToggleButtons(bool enabled)
        {
            btnReadProps.Enabled = enabled;
            btnWriteProps.Enabled = enabled;
            btnClear.Enabled = enabled;

            btnExport.Enabled = enabled;
            btnImport.Enabled = enabled;
        }

        private static bool Is3D(string path)
        {
            var ext = Path.GetExtension(path);
            if (ext == null) return false;
            ext = ext.ToLowerInvariant();
            return ext == ".m3d" || ext == ".a3d";
        }

        private static void TryClose(dynamic doc, bool save)
        {
            if (doc == null) return;
            try { doc.Close(save); }
            catch { try { doc.Close(); } catch { } }
        }

        private void btnExport_Click_1(object sender, EventArgs e)
        {
            if (_rows.Count == 0) return;

            var dir = Path.GetDirectoryName(_rows[0].FullPath);
            if (string.IsNullOrEmpty(dir) || !Directory.Exists(dir))
            {
                MessageBox.Show("Не удалось определить папку сохранения.");
                return;
            }

            var fileName = "Перечень деталей и сборок" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".xlsx";
            var fullPath = Path.Combine(dir, fileName);

            try
            {
                ExcelExportService.ExportRenameTable(fullPath, _rows);
                MessageBox.Show("Экспорт завершён:\n" + fullPath);
            }
            catch (Exception ex)
            {
                var msg = BuildExceptionText(ex);
                MessageBox.Show(msg, "Ошибка экспорта", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ProgressStart(int total)
        {
            if (total < 1) total = 1;
            niceProgressBar1.Minimum = 0;
            niceProgressBar1.Maximum = total;
            niceProgressBar1.Value = 0;
            niceProgressBar1.Visible = true;
        }

        private void ProgressStep(int done)
        {
            if (done < niceProgressBar1.Minimum) done = niceProgressBar1.Minimum;
            if (done > niceProgressBar1.Maximum) done = niceProgressBar1.Maximum;
            niceProgressBar1.Value = done;
        }

        private void ProgressDone()
        {
            niceProgressBar1.Value = niceProgressBar1.Maximum;
            // если хочешь скрывать после завершения:
            // niceProgressBar1.Visible = false;
        }

        private bool ShouldSkipByName(string fileName)
        {
            if (!cbStepExcludeOther.Checked) return false;
            if (string.IsNullOrEmpty(fileName)) return false;

            return fileName.IndexOf("[ПРОЧИЕ]", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private void tableLayoutPanel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private static string BuildExceptionText(Exception ex)
        {
            if (ex == null) return "(null exception)";

            var sb = new System.Text.StringBuilder();
            int level = 0;

            while (ex != null && level < 10)
            {
                sb.AppendLine("=== LEVEL " + level + " ===");
                sb.AppendLine(ex.GetType().FullName);
                sb.AppendLine(ex.Message ?? "");
                sb.AppendLine();
                sb.AppendLine("STACK:");
                sb.AppendLine(ex.StackTrace ?? "(no stack)");
                sb.AppendLine();
                ex = ex.InnerException;
                level++;
            }

            return sb.ToString();
        }
    }

    // DoubleBuffered extension for DataGridView
    internal static class DataGridViewExtensions
    {
        public static void DoubleBuffered(this DataGridView dgv, bool value)
        {
            try
            {
                var prop = typeof(DataGridView).GetProperty("DoubleBuffered",
                    System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
                if (prop != null) prop.SetValue(dgv, value, null);
            }
            catch { }
        }
    }


}