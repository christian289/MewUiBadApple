#:package Aprillz.MewUI@0.4.0
#:property PublishAot=true
#:property TrimMode=full

using System.Diagnostics;
using Aprillz.MewUI.Binding;
using Aprillz.MewUI.Controls;
using Aprillz.MewUI.Core;
using Aprillz.MewUI.Elements;
using Aprillz.MewUI.Markup;
using Aprillz.MewUI.Panels;
using Aprillz.MewUI.Primitives;
using Aprillz.MewUI.Rendering;

// 메타데이터 및 프레임 파일 경로
// Metadata and frame file paths
var metaPath = args.Length > 0 ? args[0] : "badapple_meta.txt";
var framesPath = args.Length > 1 ? args[1] : "badapple_frames.txt";

// 메타데이터 로드
// Load metadata
var (width, height, totalFrames) = BadAppleFrames.LoadMeta(metaPath);

// 프레임 스트리밍 리더
// Frame streaming reader
var framesReader = BadAppleFrames.OpenReader(framesPath);

// 첫 프레임 미리 읽기
// Pre-read first frame
var currentFrameData = BadAppleFrames.ReadNextFrame(framesReader, width, height)!;

// 다크 테마 설정
// Set dark theme
Theme.Current = Theme.Dark.WithAccent(Color.White);

// 픽셀 캔버스 생성
// Create pixel canvas
var canvas = new PixelCanvas(width, height);

// 애니메이션 상태
// Animation state
int currentFrame = 0;

// 성능 측정
// Performance metrics
var sw = new Stopwatch();
var fpsSw = Stopwatch.StartNew();
int frameCount = 0;
double currentFps = 0;
double lastRenderMs = 0;
int totalElapsedSeconds = 0;

// 성능 메트릭 표시 라벨
// Performance metric display labels
var fpsValue = new ObservableValue<string>("FPS: --");
var renderTimeValue = new ObservableValue<string>("Render: -- ms");
var frameValue = new ObservableValue<string>($"Frame: 0/{totalFrames}");
var elapsedValue = new ObservableValue<string>("Elapsed: 0s");

// 타이머 참조
// Timer reference
System.Threading.Timer? animTimer = null;

// 애니메이션 틱 핸들러
// Animation tick handler
void OnTick()
{
    sw.Restart();

    // 현재 프레임 렌더링
    // Render current frame
    canvas.SetFrame(currentFrameData);

    sw.Stop();
    lastRenderMs = sw.Elapsed.TotalMilliseconds;

    // 다음 프레임 읽기
    // Read next frame
    currentFrame = (currentFrame + 1) % totalFrames;
    var next = BadAppleFrames.ReadNextFrame(framesReader, width, height);
    if (next == null)
    {
        // 루프: 처음으로 되감기
        // Loop: rewind to beginning
        framesReader.DiscardBufferedData();
        framesReader.BaseStream.Seek(0, SeekOrigin.Begin);
        next = BadAppleFrames.ReadNextFrame(framesReader, width, height)!;
    }
    currentFrameData = next;

    // FPS 카운터 업데이트
    // Update FPS counter
    frameCount++;
    double elapsed = fpsSw.Elapsed.TotalSeconds;
    if (elapsed >= 1.0)
    {
        currentFps = frameCount / elapsed;
        totalElapsedSeconds += (int)elapsed;
        frameCount = 0;
        fpsSw.Restart();
    }

    // 성능 메트릭 표시 업데이트
    // Update performance metric display
    fpsValue.Value = $"FPS: {currentFps:F1}";
    renderTimeValue.Value = $"Render: {lastRenderMs:F2} ms";
    frameValue.Value = $"Frame: {currentFrame}/{totalFrames}";
    elapsedValue.Value = $"Elapsed: {totalElapsedSeconds}s";
}

// 타이머 간격 계산 (30 FPS)
// Calculate timer interval (30 FPS)
int timerIntervalMs = (int)(1000.0 / BadAppleFrames.Fps);

