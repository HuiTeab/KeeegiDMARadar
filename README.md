# KeeegiDMARadar

## Overview
KeeegiDMARadar is a .NET 10.0 Windows application for Direct Memory Access (DMA) radar, designed specifically for ARC Riders. Based on the [Lone-EFT-DMA-Radar](https://github.com/lone-dma/Lone-EFT-DMA-Radar)

## Features
- **DMA Memory Access**: Uses VmmSharpEx for high-performance, external memory reading.
- **Configurable FPGA Algorithms**: Supports multiple DMA/FPGA read strategies (see `FpgaAlgo`).
- **Robust Config System**: JSON-based, with backup and corruption recovery.
- **Worker Thread Model**: Background threads for memory polling and input management.
- **Hotkey System**: Extensible hotkey manager for user actions.
- **Modern UI**: ImGui.NET + Silk.NET + SkiaSharp for a responsive, cross-platform-ready interface.
- **Logging**: Console and debug logging with Win32 interop for native console support.

## Project Structure
```
KeeegiDMARadar/
│   ARCDMAConfig.cs         # Main config, DMA/FPGA settings, persistent cache
│   Logging.cs             # Console/debug logging utilities
│   Program.cs             # App entry, config loading, DI, update checks
│   KeeegiDMARadar.csproj  # .NET 10.0 WinExe, AOT, Skia, ImGui, VmmSharpEx
│
├── DMA/
│     Memory.cs            # Core DMA memory logic, process attach, read, cache
│     InputManager.cs      # Handles input via VmmSharpEx, hotkey polling
│     FpgaAlgo.cs          # Enum for DMA/FPGA read algorithms
│     MemoryExtensions.cs  # (Reserved for future helpers)
│
├── Misc/
│     RateLimiter.cs       # Simple rate limiter struct
│     JSON/                # JSON converters, serializer context
│     Workers/             # Worker thread base, args, sleep modes
│
├── UI/
│     RadarWindow.cs       # Main window, render loop, input, ImGui/Skia
│     Hotkeys/             # Hotkey manager, attributes, delegates, types
│     Misc/                # Clipboard, loading, message box helpers
│     Skia/                # Skia font/paint utilities
│
├── bin/, obj/             # Build outputs/intermediates
```

## Getting Started
1. **Requirements:** Windows 10/11, .NET 10.0 SDK, Visual Studio 2022+
2. **Build:** Open `KeeegiDMARadar.slnx` and build the solution.
3. **Run:** Launch the app. The radar UI will open; DMA will attempt to initialize and attach to the configured process.
4. **Config:** Edit `Config-ARC.json` in `%AppData%/Keeegi-DMA/` for DMA/FPGA settings.

## Technical Notes
- **DMA Core:**
	- Uses VmmSharpEx for memory access, with support for memory maps and auto-refresh.
	- Main memory logic in `DMA/Memory.cs` (see methods: `ReadSpan`, `ReadValue`, `ReadPtrChain`, etc).
	- Worker thread model for continuous polling and process/game state management.
- **ARC Riders Focus:**
	- The project is built specifically for ARC Riders, with modular config and future game-specific logic.
- **UI:**
	- ImGui.NET for UI, SkiaSharp for 2D rendering, Silk.NET for window/input/GL context.
	- Hotkey system is extensible and managed via `UI/Hotkeys/HotkeyManager.cs`.

## Credits
- Uses [VmmSharpEx](https://github.com/ufrisk/VmmSharpEx), [ImGui.NET](https://github.com/mellinoe/ImGui.NET), [Silk.NET](https://github.com/dotnet/Silk.NET), [SkiaSharp](https://github.com/mono/SkiaSharp).

## License
See [LICENSE](LICENSE) for details.