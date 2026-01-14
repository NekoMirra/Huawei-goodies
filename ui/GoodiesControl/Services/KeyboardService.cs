namespace GoodiesControl.Services
{
    internal enum KeyboardDetachState
    {
        Unknown,
        Enabled,
        Disabled
    }

    internal class KeyboardService
    {
        private readonly ToolLocator _toolLocator = new("kbd-detach.exe", "KeyboardService.dll");

        public async Task<KeyboardDetachState> QueryAsync()
        {
            var exe = _toolLocator.FindExecutable();
            var result = await ProcessRunner.RunAsync(exe, "-q");
            // kbd-detach 返回 0 表示 enabled，1 表示 disabled
            return result.ExitCode switch
            {
                0 => KeyboardDetachState.Enabled,
                1 => KeyboardDetachState.Disabled,
                _ => throw new InvalidOperationException($"查询失败: {result.StandardError} {result.StandardOutput}")
            };
        }

        public async Task SetAsync(bool enable)
        {
            var exe = _toolLocator.FindExecutable();
            var args = enable ? "enable" : "disable";
            var result = await ProcessRunner.RunAsync(exe, args);
            if (result.ExitCode != 0)
            {
                throw new InvalidOperationException($"设置失败: {result.StandardError} {result.StandardOutput}");
            }
        }
    }
}
