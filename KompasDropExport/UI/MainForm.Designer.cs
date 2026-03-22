namespace KompasDropExport.UI
{
    partial class MainForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.TabControl = new System.Windows.Forms.TabControl();
            this.Export_new = new System.Windows.Forms.TabPage();
            this.exportTab22 = new KompasDropExport.UI.Tabs.ExportTab2();
            this.TableTab = new System.Windows.Forms.TabPage();
            this.tableTab1 = new KompasDropExport.UI.Tabs.TableTab();
            this.BOM = new System.Windows.Forms.TabPage();
            this.graphTab1 = new KompasDropExport.UI.Tabs.GraphTab();
            this.help = new System.Windows.Forms.TabPage();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.TabControl.SuspendLayout();
            this.Export_new.SuspendLayout();
            this.TableTab.SuspendLayout();
            this.BOM.SuspendLayout();
            this.help.SuspendLayout();
            this.SuspendLayout();
            // 
            // TabControl
            // 
            this.TabControl.AccessibleName = "dsf";
            this.TabControl.Controls.Add(this.Export_new);
            this.TabControl.Controls.Add(this.TableTab);
            this.TabControl.Controls.Add(this.BOM);
            this.TabControl.Controls.Add(this.help);
            this.TabControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.TabControl.Location = new System.Drawing.Point(0, 0);
            this.TabControl.Margin = new System.Windows.Forms.Padding(4);
            this.TabControl.Name = "TabControl";
            this.TabControl.SelectedIndex = 0;
            this.TabControl.Size = new System.Drawing.Size(1621, 804);
            this.TabControl.TabIndex = 0;
            // 
            // Export_new
            // 
            this.Export_new.Controls.Add(this.exportTab22);
            this.Export_new.Location = new System.Drawing.Point(8, 39);
            this.Export_new.Margin = new System.Windows.Forms.Padding(4);
            this.Export_new.Name = "Export_new";
            this.Export_new.Padding = new System.Windows.Forms.Padding(4);
            this.Export_new.Size = new System.Drawing.Size(1605, 757);
            this.Export_new.TabIndex = 3;
            this.Export_new.Text = "Экспорт PDF, STEP";
            this.Export_new.UseVisualStyleBackColor = true;
            // 
            // exportTab22
            // 
            this.exportTab22.AllowDrop = true;
            this.exportTab22.Dock = System.Windows.Forms.DockStyle.Fill;
            this.exportTab22.Location = new System.Drawing.Point(4, 4);
            this.exportTab22.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.exportTab22.Name = "exportTab22";
            this.exportTab22.Size = new System.Drawing.Size(1597, 749);
            this.exportTab22.TabIndex = 0;
            // 
            // TableTab
            // 
            this.TableTab.AccessibleDescription = "";
            this.TableTab.AccessibleName = "";
            this.TableTab.Controls.Add(this.tableTab1);
            this.TableTab.Location = new System.Drawing.Point(8, 39);
            this.TableTab.Margin = new System.Windows.Forms.Padding(4);
            this.TableTab.Name = "TableTab";
            this.TableTab.Padding = new System.Windows.Forms.Padding(4);
            this.TableTab.Size = new System.Drawing.Size(1605, 757);
            this.TableTab.TabIndex = 1;
            this.TableTab.Text = "Редактор обозначений";
            this.TableTab.UseVisualStyleBackColor = true;
            // 
            // tableTab1
            // 
            this.tableTab1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableTab1.Location = new System.Drawing.Point(4, 4);
            this.tableTab1.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.tableTab1.Name = "tableTab1";
            this.tableTab1.Size = new System.Drawing.Size(1597, 749);
            this.tableTab1.TabIndex = 0;
            // 
            // BOM
            // 
            this.BOM.AccessibleDescription = "BOM";
            this.BOM.AccessibleName = "BOM";
            this.BOM.Controls.Add(this.graphTab1);
            this.BOM.Location = new System.Drawing.Point(8, 39);
            this.BOM.Margin = new System.Windows.Forms.Padding(4);
            this.BOM.Name = "BOM";
            this.BOM.Padding = new System.Windows.Forms.Padding(4);
            this.BOM.Size = new System.Drawing.Size(1605, 757);
            this.BOM.TabIndex = 2;
            this.BOM.Text = "Доктор проекта";
            this.BOM.UseVisualStyleBackColor = true;
            // 
            // graphTab1
            // 
            this.graphTab1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.graphTab1.Location = new System.Drawing.Point(4, 4);
            this.graphTab1.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.graphTab1.Name = "graphTab1";
            this.graphTab1.Size = new System.Drawing.Size(1597, 749);
            this.graphTab1.TabIndex = 0;
            // 
            // help
            // 
            this.help.Controls.Add(this.textBox1);
            this.help.Location = new System.Drawing.Point(8, 39);
            this.help.Name = "help";
            this.help.Padding = new System.Windows.Forms.Padding(3);
            this.help.Size = new System.Drawing.Size(1605, 757);
            this.help.TabIndex = 4;
            this.help.Text = "Справка";
            this.help.UseVisualStyleBackColor = true;
            // 
            // textBox1
            // 
            this.textBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.textBox1.Location = new System.Drawing.Point(3, 3);
            this.textBox1.Multiline = true;
            this.textBox1.Name = "textBox1";
            this.textBox1.ReadOnly = true;
            this.textBox1.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBox1.Size = new System.Drawing.Size(1599, 751);
            this.textBox1.TabIndex = 0;
            this.textBox1.Text = resources.GetString("textBox1.Text");
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(12F, 25F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1621, 804);
            this.Controls.Add(this.TabControl);
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "MainForm";
            this.Text = "Form1";
            this.TabControl.ResumeLayout(false);
            this.Export_new.ResumeLayout(false);
            this.TableTab.ResumeLayout(false);
            this.BOM.ResumeLayout(false);
            this.help.ResumeLayout(false);
            this.help.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabControl TabControl;
        private System.Windows.Forms.TabPage TableTab;
        private System.Windows.Forms.TabPage BOM;
        private System.Windows.Forms.TabPage Export_new;

        private Tabs.ExportTab2 exportTab22;
        private Tabs.TableTab tableTab1;
        private Tabs.GraphTab graphTab1;
        private System.Windows.Forms.TabPage help;
        private System.Windows.Forms.TextBox textBox1;
    }
}