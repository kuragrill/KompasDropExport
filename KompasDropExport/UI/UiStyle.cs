using System;
using System.Drawing;
using System.Windows.Forms;

namespace KompasDropExport.UI
{
    internal static class UiStyle
    {
        // Базовые цвета: мягкие, низкий контраст
        public static readonly Color BtnBack = Color.FromArgb(242, 247, 252);      // почти белый с холодным тоном
        public static readonly Color BtnHover = Color.FromArgb(212, 232, 252);     // чуть темнее
        public static readonly Color BtnDown = Color.DeepSkyBlue;      // ещё чуть темнее

        public static readonly Color BtnBorder = Color.FromArgb(190, 205, 220);    // спокойная рамка
        public static readonly Color Text = Color.Black;

        public static void ApplySoftButton(Button b)
        {
            b.FlatStyle = FlatStyle.Flat;
            b.UseVisualStyleBackColor = false;

            b.BackColor = BtnBack;
            b.ForeColor = Text;

            b.FlatAppearance.BorderSize = 1;
            b.FlatAppearance.BorderColor = BtnBorder;

            // чтобы не было “пластилина”
            b.FlatAppearance.MouseOverBackColor = BtnHover;
            b.FlatAppearance.MouseDownBackColor = BtnDown;

            // опционально: одинаковая высота/отступы
            b.Padding = new Padding(2, 2, 2, 2);
        }

        public static void ApplyPanel(Control c)
        {
            c.BackColor = Color.WhiteSmoke;
        }

        public static void ApplyCard(Control c)
        {
            c.BackColor = Color.White;
        }
    }
}