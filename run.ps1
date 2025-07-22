# TrainBooking Pro - Launch Script
# PowerShell version for modern Windows environments

param(
    [switch]$Debug,
    [switch]$Clean
)

# Set colors for better output
$Host.UI.RawUI.ForegroundColor = "White"

function Write-Header {
    Write-Host ""
    Write-Host "================================================" -ForegroundColor Cyan
    Write-Host "   🚄 TrainBooking Pro - Advanced Automation" -ForegroundColor Yellow
    Write-Host "================================================" -ForegroundColor Cyan
    Write-Host ""
}

function Write-Step {
    param($Message)
    Write-Host "⏳ $Message..." -ForegroundColor Blue
}

function Write-Success {
    param($Message)
    Write-Host "✅ $Message!" -ForegroundColor Green
}

function Write-Error {
    param($Message)
    Write-Host "❌ $Message!" -ForegroundColor Red
}

function Test-DotNet {
    try {
        $version = dotnet --version 2>$null
        if ($version) {
            Write-Success ".NET $version found"
            return $true
        }
    }
    catch {
        Write-Error ".NET 8.0 SDK not found"
        Write-Host "Please install from: https://dotnet.microsoft.com/download" -ForegroundColor Yellow
        return $false
    }
    return $false
}

function Invoke-DotNetCommand {
    param($Command, $Arguments, $Description)
    
    Write-Step $Description
    $process = Start-Process -FilePath "dotnet" -ArgumentList "$Command $Arguments" -Wait -PassThru -NoNewWindow
    
    if ($process.ExitCode -eq 0) {
        Write-Success $Description
        return $true
    } else {
        Write-Error "$Description failed"
        return $false
    }
}

# Main execution
Clear-Host
Write-Header

# Check .NET installation
if (-not (Test-DotNet)) {
    Read-Host "Press Enter to exit"
    exit 1
}

# Clean if requested
if ($Clean) {
    if (Invoke-DotNetCommand "clean" "" "Cleaning previous builds") {
        Write-Host ""
    } else {
        Read-Host "Press Enter to exit"
        exit 1
    }
}

# Restore packages
if (-not (Invoke-DotNetCommand "restore" "" "Restoring NuGet packages")) {
    Read-Host "Press Enter to exit"
    exit 1
}

# Build project
$buildConfig = if ($Debug) { "Debug" } else { "Release" }
if (-not (Invoke-DotNetCommand "build" "--configuration $buildConfig" "Building project ($buildConfig)")) {
    Read-Host "Press Enter to exit"
    exit 1
}

# Launch application
Write-Host ""
Write-Host "🚀 Starting TrainBooking Pro..." -ForegroundColor Magenta
Write-Host ""
Write-Host "================================================" -ForegroundColor Cyan
Write-Host "   Application should launch in a few seconds..." -ForegroundColor Yellow
Write-Host "================================================" -ForegroundColor Cyan
Write-Host ""

$runConfig = if ($Debug) { "--configuration Debug" } else { ""
$runProcess = Start-Process -FilePath "dotnet" -ArgumentList "run $runConfig" -Wait -PassThru -NoNewWindow

if ($runProcess.ExitCode -ne 0) {
    Write-Host ""
    Write-Error "Application failed to start"
    Write-Host "Check the error messages above." -ForegroundColor Yellow
} else {
    Write-Host ""
    Write-Host "Thanks for using TrainBooking Pro! 🚄✨" -ForegroundColor Green
}

Read-Host "Press Enter to exit" 