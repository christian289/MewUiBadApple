/// <summary>
/// Real-time frame streaming reader using ffmpeg.
/// </summary>
sealed class FfmpegFrameReader : IDisposable
{
    public const double Fps = 30.0;
    private const int Threshold = 127;

    private readonly Process _process;
    private readonly Stream _stream;
    private readonly byte[] _buffer;

    private FfmpegFrameReader(Process process, Stream stream, int frameSize)
    {
        _process = process;
        _stream = stream;
        _buffer = new byte[frameSize];
    }

    /// <summary>
    /// Start ffmpeg process and return stdout BaseStream.
    /// </summary>
    public static FfmpegFrameReader Start(string videoPath)
    {
        var psi = new ProcessStartInfo
        {
            FileName = "ffmpeg",
            Arguments = $"-loglevel error -i \"{videoPath}\" -vf scale=120:90,format=gray -f rawvideo -pix_fmt gray pipe:1",
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true
        };

        var process = Process.Start(psi);
        if (process == null)
            throw new InvalidOperationException("Failed to start ffmpeg process");

        // Drain stderr asynchronously to prevent buffer blocking
        process.BeginErrorReadLine();

        var stream = process.StandardOutput.BaseStream;
        int frameSize = 120 * 90; // width * height

        return new FfmpegFrameReader(process, stream, frameSize);
    }

    /// <summary>
    /// Read one frame from BaseStream, apply threshold, and return 0/1 byte[].
    /// </summary>
    public static byte[]? ReadNextFrame(FfmpegFrameReader reader, int width, int height)
    {
        int frameSize = width * height;
        int bytesRead = 0;

        // Loop until entire frame is read (handle partial reads)
        while (bytesRead < frameSize)
        {
            int count = reader._stream.Read(reader._buffer, bytesRead, frameSize - bytesRead);
            if (count == 0)
                return null; // EOF

            bytesRead += count;
        }

        // Apply threshold: <= 127 = 0 (black), >= 128 = 1 (white)
        var result = new byte[frameSize];
        for (int i = 0; i < frameSize; i++)
        {
            result[i] = (byte)(reader._buffer[i] > Threshold ? 1 : 0);
        }

        return result;
    }

    public void Dispose()
    {
        try
        {
            _stream.Dispose();
            _process.Kill();
            _process.WaitForExit(1000);
            _process.Dispose();
        }
        catch
        {
            // Best effort cleanup
        }
    }
}
