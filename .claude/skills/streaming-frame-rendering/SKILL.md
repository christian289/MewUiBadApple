---
name: streaming-frame-rendering
description: Guides streaming frame data loading for animation rendering to avoid OutOfMemory. Use when building frame-by-frame animation players, processing large sequential data files, or encountering OOM errors with large datasets loaded into memory.
---

# Streaming Frame Rendering Guide

## Problem: OutOfMemory with Full Frame Loading

### Symptom

- JSON file containing all frames (6572 frames × 480×360 pixels) causes ~1.1GB memory allocation
- `byte[][]` array holding all frames simultaneously → OutOfMemory or excessive GC pressure
- Utf8JsonReader streaming parse still materializes all frames into `List<byte[]>`

### Root Cause

- Even with streaming JSON parsing, the result `byte[][] frames` holds ALL frame data in memory
- 6572 frames × 172,800 bytes/frame = ~1.1 GB for frame data alone
- JSON structure overhead (brackets, commas, number formatting) inflates file size further

### Solution: Text Frame Format + StreamReader Streaming

#### Step 1: Replace JSON with Plain Text Format

**Metadata file:** single line with `width,height,frameCount`

**Frame file:** each row is '0'/'1' string (width chars), frames separated by blank lines

Example structure:

```
# metadata.txt
480,360,6572

# frames.txt
00000000000000000000...  (480 chars)
11111111111111111111...  (480 chars)
...
                         (blank line)
00000000000000000000...  (next frame, row 1)
11111111111111111111...
...
```

#### Step 2: Stream One Frame at a Time

```csharp
/// <summary>
/// Reads the next frame from StreamReader
/// </summary>
/// <param name="reader">StreamReader reading frame data</param>
/// <param name="width">Frame width in pixels</param>
/// <param name="height">Frame height in pixels</param>
/// <returns>Frame byte array, or null if no more frames</returns>
public static byte[]? ReadNextFrame(StreamReader reader, int width, int height)
{
    var flat = new byte[width * height];

    for (int y = 0; y < height; y++)
    {
        var line = reader.ReadLine();

        // Reached end of file
        if (line == null)
            return null;

        // Skip blank lines (frame separators)
        if (line.Length == 0)
        {
            line = reader.ReadLine();
            if (line == null)
                return null;
        }

        // Parse each pixel value ('0' → 0, '1' → 1)
        for (int x = 0; x < width; x++)
        {
            flat[y * width + x] = (byte)(line[x] - '0');
        }
    }

    return flat;
}
```

#### Step 3: Implement Loop Playback via StreamReader Rewind

```csharp
/// <summary>
/// Rewind to beginning when reaching last frame
/// </summary>
public void RewindFrames()
{
    framesReader.DiscardBufferedData();
    framesReader.BaseStream.Seek(0, SeekOrigin.Begin);
}
```

**Usage example:**

```csharp
using var fileStream = new FileStream("frames.txt", FileMode.Open, FileAccess.Read);
using var reader = new StreamReader(fileStream, Encoding.UTF8, bufferSize: 4096);

while (isPlaying)
{
    var frame = ReadNextFrame(reader, 480, 360);

    if (frame == null)
    {
        // Reached end of frames, loop
        RewindFrames();
        frame = ReadNextFrame(reader, 480, 360);
    }

    // Render frame
    RenderFrame(frame);

    // Control frame rate
    Thread.Sleep((int)(1000.0 / frameRate));
}
```

## Memory Comparison

| Approach | Memory |
|----------|--------|
| JSON full load (byte[][]) | ~1.1 GB |
| JSON streaming parse → List<byte[]> | ~1.1 GB (same, all frames in List) |
| Text streaming (1 frame at a time) | ~346 KB (1 frame buffer) |

**Reduction: ~99.97%**

## Key Lessons

### 1. Streaming Parse ≠ Streaming Consumption

```csharp
// ❌ This is NOT streaming
var allFrames = new List<byte[]>();
using var reader = new Utf8JsonReader(jsonBytes);

while (reader.Read())
{
    if (reader.TokenType == JsonTokenType.Number)
    {
        allFrames.Add(ParseFrame(reader));
    }
}
// → All frames still in memory

// ✅ True streaming
public byte[]? ReadNextFrame(StreamReader reader, int width, int height)
{
    // Only one frame in memory at a time
}
```

**Lesson:** Utf8JsonReader's streaming I/O improves file read efficiency, but accumulating results in a collection still makes memory O(n)

### 2. Simplest Format Wins

```csharp
// ❌ JSON parsing complexity
using var doc = JsonDocument.Parse(json);
var frames = doc.RootElement.GetProperty("frames").EnumerateArray();
// → Requires external library

// ✅ Text parsing simplicity
public static byte[]? ReadNextFrame(StreamReader reader, int width, int height)
{
    var flat = new byte[width * height];
    for (int y = 0; y < height; y++)
    {
        var line = reader.ReadLine();
        if (line == null) return null;
        for (int x = 0; x < width; x++)
            flat[y * width + x] = (byte)(line[x] - '0');
    }
    return flat;
}
// → Only simple character processing needed
```

**Lesson:** For uniform data, plain text format is simpler to parse than JSON and requires no library dependency

### 3. Loop Without Reopening File Using StreamReader.BaseStream.Seek(0) + DiscardBufferedData()

