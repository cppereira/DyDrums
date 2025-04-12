using System.Runtime.InteropServices;

namespace DyDrums.Views
{
    public class HHCVerticalProgressBar : ProgressBar
    {
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);

        private const int PBM_SETBARCOLOR = 0x0409;
        private const int PBS_VERTICAL = 0x04;

        public Color BarColor { get; set; } = Color.Aquamarine;

        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                cp.Style |= PBS_VERTICAL;
                return cp;
            }
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            SetBarColor(BarColor);
        }

        private void SetBarColor(Color color)
        {
            // Só aceita cores "simples", como Red, Green, Blue. Gradiente ou ARGB não funcionam.
            SendMessage(Handle, PBM_SETBARCOLOR, IntPtr.Zero, (IntPtr)ColorTranslator.ToWin32(color));
        }
    }
}
