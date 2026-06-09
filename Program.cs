using Microsoft.AspNetCore.Components.WebView.WindowsForms;
using Microsoft.Extensions.DependencyInjection;
using ourmclauncher.Services;

namespace ourmclauncher;

/// <summary>
/// 程序入口点
/// </summary>
internal static class Program
{
    [STAThread]
    static void Main()
    {
        ApplicationConfiguration.Initialize();

        var form = new LauncherForm();

        form.Resize += (s, e) =>
        {
            form.Region = LauncherForm.CreateRoundedRegion(form.ClientSize.Width, form.ClientSize.Height, 16);
        };

        var windowService = new WindowService(form);
        var settingsService = new SettingsService();
        var pathService = new PathService(settingsService);
        var versionService = new VersionService(pathService);
        var launchService = new LaunchService(settingsService, pathService);

        var services = new ServiceCollection();
        services.AddWindowsFormsBlazorWebView();
        services.AddSingleton<AppState>();
        services.AddSingleton<SkinService>();
        services.AddSingleton<DownloadService>(sp => new DownloadService(pathService));
        services.AddSingleton<WindowService>(windowService);
        services.AddSingleton<SettingsService>(settingsService);
        services.AddSingleton<PathService>(pathService);
        services.AddSingleton<VersionService>(versionService);
        services.AddSingleton<LaunchService>(launchService);
        services.AddSingleton<NotificationService>();

        var blazor = new BlazorWebView
        {
            Dock = DockStyle.Fill,
            HostPage = "wwwroot\\index.html",
            Services = services.BuildServiceProvider()
        };

        blazor.RootComponents.Add<Main>("#app");

        // WebView2 加载完成后使用淡入动画显示窗口
        blazor.WebView.NavigationCompleted += async (s, e) =>
        {
            await Task.Delay(100); // 稍微延迟确保 Blazor 完全渲染
            form.BeginInvoke(new Action(() =>
            {
                form.FadeIn();
            }));
        };

        form.Controls.Add(blazor);

        Application.Run(form);
    }
}
