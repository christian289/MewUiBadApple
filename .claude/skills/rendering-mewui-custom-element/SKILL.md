---
name: rendering-mewui-custom-element
description: "Renders custom FrameworkElement in MewUI using absolute Bounds coordinates instead of (0,0). Use when custom element alignment (Center, Right, Bottom) appears to be ignored or element always renders at top-left corner."
---

# MewUI Custom Element Rendering

MewUI does NOT apply coordinate transforms when rendering children. Each element must render at its absolute `Bounds` position.

## The Problem

```csharp
// ❌ WRONG - Always draws at top-left, ignoring alignment
public override void Render(IGraphicsContext context)
{
    context.FillRectangle(new Rect(0, 0, Width, Height), color);
}
```

## The Solution

```csharp
// ✅ CORRECT - Use Bounds.X and Bounds.Y
public override void Render(IGraphicsContext context)
{
    double ox = Bounds.X;
    double oy = Bounds.Y;

    context.FillRectangle(new Rect(ox, oy, Width, Height), color);

    // Apply offset to ALL nested coordinates
    context.FillRectangle(
        new Rect(ox + x * CellWidth, oy + y * CellHeight, CellWidth, CellHeight),
        color);
}
```

## Why This Happens

| Framework | Coordinate System | Child Render |
|-----------|------------------|--------------|
| WPF | Relative (0,0 = element origin) | Transform pushed automatically |
| MewUI | Absolute (0,0 = window origin) | No transform applied |

**MewUI Panel.Render** simply calls `child.Render(context)` without any translation:

```csharp
// MewUI Panel.cs - No transform!
foreach (var child in _children)
{
    child.Render(context);
}
```

## Checklist

- [ ] Use `Bounds.X` and `Bounds.Y` as offset in Render
- [ ] Apply offset to ALL drawing operations
- [ ] Override `MeasureContent` to return `new Size(Width, Height)`
