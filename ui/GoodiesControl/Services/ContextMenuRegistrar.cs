using Microsoft.Win32;
using System.Diagnostics;

namespace GoodiesControl.Services
{
    internal static class ContextMenuRegistrar
    {
        private const string MenuName = "扩展设置";
        private const string SubKeyName = "ExtensionSettings";

        /// <summary>
        /// 在当前用户注册右键菜单（桌面空白处、文件夹、任意文件）。
        /// </summary>
        public static void EnsureRegistered()
        {
            var exePath = Process.GetCurrentProcess().MainModule?.FileName;
            if (string.IsNullOrWhiteSpace(exePath)) return;

            RegisterFor("Directory\\Background", exePath);
            RegisterFor("Directory", exePath);
            RegisterFor("*", exePath);
        }

        private static void RegisterFor(string scope, string exePath)
        {
            var shellPath = $"Software\\Classes\\{scope}\\shell\\{SubKeyName}";
            using var key = Registry.CurrentUser.CreateSubKey(shellPath);
            if (key == null) return;
            key.SetValue(null, MenuName);
            key.SetValue("Icon", exePath);

            using var commandKey = key.CreateSubKey("command");
            if (commandKey == null) return;
            var existing = commandKey.GetValue(null) as string;
            var command = $"\"{exePath}\"";
            if (!string.Equals(existing, command, StringComparison.OrdinalIgnoreCase))
            {
                commandKey.SetValue(null, command);
            }
        }
    }
}