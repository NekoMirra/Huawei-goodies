# Matebook E Go Goodies

Alternative to Huawei PC Manager

## Usage

```text
Usage: kbd-detach [-q]
  Queries current status
  -q: Use exit status instead of stdout

Usage: kbd-detach [enable|disable]
  Enables or disables detached keyboard support
```

```text
Description:
  QDCM loader for Gaokun

Usage:
  qdcm-loader [options]

Options:
  --preset <DisplayP3|sRGB>  Load factory calibration
  --reset                    Reset display color
  --igc <file.cube>          Load custom input shaper (3x1D LUT applied before 3D LUT)
  --3dlut <file.cube>        Load custom 3D LUT
  -?, -h, --help             Show help and usage information

Remarks:
  Both IRIDAS and Resolve .cube format are supported.
```

```console
PS C:\path\to\goodies>.\Set-ChargeLimit.ps1 -PercentageLimit 80
```

## Notes

Both kbd-detach and qdcm-loader depend on amd64 DLLs and are built targeting amd64/arm64ec.

历史说明：此前没有 GUI，原因是 arm64ec 生态支持有限；现新增 WPF UI，仍需要对应架构的原生 DLL。

## UI（MatebookGoodies.exe）

`ui/GoodiesControl` 提供一个轻量 WPF 前端，调用现有组件：

- 键盘拆离：包装 `kbd-detach.exe`，可查询/启用/禁用。
- 屏幕校色：包装 `qdcm-loader.exe`，支持官方预设、重置与自定义 LUT (`--igc` / `--3dlut`)。
- 充电上限：直接使用 WMI 等效于 `Set-ChargeLimit.ps1`。

> Windows on Arm 仍需要对应架构的原生 DLL（`KeyboardService.dll`、`qdcmlib.dll`）。UI 发布单文件时会优先使用 `tools/win-arm64`，缺失时回退 x64。
