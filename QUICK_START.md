# ⚡ Quick Start Guide - TrainBooking Pro

## 🚀 Get Running in 5 Minutes!

### Step 1: Prerequisites Check
```bash
# Check if .NET 8.0 is installed
dotnet --version
# Should show 8.0.x or later

# If not installed, download from: https://dotnet.microsoft.com/download
```

### Step 2: Get the Code
```bash
# Fork the repo on GitHub first, then:
git clone https://github.com/YOUR_USERNAME/TrainBookingAutomation.git
cd TrainBookingAutomation
```

### Step 3: Build & Run
```bash
# Install dependencies
dotnet restore

# Build the project
dotnet build

# Run the application
dotnet run
```

**That's it!** 🎉 The application should launch in a few seconds.

## 🎯 What You'll See

1. **Beautiful Dark Interface** with glowing orange buttons
2. **Train Search Form** with station dropdowns
3. **AI Recommendations** tab with smart suggestions
4. **Real-time Analytics** dashboard
5. **Automation Settings** panel

## 🧪 Try These Features

### Basic Search
1. Select "New Delhi" → "Mumbai Central"
2. Pick tomorrow's date
3. Choose "3AC" class
4. Click the glowing "SEARCH" button

### AI Assistant
1. Click the "🤖 AI Assistant" tab
2. See intelligent route recommendations
3. Get price optimization suggestions

### Automation
1. Toggle "Auto Booking" switch
2. Configure retry settings in "⚙️ Settings"
3. Let the system handle booking automatically

## 🔧 Troubleshooting

**App won't start?**
```bash
# Try these commands:
dotnet clean
dotnet restore --force
dotnet build --verbosity normal
dotnet run
```

**Missing .NET?**
- Download from: https://dotnet.microsoft.com/download
- Install .NET 8.0 SDK (not just runtime)

**Build errors?**
```bash
# Clear NuGet cache
dotnet nuget locals all --clear
dotnet restore
```

## 🎨 Interface Overview

```
┌───────────────────────────────────────────────────────────────────────────────┐
│ 🚄 TrainBooking Pro - Advanced Automation | by techSas❤️     [─][☐][✕]      │
├───────────────────────────────────────────────────────────────────────────────┤
│ Search Form          │    Quick Stats        │    AI Assistant               │
│ [Stations]           │   ✅ 95.2%           │   🤖 Smart Tips               │
│ [Date/Class]         │   ⚡ 2.3s            │   💡 Suggestions              │
│ [SEARCH]             │   📈 12 Active       │   📊 Analytics                │
├───────────────────────────────────────────────────────────────────────────────┤
│ 🟢 Ready for automation          │    Last Update: 14:23:45 | techSas❤️     │
└───────────────────────────────────────────────────────────────────────────────┘
```

## 📋 Next Steps

1. **Explore Features**: Try all tabs and settings
2. **Test Automation**: Enable auto-booking for demo
3. **Check Analytics**: View booking history and stats
4. **Customize Settings**: Configure notifications and preferences

## 🎥 Full Demo

For detailed screenshots and feature walkthrough, see: **[DEMO_SCREENSHOTS.md](./DEMO_SCREENSHOTS.md)**

---

**Happy Booking!** 🚄✨

*This application demonstrates advanced C# .NET development with modern UI design, AI features, and enterprise architecture.* 