---
paths:
  - "**/*.cs"
---

# Streaming Frame Rendering Guide

## Problem: OutOfMemory with Full Frame Loading

When loading all frames into memory (e.g., `byte[][]` for 6572 frames × 480×360 pixels), memory allocation exceeds ~1.1GB causing OutOfMemory or excessive GC pressure.

## Solution: Stream One Frame at a Time

### Text Format

**Metadata file:** single line with `width,height,frameCount`

**Frame file:** each row is '0'/'1' string (width chars), frames separated by blank lines

### Read Next Frame

```csharp
public static byte[]? ReadNextFrame(StreamReader reader, int width, int height)
{
    var flat = new byte[width * height];

    for (int y = 0; y < height; y++)
    {
        var line = reader.ReadLine();
        if (line == null) return null;
        if (line.Length == 0)
        {
            line = reader.ReadLine();
            if (line == null) return null;
        }

        for (int x = 0; x < width; x++)
            flat[y * width + x] = (byte)(line[x] - '0');
    }

    return flat;
}
```

### Loop Playback via Rewind

```csharp
public void RewindFrames()
{
    framesReader.DiscardBufferedData();
    framesReader.BaseStream.Seek(0, SeekOrigin.Begin);
}
```

## Memory Comparison

| Approach | Memory |
|----------|--------|
| JSON full load (byte[][]) | ~1.1 GB |
| Text streaming (1 frame at a time) | ~346 KB |

**Reduction: ~99.97%**

## Key Lessons

1. **Streaming Parse ≠ Streaming Consumption** - Utf8JsonReader streaming still causes O(n) memory if collecting results
2. **Simplest Format Wins** - Plain text requires no library dependency
3. **Rewind without Reopening** - Use `DiscardBufferedData()` + `Seek(0)` for loop playback
4. **Blank Line as Separator** - Simple and unambiguous frame boundary

## Checklist

- Create metadata file (`width,height,frameCount`)
- Generate plain text frame file (rows of '0'/'1', blank line separators)
- Implement `ReadNextFrame()` method
- Implement StreamReader rewind logic
- Monitor memory usage
