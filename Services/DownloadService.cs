using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;
using ourmclauncher.Models;

namespace ourmclauncher.Services;

/// <summary>
/// 下载服务 - 负责从 Mojang 官方源下载游戏文件
/// </summary>
public class DownloadService : IDisposable
{
    private readonly HttpClient _httpClient;
    private readonly PathService _pathService;
    private CancellationTokenSource? _cts;

    public DownloadService(PathService pathService)
    {
        _pathService = pathService;
        _httpClient = new HttpClient();
        _httpClient.DefaultRequestHeaders.Add("User-Agent", "oml-launcher/1.0");
    }

    public bool IsDownloading => _cts != null;

    public void CancelDownload()
    {
        _cts?.Cancel();
        _cts = null;
    }

    public async Task<List<RemoteVersion>> GetVersionManifestAsync()
    {
        try
        {
            var json = await _httpClient.GetStringAsync("https://piston-meta.mojang.com/mc/game/version_manifest_v2.json");
            var manifest = JsonNode.Parse(json);
            var versions = new List<RemoteVersion>();

            var versionsArray = manifest?["versions"] as JsonArray;
            if (versionsArray == null) return versions;

            foreach (var v in versionsArray)
            {
                var id = v?["id"]?.ToString() ?? "";
                var type = v?["type"]?.ToString() ?? "";
                var url = v?["url"]?.ToString() ?? "";
                var releaseTime = v?["releaseTime"]?.ToString() ?? "";

                versions.Add(new RemoteVersion
                {
                    Id = id,
                    Type = type,
                    Url = url,
                    ReleaseTime = releaseTime
                });
            }

            return versions;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"获取版本清单失败: {ex.Message}");
            return new();
        }
    }

    public async Task<bool> DownloadVersionAsync(string versionId, IProgress<(int progress, string status)>? progress = null)
    {
        _cts = new CancellationTokenSource();
        var token = _cts.Token;

        try
        {
            // 1. 获取版本清单
            progress?.Report((0, "获取版本信息..."));

            string versionManifestUrl;
            try
            {
                // 从官方API获取版本URL
                var manifestJson = await _httpClient.GetStringAsync("https://piston-meta.mojang.com/mc/game/version_manifest_v2.json", token);
                var manifest = JsonNode.Parse(manifestJson);
                var versionsArray = manifest?["versions"] as JsonArray;

                versionManifestUrl = "";
                if (versionsArray != null)
                {
                    foreach (var v in versionsArray)
                    {
                        if (v?["id"]?.ToString() == versionId)
                        {
                            versionManifestUrl = v["url"]?.ToString() ?? "";
                            break;
                        }
                    }
                }

                if (string.IsNullOrEmpty(versionManifestUrl))
                {
                    progress?.Report((0, "错误: 未找到该版本"));
                    return false;
                }
            }
            catch (OperationCanceledException)
            {
                progress?.Report((0, "下载已取消"));
                return false;
            }

            // 2. 下载版本清单
            progress?.Report((5, "下载版本清单..."));
            string versionJson;
            try
            {
                versionJson = await _httpClient.GetStringAsync(versionManifestUrl, token);
            }
            catch (OperationCanceledException)
            {
                progress?.Report((0, "下载已取消"));
                return false;
            }

            // 3. 解析版本清单
            var versionData = JsonNode.Parse(versionJson);
            var clientDownload = versionData?["downloads"]?["client"];
            var clientUrl = clientDownload?["url"]?.ToString();
            var clientSha1 = clientDownload?["sha1"]?.ToString();
            var clientSize = clientDownload?["size"]?.GetValue<long>() ?? 0;

            if (string.IsNullOrEmpty(clientUrl))
            {
                progress?.Report((0, "错误: 无法获取客户端下载链接"));
                return false;
            }

            // 4. 创建版本目录
            var versionDir = _pathService.GetVersionDir(versionId);
            Directory.CreateDirectory(versionDir);

            // 5. 保存版本清单
            progress?.Report((10, "保存版本清单..."));
            var versionJsonPath = Path.Combine(versionDir, $"{versionId}.json");
            await File.WriteAllTextAsync(versionJsonPath, versionJson, token);

            // 6. 下载客户端 JAR
            progress?.Report((15, "下载客户端文件..."));
            var clientJarPath = _pathService.GetVersionJarPath(versionId);
            var success = await DownloadFileWithProgressAsync(clientUrl, clientJarPath, clientSize, token,
                (downloadProgress) =>
                {
                    var overallProgress = 15 + (int)(downloadProgress * 0.55);
                    progress?.Report((overallProgress, $"下载客户端... {downloadProgress}%"));
                });
            
            if (!success)
            {
                if (token.IsCancellationRequested)
                {
                    progress?.Report((0, "下载已取消"));
                }
                else
                {
                    progress?.Report((0, "错误: 客户端下载失败"));
                }
                return false;
            }
            
            // 校验文件完整性
            if (!string.IsNullOrEmpty(clientSha1))
            {
                progress?.Report((70, "校验文件完整性..."));
                var isValid = await VerifyFileHashAsync(clientJarPath, clientSha1);
                if (!isValid)
                {
                    progress?.Report((0, "错误: 文件校验失败，请重试"));
                    return false;
                }
            }

            // 7. 下载依赖库
            progress?.Report((75, "处理依赖库..."));
            var libraries = versionData?["libraries"] as JsonArray;
            if (libraries != null)
            {
                var libCount = libraries.Count;
                var libDir = _pathService.GetLibrariesDir();
                Directory.CreateDirectory(libDir);

                for (int i = 0; i < libCount; i++)
                {
                    token.ThrowIfCancellationRequested();

                    var lib = libraries[i];
                    var artifact = lib?["downloads"]?["artifact"];
                    var libUrl = artifact?["url"]?.ToString();
                    var libPath = artifact?["path"]?.ToString();

                    if (!string.IsNullOrEmpty(libUrl) && !string.IsNullOrEmpty(libPath))
                    {
                        var fullPath = Path.Combine(libDir, libPath);
                        var dir = Path.GetDirectoryName(fullPath);
                        if (!string.IsNullOrEmpty(dir))
                        {
                            Directory.CreateDirectory(dir);
                        }

                        var libProgress = 75 + (int)((i / (double)libCount) * 20);
                        progress?.Report((libProgress, $"下载依赖库 {i + 1}/{libCount}..."));

                        if (!File.Exists(fullPath))
                        {
                            try
                            {
                                await _httpClient.DownloadFileAsync(libUrl, fullPath, token);
                            }
                            catch (OperationCanceledException) { throw; }
                            catch (Exception ex)
                            {
                                Debug.WriteLine($"下载库失败 {libUrl}: {ex.Message}");
                            }
                        }
                    }
                }
            }

            // 8. 下载资源索引
            progress?.Report((95, "处理资源文件..."));
            var assetIndex = versionData?["assetIndex"];
            var assetUrl = assetIndex?["url"]?.ToString();
            var assetId = assetIndex?["id"]?.ToString();

            if (!string.IsNullOrEmpty(assetUrl) && !string.IsNullOrEmpty(assetId))
            {
                var assetsDir = _pathService.GetAssetIndexesDir();
                Directory.CreateDirectory(assetsDir);
                var assetIndexPath = Path.Combine(assetsDir, $"{assetId}.json");

                if (!File.Exists(assetIndexPath))
                {
                    try
                    {
                        await _httpClient.DownloadFileAsync(assetUrl, assetIndexPath, token);
                    }
                    catch (OperationCanceledException) { throw; }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"下载资源索引失败: {ex.Message}");
                    }
                }
            }

            // 9. 提取native库
            progress?.Report((98, "处理本地库..."));
            await ExtractNativesAsync(versionId, versionDir, versionData, token);

            progress?.Report((100, "下载完成!"));
            return true;
        }
        catch (OperationCanceledException)
        {
            progress?.Report((0, "下载已取消"));
            return false;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"下载版本失败: {ex}");
            progress?.Report((0, $"错误: {ex.Message}"));
            return false;
        }
        finally
        {
            _cts = null;
        }
    }

    private async Task ExtractNativesAsync(string versionName, string versionDir, JsonNode? versionData, CancellationToken token)
    {
        try
        {
            var nativesDir = _pathService.GetNativesDir(versionName);
            Directory.CreateDirectory(nativesDir);

            var libraries = versionData?["libraries"] as JsonArray;
            if (libraries == null) return;

            foreach (var lib in libraries)
            {
                token.ThrowIfCancellationRequested();

                var classifiers = lib?["downloads"]?["classifiers"];
                if (classifiers == null) continue;

                JsonNode? nativeArtifact = null;
                if (classifiers["natives-windows"] != null)
                    nativeArtifact = classifiers["natives-windows"];
                else if (classifiers["natives-windows-lgpl"] != null)
                    nativeArtifact = classifiers["natives-windows-lgpl"];

                if (nativeArtifact == null) continue;

                var nativeUrl = nativeArtifact["url"]?.ToString();
                if (string.IsNullOrEmpty(nativeUrl)) continue;

                var tempPath = Path.Combine(nativesDir, $"temp_{Guid.NewGuid():N}.jar");
                try
                {
                    await _httpClient.DownloadFileAsync(nativeUrl, tempPath, token);

                    // 解压 .dll 文件
                    System.IO.Compression.ZipFile.ExtractToDirectory(tempPath, nativesDir, overwriteFiles: true);
                    File.Delete(tempPath);
                }
                catch (OperationCanceledException) { throw; }
                catch (Exception ex)
                {
                    Debug.WriteLine($"提取native失败: {ex.Message}");
                    if (File.Exists(tempPath)) try { File.Delete(tempPath); } catch { }
                }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"提取natives失败: {ex.Message}");
        }
    }

    private async Task<bool> DownloadFileWithProgressAsync(string url, string path, long expectedSize, CancellationToken token, Action<int>? progressCallback = null)
    {
        try
        {
            using var response = await _httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, token);
            response.EnsureSuccessStatusCode();

            var totalBytes = response.Content.Headers.ContentLength ?? expectedSize;
            var downloadedBytes = 0L;

            using var contentStream = await response.Content.ReadAsStreamAsync(token);
            using var fileStream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None);

            var buffer = new byte[8192];
            int bytesRead;

            while ((bytesRead = await contentStream.ReadAsync(buffer, 0, buffer.Length, token)) > 0)
            {
                await fileStream.WriteAsync(buffer, 0, bytesRead, token);
                downloadedBytes += bytesRead;

                if (totalBytes > 0)
                {
                    var progress = (int)((downloadedBytes * 100) / totalBytes);
                    progressCallback?.Invoke(progress);
                }
            }

            return true;
        }
        catch (OperationCanceledException) { return false; }
        catch (Exception ex)
        {
            Debug.WriteLine($"下载文件失败 {url}: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// 验证文件 SHA1 哈希值
    /// </summary>
    private async Task<bool> VerifyFileHashAsync(string filePath, string expectedSha1)
    {
        try
        {
            using var sha1 = SHA1.Create();
            using var stream = File.OpenRead(filePath);
            var hash = await sha1.ComputeHashAsync(stream);
            var actualSha1 = BitConverter.ToString(hash).Replace("-", "").ToLower();
            return actualSha1 == expectedSha1.ToLower();
        }
        catch
        {
            return false;
        }
    }
    
    /// <summary>
    /// 释放资源
    /// </summary>
    public void Dispose()
    {
        _httpClient?.Dispose();
        _cts?.Dispose();
    }
}

public static class HttpClientExtensions
{
    public static async Task DownloadFileAsync(this HttpClient client, string url, string path, CancellationToken token = default)
    {
        var data = await client.GetByteArrayAsync(url, token);
        var dir = Path.GetDirectoryName(path);
        if (!string.IsNullOrEmpty(dir))
        {
            Directory.CreateDirectory(dir);
        }
        await File.WriteAllBytesAsync(path, data, token);
    }
}
