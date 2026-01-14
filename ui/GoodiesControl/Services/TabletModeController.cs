using Microsoft.Win32;
using System;
using System.Runtime.InteropServices;

namespace GoodiesControl.Services
{
    internal class TabletModeController
    {
        public void SetTabletMode(bool enable)
        {
            try
            {
                using var key = Registry.CurrentUser.CreateSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\ImmersiveShell");
                key?.SetValue("TabletMode", enable ? 1 : 0, RegistryValueKind.DWord);
                BroadcastSettingChange();
            }
            catch
            {
                // ignore errors; caller handles messaging
            }
        }

        private static void BroadcastSettingChange()
        {
            NativeMethods.SendMessageTimeout(new IntPtr(NativeMethods.HWND_BROADCAST), NativeMethods.WM_SETTINGCHANGE, IntPtr.Zero, "ImmersiveShell", NativeMethods.SendMessageTimeoutFlags.SMTO_ABORTIFHUNG, 100, out _);
        }

        private static class NativeMethods
        {
            public const int HWND_BROADCAST = 0xFFFF;
            public const int WM_SETTINGCHANGE = 0x1A;

            [Flags]
            public enum SendMessageTimeoutFlags : uint
            {
                SMTO_NORMAL = 0x0,
                SMTO_BLOCK = 0x1,
                SMTO_ABORTIFHUNG = 0x2,
                SMTO_NOTIMEOUTIFNOTHUNG = 0x8
            }

            [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
            public static extern IntPtr SendMessageTimeout(IntPtr hWnd, int Msg, IntPtr wParam, string lParam, SendMessageTimeoutFlags fuFlags, uint uTimeout, out IntPtr lpdwResult);
        }
    }
}