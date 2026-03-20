namespace KompasDropExport.UI.Tabs
{
    partial class TableTab
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
            this.cbStepExcludeOther = new System.Windows.Forms.CheckBox();
            this.dataGridFiles = new System.Windows.Forms.DataGridView();
            this.colFileName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colDesignation = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colTitle = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colPath = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colStatus = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.niceProgressBar1 = new KompasDropExport.UI.NiceProgressBar();
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this.btnExport = new System.Windows.Forms.Button();
            this.btnImport = new System.Windows.Forms.Button();
            this.btnClear = new System.Windows.Forms.Button();
            this.btnReadProps = new System.Windows.Forms.Button();
            this.btnWriteProps = new System.Windows.Forms.Button();
            this.tableLayoutPanel3 = new System.Windows.Forms.TableLayoutPanel();
            this.cbStepExcludeTrash = new System.Windows.Forms.CheckBox();
            this.tableLayoutPanel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridFiles)).BeginInit();
            this.tableLayoutPanel2.SuspendLayout();
            this.tableLayoutPanel3.SuspendLayout();
            this.SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 1;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Controls.Add(this.dataGridFiles, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.niceProgressBar1, 0, 3);
            this.tableLayoutPanel1.Controls.Add(this.tableLayoutPanel2, 0, 2);
            this.tableLayoutPanel1.Controls.Add(this.tableLayoutPanel3, 0, 1);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 5;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 50F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 60F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(1279, 553);
            this.tableLayoutPanel1.TabIndex = 0;
            this.tableLayoutPanel1.Paint += new System.Windows.Forms.PaintEventHandler(this.tableLayoutPanel1_Paint);
            // 
            // cbStepExcludeOther
            // 
            this.cbStepExcludeOther.AutoSize = true;
            this.cbStepExcludeOther.Checked = true;
            this.cbStepExcludeOther.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cbStepExcludeOther.Location = new System.Drawing.Point(5, 5);
            this.cbStepExcludeOther.Margin = new System.Windows.Forms.Padding(5);
            this.cbStepExcludeOther.Name = "cbStepExcludeOther";
            this.cbStepExcludeOther.Size = new System.Drawing.Size(375, 29);
            this.cbStepExcludeOther.TabIndex = 7;
            this.cbStepExcludeOther.Text = "Игнорировать [ПРОЧИЕ] в имени";
            this.cbStepExcludeOther.UseVisualStyleBackColor = true;
            // 
            // dataGridFiles
            // 
            this.dataGridFiles.AllowDrop = true;
            this.dataGridFiles.AllowUserToAddRows = false;
            this.dataGridFiles.AllowUserToDeleteRows = false;
            this.dataGridFiles.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridFiles.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colFileName,
            this.colDesignation,
            this.colTitle,
            this.colPath,
            this.colStatus});
            this.dataGridFiles.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dataGridFiles.Location = new System.Drawing.Point(3, 3);
            this.dataGridFiles.Name = "dataGridFiles";
            this.dataGridFiles.RowHeadersWidth = 82;
            this.dataGridFiles.RowTemplate.Height = 33;
            this.dataGridFiles.Size = new System.Drawing.Size(1273, 357);
            this.dataGridFiles.TabIndex = 0;
            // 
            // colFileName
            // 
            this.colFileName.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.colFileName.HeaderText = "Имя файла";
            this.colFileName.MinimumWidth = 10;
            this.colFileName.Name = "colFileName";
            // 
            // colDesignation
            // 
            this.colDesignation.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.colDesignation.HeaderText = "Обозначение";
            this.colDesignation.MinimumWidth = 10;
            this.colDesignation.Name = "colDesignation";
            // 
            // colTitle
            // 
            this.colTitle.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.colTitle.HeaderText = "Наименование";
            this.colTitle.MinimumWidth = 10;
            this.colTitle.Name = "colTitle";
            // 
            // colPath
            // 
            this.colPath.HeaderText = "Путь";
            this.colPath.MinimumWidth = 10;
            this.colPath.Name = "colPath";
            this.colPath.Visible = false;
            this.colPath.Width = 200;
            // 
            // colStatus
            // 
            this.colStatus.DataPropertyName = "Status";
            this.colStatus.HeaderText = "Стаус";
            this.colStatus.MinimumWidth = 10;
            this.colStatus.Name = "colStatus";
            this.colStatus.Width = 200;
            // 
            // niceProgressBar1
            // 
            this.niceProgressBar1.BackBarColor = System.Drawing.Color.LightGray;
            this.niceProgressBar1.BarColor = System.Drawing.Color.DeepSkyBlue;
            this.niceProgressBar1.BorderColor = System.Drawing.Color.Gray;
            this.niceProgressBar1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.niceProgressBar1.Location = new System.Drawing.Point(7, 456);
            this.niceProgressBar1.Margin = new System.Windows.Forms.Padding(7, 3, 7, 3);
            this.niceProgressBar1.Maximum = 100;
            this.niceProgressBar1.Minimum = 0;
            this.niceProgressBar1.Name = "niceProgressBar1";
            this.niceProgressBar1.Size = new System.Drawing.Size(1265, 34);
            this.niceProgressBar1.TabIndex = 5;
            this.niceProgressBar1.Text = "niceProgressBar1";
            this.niceProgressBar1.TextColor = System.Drawing.Color.Black;
            this.niceProgressBar1.Value = 0;
            // 
            // tableLayoutPanel2
            // 
            this.tableLayoutPanel2.ColumnCount = 5;
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.tableLayoutPanel2.Controls.Add(this.btnExport, 2, 0);
            this.tableLayoutPanel2.Controls.Add(this.btnImport, 3, 0);
            this.tableLayoutPanel2.Controls.Add(this.btnClear, 4, 0);
            this.tableLayoutPanel2.Controls.Add(this.btnReadProps, 0, 0);
            this.tableLayoutPanel2.Controls.Add(this.btnWriteProps, 1, 0);
            this.tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel2.Location = new System.Drawing.Point(0, 403);
            this.tableLayoutPanel2.Margin = new System.Windows.Forms.Padding(0);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            this.tableLayoutPanel2.RowCount = 1;
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 50F));
            this.tableLayoutPanel2.Size = new System.Drawing.Size(1279, 50);
            this.tableLayoutPanel2.TabIndex = 6;
            // 
            // btnExport
            // 
            this.btnExport.AutoSize = true;
            this.btnExport.BackColor = System.Drawing.Color.DeepSkyBlue;
            this.btnExport.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.btnExport.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnExport.Location = new System.Drawing.Point(517, 0);
            this.btnExport.Margin = new System.Windows.Forms.Padding(7, 0, 7, 0);
            this.btnExport.Name = "btnExport";
            this.btnExport.Padding = new System.Windows.Forms.Padding(2);
            this.btnExport.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.btnExport.Size = new System.Drawing.Size(241, 50);
            this.btnExport.TabIndex = 6;
            this.btnExport.Text = "Экспорт в Excel";
            this.btnExport.UseVisualStyleBackColor = false;
            this.btnExport.Click += new System.EventHandler(this.btnExport_Click_1);
            // 
            // btnImport
            // 
            this.btnImport.AutoSize = true;
            this.btnImport.BackColor = System.Drawing.Color.Gainsboro;
            this.btnImport.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.btnImport.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnImport.Location = new System.Drawing.Point(772, 0);
            this.btnImport.Margin = new System.Windows.Forms.Padding(7, 0, 7, 0);
            this.btnImport.Name = "btnImport";
            this.btnImport.Padding = new System.Windows.Forms.Padding(2);
            this.btnImport.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.btnImport.Size = new System.Drawing.Size(241, 50);
            this.btnImport.TabIndex = 7;
            this.btnImport.Text = "Импорт из Excel";
            this.btnImport.UseVisualStyleBackColor = false;
            this.btnImport.Click += new System.EventHandler(this.btnImport_Click);
            // 
            // btnClear
            // 
            this.btnClear.AutoSize = true;
            this.btnClear.BackColor = System.Drawing.Color.Gainsboro;
            this.btnClear.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.btnClear.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnClear.Location = new System.Drawing.Point(1027, 0);
            this.btnClear.Margin = new System.Windows.Forms.Padding(7, 0, 7, 0);
            this.btnClear.Name = "btnClear";
            this.btnClear.Padding = new System.Windows.Forms.Padding(2);
            this.btnClear.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.btnClear.Size = new System.Drawing.Size(245, 50);
            this.btnClear.TabIndex = 0;
            this.btnClear.Text = "Очистить очередь";
            this.btnClear.UseVisualStyleBackColor = false;
            this.btnClear.Click += new System.EventHandler(this.btnClear_Click);
            // 
            // btnReadProps
            // 
            this.btnReadProps.AutoSize = true;
            this.btnReadProps.BackColor = System.Drawing.Color.DeepSkyBlue;
            this.btnReadProps.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.btnReadProps.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnReadProps.Location = new System.Drawing.Point(7, 0);
            this.btnReadProps.Margin = new System.Windows.Forms.Padding(7, 0, 7, 0);
            this.btnReadProps.Name = "btnReadProps";
            this.btnReadProps.Padding = new System.Windows.Forms.Padding(2);
            this.btnReadProps.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.btnReadProps.Size = new System.Drawing.Size(241, 50);
            this.btnReadProps.TabIndex = 5;
            this.btnReadProps.Text = "Считать свойства";
            this.btnReadProps.UseVisualStyleBackColor = false;
            this.btnReadProps.Click += new System.EventHandler(this.btnReadProps_Click);
            // 
            // btnWriteProps
            // 
            this.btnWriteProps.AutoSize = true;
            this.btnWriteProps.BackColor = System.Drawing.Color.DeepSkyBlue;
            this.btnWriteProps.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.btnWriteProps.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnWriteProps.Location = new System.Drawing.Point(262, 0);
            this.btnWriteProps.Margin = new System.Windows.Forms.Padding(7, 0, 7, 0);
            this.btnWriteProps.Name = "btnWriteProps";
            this.btnWriteProps.Padding = new System.Windows.Forms.Padding(2);
            this.btnWriteProps.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.btnWriteProps.Size = new System.Drawing.Size(241, 50);
            this.btnWriteProps.TabIndex = 8;
            this.btnWriteProps.TabStop = false;
            this.btnWriteProps.Text = "Записать свойства";
            this.btnWriteProps.UseVisualStyleBackColor = false;
            this.btnWriteProps.Click += new System.EventHandler(this.btnWriteProps_Click);
            // 
            // tableLayoutPanel3
            // 
            this.tableLayoutPanel3.ColumnCount = 2;
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel3.Controls.Add(this.cbStepExcludeTrash, 1, 0);
            this.tableLayoutPanel3.Controls.Add(this.cbStepExcludeOther, 0, 0);
            this.tableLayoutPanel3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel3.Location = new System.Drawing.Point(0, 363);
            this.tableLayoutPanel3.Margin = new System.Windows.Forms.Padding(0);
            this.tableLayoutPanel3.Name = "tableLayoutPanel3";
            this.tableLayoutPanel3.RowCount = 1;
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel3.Size = new System.Drawing.Size(1279, 40);
            this.tableLayoutPanel3.TabIndex = 8;
            // 
            // cbStepExcludeTrash
            // 
            this.cbStepExcludeTrash.AutoSize = true;
            this.cbStepExcludeTrash.Checked = true;
            this.cbStepExcludeTrash.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cbStepExcludeTrash.Location = new System.Drawing.Point(644, 5);
            this.cbStepExcludeTrash.Margin = new System.Windows.Forms.Padding(5);
            this.cbStepExcludeTrash.Name = "cbStepExcludeTrash";
            this.cbStepExcludeTrash.Size = new System.Drawing.Size(308, 29);
            this.cbStepExcludeTrash.TabIndex = 8;
            this.cbStepExcludeTrash.Text = "Игнорировать папку Trash";
            this.cbStepExcludeTrash.UseVisualStyleBackColor = true;
            // 
            // TableTab
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(12F, 25F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.tableLayoutPanel1);
            this.Name = "TableTab";
            this.Size = new System.Drawing.Size(1279, 553);
            this.tableLayoutPanel1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridFiles)).EndInit();
            this.tableLayoutPanel2.ResumeLayout(false);
            this.tableLayoutPanel2.PerformLayout();
            this.tableLayoutPanel3.ResumeLayout(false);
            this.tableLayoutPanel3.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.DataGridView dataGridFiles;
        private System.Windows.Forms.Button btnClear;
        private System.Windows.Forms.Button btnReadProps;
        private System.Windows.Forms.Button btnExport;
        private System.Windows.Forms.Button btnImport;
        private System.Windows.Forms.Button btnWriteProps;
        private NiceProgressBar niceProgressBar1;
        private System.Windows.Forms.DataGridViewTextBoxColumn colFileName;
        private System.Windows.Forms.DataGridViewTextBoxColumn colDesignation;
        private System.Windows.Forms.DataGridViewTextBoxColumn colTitle;
        private System.Windows.Forms.DataGridViewTextBoxColumn colPath;
        private System.Windows.Forms.DataGridViewTextBoxColumn colStatus;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
        private System.Windows.Forms.CheckBox cbStepExcludeOther;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel3;
        private System.Windows.Forms.CheckBox cbStepExcludeTrash;
    }
}
