// NiceProgressBar.cs
using System;
using System.Drawing;
using System.Windows.Forms;

namespace KompasDropExport.UI
{
    public class NiceProgressBar : Control
    {
        int _minimum = 0;
        int _maximum = 100;
        int _value = 0;

        public Color BarColor { get; set; } = Color.DeepSkyBlue;
        public Color BackBarColor { get; set; } = Color.LightGray;
        public Color BorderColor { get; set; } = Color.Gray;
        public Color TextColor { get; set; } = Color.Black;

        public int Minimum
        {
            get => _minimum;
            set
            {
                if (value > _maximum) _maximum = value;
                _minimum = value;
                if (_value < _minimum) _value = _minimum;
                Invalidate();
            }
        }

        public int Maximum
        {
            get => _maximum;
            set
            {
                if (value < _minimum) _minimum = value;
                _maximum = value;
                if (_value > _maximum) _value = _maximum;
                Invalidate();
            }
        }

        public int Value
        {
            get => _value;
            set
            {
                int v = value;
                if (v < _minimum) v = _minimum;
                if (v > _maximum) v = _maximum;
                _value = v;
                Invalidate();
            }
        }

        public NiceProgressBar()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.UserPaint |
                     ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.ResizeRedraw, true);

            this.Height = 40;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            var g = e.Graphics;

            Rectangle rect = this.ClientRectangle;
            if (rect.Width <= 2 || rect.Height <= 2) return;

            // фон бара
            using (var backBrush = new SolidBrush(BackBarColor))
                g.FillRectangle(backBrush, rect);

            // заполненная часть
            float range = _maximum - _minimum;
            float percent = range <= 0 ? 0f : (_value - _minimum) / range;
            if (percent < 0f) percent = 0f;
            if (percent > 1f) percent = 1f;

            int fillWidth = (int)(rect.Width * percent);
            if (fillWidth > 0)
            {
                Rectangle fillRect = new Rectangle(rect.X, rect.Y, fillWidth, rect.Height);
                using (var barBrush = new SolidBrush(BarColor))
                    g.FillRectangle(barBrush, fillRect);
            }

            // рамка
            using (var pen = new Pen(BorderColor))
                g.DrawRectangle(pen, rect.X, rect.Y, rect.Width - 1, rect.Height - 1);

            // текст вида "42 %"
            int percText = (int)Math.Round(percent * 100.0);
            string text = percText.ToString() + " %";

            using (var textBrush = new SolidBrush(TextColor))
            using (var sf = new StringFormat
            {
                Alignment = StringAlignment.Center,
                LineAlignment = StringAlignment.Center
            })
            {
                g.DrawString(text, this.Font, textBrush, rect, sf);
            }
        }
    }
}
