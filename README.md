# SunnyNet WPF

SunnyNet WPF 是基于 SunnyNet Go 核心的 Windows 抓包分析工具。项目使用 `C# + WPF` 重绘桌面界面，核心抓包、代理、脚本、证书与协议处理仍由 Go 后端提供，并以 `c-shared DLL` 方式供 WPF 直接调用。

仓库地址：https://github.com/hmlyn/SunnyNet-wpf

作者：`hmlyn`

仓库描述：

> SunnyNet WPF 是基于 SunnyNet Go 核心的 Windows 抓包分析工具，使用 C# + WPF 重绘界面，支持 HTTP/HTTPS、WebSocket、TCP/UDP、断点拦截、重放、请求规则、请求构造器、MCP 与常用调试工具。

## 功能概览

- Windows x64 桌面应用，界面层使用 WPF，运行产物为 `SunnyNet.exe`。
- Go 后端编译为 `backend/SunnyNetBridge.dll`，WPF 通过 P/Invoke 直接调用 SunnyNet 核心能力。
- 仓库已随项目分发 SunnyNet 核心兼容源码，后端通过本地 `replace` 引用，不再依赖额外手动准备核心源码。
- 支持 HTTP/HTTPS、WebSocket、TCP、UDP 会话捕获、筛选、查看和分析。
- 支持进程筛选、域名筛选、会话收藏、备注、标记颜色、列宽记忆和窗口布局记忆。
- 支持会话比较视图，可拖入两条会话对比请求/响应差异，并显示哈希摘要。
- 支持请求/响应 XML 视图，内容匹配 XML 时自动显示，避免无关视图占用空间。
- 支持 TLS 指纹视图，可查看 JA3、JA3N、JA4、JA4-R、Akamai 等指纹信息。
- 支持断点拦截、普通重发、运行到结束、断开连接，以及请求/响应内容修改。
- 支持请求规则中心，包括请求断点、HTTP/WebSocket/TCP/UDP 屏蔽、请求重写、请求映射。
- 支持请求构造器，可自行构造 GET/POST 等请求，支持协议头/Body 多视图编辑、从会话列表回填重放和历史记录缓存。
- 支持会话右键复制请求代码，包含 C#、Java、Go、Python、JavaScript、易语言、火山等模板。
- 结构视图可按搜索结果、请求方法、进程、域名分类筛选。
- 支持全局查找与请求/响应文本高亮，查找窗口命中后可联动右侧详情视图。
- 支持 WebSocket 消息流、仅显示发送/接收、始终按文本预览，以及主动发送、消息重放、HEX/原文/JSON/Protobuf 视图。
- 支持 TCP/UDP 专用数据视图，避免复用 HTTP/WebSocket 视图造成信息冗余。
- 支持脚本扩展，可在脚本中处理、解密或转换数据，并写入显示层明文，不破坏原始请求/响应数据。
- 内置文本工具、JSON 结构编辑器、加密/解密工具、开源协议与赞赏页面。
- 文本工具支持 Base64/Base64URL、URL、UCS2、常用压缩解压、文本差异、文本去重、大小写转换、时间戳转换等。
- 加密/解密工具支持 AES/DES/RSA、常用哈希、HMAC，并支持文本/Base64/HEX/Base64URL 输入输出。
- 支持 GitHub Release 检查更新，可自动检测新版本并打开发布页。
- 支持 SunnyNet MCP，内置于 `SunnyNet.exe`，默认开启。AI 会话可读取、搜索、标记、打开、收藏、备注、查看 TLS 指纹和分析抓包会话。
- 底栏可开关 MCP 与云函数抓包，右键 MCP/云函数可查看状态或日志。

## 最新版本

当前发布版本：`v0.1.7`

发布包命名：

```text
SunnyNet-wpf-v0.1.7-win-x64.zip
SunnyNet-wpf-v0.1.7-win-x64-self-contained.zip
```

