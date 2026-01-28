---
name: streaming-video-frames
description: "Streams raw video frames via ffmpeg stdout pipe. Use for real-time video processing without preprocessing."
---

# ffmpeg Stdout Pipe Streaming

Stream raw video frames directly from ffmpeg without intermediate files.

## Pattern

```csharp
var psi = new ProcessStartInfo
{
    FileName = "ffmpeg",
    Arguments = $"-loglevel error -i \"{videoPath}\" -vf scale={width}:{height},format=gray -f rawvideo -pix_fmt gray pipe:1",
    RedirectStandardOutput = true,
    RedirectStandardError = true,
    CreateNoWindow = true
};

var process = Process.Start(psi);
process.BeginErrorReadLine();  // Drain stderr to prevent blocking

var stream = process.StandardOutput.BaseStream;
```

## Read Frame

```csharp
public static byte[]? ReadNextFrame(Stream stream, byte[] buffer, int frameSize)
{
    int bytesRead = 0;
    while (bytesRead < frameSize)
    {
        int count = stream.Read(buffer, bytesRead, frameSize - bytesRead);
        if (count == 0) return null;  // EOF
        bytesRead += count;
    }
    return buffer;
}
```

## Loop Playback

Restart ffmpeg process on EOF:

```csharp
if (frame == null)
{
    reader.Dispose();
    reader = FfmpegFrameReader.Start(videoPath);
    frame = ReadNextFrame(...);
}
```

## Memory: ~10KB per frame (vs ~1GB for full load)
