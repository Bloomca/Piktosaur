# Piktosaur - LLM Instructions

## Project Overview

This is a WinUI3 application to view images in a folder recursively -- it scans all the directories recursively and renders them all inline.

- **Framework:** WinUI 3 / Windows App SDK 1.7
- **Target:** .NET 8, Windows 10 19041+
- **Platforms:** x86, x64, ARM64

## Build Commands

WinUI3 requires MSBuild (not `dotnet build`) and a specific platform. From the repo root:

```bash
# Restore packages
dotnet restore Piktosaur.sln

# Build main project
"/c/Program Files/Microsoft Visual Studio/2022/Community/MSBuild/Current/Bin/MSBuild.exe" Piktosaur/Piktosaur.csproj -p:Platform=x64 -v:q

# Build test project
"/c/Program Files/Microsoft Visual Studio/2022/Community/MSBuild/Current/Bin/MSBuild.exe" Piktosaur.Tests/Piktosaur.Tests.csproj -p:Platform=x64 -v:q

# Run tests (after building)
cmd.exe //c "dotnet test Piktosaur.Tests\\Piktosaur.Tests.csproj -p:Platform=x64 --no-build"
```

## Project Structure

```
Piktosaur/
├── Assets/          # App icons and images
├── Converters/      # XAML value converters
├── Models/          # Data models
├── Properties/      # App properties and publish profiles
├── Services/        # Business logic services
├── Utils/           # Utility/helper classes
├── ViewModels/      # MVVM view models
├── Views/           # XAML views and code-behind
├── App.xaml         # Application entry
└── MainWindow.xaml  # Main window
```

## Code Conventions

- UI code should follow MVVM pattern unless there are clear performance benefits
- One VM (AppStateVM) is global, rest are local
- Thumbnails are generated programmatically, not with the WinRT due to reliability

## Testing

- **Framework:** xUnit
- **Project location:** `Piktosaur.Tests/`
- **Run tests:** See build commands above

## Feature Specs

Feature specifications live in this folder (`llm-instructions/`). When implementing a feature from a spec, read the spec file first.

## Notes

N/A.

