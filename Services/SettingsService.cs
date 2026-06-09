using System;
using System.IO;
using System.Text.Json;
using ourmclauncher.Models;

namespace ourmclauncher.Services;

public class SettingsService
{
    private readonly string _settingsFile;
    private AppSettings _settings;

    public SettingsService()
    {
        var appDir = GetAppDirectory();
        Directory.CreateDirectory(appDir);
        _settingsFile = Path.Combine(appDir, "settings.json");
        _settings = LoadSettings();
    }

    public string GetJavaPath() => _settings.JavaPath;
    public int GetMaxMemory() => _settings.MaxMemory;
    public string GetGameDirectory() => _settings.GameDirectory;
    public string GetPlayerName() => _settings.PlayerName;

    public void SetJavaPath(string path)
    {
        _settings.JavaPath = path;
        SaveSettings();
    }

    public void SetMaxMemory(int memory)
    {
        _settings.MaxMemory = memory;
        SaveSettings();
    }

    public void SetGameDirectory(string dir)
    {
        _settings.GameDirectory = dir;
        SaveSettings();
    }

    public void SetPlayerName(string name)
    {
        _settings.PlayerName = name;
        SaveSettings();
    }

    public AppSettings GetAllSettings() => _settings;

    private AppSettings LoadSettings()
    {
        try
        {
            if (File.Exists(_settingsFile))
            {
                var json = File.ReadAllText(_settingsFile);
                return JsonSerializer.Deserialize<AppSettings>(json) ?? new AppSettings();
            }
        }
        catch { }

        return new AppSettings();
    }

    private void SaveSettings()
    {
        try
        {
            var json = JsonSerializer.Serialize(_settings, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(_settingsFile, json);
        }
        catch { }
    }

    private string GetAppDirectory()
    {
        var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        return Path.Combine(appData, ".minecraft", "oml");
    }
}