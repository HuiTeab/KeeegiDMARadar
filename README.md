# KeeegiDMARadar

A Direct Memory Access (DMA) Radar tool for reading and visualizing game memory data.

## Overview

KeeegiDMARadar is a C# application that provides DMA-based memory reading capabilities with an overlay radar interface. This tool is designed for educational and research purposes.

## Features

- Direct Memory Access (DMA) support for efficient memory reading
- Configurable memory offsets and patterns
- Real-time radar overlay
- Support for multiple process targets
- JSON-based configuration

## Requirements

- Windows OS
- .NET 8.0 or higher
- DMA hardware (or kernel-level memory access)
- Visual Studio 2022 or JetBrains Rider (for development)

## Project Structure

```
KeeegiDMARadar/
├── src/
│   ├── KeeegiDMARadar/          # Main application
│   │   ├── Core/                 # Core DMA functionality
│   │   ├── Models/               # Data models
│   │   ├── Overlay/              # Radar overlay UI
│   │   └── Utils/                # Utility functions
│   └── KeeegiDMARadar.Tests/    # Unit tests
├── config/
│   └── config.json               # Configuration file
└── README.md
```

## Installation

1. Clone the repository:
```bash
git clone https://github.com/HuiTeab/KeeegiDMARadar.git
cd KeeegiDMARadar
```

2. Build the project:
```bash
dotnet build
```

3. Configure the application by editing `config/config.json` with your target process and memory offsets.

## Configuration

Edit `config/config.json` to customize:
- Target process name
- Memory offsets
- Radar display settings
- Update intervals

Example configuration:
```json
{
  "targetProcess": "game.exe",
  "updateInterval": 16,
  "radar": {
    "size": 400,
    "range": 100
  },
  "offsets": {
    "base": "0x12345678"
  }
}
```

## Usage

Run the application:
```bash
dotnet run --project src/KeeegiDMARadar
```

## Building

To build the release version:
```bash
dotnet build -c Release
```

To run tests:
```bash
dotnet test
```

## Legal & Ethical Notice

⚠️ **IMPORTANT**: This tool is provided for educational and research purposes only.

- Using memory reading tools may violate Terms of Service of games and applications
- Only use this tool on software you own or have explicit permission to analyze
- The authors are not responsible for any misuse of this software
- Using this tool in online games may result in account bans

## Contributing

Contributions are welcome! Please feel free to submit pull requests or open issues.

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Disclaimer

This project is for educational purposes only. The developers do not condone cheating in online games or any violation of software Terms of Service.