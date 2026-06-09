@echo off
title OML Launcher Builder
chcp 65001 >nul

echo ========================================
echo   OML Launcher - 构建脚本
echo ========================================
echo.

:: 清理 bin 目录
echo [1/4] 清理输出目录...
if exist bin rmdir /s /q bin
echo       OK

:: 构建
echo [2/4] 构建项目（Release）...
dotnet build --configuration Release --no-restore --force
if %ERRORLEVEL% NEQ 0 (
    echo.
    echo [错误] 构建失败！
    pause
    exit /b 1
)
echo       OK

:: 检查可执行文件
echo [3/4] 验证输出文件...
if not exist bin\Release\net8.0-windows\ourmclauncher.exe (
    echo [错误] 未找到输出文件！
    pause
    exit /b 1
)
echo       OK

:: 运行
echo [4/4] 启动应用程序...
echo.
start "" "bin\Release\net8.0-windows\ourmclauncher.exe"
echo       已启动！
echo.
echo ========================================
echo   构建成功完成！
echo ========================================
pause
