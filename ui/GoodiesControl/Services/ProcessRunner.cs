using System.Diagnostics;
using System.Text;

namespace GoodiesControl.Services
{
    internal record ProcessResult(int ExitCode, string StandardOutput, string StandardError);

    internal static class ProcessRunner
    {
        public static async Task<ProcessResult> RunAsync(string fileName, string? arguments = null, int timeoutMilliseconds = 30000)
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = fileName,
                Arguments = arguments ?? string.Empty,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            };

            var stdout = new StringBuilder();
            var stderr = new StringBuilder();

            using var process = new Process { StartInfo = startInfo, EnableRaisingEvents = true };
            var stdoutTaskCompletion = new TaskCompletionSource<bool>();
            var stderrTaskCompletion = new TaskCompletionSource<bool>();

            process.OutputDataReceived += (_, e) =>
            {
                if (e.Data == null)
                {
                    stdoutTaskCompletion.TrySetResult(true);
                }
                else
                {
                    stdout.AppendLine(e.Data);
                }
            };

            process.ErrorDataReceived += (_, e) =>
            {
                if (e.Data == null)
                {
                    stderrTaskCompletion.TrySetResult(true);
                }
                else
                {
                    stderr.AppendLine(e.Data);
                }
            };

            if (!process.Start())
            {
                throw new InvalidOperationException($"无法启动进程: {fileName}");
            }

            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            using var cts = new CancellationTokenSource(timeoutMilliseconds);
            var waitForExit = Task.Run(() => process.WaitForExit());
            var completed = await Task.WhenAny(waitForExit, Task.Delay(Timeout.Infinite, cts.Token));
            if (completed != waitForExit)
            {
                try
                {
                    process.Kill(true);
                }
                catch
                {
                }
                throw new TimeoutException($"操作超时 ({timeoutMilliseconds} ms): {fileName} {arguments}");
            }

            await Task.WhenAll(stdoutTaskCompletion.Task, stderrTaskCompletion.Task);
            return new ProcessResult(process.ExitCode, stdout.ToString().Trim(), stderr.ToString().Trim());
        }
    }
}
