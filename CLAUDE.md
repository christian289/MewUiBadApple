# MewUI Bad Apple!!

Script-based project (no .csproj). C# scripts run via `dotnet run src/<file>.cs`. Requires .NET 10+ and ffmpeg.

## Setup

1. Place `badapple.mp4` in `src/` directory (or project root)
2. Run: `dotnet run src/MewUiBadApple.cs`

## Files

```
MewUiBadApple/
├── src/
│   ├── MewUiBadApple.cs       # Main entry point
│   ├── PixelCanvas.cs         # Custom FrameworkElement for pixel rendering
│   ├── FfmpegFrameReader.cs   # ffmpeg process wrapper for frame streaming
│   ├── GlobalUsings.cs        # Global using directives
│   ├── Directory.Build.props  # Includes additional .cs files for dotnet run
│   └── badapple.mp4           # Source video
├── CLAUDE.md                  # This file
└── README.md                  # Project documentation
```

## How It Works

- ffmpeg runs as subprocess: `ffmpeg -loglevel error -i badapple.mp4 -vf scale=120:90,format=gray -f rawvideo -pix_fmt gray pipe:1`
- Reads raw grayscale bytes from `Process.StandardOutput.BaseStream`
- Each frame: 120×90 = 10,800 bytes
- Threshold 127 applied for binary conversion (0/1)
- On EOF, ffmpeg process restarts for seamless loop

## Key Decisions

- Direct ffmpeg pipe — no intermediate files, no preprocessing step
- BaseStream.Read for efficient binary streaming
- Process restart for looping (vs file seek)
- MewUI absolute coordinates — Render uses `Bounds.X/Y` offset

## Constraints

- Requires ffmpeg in PATH
- Frame resolution hardcoded (120×90)
- FPS: `FfmpegFrameReader.Fps` constant (default 30)
