/// <summary>
/// Custom element that renders pixels directly using IGraphicsContext.
/// </summary>
sealed class PixelCanvas : FrameworkElement
{
    public const int CellWidth = 2;
    public const int CellHeight = 2;

    public int GridWidth { get; }
    public int GridHeight { get; }

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
        HorizontalAlignment = HorizontalAlignment.Center;
        VerticalAlignment = VerticalAlignment.Center;
    }

    protected override Size MeasureContent(Size availableSize)
    {
        return new Size(Width, Height);
    }

    /// <summary>
    /// Update frame bit array and request redraw.
    /// </summary>
    public void SetFrame(byte[] bits)
    {
        Array.Copy(bits, _frameBits, Math.Min(bits.Length, _frameBits.Length));
        InvalidateVisual();
    }

    /// <summary>
    /// Render pixels directly using IGraphicsContext.
    /// </summary>
    public override void Render(IGraphicsContext context)
    {
        // MewUI uses absolute coordinates - use Bounds.X/Y as offset
        double ox = Bounds.X;
        double oy = Bounds.Y;

        context.FillRectangle(new Rect(ox, oy, Width, Height), _bgColor);

        // Only draw white pixels (skip black as it's the background)
        for (int y = 0; y < GridHeight; y++)
        {
            for (int x = 0; x < GridWidth; x++)
            {
                if (_frameBits[y * GridWidth + x] != 0)
                {
                    context.FillRectangle(
                        new Rect(ox + x * CellWidth, oy + y * CellHeight, CellWidth, CellHeight),
                        _fgColor);
                }
            }
        }
    }
}
