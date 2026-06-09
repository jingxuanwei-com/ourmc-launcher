using System;
using System.Collections.Generic;
using Microsoft.JSInterop;
using ourmclauncher.Models;

namespace ourmclauncher.Services;

public class NotificationService : IDisposable
{
    private readonly IJSRuntime _jsRuntime;
    private DotNetObjectReference<NotificationService>? _dotNetRef;
    private readonly List<NotificationItem> _notifications = new();
    private int _nextId = 1;

    public NotificationService(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    public Task InitializeAsync()
    {
        _dotNetRef = DotNetObjectReference.Create(this);
        return Task.CompletedTask;
    }

    public async Task Success(string message, int duration = 3000)
    {
        await ShowNotification("success", message, duration);
    }

    public async Task Error(string message, int duration = 5000)
    {
        await ShowNotification("error", message, duration);
    }

    public async Task Info(string message, int duration = 3000)
    {
        await ShowNotification("info", message, duration);
    }

    public async Task Warning(string message, int duration = 4000)
    {
        await ShowNotification("warning", message, duration);
    }

    private async Task ShowNotification(string type, string message, int duration)
    {
        var id = _nextId++;
        var notification = new NotificationItem { Id = id, Type = type, Message = message };
        _notifications.Add(notification);

        try
        {
            await _jsRuntime.InvokeVoidAsync("omlNotify", type, message, duration);
        }
        catch { }
    }

    public void Dispose()
    {
        _dotNetRef?.Dispose();
    }
}