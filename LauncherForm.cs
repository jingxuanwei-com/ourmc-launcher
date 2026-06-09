using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ourmclauncher;

/// <summary>
/// 自定义窗口类 - 支持Windows 11样式和动画效果
/// </summary>
public class LauncherForm : Form
{
    // Windows API 导入
    [DllImport("dwmapi.dll")]
    private static extern int DwmExtendFrameIntoClientArea(IntPtr hWnd, ref MARGINS pMarInset);

    [DllImport("dwmapi.dll")]
    private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int attrValue, int attrSize);

    [DllImport("user32.dll")]
    private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

    [DllImport("user32.dll")]
    private static extern int GetWindowLong(IntPtr hWnd, int nIndex);

    [StructLayout(LayoutKind.Sequential)]
    private struct MARGINS
    {
        public int cxLeftWidth;
        public int cxRightWidth;
        public int cyTopHeight;
        public int cyBottomHeight;
    }

    private const int DWMWA_USE_IMMERSIVE_DARK_MODE_BEFORE_20H1 = 19;
    private const int DWMWA_USE_IMMERSIVE_DARK_MODE = 20;
    private const int DWMWA_WINDOW_CORNER_PREFERENCE = 33;
    private const int DWMWCP_ROUND = 2;
    private const int GWL_STYLE = -16;
    private const int WS_MINIMIZEBOX = 0x00020000;
    private const int WS_SYSMENU = 0x00080000;

    public LauncherForm()
    {
        Text = "oml - 我们的世界启动器";
        Size = new Size(852, 532);
        FormBorderStyle = FormBorderStyle.None;
        StartPosition = FormStartPosition.CenterScreen;
        BackColor = Color.FromArgb(26, 26, 46);
        ShowInTaskbar = true;
        
        // 启用双缓冲，减少闪烁
        DoubleBuffered = true;
        SetStyle(ControlStyles.OptimizedDoubleBuffer | 
                 ControlStyles.AllPaintingInWmPaint | 
                 ControlStyles.UserPaint, true);
        
        // 初始隐藏窗口，等加载完成后再显示，避免闪烁
        Opacity = 0;
    }

    protected override void OnHandleCreated(EventArgs e)
    {
        base.OnHandleCreated(e);
        
        // 添加必要的窗口样式，支持任务栏交互
        int style = GetWindowLong(this.Handle, GWL_STYLE);
        SetWindowLong(this.Handle, GWL_STYLE, style | WS_MINIMIZEBOX | WS_SYSMENU);
    }

    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);
        ApplyWindows11Style();
    }

    protected override CreateParams CreateParams
    {
        get
        {
            CreateParams cp = base.CreateParams;
            // 添加 WS_MINIMIZEBOX 和 WS_SYSMENU 样式，支持任务栏交互
            cp.Style |= WS_MINIMIZEBOX | WS_SYSMENU;
            return cp;
        }
    }

    /// <summary>
    /// 应用Windows 11窗口样式（深色模式、圆角）
    /// </summary>
    private void ApplyWindows11Style()
    {
        try
        {
            // 启用深色模式
            int useImmersiveDarkMode = 1;
            if (Environment.OSVersion.Version.Build >= 22000)
            {
                DwmSetWindowAttribute(this.Handle, DWMWA_USE_IMMERSIVE_DARK_MODE, ref useImmersiveDarkMode, sizeof(int));
            }
            else
            {
                DwmSetWindowAttribute(this.Handle, DWMWA_USE_IMMERSIVE_DARK_MODE_BEFORE_20H1, ref useImmersiveDarkMode, sizeof(int));
            }

            // 启用圆角
            int cornerPreference = DWMWCP_ROUND;
            DwmSetWindowAttribute(this.Handle, DWMWA_WINDOW_CORNER_PREFERENCE, ref cornerPreference, sizeof(int));
        }
        catch { }
    }

    /// <summary>
    /// 窗口淡入动画
    /// </summary>
    public async void FadeIn(int duration = 250)
    {
        for (double i = 0; i <= 1; i += 0.05)
        {
            Opacity = i;
            await Task.Delay(duration / 20);
        }
        Opacity = 1;
    }

    /// <summary>
    /// 窗口最小化动画（缩小 + 淡出）
    /// </summary>
    public async void AnimateMinimize()
    {
        try
        {
            var startSize = this.Size;
            var startLocation = this.Location;
            int steps = 10;
            for (int i = 1; i <= steps; i++)
            {
                double t = (double)i / steps;
                double ease = t * t; // ease-in
                this.BeginInvoke(new Action(() =>
                {
                    this.Opacity = 1.0 - ease * 0.5;
                    int newWidth = (int)(startSize.Width * (1.0 - ease * 0.3));
                    int newHeight = (int)(startSize.Height * (1.0 - ease * 0.3));
                    int newX = startLocation.X + (startSize.Width - newWidth) / 2;
                    int newY = startLocation.Y + (startSize.Height - newHeight) / 2;
                    this.SetBounds(newX, newY, newWidth, newHeight);
                }));
                await Task.Delay(15);
            }
            this.BeginInvoke(new Action(() =>
            {
                this.WindowState = FormWindowState.Minimized;
                // 恢复原始大小和透明度
                this.Size = startSize;
                this.Location = startLocation;
                this.Opacity = 1;
            }));
        }
        catch { }
    }

    /// <summary>
    /// 创建圆角区域
    /// </summary>
    public static Region CreateRoundedRegion(int width, int height, int radius)
    {
        var path = new System.Drawing.Drawing2D.GraphicsPath();
        path.AddArc(0, 0, radius, radius, 180, 90);
        path.AddArc(width - radius, 0, radius, radius, 270, 90);
        path.AddArc(width - radius, height - radius, radius, radius, 0, 90);
        path.AddArc(0, height - radius, radius, radius, 90, 90);
        path.CloseFigure();
        return new Region(path);
    }
}