// 윈도우 생성
// Create window
var window = new Window()
    .Title("Bad Apple!! - MewUI Animation")
    .Fixed(
        canvas.GridWidth * PixelCanvas.CellWidth + 20,
        canvas.GridHeight * PixelCanvas.CellHeight + 80)
    .Padding(4)
    .Content(
        new DockPanel()
            .Children(
                new StackPanel()
                    .DockTop()
                    .Horizontal()
                    .Spacing(16)
                    .Children(
                        new Label().BindText(fpsValue).Bold().Foreground(Color.White),
                        new Label().BindText(renderTimeValue).Foreground(Color.FromRgb(200, 200, 200)),
                        new Label().BindText(frameValue).Foreground(Color.FromRgb(150, 150, 150)),
                        new Label().BindText(elapsedValue).Foreground(Color.FromRgb(150, 150, 150))
                    ),
                canvas
            )
    )
    .OnLoaded(() =>
    {
        // OnLoaded는 UI 스레드에서 실행되므로 SynchronizationContext를 캡처
        // OnLoaded runs on UI thread, so capture SynchronizationContext
        var syncContext = SynchronizationContext.Current;
        animTimer = new System.Threading.Timer(_ =>
        {
            if (syncContext != null)
                syncContext.Post(_ => OnTick(), null);
            else
                OnTick();
        }, null, 0, timerIntervalMs);
    })
    .OnClosed(() =>
    {
        animTimer?.Dispose();
        framesReader.Dispose();
    });

Application.Run(window);

/// <summary>
/// IGraphicsContext를 사용하여 픽셀을 직접 렌더링하는 커스텀 엘리먼트
/// Custom element that renders pixels directly using IGraphicsContext
/// </summary>
sealed class PixelCanvas : FrameworkElement
{
    public const int CellWidth = 2;
    public const int CellHeight = 2;

    public int GridWidth { get; }
    public int GridHeight { get; }

    // 현재 프레임 비트맵 (1 = 흰색, 0 = 검은색)
    // Current frame bitmap (1 = white, 0 = black)
    private readonly byte[] _frameBits;

    private readonly Color _bgColor = Color.Black;
    private readonly Color _fgColor = Color.White;

    public PixelCanvas(int gridWidth, int gridHeight)
    {
        GridWidth = gridWidth;
        GridHeight = gridHeight;
        _frameBits = new byte[gridWidth * gridHeight];
        Width = gridWidth * CellWidth;
        Height = gridHeight * CellHeight;
    }

    /// <summary>
    /// 프레임 비트 배열을 업데이트하고 다시 그리기 요청
    /// Update frame bit array and request redraw
    /// </summary>
    public void SetFrame(byte[] bits)
    {
        Array.Copy(bits, _frameBits, Math.Min(bits.Length, _frameBits.Length));
        InvalidateVisual();
    }

    /// <summary>
    /// IGraphicsContext를 사용하여 픽셀을 직접 렌더링
    /// Render pixels directly using IGraphicsContext
    /// </summary>
    public override void Render(IGraphicsContext context)
    {
        // 배경 클리어
        // Clear background
        context.FillRectangle(new Rect(0, 0, Width, Height), _bgColor);

        // 흰색 픽셀만 그리기 (검은색은 배경이므로 스킵)
        // Only draw white pixels (skip black as it's the background)
        for (int y = 0; y < GridHeight; y++)
        {
            for (int x = 0; x < GridWidth; x++)
            {
                if (_frameBits[y * GridWidth + x] != 0)
                {
                    context.FillRectangle(
                        new Rect(x * CellWidth, y * CellHeight, CellWidth, CellHeight),
                        _fgColor);
                }
            }
        }
    }
}

/// <summary>
/// Bad Apple!! 애니메이션 프레임 데이터 (스트리밍 방식)
/// Bad Apple!! animation frame data (streaming mode)
/// </summary>
static class BadAppleFrames
{
    public const double Fps = 30.0;

    /// <summary>
    /// 메타데이터 파일에서 width, height, totalFrames 로드
    /// Load width, height, totalFrames from metadata file
    /// </summary>
    public static (int Width, int Height, int TotalFrames) LoadMeta(string metaPath)
    {
        var parts = File.ReadAllText(metaPath).Trim().Split(',');
        return (int.Parse(parts[0]), int.Parse(parts[1]), int.Parse(parts[2]));
    }

    /// <summary>
    /// 프레임 파일의 StreamReader 반환
    /// Return StreamReader for frame file
    /// </summary>
    public static StreamReader OpenReader(string framesPath)
    {
        return new StreamReader(framesPath);
    }

    /// <summary>
    /// StreamReader에서 한 프레임(height 줄) 읽어 flat byte[] 반환
    /// Read one frame (height lines) from StreamReader and return flat byte[]
    /// </summary>
    public static byte[]? ReadNextFrame(StreamReader reader, int width, int height)
    {
        var flat = new byte[width * height];
        for (int y = 0; y < height; y++)
        {
            var line = reader.ReadLine();
            if (line == null) return null;

            // 빈 줄은 프레임 구분자 — 건너뛰고 실제 데이터 줄 읽기
            // Empty line is frame separator — skip and read actual data line
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
}
