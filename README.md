# MewUIBadApple

"Bad Apple!!" animation player using the MewUI pixel UI framework. Streams frames from text files for memory-efficient playback (~1MB vs ~1.1GB).

## Quick Start

```bash
# 1. Extract frames (choose your preferred scale)
mkdir frames
ffmpeg -i badapple.mp4 -vf scale=120:90 frames/frame_%06d.png

# 2. Convert PNG frames to text
dotnet PngToMatrix.cs

# 3. Play
dotnet MewUiBadApple.cs
```

## Prerequisites

- **.NET 10+** — supports `dotnet file.cs` with `#:package` directives
- **ffmpeg** — for video frame extraction

### Installing ffmpeg

| Platform | Command |
|----------|---------|
| Windows (Chocolatey) | `choco install ffmpeg` |
| Windows (winget) | `winget install Gyan.FFmpeg` |
| Windows (manual) | Download from https://www.gyan.dev/ffmpeg/builds/, extract, add `bin` to PATH |
| macOS | `brew install ffmpeg` |
| Ubuntu/Debian | `sudo apt-get install ffmpeg` |
| Fedora | `sudo dnf install ffmpeg` |

## ffmpeg Examples

```bash
# Original resolution
ffmpeg -i badapple.mp4 frames/frame_%06d.png

# Scale down (recommended)
ffmpeg -i badapple.mp4 -vf scale=120:90 frames/frame_%06d.png

# Keep aspect ratio (auto-calculate height)
ffmpeg -i badapple.mp4 -vf scale=160:-1 frames/frame_%06d.png

# First 10 seconds only
ffmpeg -i badapple.mp4 -t 10 frames/frame_%06d.png

# First 300 frames only
ffmpeg -i badapple.mp4 -frames:v 300 frames/frame_%06d.png

# Custom FPS
ffmpeg -i badapple.mp4 -vf fps=15 frames/frame_%06d.png

# Combine filters
ffmpeg -i badapple.mp4 -vf "scale=120:90,fps=15" -frames:v 300 frames/frame_%06d.png

# Check video info
ffprobe badapple.mp4
```

**Resolution vs. generated file size:**

| Scale | Resolution | badapple_frames.txt |
|-------|------------|---------------------|
| Original | 480×360 | ~1.06 GB |
| `scale=320:240` | 320×240 | ~470 MB |
| `scale=160:120` | 160×120 | ~120 MB |
| `scale=120:90` | 120×90 | ~68 MB |
| `scale=80:60` | 80×60 | ~30 MB |

## File Structure

```
MewUIBadApple/
├── PngToMatrix.cs            # PNG → text converter (OpenCvSharp4)
├── MewUiBadApple.cs          # Streaming player (Aprillz.MewUI)
├── badapple.mp4              # Source video
│
│  (Generated — not in git)
├── frames/                   # PNG frames from ffmpeg
├── badapple_meta.txt         # "width,height,frameCount"
└── badapple_frames.txt       # '0'/'1' text per row, blank line between frames
```

## CLI Parameters

**PngToMatrix.cs:**
```bash
dotnet PngToMatrix.cs [frames_dir] [meta_file] [frames_file]
# defaults:            frames       badapple_meta.txt  badapple_frames.txt
```

**MewUiBadApple.cs:**
```bash
dotnet MewUiBadApple.cs [meta_file] [frames_file]
# defaults:             badapple_meta.txt  badapple_frames.txt
```

## Architecture

```
badapple.mp4 → [ffmpeg] → frames/*.png → [PngToMatrix.cs] → meta.txt + frames.txt
                                                                    ↓
                                              [MewUiBadApple.cs] StreamReader → PixelCanvas
```

- **Streaming**: reads one frame at a time via `StreamReader` (~1MB memory)
- **Loop**: rewinds with `Seek(0) + DiscardBufferedData()` at end of file
- **Rendering**: `PixelCanvas` draws pixels directly via `IGraphicsContext` (2×2 cells)

## Customization

- **Resolution**: set at ffmpeg extraction time (see examples above)
- **FPS**: change `BadAppleFrames.Fps` constant in `MewUiBadApple.cs` (default: 30)
- **Colors**: change `_bgColor` / `_fgColor` in `PixelCanvas` class

## Troubleshooting

| Problem | Solution |
|---------|----------|
| `ffmpeg` not found | Install ffmpeg and add to PATH. Verify: `ffmpeg -version` |
| `FileNotFoundException` | Run `dotnet PngToMatrix.cs` first to generate data files |
| Out of memory | Use smaller scale: `ffmpeg -i badapple.mp4 -vf scale=120:90 ...` |
| Slow rendering | Lower resolution or reduce FPS |

## References

- [MewUI](https://github.com/Aprillz/MewUI) · [OpenCvSharp4](https://github.com/shimat/opencvsharp) · [ffmpeg](https://ffmpeg.org/documentation.html) · [Bad Apple!!](https://www.youtube.com/watch?v=FtutLA63Cp8)

## License

Personal learning and demonstration purposes. "Bad Apple!!" music is copyrighted.
