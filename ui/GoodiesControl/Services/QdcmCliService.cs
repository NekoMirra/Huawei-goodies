namespace GoodiesControl.Services
{
    internal class QdcmCliService
    {
        private readonly ToolLocator _toolLocator = new("qdcm-loader.exe", "qdcmlib.dll");

        public Task ApplyPresetAsync(string preset)
        {
            return RunAsync($"--preset {preset}");
        }

        public Task ResetAsync()
        {
            return RunAsync("--reset");
        }

        public Task ApplyCustomAsync(string? igcPath, string? lut3dPath)
        {
            var args = new List<string>();
            if (!string.IsNullOrWhiteSpace(igcPath))
            {
                args.Add($"--igc \"{igcPath}\"");
            }
            if (!string.IsNullOrWhiteSpace(lut3dPath))
            {
                args.Add($"--3dlut \"{lut3dPath}\"");
            }
            if (args.Count == 0)
            {
                throw new InvalidOperationException("请至少选择一个 LUT 文件。");
            }
            return RunAsync(string.Join(' ', args));
        }

        private async Task RunAsync(string arguments)
        {
            var exe = _toolLocator.FindExecutable();
            var result = await ProcessRunner.RunAsync(exe, arguments);
            if (result.ExitCode != 0)
            {
                var message = string.IsNullOrWhiteSpace(result.StandardError) ? result.StandardOutput : result.StandardError;
                throw new InvalidOperationException(string.IsNullOrWhiteSpace(message) ? "qdcm-loader 运行失败" : message);
            }
        }
    }
}
