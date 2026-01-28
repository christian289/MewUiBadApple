# MewUI Bad Apple!!

Script-based project (no .csproj). Two C# scripts run via `dotnet <file>.cs`. Requires .NET 10+.

## Setup

1. Extract frames: `ffmpeg -i badapple.mp4 -vf scale=120:90 frames/frame_%06d.png`
2. Convert: `dotnet PngToMatrix.cs`
3. Play: `dotnet MewUiBadApple.cs`

## Files

- **PngToMatrix.cs** — PNG → text converter (OpenCvSharp4)
- **MewUiBadApple.cs** — Streaming player (Aprillz.MewUI, NativeAOT)
- **badapple.mp4** — Source video
- `frames/`, `badapple_meta.txt`, `badapple_frames.txt` — Generated, not in git

## Data Format

- `badapple_meta.txt`: `width,height,frameCount` (single line)
- `badapple_frames.txt`: rows of `0`/`1` chars (width per line), blank line between frames

## Key Decisions

- Text format instead of JSON — no `System.Text.Json` dependency in player
- StreamReader streaming — ~1MB memory vs ~1.1GB full load
- `Seek(0) + DiscardBufferedData()` for loop rewind

## Constraints

- Frame resolution is static (set at ffmpeg extraction time)
- Frame file size scales with resolution (480×360 = ~1 GB, 120×90 = ~68 MB)
- FPS: `BadAppleFrames.Fps` constant (default 30)
