# Piktosaur - LLM Instructions

## Project Overview

This is a WinUI3 application to view images in a folder recursively -- it scans all the directories recursively and renders them all inline.

- **Framework:** WinUI 3 / Windows App SDK 1.7
- **Target:** .NET 8, Windows 10 19041+
- **Platforms:** x86, x64, ARM64

## Build Commands

```bash
# Build (from repo root)
dotnet build Piktosaur.sln

# Build specific configuration
dotnet build Piktosaur.sln -c Release -p:Platform=x64

# Run
dotnet run --project Piktosaur/Piktosaur.csproj
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

<!-- Fill in once test project is added -->

Test framework:
Test project location:

## Feature Specs

Feature specifications live in this folder (`llm-instructions/`). When implementing a feature from a spec, read the spec file first.

## Notes

N/A.

