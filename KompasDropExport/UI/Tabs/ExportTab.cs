using System;
using System.Windows.Forms;

namespace KompasDropExport.UI.Tabs
{
    public partial class ExportTab : UserControl
    {
        private Form1 _form;

        public ExportTab()
        {
            InitializeComponent();
            this.Load += ExportTab_Load;
        }

        private void ExportTab_Load(object sender, EventArgs e)
        {
            if (_form != null) return;

            _form = new Form1();
            _form.TopLevel = false;
            _form.FormBorderStyle = FormBorderStyle.None;
            _form.Dock = DockStyle.Fill;

            this.Controls.Add(_form);
            _form.Show();
        }
    }
}