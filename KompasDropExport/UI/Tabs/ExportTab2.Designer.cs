namespace KompasDropExport.UI.Tabs
{
    partial class ExportTab2
    {
        /// <summary> 
        /// Обязательная переменная конструктора.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Освободить все используемые ресурсы.
        /// </summary>
        /// <param name="disposing">истинно, если управляемый ресурс должен быть удален; иначе ложно.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Код, автоматически созданный конструктором компонентов

        /// <summary> 
        /// Требуемый метод для поддержки конструктора — не изменяйте 
        /// содержимое этого метода с помощью редактора кода.
        /// </summary>
        private void InitializeComponent()
        {
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.progress = new KompasDropExport.UI.NiceProgressBar();
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this.btnExportStep = new System.Windows.Forms.Button();
            this.btnAddOpenDocs = new System.Windows.Forms.Button();
            this.btnClear = new System.Windows.Forms.Button();
            this.btnExportPdf = new System.Windows.Forms.Button();
            this.listBoxFiles = new System.Windows.Forms.ListBox();
            this.lblQueue = new System.Windows.Forms.Label();
            this.tableLayoutPanel3 = new System.Windows.Forms.TableLayoutPanel();
            this.panel3 = new System.Windows.Forms.Panel();
            this.rbSpace = new System.Windows.Forms.RadioButton();
            this.rbUnderscore = new System.Windows.Forms.RadioButton();
            this.rbDash = new System.Windows.Forms.RadioButton();
            this.panel2 = new System.Windows.Forms.Panel();
            this.cbPdfExcludeSpecs = new System.Windows.Forms.CheckBox();
            this.cbPdfExcludeAsm = new System.Windows.Forms.CheckBox();
            this.label5 = new System.Windows.Forms.Label();
            this.la = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.panel1 = new System.Windows.Forms.Panel();
            this.cbStepExcludeOther = new System.Windows.Forms.CheckBox();
            this.cbStepExcludeAsm = new System.Windows.Forms.CheckBox();
            this.lblStatus = new System.Windows.Forms.Label();
            this.tableLayoutPanel1.SuspendLayout();
            this.tableLayoutPanel2.SuspendLayout();
            this.tableLayoutPanel3.SuspendLayout();
            this.panel3.SuspendLayout();
            this.panel2.SuspendLayout();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.AllowDrop = true;
            this.tableLayoutPanel1.ColumnCount = 1;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Controls.Add(this.progress, 0, 4);
            this.tableLayoutPanel1.Controls.Add(this.tableLayoutPanel2, 0, 3);
            this.tableLayoutPanel1.Controls.Add(this.listBoxFiles, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.lblQueue, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.tableLayoutPanel3, 0, 2);
            this.tableLayoutPanel1.Controls.Add(this.lblStatus, 0, 5);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 6;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 136F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 50F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 60F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(1181, 598);
            this.tableLayoutPanel1.TabIndex = 3;
            this.tableLayoutPanel1.Paint += new System.Windows.Forms.PaintEventHandler(this.tableLayoutPanel1_Paint);
            // 
            // progress
            // 
            this.progress.BackBarColor = System.Drawing.Color.LightGray;
            this.progress.BarColor = System.Drawing.Color.DeepSkyBlue;
            this.progress.BorderColor = System.Drawing.Color.Gray;
            this.progress.Dock = System.Windows.Forms.DockStyle.Fill;
            this.progress.Location = new System.Drawing.Point(7, 501);
            this.progress.Margin = new System.Windows.Forms.Padding(7, 3, 7, 3);
            this.progress.Maximum = 100;
            this.progress.Minimum = 0;
            this.progress.Name = "progress";
            this.progress.Size = new System.Drawing.Size(1167, 34);
            this.progress.TabIndex = 9;
            this.progress.Text = "niceProgressBar1";
            this.progress.TextColor = System.Drawing.Color.Black;
            this.progress.Value = 0;
            // 
            // tableLayoutPanel2
            // 
            this.tableLayoutPanel2.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tableLayoutPanel2.ColumnCount = 4;
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tableLayoutPanel2.Controls.Add(this.btnExportStep, 1, 0);
            this.tableLayoutPanel2.Controls.Add(this.btnAddOpenDocs, 2, 0);
            this.tableLayoutPanel2.Controls.Add(this.btnClear, 3, 0);
            this.tableLayoutPanel2.Controls.Add(this.btnExportPdf, 0, 0);
            this.tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel2.Location = new System.Drawing.Point(0, 448);
            this.tableLayoutPanel2.Margin = new System.Windows.Forms.Padding(0);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            this.tableLayoutPanel2.RowCount = 1;
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 50F));
            this.tableLayoutPanel2.Size = new System.Drawing.Size(1181, 50);
            this.tableLayoutPanel2.TabIndex = 4;
            this.tableLayoutPanel2.Paint += new System.Windows.Forms.PaintEventHandler(this.tableLayoutPanel2_Paint);
            // 
            // btnExportStep
            // 
            this.btnExportStep.BackColor = System.Drawing.Color.DeepSkyBlue;
            this.btnExportStep.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnExportStep.Location = new System.Drawing.Point(302, 0);
            this.btnExportStep.Margin = new System.Windows.Forms.Padding(7, 0, 7, 0);
            this.btnExportStep.Name = "btnExportStep";
            this.btnExportStep.Padding = new System.Windows.Forms.Padding(2);
            this.btnExportStep.Size = new System.Drawing.Size(281, 50);
            this.btnExportStep.TabIndex = 3;
            this.btnExportStep.Text = "Экспорт STEP";
            this.btnExportStep.UseVisualStyleBackColor = false;
            this.btnExportStep.Click += new System.EventHandler(this.btnExportStep_Click);
            // 
            // btnAddOpenDocs
            // 
            this.btnAddOpenDocs.BackColor = System.Drawing.Color.Gainsboro;
            this.btnAddOpenDocs.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnAddOpenDocs.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnAddOpenDocs.Location = new System.Drawing.Point(597, 0);
            this.btnAddOpenDocs.Margin = new System.Windows.Forms.Padding(7, 0, 7, 0);
            this.btnAddOpenDocs.Name = "btnAddOpenDocs";
            this.btnAddOpenDocs.Padding = new System.Windows.Forms.Padding(2);
            this.btnAddOpenDocs.Size = new System.Drawing.Size(281, 50);
            this.btnAddOpenDocs.TabIndex = 5;
            this.btnAddOpenDocs.Text = "Добавить открытые";
            this.btnAddOpenDocs.UseVisualStyleBackColor = false;
            this.btnAddOpenDocs.Click += new System.EventHandler(this.btnAddOpenDocs_Click);
            // 
            // btnClear
            // 
            this.btnClear.BackColor = System.Drawing.Color.Gainsboro;
            this.btnClear.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnClear.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnClear.Location = new System.Drawing.Point(892, 0);
            this.btnClear.Margin = new System.Windows.Forms.Padding(7, 0, 7, 0);
            this.btnClear.Name = "btnClear";
            this.btnClear.Padding = new System.Windows.Forms.Padding(2);
            this.btnClear.Size = new System.Drawing.Size(282, 50);
            this.btnClear.TabIndex = 2;
            this.btnClear.Text = "Очистить очередь";
            this.btnClear.UseVisualStyleBackColor = false;
            this.btnClear.Click += new System.EventHandler(this.button_clear_Click);
            // 
            // btnExportPdf
            // 
            this.btnExportPdf.BackColor = System.Drawing.Color.DeepSkyBlue;
            this.btnExportPdf.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnExportPdf.Location = new System.Drawing.Point(7, 0);
            this.btnExportPdf.Margin = new System.Windows.Forms.Padding(7, 0, 7, 0);
            this.btnExportPdf.Name = "btnExportPdf";
            this.btnExportPdf.Padding = new System.Windows.Forms.Padding(2);
            this.btnExportPdf.Size = new System.Drawing.Size(281, 50);
            this.btnExportPdf.TabIndex = 4;
            this.btnExportPdf.Text = "Экспорт PDF";
            this.btnExportPdf.UseVisualStyleBackColor = false;
            this.btnExportPdf.Click += new System.EventHandler(this.btnExportPdf_Click);
            // 
            // listBoxFiles
            // 
            this.listBoxFiles.AllowDrop = true;
            this.listBoxFiles.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listBoxFiles.FormattingEnabled = true;
            this.listBoxFiles.ItemHeight = 25;
            this.listBoxFiles.Location = new System.Drawing.Point(4, 3);
            this.listBoxFiles.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.listBoxFiles.Name = "listBoxFiles";
            this.listBoxFiles.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;
            this.listBoxFiles.Size = new System.Drawing.Size(1173, 276);
            this.listBoxFiles.TabIndex = 0;
            this.listBoxFiles.SelectedIndexChanged += new System.EventHandler(this.listBox1_SelectedIndexChanged);
            this.listBoxFiles.DoubleClick += new System.EventHandler(this.listBoxFiles_DoubleClick);
            this.listBoxFiles.KeyDown += new System.Windows.Forms.KeyEventHandler(this.listBoxFiles_KeyDown);
            // 
            // lblQueue
            // 
            this.lblQueue.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblQueue.Location = new System.Drawing.Point(0, 282);
            this.lblQueue.Margin = new System.Windows.Forms.Padding(0);
            this.lblQueue.Name = "lblQueue";
            this.lblQueue.Size = new System.Drawing.Size(1181, 30);
            this.lblQueue.TabIndex = 6;
            this.lblQueue.Text = "Всего: 0";
            // 
            // tableLayoutPanel3
            // 
            this.tableLayoutPanel3.ColumnCount = 3;
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33334F));
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33334F));
            this.tableLayoutPanel3.Controls.Add(this.panel3, 2, 1);
            this.tableLayoutPanel3.Controls.Add(this.panel2, 0, 1);
            this.tableLayoutPanel3.Controls.Add(this.label5, 2, 0);
            this.tableLayoutPanel3.Controls.Add(this.la, 1, 0);
            this.tableLayoutPanel3.Controls.Add(this.label3, 0, 0);
            this.tableLayoutPanel3.Controls.Add(this.panel1, 1, 1);
            this.tableLayoutPanel3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel3.Location = new System.Drawing.Point(2, 314);
            this.tableLayoutPanel3.Margin = new System.Windows.Forms.Padding(2);
            this.tableLayoutPanel3.Name = "tableLayoutPanel3";
            this.tableLayoutPanel3.RowCount = 2;
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 20.88608F));
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 79.11393F));
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 15F));
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 15F));
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 15F));
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 15F));
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 15F));
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 15F));
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 15F));
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 15F));
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 15F));
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 15F));
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 15F));
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 15F));
            this.tableLayoutPanel3.Size = new System.Drawing.Size(1177, 132);
            this.tableLayoutPanel3.TabIndex = 7;
            this.tableLayoutPanel3.Paint += new System.Windows.Forms.PaintEventHandler(this.tableLayoutPanel3_Paint);
            // 
            // panel3
            // 
            this.panel3.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.panel3.Controls.Add(this.rbSpace);
            this.panel3.Controls.Add(this.rbUnderscore);
            this.panel3.Controls.Add(this.rbDash);
            this.panel3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel3.Location = new System.Drawing.Point(786, 29);
            this.panel3.Margin = new System.Windows.Forms.Padding(2);
            this.panel3.Name = "panel3";
            this.panel3.Padding = new System.Windows.Forms.Padding(5);
            this.panel3.Size = new System.Drawing.Size(389, 101);
            this.panel3.TabIndex = 18;
            // 
            // rbSpace
            // 
            this.rbSpace.AutoSize = true;
            this.rbSpace.Dock = System.Windows.Forms.DockStyle.Top;
            this.rbSpace.Location = new System.Drawing.Point(5, 63);
            this.rbSpace.Margin = new System.Windows.Forms.Padding(2);
            this.rbSpace.Name = "rbSpace";
            this.rbSpace.Size = new System.Drawing.Size(379, 29);
            this.rbSpace.TabIndex = 2;
            this.rbSpace.Text = "Пробел \" \"";
            this.rbSpace.UseVisualStyleBackColor = true;
            this.rbSpace.CheckedChanged += new System.EventHandler(this.radioButton7_CheckedChanged);
            // 
            // rbUnderscore
            // 
            this.rbUnderscore.AutoSize = true;
            this.rbUnderscore.Dock = System.Windows.Forms.DockStyle.Top;
            this.rbUnderscore.Location = new System.Drawing.Point(5, 34);
            this.rbUnderscore.Margin = new System.Windows.Forms.Padding(2);
            this.rbUnderscore.Name = "rbUnderscore";
            this.rbUnderscore.Size = new System.Drawing.Size(379, 29);
            this.rbUnderscore.TabIndex = 1;
            this.rbUnderscore.Text = "Подчерк \"_\"";
            this.rbUnderscore.UseVisualStyleBackColor = true;
            // 
            // rbDash
            // 
            this.rbDash.AutoSize = true;
            this.rbDash.Checked = true;
            this.rbDash.Dock = System.Windows.Forms.DockStyle.Top;
            this.rbDash.Location = new System.Drawing.Point(5, 5);
            this.rbDash.Margin = new System.Windows.Forms.Padding(2);
            this.rbDash.Name = "rbDash";
            this.rbDash.Size = new System.Drawing.Size(379, 29);
            this.rbDash.TabIndex = 0;
            this.rbDash.TabStop = true;
            this.rbDash.Text = "Дефис \" - \"";
            this.rbDash.UseVisualStyleBackColor = true;
            // 
            // panel2
            // 
            this.panel2.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.panel2.Controls.Add(this.cbPdfExcludeSpecs);
            this.panel2.Controls.Add(this.cbPdfExcludeAsm);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel2.Location = new System.Drawing.Point(2, 29);
            this.panel2.Margin = new System.Windows.Forms.Padding(2);
            this.panel2.Name = "panel2";
            this.panel2.Padding = new System.Windows.Forms.Padding(5);
            this.panel2.Size = new System.Drawing.Size(388, 101);
            this.panel2.TabIndex = 17;
            // 
            // cbPdfExcludeSpecs
            // 
            this.cbPdfExcludeSpecs.AutoSize = true;
            this.cbPdfExcludeSpecs.Dock = System.Windows.Forms.DockStyle.Top;
            this.cbPdfExcludeSpecs.Location = new System.Drawing.Point(5, 34);
            this.cbPdfExcludeSpecs.Margin = new System.Windows.Forms.Padding(2);
            this.cbPdfExcludeSpecs.Name = "cbPdfExcludeSpecs";
            this.cbPdfExcludeSpecs.Size = new System.Drawing.Size(378, 29);
            this.cbPdfExcludeSpecs.TabIndex = 1;
            this.cbPdfExcludeSpecs.Text = "Исключить спецификации (.spw)";
            this.cbPdfExcludeSpecs.UseVisualStyleBackColor = true;
            // 
            // cbPdfExcludeAsm
            // 
            this.cbPdfExcludeAsm.AutoSize = true;
            this.cbPdfExcludeAsm.Dock = System.Windows.Forms.DockStyle.Top;
            this.cbPdfExcludeAsm.Location = new System.Drawing.Point(5, 5);
            this.cbPdfExcludeAsm.Margin = new System.Windows.Forms.Padding(2);
            this.cbPdfExcludeAsm.Name = "cbPdfExcludeAsm";
            this.cbPdfExcludeAsm.Size = new System.Drawing.Size(378, 29);
            this.cbPdfExcludeAsm.TabIndex = 0;
            this.cbPdfExcludeAsm.Text = "Исключить сборки (по «СБ» в имени)";
            this.cbPdfExcludeAsm.UseVisualStyleBackColor = true;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label5.Location = new System.Drawing.Point(786, 0);
            this.label5.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(389, 27);
            this.label5.TabIndex = 15;
            this.label5.Text = "Разделитель для архива";
            this.label5.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // la
            // 
            this.la.AutoSize = true;
            this.la.Dock = System.Windows.Forms.DockStyle.Fill;
            this.la.Location = new System.Drawing.Point(394, 0);
            this.la.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.la.Name = "la";
            this.la.Size = new System.Drawing.Size(388, 27);
            this.la.TabIndex = 14;
            this.la.Text = "STEP параметры";
            this.la.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label3.Location = new System.Drawing.Point(2, 0);
            this.label3.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(388, 27);
            this.label3.TabIndex = 13;
            this.label3.Text = "PDF параметры";
            this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // panel1
            // 
            this.panel1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.panel1.Controls.Add(this.cbStepExcludeOther);
            this.panel1.Controls.Add(this.cbStepExcludeAsm);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(394, 29);
            this.panel1.Margin = new System.Windows.Forms.Padding(2);
            this.panel1.Name = "panel1";
            this.panel1.Padding = new System.Windows.Forms.Padding(7, 8, 7, 8);
            this.panel1.Size = new System.Drawing.Size(388, 101);
            this.panel1.TabIndex = 16;
            this.panel1.Paint += new System.Windows.Forms.PaintEventHandler(this.panel1_Paint);
            // 
            // cbStepExcludeOther
            // 
            this.cbStepExcludeOther.AutoSize = true;
            this.cbStepExcludeOther.Checked = true;
            this.cbStepExcludeOther.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cbStepExcludeOther.Dock = System.Windows.Forms.DockStyle.Top;
            this.cbStepExcludeOther.Location = new System.Drawing.Point(7, 37);
            this.cbStepExcludeOther.Margin = new System.Windows.Forms.Padding(4);
            this.cbStepExcludeOther.Name = "cbStepExcludeOther";
            this.cbStepExcludeOther.Size = new System.Drawing.Size(374, 29);
            this.cbStepExcludeOther.TabIndex = 1;
            this.cbStepExcludeOther.Text = "Исключить [ПРОЧИЕ] в имени";
            this.cbStepExcludeOther.UseVisualStyleBackColor = true;
            // 
            // cbStepExcludeAsm
            // 
            this.cbStepExcludeAsm.AutoSize = true;
            this.cbStepExcludeAsm.Checked = true;
            this.cbStepExcludeAsm.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cbStepExcludeAsm.Dock = System.Windows.Forms.DockStyle.Top;
            this.cbStepExcludeAsm.Location = new System.Drawing.Point(7, 8);
            this.cbStepExcludeAsm.Margin = new System.Windows.Forms.Padding(4);
            this.cbStepExcludeAsm.Name = "cbStepExcludeAsm";
            this.cbStepExcludeAsm.Size = new System.Drawing.Size(374, 29);
            this.cbStepExcludeAsm.TabIndex = 0;
            this.cbStepExcludeAsm.Text = "Исключить сборки (.a3d)";
            this.cbStepExcludeAsm.UseVisualStyleBackColor = true;
            // 
            // lblStatus
            // 
            this.lblStatus.Location = new System.Drawing.Point(2, 540);
            this.lblStatus.Margin = new System.Windows.Forms.Padding(2);
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(450, 26);
            this.lblStatus.TabIndex = 8;
            this.lblStatus.Text = "Перетащите файлы/папки: .cdw, .spw, .m3d, .a3d.\"";
            // 
            // ExportTab2
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(12F, 25F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.tableLayoutPanel1);
            this.Name = "ExportTab2";
            this.Size = new System.Drawing.Size(1181, 598);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel2.ResumeLayout(false);
            this.tableLayoutPanel3.ResumeLayout(false);
            this.tableLayoutPanel3.PerformLayout();
            this.panel3.ResumeLayout(false);
            this.panel3.PerformLayout();
            this.panel2.ResumeLayout(false);
            this.panel2.PerformLayout();
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
        private System.Windows.Forms.Button btnExportStep;
        private System.Windows.Forms.Button btnAddOpenDocs;
        private System.Windows.Forms.Button btnClear;
        private System.Windows.Forms.Button btnExportPdf;
        private System.Windows.Forms.ListBox listBoxFiles;
        private System.Windows.Forms.Label lblQueue;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel3;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label la;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Panel panel3;
        private System.Windows.Forms.RadioButton rbSpace;
        private System.Windows.Forms.RadioButton rbUnderscore;
        private System.Windows.Forms.RadioButton rbDash;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.CheckBox cbPdfExcludeSpecs;
        private System.Windows.Forms.CheckBox cbPdfExcludeAsm;
        private System.Windows.Forms.CheckBox cbStepExcludeOther;
        private System.Windows.Forms.CheckBox cbStepExcludeAsm;
        private System.Windows.Forms.Label lblStatus;
        private NiceProgressBar progress;
    }
}
