using System;
using System.Windows.Threading;
using Microsoft.Win32;

namespace GoodiesControl.Services
{
    internal sealed class TabletModeWatcher : IDisposable
    {
        private readonly DispatcherTimer _timer;
        private bool _lastIsTablet;

        public event EventHandler<bool>? TabletModeChanged;

        public bool Current => _lastIsTablet;

        public TabletModeWatcher()
        {
            _timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(2)
            };
            _timer.Tick += (_, __) => Poll();
            _lastIsTablet = IsTabletMode();
        }

        public void Start()
        {
            _timer.Start();
        }

        public static bool GetCurrent() => IsTabletMode();

        public void Dispose()
        {
            _timer.Stop();
        }

        private void Poll()
        {
            var current = IsTabletMode();
            if (current != _lastIsTablet)
            {
                _lastIsTablet = current;
                TabletModeChanged?.Invoke(this, current);
            }
        }

        private static bool IsTabletMode()
        {
            try
            {
                using var key = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\ImmersiveShell");
                if (key?.GetValue("TabletMode") is int v)
                {
                    return v == 1;
                }
            }
            catch
            {
            }
            return false;
        }
    }
}