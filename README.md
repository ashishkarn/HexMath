# HexMath

A C# library for hexagonal grid mathematics with type-safe coordinates.

## Installation

```
dotnet add package Hexmath
```

## Features

- Type-safe `HexCoord` struct (axial storage, computed S coordinate)
- `HexDirection` enum for 6 cardinal directions
- Coordinate conversions between hex and pixel systems
- Distance calculations, rings, and spirals
- 60-degree rotation operations
- Support for flat-top and pointy-top orientations
- Configurable stretch factors for non-uniform grids

## Usage

```csharp
using Hexmath;

// Create hex coordinates
var origin = HexCoord.Zero;
var hex = new HexCoord(2, -1);  // Q=2, R=-1, S=-1 (computed)

// Directions
var neighbor = HMath.Neighbor(origin, HexDirection.East);
var opposite = HexDirection.East.Opposite();  // West
var next = HexDirection.East.Next();          // SouthEast

// Distance between hexes
int dist = HMath.Distance(origin, hex);

// Get all neighbors
var neighbors = HMath.GetAllNeighbors(origin);

// Pixel conversions
var metadata = new HexMetaData(Size: 32.0f, IsPointyTop: true);
Vector2 pixel = HMath.HexToPixel(hex, metadata);
HexCoord back = HMath.PixelToHex(pixel, metadata);

// Rings and spirals
var ring = HMath.GetRing(origin, radius: 2);      // 12 hexes
var spiral = HMath.GetSpiral(origin, maxRadius: 3); // 37 hexes

// Rotation (2 steps = 120 degrees)
HexCoord rotated = HMath.RotateBySteps(hex, pivot: origin, steps: 2);

// Game engine interop
Vector3 vec = hex.ToVector3();           // (Q, R, S)
HexCoord coord = HexCoord.FromVector3(vec);
```

## Types

### HexCoord

```csharp
public readonly struct HexCoord
{
    public int Q { get; }      // Column
    public int R { get; }      // Row
    public int S => -Q - R;    // Computed (Q + R + S = 0)

    public static readonly HexCoord Zero;

    // Operators: +, -, *, ==, !=
    // Methods: ToVector3(), FromVector3(), Equals(), GetHashCode()
}
```

### HexDirection

```csharp
public enum HexDirection
{
    East = 0,      // (1, 0, -1)
    SouthEast = 1, // (1, -1, 0)
    SouthWest = 2, // (0, -1, 1)
    West = 3,      // (-1, 0, 1)
    NorthWest = 4, // (-1, 1, 0)
    NorthEast = 5  // (0, 1, -1)
}

// Extension methods
direction.ToOffset()              // HexCoord offset
direction.Next(clockwise: true)   // Next direction
direction.Opposite()              // Opposite direction
```

## Requirements

- .NET 8.0+

## Breaking Changes in 2.0

- `Vector3` replaced with `HexCoord` struct
- `Directions` array replaced with `HexDirection` enum
- Removed: `IsValidHexCoordinate`, `IsValidHexDirection`, `ToAxial`, `ToCube`
