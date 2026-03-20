// Form1.cs
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Drawing;
using WinFormsApp = System.Windows.Forms.Application;
using KompasAPI7;

namespace KompasDropExport
{
    public class Form1 : Form
    {
        // --- Расширения ---
        readonly HashSet<string> DrawExt = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { ".cdw" };
        readonly HashSet<string> SpecExt = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { ".spw" };
        readonly HashSet<string> PartExt = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { ".m3d" };
        readonly HashSet<string> AsmExt = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { ".a3d" };

        // --- COM / App ownership ---
        dynamic _app = null;       // KOMPAS.Application.7 (late binding)
        IApplication _app7 = null; // API7 (раннее связывание)
        IPropertyMng _propMng = null;
        bool _ownApp = false;      // true если запустили сами

        // --- UI ---
        ListBox list;
        Button btnPdf, btnStep, btnAll, btnClear;
        GroupBox gbDest, gbStep, gbPdf;
        RadioButton rbWork, rbArch;
        CheckBox cbStepSkipAsm, cbStepSkipOther;
        CheckBox cbPdfSkipAsm, cbPdfSkipSpec;
        Label lblHint, lblStatus, lblQueue;
        NiceProgressBar progress;

        // --- Лог ---
        string _logPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "_export_log.txt");

        

        public Form1()
        {
            // базовые настройки
            this.AutoScaleMode = AutoScaleMode.Dpi;
            this.AutoScaleDimensions = new SizeF(96f, 96f);
            this.Font = SystemFonts.MessageBoxFont;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.DoubleBuffered = true;

            this.Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);

            InitializeComponent();

            this.Text = "KOMPAS Drag&Drop Export";
            this.Width = 1100;
            this.Height = 820;
            this.MinimumSize = new Size(900, 700);
            this.AllowDrop = true;
            this.DragEnter += Form1_DragEnter;
            this.DragDrop += Form1_DragDrop;

            // --- список файлов ---
            list = new ListBox { HorizontalScrollbar = true };
            this.Controls.Add(list);

            // --- группа: имена (рабочие/архив) ---
            gbDest = new GroupBox { Text = "Имена файлов", Padding = new Padding(8) };
            rbWork = new RadioButton { Text = "Рабочие", Checked = true };
            rbArch = new RadioButton { Text = "В Архив" };
            gbDest.Controls.Add(rbWork);
            gbDest.Controls.Add(rbArch);
            this.Controls.Add(gbDest);

            // --- группа: STEP параметры ---
            gbStep = new GroupBox { Text = "STEP параметры", Padding = new Padding(8) };
            cbStepSkipAsm = new CheckBox { Text = "Исключить сборки (.a3d)", Checked = true };
            cbStepSkipOther = new CheckBox { Text = "Исключить [ПРОЧИЕ] в имени", Checked = true };
            gbStep.Controls.Add(cbStepSkipAsm);
            gbStep.Controls.Add(cbStepSkipOther);
            this.Controls.Add(gbStep);

            // --- группа: PDF параметры ---
            gbPdf = new GroupBox { Text = "PDF параметры", Padding = new Padding(8) };
            cbPdfSkipAsm = new CheckBox { Text = "Исключить сборки (по «СБ» в имени)" };
            cbPdfSkipSpec = new CheckBox { Text = "Исключить спецификации (.spw)" };
            gbPdf.Controls.Add(cbPdfSkipAsm);
            gbPdf.Controls.Add(cbPdfSkipSpec);
            this.Controls.Add(gbPdf);

            // --- статус очереди ---
            lblQueue = new Label { Text = "В очереди: 0 файлов" };
            this.Controls.Add(lblQueue);

            // --- кнопки ---
            btnPdf = NewBtn("Экспорт в PDF (ЧЕРТЕЖИ)", (s, e) => RunExport(true, false));
            btnStep = NewBtn("Экспорт в STEP (МОДЕЛИ)", (s, e) => RunExport(false, true));
            btnAll = NewBtn("Экспорт ВСЁ (PDF+STEP)", (s, e) => RunExport(true, true));
            btnClear = NewBtn("Очистить очередь", (s, e) => {
                list.Items.Clear(); RefreshQueueStats(); ResetProgress();
            });
            this.Controls.AddRange(new Control[] { btnPdf, btnStep, btnAll, btnClear });

