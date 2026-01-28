[![English](https://img.shields.io/badge/README.md-English-blue.svg)](README.md)

<p align="center">
  <img src="https://raw.githubusercontent.com/aprillz/MewUI/main/assets/logo/logo-256.png" alt="MewUI Logo" width="128"/>
</p>

# 🍎 MewUIBadApple

![.NET](https://img.shields.io/badge/.NET-10%2B-512BD4?logo=dotnet&logoColor=white)
![Windows](https://img.shields.io/badge/Windows-10%2B-0078D4?logo=windows&logoColor=white)
![NativeAOT](https://img.shields.io/badge/NativeAOT-Ready-2E7D32)
![ffmpeg](https://img.shields.io/badge/ffmpeg-Required-007808?logo=ffmpeg&logoColor=white)
![MewUI](https://img.shields.io/badge/MewUI-0.9.0-FF69B4)

---

**🎬 Bad Apple!!** 애니메이션 플레이어 - [MewUI](https://github.com/aprillz/MewUI) 픽셀 UI 프레임워크 사용.
ffmpeg stdout 파이프를 통해 프레임을 직접 스트리밍하여 **전처리 없이 재생**합니다.

---

## 🚀 빠른 시작

```bash
# 바로 실행!
dotnet run src/MewUiBadApple.cs
```

전처리가 필요 없습니다.

---

## 📋 필수 요건

- **.NET 10+** — `#:package` 지시문이 있는 `dotnet run file.cs` 지원
- **ffmpeg** — 실시간 비디오 디코딩용

### 📦 ffmpeg 설치

| 플랫폼 | 명령어 |
|--------|--------|
| Windows (Chocolatey) | `choco install ffmpeg` |
| Windows (winget) | `winget install Gyan.FFmpeg` |
| Windows (수동) | https://www.gyan.dev/ffmpeg/builds/ 에서 다운로드 후 `bin` 폴더를 PATH에 추가 |
| macOS | `brew install ffmpeg` |
| Ubuntu/Debian | `sudo apt-get install ffmpeg` |
| Fedora | `sudo dnf install ffmpeg` |

---

## 📁 파일 구조

```
MewUIBadApple/
├── src/
│   ├── MewUiBadApple.cs       # 메인 진입점 (Aprillz.MewUI)
│   ├── PixelCanvas.cs         # 픽셀 렌더링용 커스텀 FrameworkElement
│   ├── FfmpegFrameReader.cs   # ffmpeg 프로세스 래퍼
│   ├── GlobalUsings.cs        # 전역 using 지시문
│   ├── Directory.Build.props  # 빌드 설정
│   ├── appicon.ico            # 창 아이콘 (MewUI 로고)
│   └── badapple.mp4           # 소스 비디오
└── README.md
```

---

## ⚙️ CLI 파라미터

```bash
dotnet run src/MewUiBadApple.cs [비디오_파일]
# 기본값:                        src/badapple.mp4
```

---

## 🏗️ 아키텍처

```
badapple.mp4 → [ffmpeg 서브프로세스] → stdout (raw grayscale 바이트) → PixelCanvas
```

| 컴포넌트 | 설명 |
|----------|------|
| **ffmpeg** | `ffmpeg -loglevel error -i badapple.mp4 -vf scale=120:90,format=gray -f rawvideo -pix_fmt gray pipe:1` |
| **스트리밍** | `Process.StandardOutput.BaseStream`에서 raw 바이트 읽기 (~10KB/프레임) |
| **임계값** | grayscale 값 > 127 = 흰색, 그 외 검은색 |
| **반복** | 비디오 끝에서 ffmpeg 프로세스 재시작 |
| **렌더링** | `PixelCanvas`가 `IGraphicsContext`를 통해 픽셀 직접 그리기 (2×2 셀) |

---

## 🎨 커스터마이징

| 설정 | 위치 | 기본값 |
|------|------|--------|
| 해상도 | `width`/`height` 상수 + ffmpeg scale 필터 | 120×90 |
| FPS | `FfmpegFrameReader.Fps` | 30 |
| 임계값 | `FfmpegFrameReader.Threshold` | 127 |
| 색상 | `PixelCanvas._bgColor` / `_fgColor` | 검정/흰색 |

---

## 🔧 문제 해결

| 문제 | 해결 방법 |
|------|-----------|
| `ffmpeg`를 찾을 수 없음 | ffmpeg를 설치하고 PATH에 추가. 확인: `ffmpeg -version` |
| `FileNotFoundException` | `src/` 디렉토리에 `badapple.mp4`가 있는지 확인 |
| 검은 화면 | 비디오 파일이 유효한지 확인: `ffprobe badapple.mp4` |
| 끊기는 재생 | ffmpeg 디코딩이 CPU 바운드일 수 있음; 더 짧거나 작은 비디오 시도 |

---

## 🔗 참고 자료

- [MewUI](https://github.com/Aprillz/MewUI) — 경량 픽셀 UI 프레임워크
- [ffmpeg](https://ffmpeg.org/documentation.html) — 비디오 처리
- [Bad Apple!!](https://www.youtube.com/watch?v=FtutLA63Cp8) — 원본 영상

---

## 📄 라이선스

개인 학습 및 데모 목적. "Bad Apple!!" 음악은 저작권이 있습니다.

---

## 🙏 감사의 말

이 프로젝트를 구동하는 경량 픽셀 UI 프레임워크 [MewUI](https://github.com/aprillz/MewUI)를 만든 [aprillz](https://github.com/aprillz)에게 감사드립니다.
