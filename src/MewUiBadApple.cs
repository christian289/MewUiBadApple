#:package Aprillz.MewUI@0.9.0
#:property PublishAot=true
#:property TrimMode=full

var videoPath = args.Length > 0 ? args[0] : "badapple.mp4";
if (!File.Exists(videoPath))
    videoPath = "src/badapple.mp4";

var iconPath = "appicon.ico";
if (!File.Exists(iconPath))
    iconPath = "src/appicon.ico";

const int width = 120;
const int height = 90;
const int totalFrames = 6572; // ~3m39s @ 30 FPS (approximate)

ThemeManager.Default = ThemeVariant.Dark;

var framesReader = FfmpegFrameReader.Start(videoPath);
var currentFrameData = FfmpegFrameReader.ReadNextFrame(framesReader, width, height)!;

var canvas = new PixelCanvas(width, height);
int currentFrame = 0;

var sw = new Stopwatch();
var fpsSw = Stopwatch.StartNew();
int frameCount = 0;
double currentFps = 0;
double lastRenderMs = 0;
int totalElapsedSeconds = 0;

var fpsValue = new ObservableValue<string>("FPS: --");
var renderTimeValue = new ObservableValue<string>("Render: -- ms");
var frameValue = new ObservableValue<string>($"Frame: 0/{totalFrames}");
var elapsedValue = new ObservableValue<string>("Elapsed: 0s");

System.Threading.Timer? animTimer = null;

void OnTick()
{
    sw.Restart();
    canvas.SetFrame(currentFrameData);
    sw.Stop();
    lastRenderMs = sw.Elapsed.TotalMilliseconds;

    currentFrame = (currentFrame + 1) % totalFrames;
    var next = FfmpegFrameReader.ReadNextFrame(framesReader, width, height);
    if (next == null)
    {
        // Loop: restart ffmpeg process
        framesReader.Dispose();
        framesReader = FfmpegFrameReader.Start(videoPath);
        next = FfmpegFrameReader.ReadNextFrame(framesReader, width, height)!;
    }
    currentFrameData = next;

    frameCount++;
    double elapsed = fpsSw.Elapsed.TotalSeconds;
    if (elapsed >= 1.0)
    {
        currentFps = frameCount / elapsed;
        totalElapsedSeconds += (int)elapsed;
        frameCount = 0;
        fpsSw.Restart();
    }

    fpsValue.Value = $"FPS: {currentFps:F1}";
    renderTimeValue.Value = $"Render: {lastRenderMs:F2} ms";
    frameValue.Value = $"Frame: {currentFrame}/{totalFrames}";
    elapsedValue.Value = $"Elapsed: {totalElapsedSeconds}s";
}

int timerIntervalMs = (int)(1000.0 / FfmpegFrameReader.Fps);
var window = new Window()
    .Title("Bad Apple!! - MewUI Animation")
    .Fixed(
        canvas.GridWidth * PixelCanvas.CellWidth + 120,
        canvas.GridHeight * PixelCanvas.CellHeight + 120)
    .Content(
        new DockPanel()
            .LastChildFill()
            .Children(
                new StackPanel()
                    .DockTop()
                    .Horizontal()
                    .CenterHorizontal()
                    .Spacing(16)
                    .Padding(4)
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

if (File.Exists(iconPath))
    window.Icon = IconSource.FromFile(iconPath);

Application.Run(window);
