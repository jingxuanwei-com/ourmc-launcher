using System;
using System.Collections.Generic;

namespace ourmclauncher.Models;

/// <summary>
/// 游戏版本信息
/// </summary>
public class GameVersion
{
    /// <summary>
    /// 版本名称（如 1.20.1）
    /// </summary>
    public string Name { get; set; } = "";
    
    /// <summary>
    /// 版本类型（如 正式版、Forge、Fabric）
    /// </summary>
    public string Type { get; set; } = "";
}

/// <summary>
/// 远程版本信息（从Mojang官方获取）
/// </summary>
public class RemoteVersion
{
    /// <summary>
    /// 版本ID（如 1.20.1）
    /// </summary>
    public string Id { get; set; } = "";
    
    /// <summary>
    /// 版本类型（release/snapshot）
    /// </summary>
    public string Type { get; set; } = "";
    
    /// <summary>
    /// 版本清单URL
    /// </summary>
    public string Url { get; set; } = "";
    
    /// <summary>
    /// 发布时间
    /// </summary>
    public string ReleaseTime { get; set; } = "";
}

/// <summary>
/// 用户信息
/// </summary>
public class UserInfo
{
    /// <summary>
    /// 用户邮箱
    /// </summary>
    public string Email { get; set; } = string.Empty;
    
    /// <summary>
    /// 用户昵称
    /// </summary>
    public string Nickname { get; set; } = string.Empty;
    
    /// <summary>
    /// 用户名
    /// </summary>
    public string Username { get; set; } = string.Empty;
    
    /// <summary>
    /// 头像URL
    /// </summary>
    public string Avatar { get; set; } = string.Empty;
}

/// <summary>
/// 登录结果
/// </summary>
public class LoginResult
{
    /// <summary>
    /// 是否登录成功
    /// </summary>
    public bool Success { get; set; }
    
    /// <summary>
    /// 错误消息
    /// </summary>
    public string Message { get; set; } = "";
    
    /// <summary>
    /// 用户信息
    /// </summary>
    public UserInfo? User { get; set; }
}

/// <summary>
/// 用户资料（包含皮肤信息）
/// </summary>
public class UserProfile
{
    /// <summary>
    /// 用户UUID
    /// </summary>
    public string Id { get; set; } = string.Empty;
    
    /// <summary>
    /// 用户名
    /// </summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// 属性（皮肤等）
    /// </summary>
    public Dictionary<string, TextureProperty> Properties { get; set; } = new();
}

/// <summary>
/// 纹理属性
/// </summary>
public class TextureProperty
{
    /// <summary>
    /// 属性名称
    /// </summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// 属性值（Base64编码）
    /// </summary>
    public string Value { get; set; } = string.Empty;
}

/// <summary>
/// 应用设置
/// </summary>
public class AppSettings
{
    /// <summary>
    /// Java可执行文件路径
    /// </summary>
    public string JavaPath { get; set; } = "";
    
    /// <summary>
    /// 最大内存（MB）
    /// </summary>
    public int MaxMemory { get; set; } = 2048;
    
    /// <summary>
    /// 游戏目录路径
    /// </summary>
    public string GameDirectory { get; set; }
    
    /// <summary>
    /// 玩家名称（离线模式）
    /// </summary>
    public string PlayerName { get; set; } = "Player";
    
    public AppSettings()
    {
        var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        GameDirectory = Path.Combine(appData, ".minecraft");
    }
}

/// <summary>
/// 通知项
/// </summary>
public class NotificationItem
{
    /// <summary>
    /// 通知ID
    /// </summary>
    public int Id { get; set; }
    
    /// <summary>
    /// 通知类型（success/error/info/warning）
    /// </summary>
    public string Type { get; set; } = "info";
    
    /// <summary>
    /// 通知消息
    /// </summary>
    public string Message { get; set; } = "";
}

/// <summary>
/// 版本类型枚举
/// </summary>
public enum VersionType
{
    /// <summary>
    /// 原版正式版
    /// </summary>
    Release,
    
    /// <summary>
    /// 快照版本
    /// </summary>
    Snapshot,
    
    /// <summary>
    /// Forge模组加载器
    /// </summary>
    Forge,
    
    /// <summary>
    /// Fabric模组加载器
    /// </summary>
    Fabric,
    
    /// <summary>
    /// Quilt模组加载器
    /// </summary>
    Quilt,
    
    /// <summary>
    /// OptiFine优化模组
    /// </summary>
    OptiFine,
    
    /// <summary>
    /// 其他模组版本
    /// </summary>
    Modded
}

/// <summary>
/// 模组加载器类型
/// </summary>
public enum LoaderType
{
    /// <summary>
    /// 原版（无模组加载器）
    /// </summary>
    Vanilla,
    
    /// <summary>
    /// Forge
    /// </summary>
    Forge,
    
    /// <summary>
    /// Fabric
    /// </summary>
    Fabric
}
