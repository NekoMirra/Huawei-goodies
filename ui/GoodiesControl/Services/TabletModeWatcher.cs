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

        public TabletModeWatcher()
        {
            _timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(2)
            };
            _timer.Tick += (_, __) => Poll();
        }

        public void Start()
        {
            _lastIsTablet = IsTabletMode();
            _timer.Start();
        }

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