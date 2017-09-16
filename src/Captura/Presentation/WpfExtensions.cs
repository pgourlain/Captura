using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Input;

namespace Captura
{
    public static class WpfExtensions
    {
        public static void ShowAndFocus(this Window W)
        {
            W.Show();

            W.Activate();
        }

        #region Keyboard helper

        [DllImport("user32.dll")]
        static extern int ToUnicode(uint virtualKeyCode, uint scanCode, byte[] keyboardState,
            [Out, MarshalAs(UnmanagedType.LPWStr, SizeConst = 64)]
            StringBuilder receivingBuffer,
            int bufferSize, uint flags);

        static string GetCharsFromKeys(System.Windows.Forms.Keys keys, bool shift, bool altGr)
        {
            var buf = new StringBuilder(256);
            var keyboardState = new byte[256];
            if (shift)
                keyboardState[(int)System.Windows.Forms.Keys.ShiftKey] = 0xff;
            if (altGr)
            {
                keyboardState[(int)System.Windows.Forms.Keys.ControlKey] = 0xff;
                keyboardState[(int)System.Windows.Forms.Keys.Menu] = 0xff;
            }
            ToUnicode((uint)keys, 0, keyboardState, buf, 256, 0);
            return buf.ToString();
        }

        public static string getKeyAsString(this KeyEventArgs e)
        {
            var vk = System.Windows.Input.KeyInterop.VirtualKeyFromKey(e.Key);

            var shift = ((e.KeyboardDevice.Modifiers & ModifierKeys.Shift) == ModifierKeys.Shift);
            var altgr = (e.KeyboardDevice.Modifiers == (ModifierKeys.Alt | ModifierKeys.Control));
            return GetCharsFromKeys((System.Windows.Forms.Keys)vk, shift, altgr);
        }
        #endregion
    }
}