```csharp
// ❌ Reopen file for each loop (inefficient)
for (int loop = 0; loop < loopCount; loop++)
{
    using var reader = new StreamReader("frames.txt");
    // ...
    // → Repeated file open/close overhead
}

// ✅ Open once and rewind
using var reader = new StreamReader("frames.txt");
for (int loop = 0; loop < loopCount; loop++)
{
    byte[]? frame;
    while ((frame = ReadNextFrame(reader, width, height)) != null)
    {
        RenderFrame(frame);
    }

    // Prepare for next loop
    reader.DiscardBufferedData();
    reader.BaseStream.Seek(0, SeekOrigin.Begin);
    // → Minimize file I/O overhead
}
```

**Lesson:** DiscardBufferedData() clears buffer, Seek(0) moves file pointer to beginning, enabling loop playback without file reopening

### 4. Use Blank Line as Frame Separator (Simple and Unambiguous)

```
# Frame 1, Row 1
00000000000000000000...
# Frame 1, Row 2
11111111111111111111...
# Frame 1, Row 3
00001111000011110000...

# ← Blank line (frame separator)

# Frame 2, Row 1
11110000111100001111...
...
```

**Lesson:** Blank line separator simplifies parsing logic and clearly marks frame boundaries

### 5. File Size is Within Expected Range

```
Per-frame data:
- Pixel data: 480 × 360 = 172,800 bytes (1 byte/pixel)
- Character overhead ('0'/'1' text representation): 172,800 bytes
- Line endings (CRLF): ~432 bytes (360 lines)

Total per-frame: ~346 KB (including text overhead)

Total file:
- Text format: 6572 frames × 346 KB ≈ 2.27 GB
- After gzip: ~200-300 MB (roughly 90% compression rate)

Note: File size may be larger than JSON, but memory usage reduces to 1/3200 of full load
```

**Lesson:** Text format is large uncompressed, but the tradeoff in memory efficiency and simplicity is worthwhile. gzip compression can reduce size if needed

## When to Apply

### Frame-by-Frame Animation Players

```csharp
// MewUI Bad Apple-style animation
var (width, height) = LoadMetadata("metadata.txt");
using var reader = new StreamReader("frames.txt");

while (isPlaying)
{
    var frame = ReadNextFrame(reader, width, height);
    if (frame == null)
    {
        reader.DiscardBufferedData();
        reader.BaseStream.Seek(0, SeekOrigin.Begin);
        frame = ReadNextFrame(reader, width, height);
    }

    UpdateDisplay(frame);
}
```

### Processing Large Arrays of Sequential Data

```csharp
// Sensor data stream (e.g., time-series data)
using var reader = new StreamReader("sensor-data.txt");
while ((var dataPoint = ReadNextDataPoint(reader)) != null)
{
    ProcessDataPoint(dataPoint);
}
```

### Video/Image Processing Pipelines

```csharp
// Large-scale image batch processing
public static void ProcessImages(string imageListFile)
{
    using var reader = new StreamReader(imageListFile);
    string? imagePath;

    while ((imagePath = reader.ReadLine()) != null)
    {
        var image = LoadImage(imagePath);  // Only one image at a time
        ProcessImage(image);
        image.Dispose();
    }
}
```

### When Frame Count × Frame Size > Available RAM

```csharp
// Memory-constrained environment

// Example: 10000 frames × 1 MB/frame = 10 GB
// → Full load impossible

// Streaming solution required
var frame = ReadNextFrame(reader, width, height);  // Only 1 MB in memory
```

## Additional Optimization Tips

### Tune Buffer Size

```csharp
// Default (4KB) is sufficient for small frames
using var reader = new StreamReader("frames.txt", Encoding.UTF8, bufferSize: 4096);

// For larger frames, use bigger buffer
using var reader = new StreamReader("frames.txt", Encoding.UTF8, bufferSize: 65536);
```

### Prefetch with Multithreading

```csharp
// Prefetch next frame while rendering
private byte[]? nextFrame;

Task.Run(() =>
{
    while (true)
    {
        nextFrame = ReadNextFrame(reader, width, height);
        if (nextFrame == null) { /* rewind */ }
        Thread.Sleep(frameDelay);
    }
});

while (isPlaying)
{
    var currentFrame = nextFrame;
    RenderFrame(currentFrame);
}
```

## Checklist

- [ ] Create metadata file (`width,height,frameCount`)
- [ ] Generate plain text frame file (each row '0'/'1', separated by blank lines)
- [ ] Implement ReadNextFrame() method
- [ ] Implement StreamReader rewind logic
- [ ] Test loop playback (multiple loops)
- [ ] Monitor memory usage (Task Manager)
- [ ] Control frame rate (Thread.Sleep or Timer)
- [ ] Verify file encoding consistency (UTF-8)

## References

- [StreamReader Class](https://docs.microsoft.com/en-us/dotnet/api/system.io.streamreader)
- [FileStream Class](https://docs.microsoft.com/en-us/dotnet/api/system.io.filestream)
- [Memory Management in .NET](https://docs.microsoft.com/en-us/dotnet/standard/garbage-collection/)
- [Utf8JsonReader Struct](https://docs.microsoft.com/en-us/dotnet/api/system.text.json.utf8jsonreader)
