[![한국어](https://img.shields.io/badge/README.md-한국어-green.svg)](README.ko.md)

<p align="center">
  <img src="https://raw.githubusercontent.com/aprillz/MewUI/main/assets/logo/logo-256.png" alt="MewUI Logo" width="128"/>
</p>

# 🍎 MewUIBadApple

![.NET](https://img.shields.io/badge/.NET-10%2B-512BD4?logo=dotnet&logoColor=white)
![Windows](https://img.shields.io/badge/Windows-10%2B-0078D4?logo=windows&logoColor=white)
![NativeAOT](https://img.shields.io/badge/NativeAOT-Ready-2E7D32)
![ffmpeg](https://img.shields.io/badge/ffmpeg-Required-007808?logo=ffmpeg&logoColor=white)
![MewUI](https://img.shields.io/badge/MewUI-0.9.0-FF69B4)

---

**🎬 Bad Apple!!** animation player using the [MewUI](https://github.com/aprillz/MewUI) pixel UI framework.
Streams frames directly from ffmpeg via stdout pipe for **zero-preprocessing playback**.

---

## 🚀 Quick Start

```bash
# Just run it!
dotnet run src/MewUiBadApple.cs
```

That's it. No preprocessing required.

---

## 📋 Prerequisites

- **.NET 10+** — supports `dotnet run file.cs` with `#:package` directives
- **ffmpeg** — for real-time video decoding

### 📦 Installing ffmpeg

| Platform | Command |
|----------|---------|
| Windows (Chocolatey) | `choco install ffmpeg` |
| Windows (winget) | `winget install Gyan.FFmpeg` |
| Windows (manual) | Download from https://www.gyan.dev/ffmpeg/builds/, extract, add `bin` to PATH |
| macOS | `brew install ffmpeg` |
| Ubuntu/Debian | `sudo apt-get install ffmpeg` |
| Fedora | `sudo dnf install ffmpeg` |

---

## 📁 File Structure

```
MewUIBadApple/
├── src/
│   ├── MewUiBadApple.cs       # Main entry point (Aprillz.MewUI)
│   ├── PixelCanvas.cs         # Custom FrameworkElement for pixel rendering
│   ├── FfmpegFrameReader.cs   # ffmpeg process wrapper
│   ├── GlobalUsings.cs        # Global using directives
│   ├── Directory.Build.props  # Build configuration
│   └── badapple.mp4           # Source video
└── README.md
```

---

## ⚙️ CLI Parameters

```bash
dotnet run src/MewUiBadApple.cs [video_file]
# default:                       src/badapple.mp4
```

---

## 🏗️ Architecture

```
badapple.mp4 → [ffmpeg subprocess] → stdout (raw grayscale bytes) → PixelCanvas
```

| Component | Description |
|-----------|-------------|
| **ffmpeg** | `ffmpeg -loglevel error -i badapple.mp4 -vf scale=120:90,format=gray -f rawvideo -pix_fmt gray pipe:1` |
| **Streaming** | Reads raw bytes from `Process.StandardOutput.BaseStream` (~10KB per frame) |
| **Threshold** | grayscale value > 127 = white, else black |
| **Loop** | Restarts ffmpeg process at end of video |
| **Rendering** | `PixelCanvas` draws pixels directly via `IGraphicsContext` (2×2 cells) |

---

## 🎨 Customization

| Setting | Location | Default |
|---------|----------|---------|
| Resolution | `width`/`height` constants + ffmpeg scale filter | 120×90 |
| FPS | `FfmpegFrameReader.Fps` | 30 |
| Threshold | `FfmpegFrameReader.Threshold` | 127 |
| Colors | `PixelCanvas._bgColor` / `_fgColor` | Black/White |

---

## 🔧 Troubleshooting

| Problem | Solution |
|---------|----------|
| `ffmpeg` not found | Install ffmpeg and add to PATH. Verify: `ffmpeg -version` |
| `FileNotFoundException` | Ensure `badapple.mp4` exists in `src/` directory |
| Black screen | Check if video file is valid: `ffprobe badapple.mp4` |
| Choppy playback | ffmpeg decoding may be CPU-bound; try a shorter/smaller video |

---

## 🔗 References

- [MewUI](https://github.com/Aprillz/MewUI) — Lightweight pixel UI framework
- [ffmpeg](https://ffmpeg.org/documentation.html) — Video processing
- [Bad Apple!!](https://www.youtube.com/watch?v=FtutLA63Cp8) — Original video

---

## 📄 License

Personal learning and demonstration purposes. "Bad Apple!!" music is copyrighted.

---

## 🙏 Acknowledgements

Special thanks to [aprillz](https://github.com/aprillz) for creating [MewUI](https://github.com/aprillz/MewUI), the lightweight pixel UI framework that powers this project.
