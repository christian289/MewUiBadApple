---
name: absolute-coords
description: "MewUI custom element rendering uses absolute Bounds coordinates. Use when alignment appears ignored or element renders at top-left."
---

# MewUI Absolute Coordinates

MewUI does NOT apply coordinate transforms. Each element must render at its absolute `Bounds` position.

## Problem

```csharp
// ❌ WRONG - Always draws at top-left
public override void Render(IGraphicsContext context)
{
    context.FillRectangle(new Rect(0, 0, Width, Height), color);
}
```

## Solution

```csharp
// ✅ CORRECT - Use Bounds.X and Bounds.Y
public override void Render(IGraphicsContext context)
{
    double ox = Bounds.X;
    double oy = Bounds.Y;

    context.FillRectangle(new Rect(ox, oy, Width, Height), color);
}
```

## Checklist

- Use `Bounds.X` and `Bounds.Y` as offset in Render
- Apply offset to ALL drawing operations
- Override `MeasureContent` to return `new Size(Width, Height)`