Release 包含 Windows x64 可运行程序、WPF 主程序、Go 后端 DLL 与运行所需资源。MCP 已内置到 `SunnyNet.exe`，不再依赖单独的 `sunnynet-mcp.exe`。普通包需要本机安装 .NET Desktop Runtime；自包含包内置运行时，不依赖用户机器上的 .NET 版本。

## 本次核心更新

- MCP 内置到主程序，默认启动；Cursor / Claude 配置 `SunnyNet.exe --mcp -port 29999` 即可。
- 主程序未打开时，`--mcp` 会自动拉起界面；也可用 `app_open` 激活主窗口。
- 界面 `#` 序号就是 `theology`。MCP 可列出、筛选、打开已标记会话。
- 结构视图增加请求方法分类和搜索结果分类。
- WebSocket 消息可只看发送或接收，乱码也会按文本显示。
- 请求重写新增响应 JSON 键值改写，支持按路径设置或删除嵌套字段，例如 `Data.User.IsInsider=true`、`Data.Membership.Role=7`。
- JSON 响应重写采用路径级替换，尽量保留原始响应结构，并自动修正响应长度、压缩等相关协议头。
- 规则中心新增/编辑规则窗口改为普通窗口模式，编辑规则时可继续点击主窗口或其它工具窗口查看上下文。

## 目录结构

- `backend/`：Go 后端桥接层，编译为 `backend/SunnyNetBridge.dll`。
- `third_party/SunnyNet/`：随仓库分发的 SunnyNet Go 核心兼容源码。
- `src/SunnyNet.Wpf/`：WPF 桌面程序源码。
- `docs/`：项目文档，例如 MCP 工具清单。
- `_toolchain/`：可选本地工具链目录，例如 Zig。
- `build-debug.bat`：Debug 构建脚本，输出到 `artifacts/Debug/`。
- `build-release.bat`：Release 构建脚本，输出到 `artifacts/Release/`，并移除 pdb。

## 编译环境

必须环境：

- Windows 10/11 x64
- .NET SDK 8.0+
- Go 1.24.x
- Zig 0.15.2 或兼容版本，用于让 Go 生成 Windows 可加载的 `c-shared DLL`
- Git

推荐环境：

- Visual Studio 2022，用于打开、调试 WPF 项目。
- PowerShell 7 或 Windows PowerShell，用于运行构建脚本。

运行环境：

- 普通发布包 `SunnyNet-wpf-v*-win-x64.zip` 需要安装 .NET Desktop Runtime 8.0 或更高主版本。程序已启用 `RollForward=LatestMajor`，如果本机没有 .NET 8 Desktop Runtime，但存在兼容的更高主版本 Desktop Runtime，也允许继续启动。
- 自包含发布包 `SunnyNet-wpf-v*-win-x64-self-contained.zip` 已内置 .NET Desktop Runtime，用户无需单独安装 .NET，适合直接分发给普通用户。

## SunnyNet 核心分发说明

项目已随仓库分发 SunnyNet Go 核心兼容源码：

```text
third_party/SunnyNet
```

当前 `backend/go.mod` 使用本地 replace：

```text
replace github.com/qtgolang/SunnyNet => ../third_party/SunnyNet
```

这样首次拉取仓库后即可直接编译后端，也方便项目维护必要的核心层补丁。后续如果 SunnyNet 核心需要增强或修复，建议在 `third_party/SunnyNet` 内维护兼容补丁，并同步验证 WPF 调用链。

## 获取源码

```powershell
git clone https://github.com/hmlyn/SunnyNet-wpf.git
cd SunnyNet-wpf
```

仓库当前不依赖 Git submodule，`third_party/SunnyNet` 已在仓库内分发。

## Zig 准备

两种方式任选一种。

方式一：安装 `zig` 并加入 `PATH`：

```powershell
zig version
```

方式二：将 Zig 解压到项目本地路径：

```text
_toolchain/zig-x86_64-windows-0.15.2/zig.exe
```