            // стили кнопок
            Color c1 = Color.FromArgb(52, 152, 219), c2 = Color.FromArgb(41, 128, 185),
                  c3 = Color.FromArgb(34, 112, 170), c4 = Color.FromArgb(23, 95, 148);
            StyleButton(btnPdf, c1); StyleButton(btnStep, c2); StyleButton(btnAll, c3); StyleButton(btnClear, c4);

            // --- прогресс ---
            progress = new NiceProgressBar
            {
                Minimum = 0,
                Maximum = 100,
                Value = 0,
                BarColor = Color.DeepSkyBlue,   // ← вот тут задаёшь нужный цвет
                BackBarColor = Color.Gainsboro, // фон шкалы, на вкус
                BorderColor = Color.Silver,     // рамка
                TextColor = Color.Black         // проценты на баре
            };
            this.Controls.Add(progress);

            // --- подсказка и статус ---
            lblHint = new Label { Text = "Перетащите файлы/папки: .cdw, .spw, .m3d, .a3d." };
            lblStatus = new Label { Text = "Готово к работе." };
            this.Controls.Add(lblHint);
            this.Controls.Add(lblStatus);

            Try(delegate { File.WriteAllText(_logPath, ""); });

            this.Resize += (s, e) => LayoutControls();
            this.Load += (s, e) => LayoutControls();
        }

        Button NewBtn(string text, EventHandler onClick)
        {
            var b = new Button { Text = text };
            b.Click += onClick;
            return b;
        }

        void StyleButton(Button b, Color back)
        {
            b.FlatStyle = FlatStyle.Flat;
            b.FlatAppearance.BorderSize = 0;
            b.UseVisualStyleBackColor = false;
            b.BackColor = back;
            b.ForeColor = Color.White;
        }

        void InitializeComponent() { }

        // аккуратная перерисовка
        protected override CreateParams CreateParams
        {
            get { var cp = base.CreateParams; cp.ExStyle |= 0x02000000; return cp; } // WS_EX_COMPOSITED
        }

        // Автолэйаут
        void LayoutControls()
        {
            if (ClientSize.Width <= 0 || ClientSize.Height <= 0) return;
            int w = ClientSize.Width, h = ClientSize.Height;
            int margin = 12, inter = 8;
            int groupHeight = 190, buttonHeight = 80, progressHeight = 40, minListHeight = 100;
            int approxLabelH = TextRenderer.MeasureText("Hg", this.Font).Height + 4;

            int bottomFixed = groupHeight + inter + buttonHeight + inter + progressHeight + inter
                              + approxLabelH + inter + approxLabelH + margin + approxLabelH + 4;
            if (h < bottomFixed + minListHeight) h = bottomFixed + minListHeight;

            int listTop = margin;
            int listHeight = h - bottomFixed; if (listHeight < minListHeight) listHeight = minListHeight;
            list.SetBounds(margin, listTop, w - 2 * margin, listHeight);

            int queueTop = list.Bottom + 4;
            lblQueue.SetBounds(margin, queueTop, w - 2 * margin, approxLabelH);

            int groupsTop = lblQueue.Bottom + 4;
            int colWidth = (w - 2 * margin - 2 * inter) / 3; if (colWidth < 200) colWidth = 200;
            gbDest.SetBounds(margin, groupsTop, colWidth, groupHeight);
            gbStep.SetBounds(margin + colWidth + inter, groupsTop, colWidth, groupHeight);
            gbPdf.SetBounds(margin + 2 * (colWidth + inter), groupsTop, colWidth, groupHeight);

            int innerLeft = 12, innerTop = 45, cbSpacing = 28;
            int wDest = gbDest.ClientSize.Width - innerLeft - 8;
            rbWork.SetBounds(innerLeft, innerTop, wDest, approxLabelH);
            rbArch.SetBounds(innerLeft, rbWork.Bottom + cbSpacing, wDest, approxLabelH);

            int wStep = gbStep.ClientSize.Width - innerLeft - 8;
            cbStepSkipAsm.SetBounds(innerLeft, innerTop, wStep, approxLabelH);
            cbStepSkipOther.SetBounds(innerLeft, cbStepSkipAsm.Bottom + cbSpacing, wStep, approxLabelH);

            int wPdf = gbPdf.ClientSize.Width - innerLeft - 8;
            cbPdfSkipAsm.SetBounds(innerLeft, innerTop, wPdf, approxLabelH);
            cbPdfSkipSpec.SetBounds(innerLeft, cbPdfSkipAsm.Bottom + cbSpacing, wPdf, approxLabelH);

            int buttonsTop = groupsTop + groupHeight + inter;
            int bw = (w - 2 * margin - 3 * inter) / 4; if (bw < 140) bw = 140;
            int xBtn = margin;
            btnPdf.SetBounds(xBtn, buttonsTop, bw, buttonHeight); xBtn += bw + inter;
            btnStep.SetBounds(xBtn, buttonsTop, bw, buttonHeight); xBtn += bw + inter;
            btnAll.SetBounds(xBtn, buttonsTop, bw, buttonHeight); xBtn += bw + inter;
            btnClear.SetBounds(xBtn, buttonsTop, bw, buttonHeight);

            int progressTop = buttonsTop + buttonHeight + inter;
            progress.SetBounds(margin, progressTop, w - 2 * margin, progressHeight);

            int hintTop = progressTop + progressHeight + inter;
            lblHint.SetBounds(margin, hintTop, w - 2 * margin, approxLabelH);

            int statusTop = hintTop + approxLabelH + inter;
            lblStatus.SetBounds(margin, statusTop, w - 2 * margin, approxLabelH);
        }

        // === Drag & Drop ===
        void Form1_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop)) e.Effect = DragDropEffects.Copy;
        }
        void Form1_DragDrop(object sender, DragEventArgs e)
        {
            string[] dropped = (string[])e.Data.GetData(DataFormats.FileDrop);
            foreach (string p in dropped)
            {
                if (File.Exists(p)) list.Items.Add(p);
                else if (Directory.Exists(p))
                    foreach (string f in Directory.GetFiles(p, "*.*", SearchOption.AllDirectories))
                        list.Items.Add(f);
            }
            RefreshQueueStats();
            ResetProgress();
        }

        void RefreshQueueStats()
        {
            int nDraw = 0, nSpec = 0, nPart = 0, nAsm = 0;
            foreach (object it in list.Items)
            {
                string file = it.ToString();
                string ext = Path.GetExtension(file);
                if (DrawExt.Contains(ext)) nDraw++;
                else if (SpecExt.Contains(ext)) nSpec++;
                else if (PartExt.Contains(ext)) nPart++;
                else if (AsmExt.Contains(ext)) nAsm++;
            }
            int total = nDraw + nSpec + nPart + nAsm;
            lblQueue.Text = "В очереди: " + total +
                            " | Чертежей: " + nDraw +
                            " | Спецификаций: " + nSpec +
                            " | Деталей: " + nPart +
                            " | Сборок: " + nAsm;
        }

        void ResetProgress() { progress.Value = 0; lblStatus.Text = "Готово к работе."; }

        // === Короткий helper для «живого» статуса ===
        void Stage(string text)
        {
            lblHint.Text = text;
            WinFormsApp.DoEvents();
        }

        // === Экспорт ===
        void RunExport(bool doPdf, bool doStep)
        {
            if (list.Items.Count == 0) { MessageBox.Show("Нет файлов."); return; }

            ToggleUi(false);
            lblStatus.Text = "Старт...";
            Cursor = Cursors.WaitCursor;

            try
            {
                Stage("Запуск/подключение KOMPAS…");
                EnsureKompas();
                EnsureKompas7();
                EnsurePropertyManager();

                int prevHide = 0;
                Try(delegate { prevHide = Convert.ToInt32(_app.HideMessage); });
                EnterSilentMode();

                int ok = 0, skip = 0, err = 0;
                int processed = 0, total = list.Items.Count;
                progress.Minimum = 0; progress.Maximum = total; progress.Value = 0;

                foreach (object obj in list.Items)
                {
                    string file = obj.ToString();
                    string shortName = Path.GetFileName(file);
                    string ext = Path.GetExtension(file);
                    bool anyDid = false;

                    int index = processed + 1;

                    try
                    {
                        // --- PDF ---
                        if (doPdf && (DrawExt.Contains(ext) || SpecExt.Contains(ext)))
                        {
                            bool skipThis = false;
                            if (SpecExt.Contains(ext) && cbPdfSkipSpec.Checked) skipThis = true;
                            if (DrawExt.Contains(ext) && cbPdfSkipAsm.Checked && LooksLikeAssemblyDrawing(file)) skipThis = true;

                            if (!skipThis)
                            {
                                dynamic doc = null;
                                try
                                {
                                    Stage($"[{index}/{total}] {shortName}: открытие для PDF…");
                                    doc = OpenDoc(file);
                                    if (doc != null)
                                    {
                                        Try(delegate { doc.Activate(); });

                                        Stage($"[{index}/{total}] {shortName}: перестроение/обновление…");
                                        TryRebuild(doc);
                                        Preflight2D(doc);

                                        Stage($"[{index}/{total}] {shortName}: чтение свойств/формирование имени…");
                                        string baseName = MakeBaseName_FromKompas(doc, file, rbArch.Checked);

                                        string destDir = GetDestDir(file, true, rbArch.Checked);
                                        Directory.CreateDirectory(destDir);

                                        string outPath = Path.Combine(destDir, baseName + ".pdf");
                                        PrepareOldIfExists(outPath, destDir);

                                        Stage($"[{index}/{total}] {shortName}: печать в PDF…");
                                        bool pdfOk = ExportDrawingToPdf(doc, outPath);

                                        if (pdfOk && File.Exists(outPath))
                                        { ok++; anyDid = true; Stage($"[{index}/{total}] {shortName}: PDF сохранен."); }
                                        else { err++; Stage($"[{index}/{total}] {shortName}: ошибка сохранения PDF."); }
                                    }
                                    else { err++; Stage($"[{index}/{total}] {shortName}: не удалось открыть файл."); }
                                }
                                finally { SafeClose(doc); }
                            }
                        }

                        // --- STEP ---
                        if (doStep && (PartExt.Contains(ext) || AsmExt.Contains(ext)))
                        {
                            bool skipThis = false;
                            if (AsmExt.Contains(ext) && cbStepSkipAsm.Checked) skipThis = true;
                            if (cbStepSkipOther.Checked && HasOtherTag(file)) skipThis = true;

                            if (!skipThis)
                            {
                                dynamic doc = null;
                                try
                                {
                                    Stage($"[{index}/{total}] {shortName}: подготовка имени STEP…");
                                    string baseName = Path.GetFileNameWithoutExtension(file);
                                    if (rbArch.Checked)
                                    {
                                        NameMark nm = Read3D(file);
                                        if (!string.IsNullOrWhiteSpace(nm.Marking) || !string.IsNullOrWhiteSpace(nm.Name))
                                            baseName = ((nm.Marking ?? "").Trim() + " " + (nm.Name ?? "").Trim()).Trim();
                                    }
                                    baseName = SanitizeFileName(baseName);

                                    string destDir = GetDestDir(file, false, rbArch.Checked);
                                    Directory.CreateDirectory(destDir);

                                    string outPath = Path.Combine(destDir, baseName + ".step");
                                    PrepareOldIfExists(outPath, destDir);

                                    Stage($"[{index}/{total}] {shortName}: открытие для STEP…");
                                    doc = OpenDoc(file);
                                    if (doc != null)
                                    {
                                        Try(delegate { doc.Activate(); });

                                        Stage($"[{index}/{total}] {shortName}: перестроение…");
                                        TryRebuild(doc);

                                        Stage($"[{index}/{total}] {shortName}: сохранение STEP…");
                                        Try(delegate { doc.SaveAs(outPath); });

                                        if (File.Exists(outPath))
                                        { ok++; anyDid = true; Stage($"[{index}/{total}] {shortName}: STEP сохранен."); }
                                        else { err++; Stage($"[{index}/{total}] {shortName}: ошибка сохранения STEP."); }
                                    }
                                    else { err++; Stage($"[{index}/{total}] {shortName}: не удалось открыть файл."); }
                                }
                                finally { SafeClose(doc); }
                            }
                        }

                        if (!anyDid) skip++;
                    }
                    catch
                    {
                        err++; Stage($"[{index}/{total}] {shortName}: непредвиденная ошибка.");
                    }

                    processed++;
                    if (processed <= total) progress.Value = processed;
                    lblStatus.Text = $"Выполнено: {processed}/{total} | OK: {ok} | Пропуск: {skip} | Ошибок: {err}";
                    WinFormsApp.DoEvents();
                }

                MessageBox.Show($"Готово.\nУспешно: {ok}\nПропущено: {skip}\nОшибок: {err}");

                Try(delegate { _app.HideMessage = prevHide; });
            }
            finally
            {
                Cursor = Cursors.Default;
                ToggleUi(true);
                ExitSilentMode();
                ReleaseKompas();
                Stage("Перетащите файлы/папки: .cdw, .spw, .m3d, .a3d.");
            }
        }

        // === Формирование базового имени ===
        string MakeBaseName_FromKompas(dynamic doc, string filePath, bool rename)
        {
            string baseName = Path.GetFileNameWithoutExtension(filePath);
            if (!rename) return SanitizeFileName(baseName);

            string ext = Path.GetExtension(filePath);
            string des = null, nam = null, how = "NONE";

            if (DrawExt.Contains(ext) || SpecExt.Contains(ext))
            {
                Tuple<string, string> t = Read2D_NameMarking_FromKeeper(doc, out how);
                nam = t.Item1; // Наименование (Id=5)
                des = t.Item2; // Обозначение (Id=4)
            }
            else if (PartExt.Contains(ext) || AsmExt.Contains(ext))
            {
                NameMark t3 = Read3D(filePath);
                nam = t3.Name; des = t3.Marking; how = "3D";
            }

            if (!string.IsNullOrWhiteSpace(des) || !string.IsNullOrWhiteSpace(nam))
                baseName = ((des ?? "").Trim() + " " + (nam ?? "").Trim()).Trim();

            baseName = SanitizeFileName(baseName);

            Try(delegate {
                File.AppendAllText(_logPath,
                    DateTime.Now.ToString("HH:mm:ss") + " | " + Path.GetFileName(filePath) + " | " + how +
                    " | des='" + (des ?? "") + "', nam='" + (nam ?? "") + "' -> " + baseName + Environment.NewLine);
            });

            return baseName;
        }

        string SanitizeFileName(string s)
        {
            if (string.IsNullOrWhiteSpace(s)) return "noname";
            s = s.Trim();
            char[] bad = Path.GetInvalidFileNameChars();
            for (int i = 0; i < bad.Length; i++) s = s.Replace(bad[i].ToString(), " ");
            s = s.Replace("@", "");
            s = s.Replace("_", " ");
            while (s.Contains("  ")) s = s.Replace("  ", " ");
            return s;
        }

        // === 3D имя/обозначение ===
        struct NameMark { public string Name; public string Marking; }

        NameMark Read3D(string path)
        {
            NameMark nm = new NameMark();
            dynamic doc = OpenDoc(path);
            if (doc == null) return nm;

            try
            {
                IKompasDocument3D d3 = null;
                try { d3 = (IKompasDocument3D)doc; } catch { }
                if (d3 == null) return nm;

                object top = null, main = null, active = null;
                Try(delegate { top = d3.TopPart; });
                Try(delegate { dynamic dd = d3; main = dd.MainPart; });
                Try(delegate { dynamic dd = d3; active = dd.ActivePart; });

                object[] parts = new object[] { active, main, top };

                for (int i = 0; i < parts.Length; i++)
                {
                    object p = parts[i];
                    if (p == null) continue;
                    if (nm.Marking == null) nm.Marking = TryGetPartMarking(p);
                    if (nm.Name == null) nm.Name = TryGetPartName(p);
                    if (!string.IsNullOrWhiteSpace(nm.Name) && !string.IsNullOrWhiteSpace(nm.Marking)) break;
                }

                nm.Name = Clean(nm.Name);
                nm.Marking = Clean(nm.Marking);
                return nm;
            }
            finally { SafeClose(doc); }
        }

        string TryGetPartMarking(object part)
        {
            if (part == null) return null;
            try
            {
                string s = null;
                try { dynamic p = part; s = Convert.ToString(p.marking); } catch { }
                if (string.IsNullOrWhiteSpace(s))
                    try { dynamic p = part; s = Convert.ToString(p.GetMarking()); } catch { }
                return Clean(s);
            }
            catch { return null; }
        }

        string TryGetPartName(object part)
        {
            if (part == null) return null;
            try
            {
                string s = null;
                try { dynamic p = part; s = Convert.ToString(p.name); } catch { }
                if (string.IsNullOrWhiteSpace(s))
                    try { dynamic p = part; s = Convert.ToString(p.GetName()); } catch { }
                return Clean(s);
            }
            catch { return null; }
        }

        // === 2D свойства документа (Id 5 — Наименование, Id 4 — Обозначение) ===
        Tuple<string, string> Read2D_NameMarking_FromKeeper(dynamic docDyn, out string how)
        {
            how = "NONE";
            if (_propMng == null) _propMng = TryGetPropertyManager(_app7);
            if (_propMng == null) return new Tuple<string, string>(null, null);

            IPropertyKeeper keeper = null;
            try { keeper = (IPropertyKeeper)docDyn; } catch { }
            if (keeper == null)
            {
                try { IKompasDocument2D d2 = (IKompasDocument2D)docDyn; keeper = (IPropertyKeeper)d2; }
                catch { }
            }
            if (keeper == null) return new Tuple<string, string>(null, null);

            IProperty[] props = GetProps(keeper);
            IProperty pName = null, pMark = null;

            for (int i = 0; i < props.Length; i++)
            {
                double id = SafeId(props[i]);
                if (id == 5 && pName == null) pName = props[i];
                if (id == 4 && pMark == null) pMark = props[i];
            }

            string name = null, marking = null;
            if (pName != null) name = Clean(ReadPropBase(keeper, pName));
            if (pMark != null) marking = Clean(ReadPropBase(keeper, pMark));

            if (!string.IsNullOrWhiteSpace(name) || !string.IsNullOrWhiteSpace(marking))
                how = "Keeper(Ids4/5)";
            else
            {
                try
                {
                    object pp4 = _propMng.GetProperty(keeper, 4);
                    object pp5 = _propMng.GetProperty(keeper, 5);
                    if (pp5 != null && string.IsNullOrWhiteSpace(name)) name = Clean(ReadPropBase(keeper, pp5));
                    if (pp4 != null && string.IsNullOrWhiteSpace(marking)) marking = Clean(ReadPropBase(keeper, pp4));
                    if (!string.IsNullOrWhiteSpace(name) || !string.IsNullOrWhiteSpace(marking))
                        how = "Keeper(GetProperty 4/5)";
                }
                catch { }
            }

            return new Tuple<string, string>(name, marking);
        }

        IProperty[] GetProps(object keeper)
        {
            try
            {
                object arr = _propMng.GetProperties(keeper);
                if (arr is Array a)
                {
                    var listProps = new List<IProperty>();
                    foreach (object it in a) if (it is IProperty p) listProps.Add(p);
                    return listProps.ToArray();
                }
            }
            catch { }
            return new IProperty[0];
        }

        double SafeId(IProperty p) { try { return p.Id; } catch { return double.NaN; } }

        string ReadPropBase(object keeper, object prop)
        {
            try
            {
                dynamic k = keeper;
                object val; bool fromSrc;
                bool ok = k.GetPropertyValue(prop, out val, true, out fromSrc);
                return ok && val != null ? val.ToString() : null;
            }
            catch { return null; }
        }

        // === PDF ===
        bool ExportDrawingToPdf(dynamic doc, string outPdf)
        {
            try
            {
                Try(delegate { doc.Activate(); });
                TryRebuild(doc);
                Preflight2D(doc);

                dynamic pj = _app.PrintJob;
                Try(delegate { pj.Clear(); });

                bool added = false;
                if (!added) added = Try(delegate { pj.AddSheets(doc); });
                if (!added) added = Try(delegate { Try(delegate { doc.Activate(); }); pj.AddSheets(_app.ActiveDocument); });

                Try(delegate { pj.PlotToFile = true; });
                Try(delegate { pj.FileName = outPdf; });
                Try(delegate { pj.OutputFileName = outPdf; });

                bool executed = false;
                if (!executed) executed = Try(delegate { pj.SpecialExecute(outPdf); });
                if (!executed) executed = Try(delegate { pj.Execute(); });

                if (!File.Exists(outPdf))
                {
                    Preflight2D(doc);
                    Try(delegate { pj.Execute(); });
                }

                Try(delegate { pj.Clear(); });

                if (File.Exists(outPdf)) return true;
            }
            catch { }

            // запасной путь: SaveAs
            try
            {
                Try(delegate { doc.Activate(); });
                Preflight2D(doc);
                doc.SaveAs(outPdf);
                if (File.Exists(outPdf)) return true;
            }
            catch { }

            return false;
        }

        // === Preflight для 2D/спецификаций ===
        void Preflight2D(dynamic doc)
        {
            Try(delegate { doc.UpdateDocument(); });
            Try(delegate { doc.RebuildDocument(); });
            Try(delegate { doc.Rebuild(); });

            Try(delegate {
                dynamic d = doc;
                var spec = d.Specification;
                if (spec != null)
                {
                    Try(delegate { spec.Update(); });
                    Try(delegate { spec.Refresh(); });
                    Try(delegate { spec.Rebuild(); });
                }
            });

            Try(delegate {
                dynamic d = doc;
                var vlm = d.ViewsAndLayersManager;
                if (vlm != null)
                {
                    Try(delegate { vlm.UpdateViews(); });
                    Try(delegate { vlm.Rebuild(); });
                    Try(delegate { vlm.Refresh(); });
                }
            });

            Try(delegate { doc.UpdateDocument(); });
            Try(delegate { doc.RebuildDocument(); });
        }

        // === Папки и архивирование старых ===
        string GetDestDir(string srcPath, bool isPdf, bool useArchive)
        {
            string dir = Path.GetDirectoryName(srcPath);
            string leaf = isPdf ? (useArchive ? "PDF_archive" : "PDF")
                                : (useArchive ? "STEP_archive" : "STEP");
            return Path.Combine(dir, leaf);
        }

        void PrepareOldIfExists(string outPath, string destDir)
        {
            if (!File.Exists(outPath)) return;
            string oldDir = Path.Combine(destDir, "old");
            Directory.CreateDirectory(oldDir);
            string name = Path.GetFileNameWithoutExtension(outPath);
            string ext = Path.GetExtension(outPath);
            string stamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            string moved = Path.Combine(oldDir, name + "_" + stamp + ext);
            File.Move(outPath, moved);
        }

        bool HasOtherTag(string filePath)
        {
            string nm = Path.GetFileNameWithoutExtension(filePath);
            return nm.IndexOf("[ПРОЧИЕ]", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        bool LooksLikeAssemblyDrawing(string filePath)
        {
            string up = Path.GetFileNameWithoutExtension(filePath).ToUpperInvariant();
            return up.Contains(" СБ") || up.Contains("_СБ") || up.Contains("[СБ]") || up.EndsWith("СБ");
        }

        // === Инициализация/завершение KOMPAS ===
        void EnsureKompas()
        {
            if (_app != null) return;

            try
            {
                _app7 = (IApplication)Marshal.GetActiveObject("KOMPAS.Application.7");
                _app = _app7;
                _ownApp = false;
                return;
            }
            catch { }

            Type t = Type.GetTypeFromProgID("KOMPAS.Application.7");
            _app = Activator.CreateInstance(t);
            Try(delegate { _app.Visible = false; });
            _ownApp = true;
        }

        void EnsureKompas7()
        {
            if (_app7 != null) return;
            try { _app7 = (IApplication)_app; }
            catch
            {
                try { _app7 = (IApplication)Marshal.GetActiveObject("KOMPAS.Application.7"); }
                catch
                {
                    Type t = Type.GetTypeFromProgID("KOMPAS.Application.7");
                    object obj = Activator.CreateInstance(t);
                    _app7 = (IApplication)obj;
                    _ownApp = true;
                }
            }
            Try(delegate { _app7.Visible = false; });
        }

        void EnsurePropertyManager()
        {
            if (_propMng != null) return;
            _propMng = TryGetPropertyManager(_app7);
        }

        IPropertyMng TryGetPropertyManager(IApplication app7)
        {
            try { return (IPropertyMng)app7; } catch { return null; }
        }

        // «тихий режим» — всё через _app (API5 COM)
        void EnterSilentMode()
        {
            Try(delegate { _app.HideMessage = 1; });       // 0=show, 1=auto-Yes, 2=auto-No
            Try(delegate { _app.SilentMode = true; });
            Try(delegate { _app.ApplicationSilentMode = true; });
            Try(delegate { _app.Interactive = false; });
            Try(delegate { _app.EnablePrompt = false; });
            Try(delegate { _app.Visible = false; });
            Try(delegate { _app.SuppressUi = true; });
        }

        void ExitSilentMode()
        {
            Try(delegate { _app.HideMessage = 0; });
            Try(delegate { _app.SilentMode = false; });
            Try(delegate { _app.ApplicationSilentMode = false; });
            Try(delegate { _app.Interactive = true; });
            Try(delegate { _app.EnablePrompt = true; });
            Try(delegate { _app.SuppressUi = false; });
        }

        dynamic OpenDoc(string path)
        {
            EnsureKompas7();
            try { return _app7.Documents.Open(path, true, false); } // readOnly=true, visible=false
            catch
            {
                try { return _app.Documents.Open(path, true, false); }
                catch { return null; }
            }
        }

        void TryRebuild(dynamic doc)
        {
            Try(delegate { doc.UpdateDocument(); });
            Try(delegate { doc.RebuildDocument(); });
            Try(delegate { doc.Rebuild(); });
        }

        void SafeClose(dynamic doc)
        {
            if (doc == null) return;
            Try(delegate { doc.Close(false); });
        }

        void ReleaseKompas()
        {
            if (_ownApp && _app != null)
            {
                Try(delegate { _app.Quit(); });
                try { Marshal.FinalReleaseComObject(_app); } catch { }
                _app = null;
                _app7 = null;
            }
            else
            {
                _app = null;
                _app7 = null;
            }
            _propMng = null;
        }

        // === Хелперы ===
        void ToggleUi(bool enable)
        {
            btnPdf.Enabled = enable; btnStep.Enabled = enable; btnAll.Enabled = enable; btnClear.Enabled = enable;
            gbDest.Enabled = enable; gbStep.Enabled = enable; gbPdf.Enabled = enable;
        }

        bool Try(Action a) { try { a(); return true; } catch { return false; } }

        string Clean(string s)
        {
            if (string.IsNullOrWhiteSpace(s)) return null;
            s = s.Trim();
            if (s == "-" || s.Equals("string", StringComparison.OrdinalIgnoreCase)) return null;

            char[] bad = Path.GetInvalidFileNameChars();
            for (int i = 0; i < bad.Length; i++) s = s.Replace(bad[i].ToString(), " ");

            s = s.Replace("@", "");
            s = s.Replace("_", " ");
            while (s.Contains("  ")) s = s.Replace("  ", " ");
            return s;
        }
    }
}
