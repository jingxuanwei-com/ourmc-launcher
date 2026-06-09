# OML Launcher - 我们的世界启动器

![版本](https://img.shields.io/badge/version-1.0.0-blue)
![.NET](https://img.shields.io/badge/.NET-8.0-purple)
![平台](https://img.shields.io/badge/platform-Windows-lightgrey)
![许可证](https://img.shields.io/badge/license-MIT-green)

一个现代化的 Minecraft 启动器，专注于 our-mc.cn 皮肤站生态，采用 Blazor WebView + WinForms 构建。

## ✨ 功能特性

- 🎮 **游戏启动** - 支持多版本管理，一键启动 Minecraft
- 📥 **版本下载** - 从 Mojang 官方源自动下载完整游戏文件（客户端 + 依赖库 + 资源）
- 👤 **账号系统** - 集成 our-mc.cn 皮肤站登录，同步用户信息
- 🎨 **现代 UI** - Windows 11 风格设计，毛玻璃效果，流畅动画
- ⚙️ **灵活配置** - Java 路径、内存分配、游戏目录均可自定义
- 🔔 **智能通知** - Toast 通知反馈操作状态，清晰明了
- 🔍 **版本管理** - 自动扫描已安装版本，持久化存储

## 📦 安装指南

### 系统要求

- **操作系统**: Windows 10/11
- **运行环境**: .NET 8.0 Runtime
- **Java 环境**: Java 8 或更高版本（用于启动 Minecraft）
- **内存**: 至少 4GB RAM
- **存储**: 至少 1GB 可用空间（游戏文件另计）

### 安装步骤

#### 方式一：使用预编译版本

1. 从 [Releases](https://github.com/your-org/ourmclauncher/releases) 下载最新版本
2. 解压到任意目录（建议不要包含中文或空格）
3. 双击 `ourmclauncher.exe` 运行
4. 首次启动会自动检测 Java 环境

#### 方式二：从源码构建

```bash
# 1. 克隆仓库
git clone https://github.com/your-org/ourmclauncher.git
cd ourmclauncher

# 2. 还原 NuGet 依赖
dotnet restore

# 3. 构建项目
dotnet build -c Release

# 4. 运行
dotnet run
```

## 🚀 快速开始

### 首次使用流程

1. **配置 Java 环境**
   - 打开"设置"页面（侧边栏齿轮图标）
   - 点击"自动检测"按钮，或手动输入 `javaw.exe` 路径
   - 设置最大内存（推荐 2048MB 或更高）

2. **下载游戏版本**
   - 切换到"下载"页面（侧边栏下载图标）
   - 浏览或搜索想要安装的版本
   - 点击"安装"按钮，等待下载完成
   - 下载内容包括：客户端 JAR、依赖库、资源文件、Native 库

3. **启动游戏**
   - 返回"主页"或进入"版本"页面
   - 选择已安装的版本
   - 点击"启动"按钮

### 登录 our-mc.cn 账号

1. 切换到"账户"页面（侧边栏人物图标）
2. 输入邮箱/用户名和密码
3. 点击"登录"
4. 登录成功后可查看头像和昵称
5. 点击"管理皮肤"可跳转到皮肤站网页

## 🏗️ 项目架构

### 技术栈

| 技术 | 版本 | 用途 |
|------|------|------|
| .NET | 8.0 | 运行时框架 |
| Blazor WebView | 8.0.* | UI 渲染引擎 |
| WinForms | - | 窗口容器 |
| C# | 12.0 | 编程语言 |
| Razor | - | 组件模板 |
| CSS3 | - | 样式和动画 |

### 目录结构

```
ourmclauncher/
├── Components/                    # UI 组件（规划中）
│   ├── Pages/                    # 页面组件
│   │   ├── HomePage.razor        # 首页
│   │   ├── VersionsPage.razor    # 版本管理
│   │   ├── DownloadsPage.razor   # 下载管理
│   │   ├── AccountPage.razor     # 账户登录
│   │   └── SettingsPage.razor    # 设置配置
│   └── Shared/                   # 共享组件
│       ├── Sidebar.razor         # 侧边栏导航
│       └── TopBar.razor          # 顶部栏
├── Services/                     # 业务逻辑服务
│   ├── DownloadService.cs        # 下载服务（版本清单、文件下载、SHA1校验）
│   ├── LaunchService.cs          # 启动服务（参数构建、Java检测、进程管理）
│   ├── NotificationService.cs    # 通知服务（Toast提示）
│   ├── SettingsService.cs        # 设置服务（持久化配置）
│   ├── SkinService.cs            # 皮肤站服务（登录、用户信息）
│   ├── VersionService.cs         # 版本服务（扫描、持久化）
│   ├── PathService.cs            # 路径服务（统一路径管理）
│   └── WindowService.cs          # 窗口服务（窗口操作）
├── Models.cs                     # 数据模型（集中定义）
├── Program.cs                    # 程序入口 + DI配置
├── LauncherForm.cs               # 自定义窗口（Windows API、动画）
├── wwwroot/                      # Web 资源
│   ├── index.html                # HTML 入口 + JS 桥接
│   ├── css/app.css               # 全局样式
│   └── *.png, *.jpg              # 图片资源
└── Properties/
    └── launchSettings.json       # 调试配置
```

### 核心模块说明

#### Services 层

**DownloadService**
- 从 Mojang 官方 API 获取版本清单
- 下载游戏客户端、依赖库、资源文件
- SHA1 文件完整性校验
- 支持取消下载和进度报告

**LaunchService**
- 解析版本 JSON 文件
- 构建 Minecraft 启动参数（ClassPath、JVM 参数）
- 自动检测 Java 环境
- 启动游戏进程并捕获输出

**SettingsService**
- 管理用户配置（Java 路径、内存、游戏目录）
- 持久化存储到 `%APPDATA%\.minecraft\oml\settings.json`

**SkinService**
- 与 our-mc.cn 皮肤站 API 交互
- 处理用户登录（CSRF Token、Cookie 管理）
- 获取用户信息和头像

**VersionService**
- 扫描 `.minecraft/versions` 目录
- 识别版本类型（原版、Forge、Fabric 等）
- 持久化版本列表到 `oml_versions.json`

**PathService**
- 统一管理所有文件系统路径
- 提供便捷的路径获取方法
- 消除重复代码

#### 数据流

```
用户操作 → UI 组件 (Razor) → Service 层 → 文件系统/网络
                ↓                          ↓
           状态更新 ← ← ← ← ← ← ← ← ← 结果返回
                ↓
           UI 刷新 (StateHasChanged)
```

#### 配置存储

所有配置文件保存在 `%APPDATA%\.minecraft\oml\` 目录：

- `settings.json` - 启动器设置
- `oml_versions.json` - 已安装版本列表
- `session.dat` - 登录会话（规划中）
- `launcher.log` - 运行日志（规划中）

## 💻 开发指南

### 环境配置

```bash
# 安装 .NET 8.0 SDK
# 下载地址: https://dotnet.microsoft.com/download/dotnet/8.0

# 验证安装
dotnet --version  # 应输出 8.0.x

# 安装 IDE（推荐）
# - Visual Studio 2022 (带 ASP.NET 和 Web 开发工作负载)
# - VS Code (带 C# 扩展)
# - JetBrains Rider
```

### 运行项目

```bash
# 开发模式（热重载）
dotnet watch run

# 普通运行
dotnet run

# 指定配置
dotnet run --configuration Debug
```

### 构建发布版本

```bash
# 发布为独立可执行文件（推荐）
dotnet publish -c Release -r win-x64 --self-contained

# 发布为依赖框架版本（体积更小）
dotnet publish -c Release -r win-x64

# 输出目录
# bin/Release/net8.0-windows/win-x64/publish/
```

### 代码规范

1. **命名约定**
   - 类名：PascalCase（如 `LaunchService`）
   - 方法名：PascalCase（如 `LaunchAsync`）
   - 私有字段：camelCase 带下划线前缀（如 `_settings`）
   - 属性：PascalCase（如 `GameDirectory`）

2. **异步方法**
   - 所有异步方法以 `Async` 结尾
   - 使用 `async/await` 而非 `.Result` 或 `.Wait()`

3. **依赖注入**
   - 服务类通过构造函数注入依赖
   - 在 `Program.cs` 中注册服务

4. **可空引用类型**
   - 启用 Nullable Reference Types
   - 使用 `?` 标记可空类型

5. **资源管理**
   - 实现 `IDisposable` 的服务必须释放资源
   - 使用 `using` 语句管理临时资源

### 添加新功能

1. **创建服务类**
   ```csharp
   // Services/MyFeatureService.cs
   public class MyFeatureService : IDisposable
   {
       public void Dispose() { }
   }
   ```

2. **注册服务**
   ```csharp
   // Program.cs
   services.AddSingleton<MyFeatureService>();
   ```

3. **创建 UI 组件**
   ```razor
   @* Components/Pages/MyFeaturePage.razor *@
   @inject MyFeatureService MyFeature
   
   <div>...</div>
   ```

4. **注入并使用**
   ```razor
   @code {
       private async Task DoSomething()
       {
           await MyFeature.DoWorkAsync();
       }
   }
   ```

## 🤝 贡献指南

欢迎提交 Issue 和 Pull Request！

### 提交 Issue

请提供以下信息：

- **问题描述**：清晰描述遇到的问题
- **复现步骤**：详细的操作步骤
- **预期行为**：期望发生什么
- **实际行为**：实际发生了什么
- **环境信息**：操作系统、.NET 版本、Java 版本
- **日志文件**：如有错误日志请附上

### 提交 Pull Request

1. Fork 本仓库
2. 创建特性分支 (`git checkout -b feature/amazing-feature`)
3. 提交更改 (`git commit -m 'Add amazing feature'`)
4. 推送到分支 (`git push origin feature/amazing-feature`)
5. 开启 Pull Request

### PR 要求

- [ ] 代码符合项目规范
- [ ] 添加了必要的注释
- [ ] 更新了相关文档
- [ ] 测试通过（手动测试所有相关功能）

## 📝 许可证

本项目采用 **MIT 许可证** - 查看 [LICENSE](LICENSE) 文件了解详情

### 允许

- ✅ 商业使用
- ✅ 修改代码
- ✅ 分发
- ✅ 私人使用

### 条件

- 📄 保留版权声明
- 📄 包含许可证副本

## 🙏 致谢

- **[Mojang Studios](https://www.mojang.com/)** - Minecraft 游戏开发方
- **[our-mc.cn](https://www.our-mc.cn/)** - 皮肤站平台支持
- **[.NET Team](https://dotnet.microsoft.com/)** - 优秀的开发框架
- **[Blazor](https://dotnet.microsoft.com/apps/aspnet/web-apps/blazor)** - Web UI 技术
- **所有贡献者** - 感谢每一位帮助改进项目的人

## 📞 联系方式

- **项目主页**: https://github.com/your-org/ourmclauncher
- **问题反馈**: https://github.com/your-org/ourmclauncher/issues
- **皮肤站**: https://skin.our-mc.cn/
- **官方网站**: https://www.our-mc.cn/

---

**Made with ❤️ by OML Team**
