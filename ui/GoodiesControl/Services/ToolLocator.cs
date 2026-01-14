using System.IO;
using System.Runtime.InteropServices;

namespace GoodiesControl.Services
{
    internal class ToolLocator
    {
        private readonly string _executable;
        private readonly string[] _requiredFiles;

        public ToolLocator(string executable, params string[] requiredFiles)
        {
            _executable = executable;
            _requiredFiles = requiredFiles;
        }

        public string FindExecutable()
        {
            var baseDir = AppContext.BaseDirectory;
            var ridOrder = RuntimeInformation.OSArchitecture == Architecture.Arm64
                ? new[] { "win-arm64", "win-x64" }
                : new[] { "win-x64", "win-arm64" };

            foreach (var rid in ridOrder)
            {
                var candidate = Path.Combine(baseDir, "tools", rid, _executable);
                if (File.Exists(candidate) && DependenciesPresent(candidate))
                {
                    return candidate;
                }
            }

            var fallback = Path.Combine(baseDir, _executable);
            if (File.Exists(fallback) && DependenciesPresent(fallback))
            {
                return fallback;
            }

            throw new FileNotFoundException($"找不到 {_executable}，请确保 tools/<rid>/ 下包含可执行文件及依赖。");
        }

        private bool DependenciesPresent(string exePath)
        {
            if (_requiredFiles.Length == 0)
            {
                return true;
            }

            var root = Path.GetDirectoryName(exePath)!;
            foreach (var dep in _requiredFiles)
            {
                if (!File.Exists(Path.Combine(root, dep)))
                {
                    return false;
                }
            }
            return true;
        }
    }
}
