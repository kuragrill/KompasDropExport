namespace KompasDropExport.UI.Tabs
{
    partial class GraphTab
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
            this.lblStatus = new System.Windows.Forms.Label();
            this.niceProgressBar1 = new KompasDropExport.UI.NiceProgressBar();
            this.tabGraph = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.tableLayoutPanel4 = new System.Windows.Forms.TableLayoutPanel();
            this.lbOrphans = new System.Windows.Forms.ListBox();
            this.tableLayoutPanel6 = new System.Windows.Forms.TableLayoutPanel();
            this.button3 = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.btToTrash = new System.Windows.Forms.Button();
            this.tabPage3 = new System.Windows.Forms.TabPage();
            this.tableLayoutPanel5 = new System.Windows.Forms.TableLayoutPanel();
            this.dgRename = new System.Windows.Forms.DataGridView();
            this.Kind = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.BaseName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Designation = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Title = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.FullPath = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Level = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.NodeId = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.OriginalBaseName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.IsModified = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.tableLayoutPanel7 = new System.Windows.Forms.TableLayoutPanel();
            this.button1 = new System.Windows.Forms.Button();
            this.button4 = new System.Windows.Forms.Button();
            this.btRename = new System.Windows.Forms.Button();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.tableLayoutPanel3 = new System.Windows.Forms.TableLayoutPanel();
            this.dgNodes = new System.Windows.Forms.DataGridView();
            this.Id = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colNodeType = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Location = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Exists = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.FileName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.RelPath = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colPath = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dgEdges = new System.Windows.Forms.DataGridView();
            this.EdgeType = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.FromId = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ToId = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Count = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.IsResolved = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this.btnClear = new System.Windows.Forms.Button();
            this.btAnalyze = new System.Windows.Forms.Button();
            this.lbDrag = new System.Windows.Forms.Label();
            this.tableLayoutPanel1.SuspendLayout();
            this.tabGraph.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.tableLayoutPanel4.SuspendLayout();
            this.tableLayoutPanel6.SuspendLayout();
            this.tabPage3.SuspendLayout();
            this.tableLayoutPanel5.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgRename)).BeginInit();
            this.tableLayoutPanel7.SuspendLayout();
            this.tabPage2.SuspendLayout();
            this.tableLayoutPanel3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgNodes)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgEdges)).BeginInit();
            this.tableLayoutPanel2.SuspendLayout();
            this.SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 1;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Controls.Add(this.lblStatus, 0, 3);
            this.tableLayoutPanel1.Controls.Add(this.niceProgressBar1, 0, 2);
            this.tableLayoutPanel1.Controls.Add(this.tabGraph, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.tableLayoutPanel2, 0, 0);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 4;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 50F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 60F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(1757, 1133);
            this.tableLayoutPanel1.TabIndex = 0;
            // 
            // lblStatus
            // 
            this.lblStatus.Location = new System.Drawing.Point(2, 1075);
            this.lblStatus.Margin = new System.Windows.Forms.Padding(2);
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(450, 26);
            this.lblStatus.TabIndex = 9;
            this.lblStatus.Text = "Перетащите файл сборки (.a3d) .....";
            // 
            // niceProgressBar1
            // 
            this.niceProgressBar1.BackBarColor = System.Drawing.Color.LightGray;
            this.niceProgressBar1.BarColor = System.Drawing.Color.DeepSkyBlue;
            this.niceProgressBar1.BorderColor = System.Drawing.Color.Gray;
            this.niceProgressBar1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.niceProgressBar1.Location = new System.Drawing.Point(7, 1036);
            this.niceProgressBar1.Margin = new System.Windows.Forms.Padding(7, 3, 7, 3);
            this.niceProgressBar1.Maximum = 100;
            this.niceProgressBar1.Minimum = 0;
            this.niceProgressBar1.Name = "niceProgressBar1";
            this.niceProgressBar1.Size = new System.Drawing.Size(1743, 34);
            this.niceProgressBar1.TabIndex = 6;
            this.niceProgressBar1.Text = "niceProgressBar1";
            this.niceProgressBar1.TextColor = System.Drawing.Color.Black;
            this.niceProgressBar1.Value = 0;
            // 
            // tabGraph
            // 
            this.tabGraph.Controls.Add(this.tabPage1);
            this.tabGraph.Controls.Add(this.tabPage3);
            this.tabGraph.Controls.Add(this.tabPage2);
            this.tabGraph.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabGraph.Location = new System.Drawing.Point(3, 53);
            this.tabGraph.Name = "tabGraph";
            this.tabGraph.SelectedIndex = 0;
            this.tabGraph.Size = new System.Drawing.Size(1751, 977);
            this.tabGraph.TabIndex = 0;
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.tableLayoutPanel4);
            this.tabPage1.Location = new System.Drawing.Point(8, 39);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(1735, 930);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "Проблемы в структуре";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // tableLayoutPanel4
            // 
            this.tableLayoutPanel4.ColumnCount = 1;
            this.tableLayoutPanel4.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel4.Controls.Add(this.lbOrphans, 0, 0);
            this.tableLayoutPanel4.Controls.Add(this.tableLayoutPanel6, 0, 1);
            this.tableLayoutPanel4.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel4.Location = new System.Drawing.Point(3, 3);
            this.tableLayoutPanel4.Name = "tableLayoutPanel4";
            this.tableLayoutPanel4.RowCount = 2;
            this.tableLayoutPanel4.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel4.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40F));
            this.tableLayoutPanel4.Size = new System.Drawing.Size(1729, 924);
            this.tableLayoutPanel4.TabIndex = 0;
            // 
            // lbOrphans
            // 
            this.lbOrphans.AllowDrop = true;
            this.lbOrphans.Cursor = System.Windows.Forms.Cursors.Default;
            this.lbOrphans.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lbOrphans.FormattingEnabled = true;
            this.lbOrphans.ItemHeight = 25;
            this.lbOrphans.Location = new System.Drawing.Point(5, 4);
            this.lbOrphans.Margin = new System.Windows.Forms.Padding(5, 4, 5, 4);
            this.lbOrphans.Name = "lbOrphans";
            this.lbOrphans.Size = new System.Drawing.Size(1719, 876);
            this.lbOrphans.TabIndex = 1;
            this.lbOrphans.DoubleClick += new System.EventHandler(this.lbOrphans_DoubleClick);
            // 
            // tableLayoutPanel6
            // 
            this.tableLayoutPanel6.ColumnCount = 4;
            this.tableLayoutPanel6.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tableLayoutPanel6.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tableLayoutPanel6.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tableLayoutPanel6.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tableLayoutPanel6.Controls.Add(this.button3, 2, 0);
            this.tableLayoutPanel6.Controls.Add(this.button2, 1, 0);
            this.tableLayoutPanel6.Controls.Add(this.btToTrash, 0, 0);
            this.tableLayoutPanel6.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel6.Location = new System.Drawing.Point(0, 884);
            this.tableLayoutPanel6.Margin = new System.Windows.Forms.Padding(0);
            this.tableLayoutPanel6.Name = "tableLayoutPanel6";
            this.tableLayoutPanel6.RowCount = 1;
            this.tableLayoutPanel6.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel6.Size = new System.Drawing.Size(1729, 40);
            this.tableLayoutPanel6.TabIndex = 3;
            // 
            // button3
            // 
            this.button3.BackColor = System.Drawing.Color.SteelBlue;
            this.button3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.button3.Location = new System.Drawing.Point(864, 0);
            this.button3.Margin = new System.Windows.Forms.Padding(0);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(432, 40);
            this.button3.TabIndex = 2;
            this.button3.Text = "Перенести в мусор";
            this.button3.UseVisualStyleBackColor = false;
            // 
            // button2
            // 
            this.button2.BackColor = System.Drawing.Color.SteelBlue;
            this.button2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.button2.Location = new System.Drawing.Point(432, 0);
            this.button2.Margin = new System.Windows.Forms.Padding(0);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(432, 40);
            this.button2.TabIndex = 1;
            this.button2.Text = "Перенести в мусор";
            this.button2.UseVisualStyleBackColor = false;
            // 
            // btToTrash
            // 
            this.btToTrash.BackColor = System.Drawing.Color.SteelBlue;
            this.btToTrash.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btToTrash.Location = new System.Drawing.Point(0, 0);
            this.btToTrash.Margin = new System.Windows.Forms.Padding(0);
            this.btToTrash.Name = "btToTrash";
            this.btToTrash.Size = new System.Drawing.Size(432, 40);
            this.btToTrash.TabIndex = 0;
            this.btToTrash.Text = "Перенести в мусор";
            this.btToTrash.UseVisualStyleBackColor = false;
            this.btToTrash.Click += new System.EventHandler(this.btToTrash_Click);
            // 
            // tabPage3
            // 
            this.tabPage3.Controls.Add(this.tableLayoutPanel5);
            this.tabPage3.Location = new System.Drawing.Point(8, 39);
            this.tabPage3.Name = "tabPage3";
            this.tabPage3.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage3.Size = new System.Drawing.Size(1735, 930);
            this.tabPage3.TabIndex = 2;
            this.tabPage3.Text = "Переименование файлов";
            this.tabPage3.UseVisualStyleBackColor = true;
            // 
            // tableLayoutPanel5
            // 
            this.tableLayoutPanel5.ColumnCount = 1;
            this.tableLayoutPanel5.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel5.Controls.Add(this.dgRename, 0, 0);
            this.tableLayoutPanel5.Controls.Add(this.tableLayoutPanel7, 0, 1);
            this.tableLayoutPanel5.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel5.Location = new System.Drawing.Point(3, 3);
            this.tableLayoutPanel5.Name = "tableLayoutPanel5";
            this.tableLayoutPanel5.RowCount = 2;
            this.tableLayoutPanel5.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel5.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40F));
            this.tableLayoutPanel5.Size = new System.Drawing.Size(1729, 924);
            this.tableLayoutPanel5.TabIndex = 1;
            // 
            // dgRename
            // 
            this.dgRename.AllowDrop = true;
            this.dgRename.AllowUserToAddRows = false;
            this.dgRename.AllowUserToDeleteRows = false;
            this.dgRename.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgRename.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.Kind,
            this.BaseName,
            this.Designation,
            this.Title,
            this.FullPath,
            this.Level,
            this.NodeId,
            this.OriginalBaseName,
            this.IsModified});
            this.dgRename.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgRename.Location = new System.Drawing.Point(3, 3);
            this.dgRename.Name = "dgRename";
            this.dgRename.RowHeadersWidth = 82;
            this.dgRename.RowTemplate.Height = 33;
            this.dgRename.Size = new System.Drawing.Size(1723, 878);
            this.dgRename.TabIndex = 4;
            this.dgRename.CellContentClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgRename_CellContentClick);
            // 
            // Kind
            // 
            this.Kind.DataPropertyName = "Kind";
            this.Kind.HeaderText = "Тип";
            this.Kind.MinimumWidth = 10;
            this.Kind.Name = "Kind";
            this.Kind.ReadOnly = true;
            this.Kind.Width = 200;
            // 
            // BaseName
            // 
            this.BaseName.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.BaseName.DataPropertyName = "BaseName";
            this.BaseName.HeaderText = "Имя файла";
            this.BaseName.MinimumWidth = 10;
            this.BaseName.Name = "BaseName";
            // 
            // Designation
            // 
            this.Designation.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.Designation.DataPropertyName = "Designation";
            this.Designation.HeaderText = "Обозначение";
            this.Designation.MinimumWidth = 10;
            this.Designation.Name = "Designation";
            this.Designation.ReadOnly = true;
            // 
            // Title
            // 
            this.Title.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.Title.DataPropertyName = "Title";
            this.Title.HeaderText = "Наименование";
            this.Title.MinimumWidth = 10;
            this.Title.Name = "Title";
            this.Title.ReadOnly = true;
            // 
            // FullPath
            // 
            this.FullPath.DataPropertyName = "FullPath";
            this.FullPath.HeaderText = "Путь";
            this.FullPath.MinimumWidth = 10;
            this.FullPath.Name = "FullPath";
            this.FullPath.ReadOnly = true;
            this.FullPath.Visible = false;
            this.FullPath.Width = 200;
            // 
            // Level
            // 
            this.Level.DataPropertyName = "Level";
            this.Level.HeaderText = "Уровень";
            this.Level.MinimumWidth = 10;
            this.Level.Name = "Level";
            this.Level.ReadOnly = true;
            this.Level.Visible = false;
            this.Level.Width = 200;
            // 
            // NodeId
            // 
            this.NodeId.DataPropertyName = "NodeId";
            this.NodeId.HeaderText = "NodeId";
            this.NodeId.MinimumWidth = 10;
            this.NodeId.Name = "NodeId";
            this.NodeId.ReadOnly = true;
            this.NodeId.Visible = false;
            this.NodeId.Width = 200;
            // 
            // OriginalBaseName
            // 
            this.OriginalBaseName.DataPropertyName = "OriginalBaseName";
            this.OriginalBaseName.HeaderText = "OriginalBaseName";
            this.OriginalBaseName.MinimumWidth = 10;
            this.OriginalBaseName.Name = "OriginalBaseName";
            this.OriginalBaseName.ReadOnly = true;
            this.OriginalBaseName.Visible = false;
            this.OriginalBaseName.Width = 200;
            // 
            // IsModified
            // 
            this.IsModified.DataPropertyName = "IsModified";
            this.IsModified.HeaderText = "IsModified";
            this.IsModified.MinimumWidth = 10;
            this.IsModified.Name = "IsModified";
            this.IsModified.ReadOnly = true;
            this.IsModified.Visible = false;
            this.IsModified.Width = 200;
            // 
            // tableLayoutPanel7
            // 
            this.tableLayoutPanel7.ColumnCount = 4;
            this.tableLayoutPanel7.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tableLayoutPanel7.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tableLayoutPanel7.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tableLayoutPanel7.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tableLayoutPanel7.Controls.Add(this.button1, 2, 0);
            this.tableLayoutPanel7.Controls.Add(this.button4, 1, 0);
            this.tableLayoutPanel7.Controls.Add(this.btRename, 0, 0);
            this.tableLayoutPanel7.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel7.Location = new System.Drawing.Point(0, 884);
            this.tableLayoutPanel7.Margin = new System.Windows.Forms.Padding(0);
            this.tableLayoutPanel7.Name = "tableLayoutPanel7";
            this.tableLayoutPanel7.RowCount = 1;
            this.tableLayoutPanel7.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel7.Size = new System.Drawing.Size(1729, 40);
            this.tableLayoutPanel7.TabIndex = 3;
            // 
            // button1
            // 
            this.button1.BackColor = System.Drawing.Color.SteelBlue;
            this.button1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.button1.Location = new System.Drawing.Point(864, 0);
            this.button1.Margin = new System.Windows.Forms.Padding(0);
            this.button1.Name = "button1";
            this.button1.Padding = new System.Windows.Forms.Padding(2);
            this.button1.Size = new System.Drawing.Size(432, 40);
            this.button1.TabIndex = 2;
            this.button1.Text = "Перенести в мусор";
            this.button1.UseVisualStyleBackColor = false;
            // 
            // button4
            // 
            this.button4.BackColor = System.Drawing.Color.SteelBlue;
            this.button4.Dock = System.Windows.Forms.DockStyle.Fill;
            this.button4.Location = new System.Drawing.Point(432, 0);
            this.button4.Margin = new System.Windows.Forms.Padding(0);
            this.button4.Name = "button4";
            this.button4.Padding = new System.Windows.Forms.Padding(2);
            this.button4.Size = new System.Drawing.Size(432, 40);
            this.button4.TabIndex = 1;
            this.button4.Text = "тест";
            this.button4.UseVisualStyleBackColor = false;
            this.button4.Click += new System.EventHandler(this.button4_Click);
            // 
            // btRename
            // 
            this.btRename.BackColor = System.Drawing.Color.SteelBlue;
            this.btRename.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btRename.Location = new System.Drawing.Point(0, 0);
            this.btRename.Margin = new System.Windows.Forms.Padding(0);
            this.btRename.Name = "btRename";
            this.btRename.Padding = new System.Windows.Forms.Padding(2);
            this.btRename.Size = new System.Drawing.Size(432, 40);
            this.btRename.TabIndex = 0;
            this.btRename.Text = "Переименовать";
            this.btRename.UseVisualStyleBackColor = false;
            this.btRename.Click += new System.EventHandler(this.btRename_Click);
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.tableLayoutPanel3);
            this.tabPage2.Location = new System.Drawing.Point(8, 39);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(1735, 930);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "Граф проекта";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // tableLayoutPanel3
            // 
            this.tableLayoutPanel3.ColumnCount = 1;
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel3.Controls.Add(this.dgNodes, 0, 1);
            this.tableLayoutPanel3.Controls.Add(this.dgEdges, 0, 3);
            this.tableLayoutPanel3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel3.Location = new System.Drawing.Point(3, 3);
            this.tableLayoutPanel3.Name = "tableLayoutPanel3";
            this.tableLayoutPanel3.RowCount = 4;
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 50F));
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 50F));
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel3.Size = new System.Drawing.Size(1729, 924);
            this.tableLayoutPanel3.TabIndex = 0;
            // 
            // dgNodes
            // 
            this.dgNodes.AllowUserToDeleteRows = false;
            this.dgNodes.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgNodes.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.Id,
            this.colNodeType,
            this.Location,
            this.Exists,
            this.FileName,
            this.RelPath,
            this.colPath});
            this.dgNodes.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgNodes.Location = new System.Drawing.Point(3, 53);
            this.dgNodes.Name = "dgNodes";
            this.dgNodes.ReadOnly = true;
            this.dgNodes.RowHeadersWidth = 82;
            this.dgNodes.RowTemplate.Height = 33;
            this.dgNodes.Size = new System.Drawing.Size(1723, 406);
            this.dgNodes.TabIndex = 0;
            this.dgNodes.CellContentClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgNodes_CellContentClick);
            // 
            // Id
            // 
            this.Id.DataPropertyName = "Id";
            this.Id.HeaderText = "Id";
            this.Id.MinimumWidth = 10;
            this.Id.Name = "Id";
            this.Id.ReadOnly = true;
            this.Id.Width = 200;
            // 
            // colNodeType
            // 
            this.colNodeType.DataPropertyName = "NodeType";
            this.colNodeType.HeaderText = "NodeType";
            this.colNodeType.MinimumWidth = 10;
            this.colNodeType.Name = "colNodeType";
            this.colNodeType.ReadOnly = true;
            this.colNodeType.Width = 200;
            // 
            // Location
            // 
            this.Location.DataPropertyName = "Location";
            this.Location.HeaderText = "Location";
            this.Location.MinimumWidth = 10;
            this.Location.Name = "Location";
            this.Location.ReadOnly = true;
            this.Location.Width = 200;
            // 
            // Exists
            // 
            this.Exists.DataPropertyName = "Exists";
            this.Exists.HeaderText = "Exists";
            this.Exists.MinimumWidth = 10;
            this.Exists.Name = "Exists";
            this.Exists.ReadOnly = true;
            this.Exists.Width = 200;
            // 
            // FileName
            // 
            this.FileName.DataPropertyName = "FileName";
            this.FileName.HeaderText = "FileName";
            this.FileName.MinimumWidth = 10;
            this.FileName.Name = "FileName";
            this.FileName.ReadOnly = true;
            this.FileName.Width = 200;
            // 
            // RelPath
            // 
            this.RelPath.DataPropertyName = "RelPath";
            this.RelPath.HeaderText = "RelPath";
            this.RelPath.MinimumWidth = 10;
            this.RelPath.Name = "RelPath";
            this.RelPath.ReadOnly = true;
            this.RelPath.Width = 200;
            // 
            // colPath
            // 
            this.colPath.DataPropertyName = "colPath";
            this.colPath.HeaderText = "Path";
            this.colPath.MinimumWidth = 10;
            this.colPath.Name = "colPath";
            this.colPath.ReadOnly = true;
            this.colPath.Width = 200;
            // 
            // dgEdges
            // 
            this.dgEdges.AllowUserToDeleteRows = false;
            this.dgEdges.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgEdges.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.EdgeType,
            this.FromId,
            this.ToId,
            this.Count,
            this.IsResolved});
            this.dgEdges.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgEdges.Location = new System.Drawing.Point(3, 515);
            this.dgEdges.Name = "dgEdges";
            this.dgEdges.ReadOnly = true;
            this.dgEdges.RowHeadersWidth = 82;
            this.dgEdges.RowTemplate.Height = 33;
            this.dgEdges.Size = new System.Drawing.Size(1723, 406);
            this.dgEdges.TabIndex = 1;
            // 
            // EdgeType
            // 
            this.EdgeType.DataPropertyName = "EdgeType";
            this.EdgeType.HeaderText = "EdgeType";
            this.EdgeType.MinimumWidth = 10;
            this.EdgeType.Name = "EdgeType";
            this.EdgeType.ReadOnly = true;
            this.EdgeType.Width = 200;
            // 
            // FromId
            // 
            this.FromId.DataPropertyName = "FromId";
            this.FromId.HeaderText = "FromId";
            this.FromId.MinimumWidth = 10;
            this.FromId.Name = "FromId";
            this.FromId.ReadOnly = true;
            this.FromId.Width = 200;
            // 
            // ToId
            // 
            this.ToId.DataPropertyName = "ToId";
            this.ToId.HeaderText = "ToId";
            this.ToId.MinimumWidth = 10;
            this.ToId.Name = "ToId";
            this.ToId.ReadOnly = true;
            this.ToId.Width = 200;
            // 
            // Count
            // 
            this.Count.DataPropertyName = "Count";
            this.Count.HeaderText = "Count";
            this.Count.MinimumWidth = 10;
            this.Count.Name = "Count";
            this.Count.ReadOnly = true;
            this.Count.Width = 200;
            // 
            // IsResolved
            // 
            this.IsResolved.DataPropertyName = "IsResolved";
            this.IsResolved.HeaderText = "IsResolved";
            this.IsResolved.MinimumWidth = 10;
            this.IsResolved.Name = "IsResolved";
            this.IsResolved.ReadOnly = true;
            this.IsResolved.Width = 200;
            // 
            // tableLayoutPanel2
            // 
            this.tableLayoutPanel2.ColumnCount = 3;
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 288F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 288F));
            this.tableLayoutPanel2.Controls.Add(this.btnClear, 2, 0);
            this.tableLayoutPanel2.Controls.Add(this.btAnalyze, 1, 0);
            this.tableLayoutPanel2.Controls.Add(this.lbDrag, 0, 0);
            this.tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel2.Location = new System.Drawing.Point(3, 3);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            this.tableLayoutPanel2.RowCount = 1;
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel2.Size = new System.Drawing.Size(1751, 44);
            this.tableLayoutPanel2.TabIndex = 1;
            // 
            // btnClear
            // 
            this.btnClear.AutoSize = true;
            this.btnClear.BackColor = System.Drawing.Color.Gainsboro;
            this.btnClear.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.btnClear.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnClear.Location = new System.Drawing.Point(1470, 0);
            this.btnClear.Margin = new System.Windows.Forms.Padding(7, 0, 7, 0);
            this.btnClear.Name = "btnClear";
            this.btnClear.Padding = new System.Windows.Forms.Padding(2);
            this.btnClear.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.btnClear.Size = new System.Drawing.Size(274, 44);
            this.btnClear.TabIndex = 2;
            this.btnClear.Text = "Очистить очередь";
            this.btnClear.UseVisualStyleBackColor = false;
            this.btnClear.Click += new System.EventHandler(this.btnClear_Click);
            // 
            // btAnalyze
            // 
            this.btAnalyze.BackColor = System.Drawing.Color.DeepSkyBlue;
            this.btAnalyze.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btAnalyze.Location = new System.Drawing.Point(1182, 0);
            this.btAnalyze.Margin = new System.Windows.Forms.Padding(7, 0, 7, 0);
            this.btAnalyze.Name = "btAnalyze";
            this.btAnalyze.Padding = new System.Windows.Forms.Padding(2);
            this.btAnalyze.Size = new System.Drawing.Size(274, 44);
            this.btAnalyze.TabIndex = 0;
            this.btAnalyze.Text = "Проанализировать";
            this.btAnalyze.UseVisualStyleBackColor = false;
            this.btAnalyze.Click += new System.EventHandler(this.btAnalyze_Click);
            // 
            // lbDrag
            // 
            this.lbDrag.AllowDrop = true;
            this.lbDrag.AutoSize = true;
            this.lbDrag.BackColor = System.Drawing.Color.SteelBlue;
            this.lbDrag.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lbDrag.Location = new System.Drawing.Point(7, 5);
            this.lbDrag.Margin = new System.Windows.Forms.Padding(7, 5, 7, 5);
            this.lbDrag.Name = "lbDrag";
            this.lbDrag.Padding = new System.Windows.Forms.Padding(2);
            this.lbDrag.Size = new System.Drawing.Size(1161, 34);
            this.lbDrag.TabIndex = 1;
            this.lbDrag.Text = "файл.a3d";
            this.lbDrag.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // GraphTab
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(12F, 25F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.tableLayoutPanel1);
            this.Name = "GraphTab";
            this.Size = new System.Drawing.Size(1757, 1133);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tabGraph.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.tableLayoutPanel4.ResumeLayout(false);
            this.tableLayoutPanel6.ResumeLayout(false);
            this.tabPage3.ResumeLayout(false);
            this.tableLayoutPanel5.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgRename)).EndInit();
            this.tableLayoutPanel7.ResumeLayout(false);
            this.tabPage2.ResumeLayout(false);
            this.tableLayoutPanel3.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgNodes)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgEdges)).EndInit();
            this.tableLayoutPanel2.ResumeLayout(false);
            this.tableLayoutPanel2.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.TabControl tabGraph;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
        private System.Windows.Forms.Button btAnalyze;
        private System.Windows.Forms.Label lbDrag;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel3;
        private System.Windows.Forms.DataGridView dgNodes;
        private System.Windows.Forms.DataGridView dgEdges;
        private System.Windows.Forms.DataGridViewTextBoxColumn EdgeType;
        private System.Windows.Forms.DataGridViewTextBoxColumn FromId;
        private System.Windows.Forms.DataGridViewTextBoxColumn ToId;
        private System.Windows.Forms.DataGridViewTextBoxColumn Count;
        private System.Windows.Forms.DataGridViewTextBoxColumn IsResolved;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel4;
        private System.Windows.Forms.ListBox lbOrphans;
        private NiceProgressBar niceProgressBar1;
        private System.Windows.Forms.Button btToTrash;
        private System.Windows.Forms.Button btnClear;
        private System.Windows.Forms.DataGridViewTextBoxColumn Id;
        private System.Windows.Forms.DataGridViewTextBoxColumn colNodeType;
        private System.Windows.Forms.DataGridViewTextBoxColumn Location;
        private System.Windows.Forms.DataGridViewTextBoxColumn Exists;
        private System.Windows.Forms.DataGridViewTextBoxColumn FileName;
        private System.Windows.Forms.DataGridViewTextBoxColumn RelPath;
        private System.Windows.Forms.DataGridViewTextBoxColumn colPath;
        private System.Windows.Forms.Label lblStatus;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel6;
        private System.Windows.Forms.Button button3;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.TabPage tabPage3;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel5;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel7;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button button4;
        private System.Windows.Forms.Button btRename;
        private System.Windows.Forms.DataGridView dgRename;
        private System.Windows.Forms.DataGridViewTextBoxColumn Kind;
        private System.Windows.Forms.DataGridViewTextBoxColumn BaseName;
        private System.Windows.Forms.DataGridViewTextBoxColumn Designation;
        private System.Windows.Forms.DataGridViewTextBoxColumn Title;
        private System.Windows.Forms.DataGridViewTextBoxColumn FullPath;
        private System.Windows.Forms.DataGridViewTextBoxColumn Level;
        private System.Windows.Forms.DataGridViewTextBoxColumn NodeId;
        private System.Windows.Forms.DataGridViewTextBoxColumn OriginalBaseName;
        private System.Windows.Forms.DataGridViewTextBoxColumn IsModified;
    }
}
