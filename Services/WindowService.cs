using System;
using System.Drawing;
using System.Windows.Forms;

namespace ourmclauncher.Services;

/// <summary>
/// 窗口服务 - 供Blazor组件调用WinForms窗口操作
/// </summary>
public class WindowService
{
    private readonly Form _form;

    public WindowService(Form form)
    {
        _form = form;
    }

    /// <summary>
    /// 最小化窗口（带动画）
    /// </summary>
    public void Minimize()
    {
        if (_form.InvokeRequired)
            _form.Invoke(new Action(() => {
                if (_form is LauncherForm lf)
                    lf.AnimateMinimize();
                else
                    _form.WindowState = FormWindowState.Minimized;
            }));
        else
        {
            if (_form is LauncherForm lf)
                lf.AnimateMinimize();
            else
                _form.WindowState = FormWindowState.Minimized;
        }
    }

    /// <summary>
    /// 关闭窗口
    /// </summary>
    public void Close()
    {
        try
        {
            if (_form.InvokeRequired)
                _form.Invoke(new Action(() => _form.Close()));
            else
                _form.Close();
        }
        catch { }
    }

    /// <summary>
    /// 拖动窗口
    /// </summary>
    public void Drag(int deltaX, int deltaY)
    {
        if (_form.InvokeRequired)
            _form.Invoke(new Action(() => _form.Location = new Point(_form.Location.X + deltaX, _form.Location.Y + deltaY)));
        else
            _form.Location = new Point(_form.Location.X + deltaX, _form.Location.Y + deltaY);
    }
}
