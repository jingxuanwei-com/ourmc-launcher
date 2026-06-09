using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.Json;

namespace ourmclauncher.Services;

/// <summary>
/// 游戏启动服务 - 负责构建启动参数和启动Minecraft进程
/// </summary>
public class LaunchService : IDisposable
{
    private readonly SettingsService _settings;
    private readonly PathService _pathService;
    private Process? _gameProcess;

    public LaunchService(SettingsService settings, PathService pathService)
    {
        _settings = settings;
        _pathService = pathService;
    }

    public event Action<string>? OnLaunchOutput;
    public event Action<string>? OnLaunchError;
    
    /// <summary>
    /// 检查游戏是否正在运行
    /// </summary>
    public bool IsGameRunning => _gameProcess != null && !_gameProcess.HasExited;

    public bool Launch(string versionName, string? playerName = null, int maxMemory = 2048)
    {
        try
        {
            var gameDir = _pathService.GameDir;
            var versionDir = _pathService.GetVersionDir(versionName);
            var jarPath = _pathService.GetVersionJarPath(versionName);
            var jsonPath = _pathService.GetVersionJsonPath(versionName);

            if (!File.Exists(jsonPath))
            {
                OnLaunchError?.Invoke($"找不到版本清单: {jsonPath}");
                return false;
            }

            var javaPath = _settings.GetJavaPath();
            if (string.IsNullOrEmpty(javaPath) || !File.Exists(javaPath))
            {
                javaPath = AutoDetectJava();
                if (string.IsNullOrEmpty(javaPath))
                {
                    OnLaunchError?.Invoke("未找到Java，请在设置中配置Java路径");
                    return false;
                }
            }

            var arguments = BuildLaunchArguments(javaPath, gameDir, versionDir, jarPath, jsonPath, versionName, playerName, maxMemory);
            if (arguments == null)
            {
                OnLaunchError?.Invoke("构建启动参数失败");
                return false;
            }

            _gameProcess = new Process();
            _gameProcess.StartInfo.FileName = javaPath;
            _gameProcess.StartInfo.Arguments = arguments;
            _gameProcess.StartInfo.WorkingDirectory = gameDir;
            _gameProcess.StartInfo.UseShellExecute = false;
            _gameProcess.StartInfo.RedirectStandardOutput = true;
            _gameProcess.StartInfo.RedirectStandardError = true;
            _gameProcess.StartInfo.CreateNoWindow = true;

            _gameProcess.OutputDataReceived += (s, e) =>
            {
                if (!string.IsNullOrEmpty(e.Data))
                    OnLaunchOutput?.Invoke(e.Data);
            };

            _gameProcess.ErrorDataReceived += (s, e) =>
            {
                if (!string.IsNullOrEmpty(e.Data))
                    OnLaunchError?.Invoke(e.Data);
            };

            _gameProcess.Start();
            _gameProcess.BeginOutputReadLine();
            _gameProcess.BeginErrorReadLine();

            return true;
        }
        catch (Exception ex)
        {
            OnLaunchError?.Invoke($"启动失败: {ex.Message}");
            return false;
        }
    }

    private string? BuildLaunchArguments(string javaPath, string gameDir, string versionDir, string jarPath, string jsonPath, string versionName, string? playerName, int maxMemory)
    {
        try
        {
            var json = File.ReadAllText(jsonPath);
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            var mainClass = root.GetProperty("mainClass").GetString();
            var classPath = BuildClassPath(gameDir, versionDir, jarPath, root);

            var playerNameValue = playerName ?? "Player";
            var assetsDir = Path.Combine(gameDir, "assets");

            var assetId = "legacy";
            if (root.TryGetProperty("assetIndex", out var assetIndex))
            {
                assetId = assetIndex.GetProperty("id").GetString() ?? "legacy";
            }

            var args = $"-Xmx{maxMemory}M " +
                       $"-Xms512M " +
                       $"-Djava.library.path=\"{Path.Combine(versionDir, "natives")}\" " +
                       $"-cp \"{classPath}\" " +
                       $"\"{mainClass}\" " +
                       $"--username {playerNameValue} " +
                       $"--version \"{versionName}\" " +
                       $"--gameDir \"{gameDir}\" " +
                       $"--assetsDir \"{assetsDir}\" " +
                       $"--assetIndex {assetId} " +
                       $"--uuid 0 " +
                       $"--accessToken 0 " +
                       $"--userType mojang " +
                       $"--versionType \"oml\"";

            return args;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"构建启动参数失败: {ex.Message}");
            return null;
        }
    }

    private string BuildClassPath(string gameDir, string versionDir, string jarPath, JsonElement root)
    {
        var cpEntries = new List<string> { jarPath };

        if (root.TryGetProperty("libraries", out var libraries))
        {
            foreach (var lib in libraries.EnumerateArray())
            {
                if (lib.TryGetProperty("rules", out var rules))
                {
                    if (!CheckRules(rules)) continue;
                }

                if (lib.TryGetProperty("downloads", out var downloads))
                {
                    if (downloads.TryGetProperty("artifact", out var artifact))
                    {
                        if (artifact.TryGetProperty("path", out var path))
                        {
                            var libPath = Path.Combine(gameDir, "libraries", path.GetString()!);
                            if (File.Exists(libPath))
                            {
                                cpEntries.Add(libPath);
                            }
                        }
                    }
                }
            }
        }

        return string.Join(";", cpEntries);
    }

    private bool CheckRules(JsonElement rules)
    {
        var allowed = true;
        foreach (var rule in rules.EnumerateArray())
        {
            var action = rule.GetProperty("action").GetString();
            if (action == "disallow") allowed = false;
            else if (action == "allow") allowed = true;
        }
        return allowed;
    }

    public string? AutoDetectJava()
    {
        var candidates = new List<string>
        {
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "Java"),
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "Java"),
        };

        foreach (var dir in candidates)
        {
            if (!Directory.Exists(dir)) continue;
            foreach (var jvmDir in Directory.GetDirectories(dir))
            {
                var javaw = Path.Combine(jvmDir, "bin", "javaw.exe");
                if (File.Exists(javaw)) return javaw;
            }
        }

        try
        {
            var psi = new ProcessStartInfo("where", "javaw.exe")
            {
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };
            var proc = Process.Start(psi);
            if (proc != null)
            {
                var output = proc.StandardOutput.ReadLine();
                proc.WaitForExit(3000);
                if (!string.IsNullOrEmpty(output) && File.Exists(output))
                    return output;
            }
        }
        catch { }

        return null;
    }

    /// <summary>
    /// 释放资源，清理事件订阅
    /// </summary>
    public void Dispose()
    {
        OnLaunchOutput = null;
        OnLaunchError = null;
        _gameProcess?.Dispose();
        _gameProcess = null;
    }
}
