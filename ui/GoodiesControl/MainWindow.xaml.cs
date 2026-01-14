using GoodiesControl.Services;
using Microsoft.Win32;
using System.Windows;
using System.Windows.Controls;

namespace GoodiesControl
{
    public partial class MainWindow : Window
    {
        private readonly KeyboardService _keyboardService = new();
        private readonly QdcmCliService _qdcmService = new();
        private readonly ChargeLimitService _chargeService = new();

        public MainWindow()
        {
            InitializeComponent();
            ChargeSlider.ValueChanged += (_, __) => ChargeValueText.Text = $"{(int)ChargeSlider.Value}%";
            Loaded += MainWindow_Loaded;
        }

        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            await QueryKeyboardStateAsync();
        }

        private async void QueryKeyboardButton_Click(object sender, RoutedEventArgs e)
        {
            await QueryKeyboardStateAsync();
        }

        private Task QueryKeyboardStateAsync()
        {
            return GuardAsync(async () =>
            {
                KeyboardStatusText.Text = "查询中...";
                var state = await _keyboardService.QueryAsync();
                KeyboardStatusText.Text = state switch
                {
                    KeyboardDetachState.Enabled => "当前：已启用",
                    KeyboardDetachState.Disabled => "当前：已禁用",
                    _ => "未知"
                };
            }, KeyboardStatusText);
        }

        private async void EnableKeyboardButton_Click(object sender, RoutedEventArgs e)
        {
            await GuardAsync(async () =>
            {
                KeyboardStatusText.Text = "正在启用...";
                await _keyboardService.SetAsync(true);
                KeyboardStatusText.Text = "已启用";
            }, KeyboardStatusText);
        }

        private async void DisableKeyboardButton_Click(object sender, RoutedEventArgs e)
        {
            await GuardAsync(async () =>
            {
                KeyboardStatusText.Text = "正在禁用...";
                await _keyboardService.SetAsync(false);
                KeyboardStatusText.Text = "已禁用";
            }, KeyboardStatusText);
        }

        private async void ApplyP3Button_Click(object sender, RoutedEventArgs e)
        {
            await RunQdcmAsync(() => _qdcmService.ApplyPresetAsync("DisplayP3"), "Display P3 已应用");
        }

        private async void ApplySrgbButton_Click(object sender, RoutedEventArgs e)
        {
            await RunQdcmAsync(() => _qdcmService.ApplyPresetAsync("sRGB"), "sRGB 已应用");
        }

        private async void ResetDisplayButton_Click(object sender, RoutedEventArgs e)
        {
            await RunQdcmAsync(() => _qdcmService.ResetAsync(), "已重置为出厂校色");
        }

        private void BrowseIgcButton_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new OpenFileDialog
            {
                Filter = "cube LUT (*.cube)|*.cube|所有文件 (*.*)|*.*"
            };
            if (dlg.ShowDialog() == true)
            {
                IgcPathBox.Text = dlg.FileName;
            }
        }

        private void BrowseLutButton_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new OpenFileDialog
            {
                Filter = "cube LUT (*.cube)|*.cube|所有文件 (*.*)|*.*"
            };
            if (dlg.ShowDialog() == true)
            {
                Lut3DPathBox.Text = dlg.FileName;
            }
        }

        private async void ApplyCustomLutButton_Click(object sender, RoutedEventArgs e)
        {
            await RunQdcmAsync(() => _qdcmService.ApplyCustomAsync(IgcPathBox.Text, Lut3DPathBox.Text), "自定义 LUT 已应用");
        }

        private async void ApplyChargeButton_Click(object sender, RoutedEventArgs e)
        {
            await GuardAsync(async () =>
            {
                var value = (int)ChargeSlider.Value;
                ChargeStatusText.Text = "正在设置...";
                await _chargeService.SetLimitAsync(value);
                ChargeStatusText.Text = $"已设置为 {value}%";
            }, ChargeStatusText);
        }

        private async Task RunQdcmAsync(Func<Task> action, string successText)
        {
            await GuardAsync(async () =>
            {
                QdcmStatusText.Text = "执行中...";
                await action();
                QdcmStatusText.Text = successText;
            }, QdcmStatusText);
        }

        private async Task GuardAsync(Func<Task> action, FrameworkElement busyScope)
        {
            try
            {
                SetBusy(true, busyScope);
                await action();
            }
            catch (Exception ex)
            {
                if (busyScope is TextBlock tb)
                {
                    tb.Text = ex.Message;
                }
                MessageBox.Show(ex.Message, "操作失败", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                SetBusy(false, busyScope);
            }
        }

        private void SetBusy(bool isBusy, FrameworkElement scope)
        {
            QueryKeyboardButton.IsEnabled = !isBusy;
            EnableKeyboardButton.IsEnabled = !isBusy;
            DisableKeyboardButton.IsEnabled = !isBusy;
            ApplyP3Button.IsEnabled = !isBusy;
            ApplySrgbButton.IsEnabled = !isBusy;
            ResetDisplayButton.IsEnabled = !isBusy;
            ApplyCustomLutButton.IsEnabled = !isBusy;
            ApplyChargeButton.IsEnabled = !isBusy;
            scope.IsEnabled = !isBusy;
        }
    }
}
