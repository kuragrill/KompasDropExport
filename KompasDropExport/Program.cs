using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace KompasDropExport
{
    internal static class Program
    {
        // user32.dll (Windows 10): самый новый способ
        [DllImport("user32.dll")]
        static extern bool SetProcessDpiAwarenessContext(IntPtr dpiContext);
        // shcore.dll (Windows 8.1): средний способ
        [DllImport("shcore.dll")]
        static extern int SetProcessDpiAwareness(int value); // 0=Unaware, 1=System, 2=PerMonitor
        // user32.dll (Vista): старый способ
        [DllImport("user32.dll")]
        static extern bool SetProcessDPIAware();

        // Константы для SetProcessDpiAwarenessContext
        static readonly IntPtr DPI_AWARENESS_CONTEXT_PER_MONITOR_AWARE_V2 = (IntPtr)(-4);
        static readonly IntPtr DPI_AWARENESS_CONTEXT_PER_MONITOR_AWARE = (IntPtr)(-3);
        static readonly IntPtr DPI_AWARENESS_CONTEXT_SYSTEM_AWARE = (IntPtr)(-2);

        [STAThread]
        static void Main()
        {
            // 1) Форсим DPI-осведомлённость ДО создания каких-либо окон/контролов
            TryEnablePerMonitorV2Dpi();

            // 2) Оставляем обычные WinForms штуки
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Никакого SetHighDpiMode тут не нужно
            Application.Run(new UI.MainForm());
        }

        static void TryEnablePerMonitorV2Dpi()
        {
            // Порядок: v2 -> v1 -> shcore -> legacy
            try
            {
                if (SetProcessDpiAwarenessContext(DPI_AWARENESS_CONTEXT_PER_MONITOR_AWARE_V2))
                    return;
            }
            catch { /* нет функции на старой ОС */ }

            try
            {
                if (SetProcessDpiAwarenessContext(DPI_AWARENESS_CONTEXT_PER_MONITOR_AWARE))
                    return;
            }
            catch { }

            try
            {
                // 2 = PerMonitor
                if (SetProcessDpiAwareness(2) == 0)
                    return;
            }
            catch { }

            try
            {
                SetProcessDPIAware(); // хотя бы System DPI Aware
            }
            catch { }
        }
    }
}
