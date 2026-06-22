<div align="center">

# ⚡ KN2

**Universal Computer-Vision Aim Assist**

[![.NET](https://img.shields.io/badge/.NET-9.0-512BD4?style=for-the-badge&logo=dotnet)](https://dotnet.microsoft.com/download/dotnet/9.0)
[![WPF](https://img.shields.io/badge/Framework-WPF-512BD4?style=for-the-badge&logo=windows)](https://learn.microsoft.com/en-us/dotnet/desktop/wpf/)
[![Platform](https://img.shields.io/badge/Platform-Windows_10|11-0078D4?style=for-the-badge&logo=windows)](https://www.microsoft.com/windows)
[![License](https://img.shields.io/badge/License-All_Rights_Reserved-red?style=for-the-badge)](LICENSE)

</div>

---

## Overview

KN2 is a universal, game-agnostic aim assist powered by real-time screen capture and computer vision. Unlike traditional tools that rely on brittle memory offsets for each game, KN2 works entirely through pixel analysis — detecting targets by color, contrast, and shape directly from the screen.

Built with **.NET 9** and **WPF**, featuring a custom cyberpunk "Neon Phantom" UI with full hardware acceleration.

### How It Works

1. **Screen Capture** — Captures a region around the crosshair using GDI `BitBlt`
2. **Color Detection** — Scans pixels against a configurable color profile (RGB target + tolerance)
3. **Flood-Fill Clustering** — Groups matching pixels into connected clusters
4. **Target Selection** — Selects the best target by proximity, confidence, and contrast
5. **Input Simulation** — Moves the mouse via `SendInput` with smoothing, noise, and humanization

No game memory access required for 5 of 7 features. No AI model dependencies. No external API calls.

---

## Features

### Detection Config
| | |
|---|---|
| **Color Profile** | Configurable target RGB, tolerance, min/max cluster size, contrast threshold |
| **Scan Radius** | Adjustable detection area (default 150px) |
| **Performance** | Configurable scan stride for CPU optimization |

### Crosshair Placement Assist
Gently pulls the crosshair toward detected targets with micro-adjustments. Configurable drag strength and activation radius.

### Shot Override
Auto-fires when a target enters the trigger radius. Includes configurable delay with jitter and 150ms fire cooldown.

### Visibility Aim Lock
Blocks aiming when the target is obscured (smoke, fog, low contrast). Uses local contrast analysis of the captured region.

### Flick Assist
Activates only on fast flicks (60°+). Adds controlled landing error for natural movement. Configurable threshold and strength.

### No-Recoil Noise
Per-bullet recoil compensation with randomized noise for humanization. Compensates both pitch and yaw independently.

### HWID Spoofing
Rotates window class names, titles, and styles at configurable intervals to evade basic detection.

### Standstill Accuracy
Improves accuracy recovery when the player stops moving. *(Requires game memory access — CS2 offset placeholder)*

---

## UI — Neon Phantom

The interface is built from scratch using a custom WPF component library with a cyberpunk aesthetic:

- **Lateral Sidebar** — 52px icon-only navigation with gradient accent bars
- **Hex Grid Background** — Rotating honeycomb pattern with scanline overlay
- **Neon Palette** — Deep dark surfaces (#07070D) with B44CFF / FF2D95 / 00E5FF accents
- **Component Library** — 12 custom controls (toggles, sliders, dropdowns, keybind changers, color wheel, display selector)
- **Material Design 3** — Dark theme foundation with custom overrides

---

## Tech Stack

| Layer | Technology |
|---|---|
| **Runtime** | .NET 9.0 (Windows) |
| **UI Framework** | Windows Presentation Foundation (WPF) |
| **Theme** | Material Design 3 + Custom Neon Phantom |
| **Screen Capture** | GDI `BitBlt` (native P/Invoke) |
| **Input** | `SendInput` Win32 API |
| **Memory** | `ReadProcessMemory` / `WriteProcessMemory` (optional) |
| **Dependencies** | MaterialDesignThemes 5.2.1, AntWpf, System.Management |

---

## Getting Started

### Prerequisites

- Windows 10 / 11
- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)

### Build & Run

```bash
# Clone (if you have access)
git clone https://github.com/your-org/KN2
cd KN2

# Restore and build
dotnet restore
dotnet build -c Release

# Run
dotnet run -c Release

# Publish single-file
dotnet publish -c Release -r win-x64 --self-contained true -P:PublishSingleFile=true
```

Output: `bin/Release/net9.0-windows/ShadowCheat.exe`

### Project Structure

```
KN2/
├── App.xaml(.cs)          # Application entry, global error handling
├── LoginWindow.xaml(.cs)  # Authentication UI
├── MainWindow.xaml(.cs)   # Main application window + sidebar
├── Class/
│   ├── Features/          # 7 feature implementations + detection engine
│   ├── OverlayWindow.cs   # Transparent click-through overlay
│   ├── ScreenCapture.cs   # GDI BitBlt capture
│   ├── InputSimulator.cs  # SendInput wrapper
│   └── Dictionary.cs      # Centralized configuration state
├── Controls/              # Page controls (Aim, Model, Settings, About)
├── UILibrary/             # 12 custom WPF components
└── Other/                 # Utilities
```

---

## Legal Notice

**KN2 is provided for educational and research purposes only.**

- **Forking, redistribution, or rehosting of this project is strictly prohibited.**
- Unauthorized copies, mirrors, or derivative works will be reported and taken down.
- You may not use this software in violation of any applicable laws or third-party terms of service.
- The authors assume no liability for any misuse of this software.

All rights reserved. © 2026

---

<div align="center">

**Built with .NET and WPF**

</div>
