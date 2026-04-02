# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Build & Run

```bash
# Build (Debug)
dotnet build MyriaRPG.sln

# Build (Release)
dotnet build -c Release MyriaRPG.sln

# Run
dotnet run --project MyriaRPG

# Clean
dotnet clean
```

> **Note:** MyriaLib is referenced as a precompiled binary from `..\MyriaLib\bin\Debug\net8.0\`. If MyriaLib changes are needed, build that project separately first.

There are no automated tests in this repository.

## Architecture Overview

MyriaRPG is a WPF desktop RPG (.NET 8.0, WinExe) following **MVVM** with a static **Service Locator** for cross-cutting concerns.

### Two-Project Solution

- **MyriaRPG** — WPF frontend (this repo). All UI, ViewModels, and WPF-specific services.
- **MyriaLib** — Shared game logic library (referenced as a compiled binary). Contains core entities (`Player`, `Item`, `Room`, `Monster`, `Skill`), services (`GameService`, `RoomService`, `UserAccountService`, `SettingsService`), combat system, localization engine, and map/navigation systems.

### Layer Breakdown

```
View (.xaml/.xaml.cs)
  └─> ViewModel (INotifyPropertyChanged + RelayCommand)
        └─> Services (Navigation, ThemeManager — static singletons)
              └─> MyriaLib (game logic, entities, persistence)
```

**View** (`View/`) — XAML pages, windows, and user controls. Minimal code-behind; logic lives in ViewModels.

**ViewModel** (`ViewModel/`) — One ViewModel per page/window/control.
- `BaseViewModel` — base `INotifyPropertyChanged` implementation.
- `BaseLocalizedViewModel` — extends base with attribute-driven auto-localization via `LocalizedKeyAttribute`.

**Services** (`Services/`) — Static singletons:
- `Navigation.cs` — manages multiple named `Frame` instances (Main, Startup, Game, IngameWindow, IngameMenu, IngameSettings, Settings). All page routing goes through here.
- `ThemeManager.cs` — swaps Light/Dark XAML resource dictionaries at runtime.

**Model** (`Model/`) — UI-specific wrappers:
- `ItemVm.cs` — wraps MyriaLib item entities with rarity color, icon loading, localization, and equipment slot info for data binding.
- `ShopEquipmentItemVm.cs` — equipment-specific variant.

**Utils** (`Utils/`) — `RelayCommand.cs` (generic/non-generic ICommand), `LocalizationAutoWire.cs`, `InventoryHelpers.cs`.

**Systems** (`Systems/`) — `MapNode`/`MapEdge`/`MapSnapshot` data structures for the canvas-based local map rendering in `LocalMapControl.xaml`.

### Navigation Flow

```
App.OnStartup → MainWindow (hosts frames) → Page_StartupMenue
  → Page_Login / Page_CharacterSelection / Page_CharacterCreation
  → Page_Game (hub)
      → Page_Room → Page_Fight
      → GameWindow popup (IngameWindow pages: inventory, character sheet, skills, quests, map, NPC interaction)
```

### Game Data

All game content is defined in JSON files under `Data/common/` (items, monsters, skills, npcs, rooms, quests, cities, dungeons, shops, etc.) and loaded by MyriaLib at startup.

Localization strings live in `Data/locales/en.json` and `Data/locales/de.json`.

UI themes are XAML resource dictionaries in `Assets/` (`Light.xaml`, `Dark.xaml`, `Layout.xaml`, `Icons.xaml`).

### App Startup Sequence

```csharp
// App.xaml.cs OnStartup:
SettingsService.Load();
Localization.Load(Settings.Current.LanguageSettings.Local);
ThemeManager.Apply(Settings.Current.VisualSettings.DarkMode);
GameService.InitializeGame();
// then MainWindow loads Page_StartupMenue
```