项目会优先使用本地 `_toolchain`，如果不存在则使用 `PATH` 中的 `zig`。

## MCP 配置

MCP 已内置，默认开启，不再需要单独的 `sunnynet-mcp.exe`。

Cursor / Claude Desktop 示例：

```json
{
  "mcpServers": {
    "sunnynet": {
      "command": "C:\\path\\to\\SunnyNet.exe",
      "args": ["--mcp", "-port", "29999"]
    }
  }
}
```

说明：

- `SunnyNet.exe` 不带 `--mcp` 时打开图形界面，并在 `http://127.0.0.1:29999/mcp` 提供 HTTP MCP。
- `SunnyNet.exe --mcp` 作为 Cursor 的 stdio 桥。界面没开时会自动拉起主程序。
- 底栏可关闭 MCP。关闭后工具不可用，重新打开即可。
- 界面 `#` 序号就是 `theology`。用户说「序号 12」时传 `theology=12`。
- 常用工具：`request_list`、`request_tags_list`、`request_open`、`request_get`、`app_open`。完整清单见 `docs/MCP工具清单.md`。

## 编译方式

Debug：

```bat
build-debug.bat
```

Release：

```bat
build-release.bat
```

也可以直接使用：

```powershell
dotnet build .\SunnyNet.sln -c Debug
dotnet build .\SunnyNet.sln -c Release
```

建议发布包使用 `build-release.bat`，因为该脚本会输出到固定目录，并删除 pdb 文件。

## 输出位置

使用脚本构建后：

```text
artifacts/Debug/SunnyNet.exe
artifacts/Release/SunnyNet.exe
```

使用 `dotnet build` 默认构建后：

```text
src/SunnyNet.Wpf/bin/Debug/net8.0-windows/SunnyNet.exe
src/SunnyNet.Wpf/bin/Release/net8.0-windows/SunnyNet.exe
```

运行时主程序会加载同目录下的 Go 后端 DLL：

```text
backend/SunnyNetBridge.dll
```

MCP 由 `SunnyNet.exe` 自身提供，不需要额外的 `mcp/sunnynet-mcp.exe`。

## 发布打包

推荐流程：

```bat
build-release.bat
```

脚本会生成两个输出目录：

```text
artifacts/Release/
artifacts/Release-self-contained/
```

并自动生成两个 zip：

```text
SunnyNet-wpf-v0.1.7-win-x64.zip
SunnyNet-wpf-v0.1.7-win-x64-self-contained.zip
```

普通包体积小，但依赖本机 .NET Desktop Runtime；自包含包体积更大，但不受用户机器已安装 .NET 版本影响。

## 常见问题

如果提示找不到 Zig：

- 确认 `zig version` 可以在 PowerShell 中正常执行。
- 或确认本地路径 `_toolchain/zig-x86_64-windows-0.15.2/zig.exe` 存在。

如果 Go 后端 DLL 没有生成：

- 确认已经安装 Go 1.24.x。
- 确认 Zig 可用。
- 确认 `third_party/SunnyNet` 目录存在。
- 重新运行 `build-debug.bat` 或 `build-release.bat`。

如果 Cursor 连不上 MCP：

- 确认配置是 `SunnyNet.exe --mcp -port 29999`，不要只写 exe 且 `args` 为空。
- 确认配置里没有 `disabled: true`。
- 底栏 MCP 开关保持开启。
- 改完配置后在 Cursor 里重连该 MCP。

## 文档

- `docs/MCP工具清单.md`：当前 WPF 版支持的 MCP 工具、参数与返回字段说明。

## 开源协议

SunnyNet WPF 版采用 MIT 协议开源。你可以自由使用、复制、修改、合并、发布、分发或二次开发本项目，但需要保留原始版权声明和协议声明。

本项目仅用于合法的网络调试、接口分析、测试与技术研究场景。请勿将本工具用于未授权抓包、绕过访问控制、攻击或其它违法用途。
