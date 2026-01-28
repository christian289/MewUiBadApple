#:package OpenCvSharp4@4.10.0.20250107
#:package OpenCvSharp4.runtime.win@4.10.0.20250107
#:property PublishAot=false

using System.Text;
using OpenCvSharp;

var framesDir = args.Length > 0 ? args[0] : "frames";
var metaPath = args.Length > 1 ? args[1] : "badapple_meta.txt";
var framesPath = args.Length > 2 ? args[2] : "badapple_frames.txt";

var files = Directory.GetFiles(framesDir, "*.png")
    .OrderBy(f => f)
    .ToArray();

Console.WriteLine($"총 {files.Length}개 프레임 처리 중...");
// Processing {files.Length} frames...

// 첫 번째 이미지에서 해상도 추출
// Extract resolution from first image
using (var first = Cv2.ImRead(files[0], ImreadModes.Grayscale))
{
    Console.WriteLine($"해상도: {first.Cols}x{first.Rows}");
    // Resolution: {first.Cols}x{first.Rows}
}

int imgWidth = 0;
int imgHeight = 0;

// 프레임 데이터 스트리밍 텍스트 출력
// Streaming text output for frame data
using var sw = new StreamWriter(framesPath);

foreach (var (file, index) in files.Select((f, i) => (f, i)))
{
    using var gray = Cv2.ImRead(file, ImreadModes.Grayscale);
    using var binary = new Mat();
    Cv2.Threshold(gray, binary, 127, 1, ThresholdTypes.Binary);

    if (index == 0)
    {
        imgWidth = binary.Cols;
        imgHeight = binary.Rows;
    }

    // 프레임 간 빈 줄 구분
    // Blank line separator between frames
    if (index > 0) sw.WriteLine();

    // 프레임 데이터: 각 행을 '0'/'1' 문자열로 기록
    // Frame data: write each row as '0'/'1' string
    for (int y = 0; y < binary.Rows; y++)
    {
        var sb = new StringBuilder(binary.Cols);
        for (int x = 0; x < binary.Cols; x++)
            sb.Append(binary.At<byte>(y, x));
        sw.WriteLine(sb.ToString());
    }

    if ((index + 1) % 100 == 0)
        Console.WriteLine($"{index + 1}/{files.Length} 완료");
        // {index + 1}/{files.Length} done
}

// 메타데이터 파일 출력: width,height,frameCount
// Write metadata file: width,height,frameCount
File.WriteAllText(metaPath, $"{imgWidth},{imgHeight},{files.Length}");

Console.WriteLine($"완료! 메타: {metaPath}, 프레임: {framesPath} ({files.Length} 프레임, {imgWidth}x{imgHeight})");
// Done! Meta: {metaPath}, Frames: {framesPath} ({files.Length} frames, {imgWidth}x{imgHeight})
