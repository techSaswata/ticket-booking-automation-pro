@echo off
echo.
echo ================================================
echo   🚄 TrainBooking Pro - Advanced Automation
echo ================================================
echo.

echo ⏳ Checking .NET installation...
dotnet --version > nul 2>&1
if %errorlevel% neq 0 (
    echo ❌ .NET 8.0 SDK not found!
    echo Please install from: https://dotnet.microsoft.com/download
    pause
    exit /b 1
)

echo ✅ .NET found!
echo.

echo ⏳ Restoring NuGet packages...
dotnet restore
if %errorlevel% neq 0 (
    echo ❌ Failed to restore packages!
    pause
    exit /b 1
)

echo ✅ Packages restored!
echo.

echo ⏳ Building project...
dotnet build --configuration Release
if %errorlevel% neq 0 (
    echo ❌ Build failed!
    pause
    exit /b 1
)

echo ✅ Build successful!
echo.

echo 🚀 Starting TrainBooking Pro...
echo.
echo ================================================
echo   Application should launch in a few seconds...
echo ================================================
echo.

dotnet run

if %errorlevel% neq 0 (
    echo.
    echo ❌ Application failed to start!
    echo Check the error messages above.
    pause
)

echo.
echo Thanks for using TrainBooking Pro! 🚄✨
pause 