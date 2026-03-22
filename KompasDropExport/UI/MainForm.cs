using System.Drawing;
using System.Windows.Forms;

namespace KompasDropExport.UI
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            // базовые настройки
            this.AutoScaleMode = AutoScaleMode.Dpi;
            this.AutoScaleDimensions = new SizeF(96f, 96f);
            this.Font = SystemFonts.MessageBoxFont;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.DoubleBuffered = true;
            this.Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);

            InitializeComponent();

            this.Text = "Funny KOMPAS: Drag&Drop Export, Rename & Doctor ver. 0.95";
        }


    }
}
