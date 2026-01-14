# 构建说明（arm64 轻量版）

## 前置依赖

- Windows 10/11 on Arm（可兼容 x64 模拟）。
- .NET 8 SDK（含 `dotnet` 命令）。
- Visual Studio 2022（或 Build Tools）安装以下组件：
  - MSVC v143（包含 ARM64EC 工具集）。
  - Windows 11 SDK。
  - C++ CMake/工具链（可选，用于 nmake）。
- `git` 可选，用于拉取仓库。

## 目录约定

- UI 工程：`ui/GoodiesControl`。
- 工具输出目录（供 UI 运行时调用）：
  - `ui/GoodiesControl/tools/win-arm64`
  - `ui/GoodiesControl/tools/win-x64`

## 构建 kbd-detach（arm64ec/x64）

1. 打开 *x64 Native Tools Command Prompt for VS 2022*（该环境会设置 `VSCMD_ARG_TGT_ARCH`）。
2. 进入源码目录并编译：

  ```cmd
  cd kbd-detach
  nmake /f makefile clean
  nmake /f makefile
  ```

1. 将生成的 `kbd-detach.exe` 与 `KeyboardService.dll` 复制到：
   - Arm64EC：`ui/GoodiesControl/tools/win-arm64/`
   - x64：`ui/GoodiesControl/tools/win-x64/`

> makefile 对 `VSCMD_ARG_TGT_ARCH=arm64` 会启用 `/arm64EC`，以便在 Arm64 系统调用 amd64 驱动 DLL。

## 构建 qdcm-loader（arm64 与 x64）

在常规 PowerShell/命令行执行：

```powershell
cd qdcm-loader
# Arm64 版
dotnet publish -c Release -r win-arm64 -o ..\ui\GoodiesControl\tools\win-arm64
# x64 版（保留 AOT）
dotnet publish -c Release -r win-x64 -p:PublishAot=true -o ..\ui\GoodiesControl\tools\win-x64
```

> 输出目录会包含 `qdcm-loader.exe` 与 `qdcmlib.dll`（已在项目中标记复制）。

## 构建 UI 单文件可执行（arm64）

```powershell
cd ui/GoodiesControl
# 发布单文件、自包含 arm64 版
dotnet publish -c Release -r win-arm64 -p:PublishSingleFile=true -p:SelfContained=true -o ..\..\artifacts\ui-arm64
```

- 发布结果：`artifacts/ui-arm64/MatebookGoodies.exe`。
- 运行时会自动选择 `tools/win-arm64`（若不存在则回退 x64）。

## 运行

直接双击发布的 `MatebookGoodies.exe` 即可。若缺少依赖文件，应用会提示需要的可执行或 DLL 名称。

## 常见问题

- **未找到 .NET SDK**：安装 .NET 8 SDK 后重新打开终端。
- **qdcm 或 kbd 调用失败**：确认对应架构的 DLL (`qdcmlib.dll` / `KeyboardService.dll`) 与可执行位于 `tools/<rid>/`。
- **WMI 权限问题**：以管理员启动可减少失败概率。
