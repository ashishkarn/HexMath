# HexMath

A C# library for hexagonal grid mathematics using cube coordinates.

## Features

- Cube coordinate system (`Vector3` where q + r + s = 0)
- Coordinate conversions between hex, pixel, and axial systems
- Distance calculations, rings, and spirals
- 60-degree rotation operations
- Support for flat-top and pointy-top orientations
- Configurable stretch factors for non-uniform grids

## Usage

```csharp
using Hexmath.src.HexMath;

var metadata = new HexMetaData(Size: 32.0f, IsPointyTop: true);

// Distance between hexes
int dist = HexMath.Distance(hexA, hexB);

// Get all neighbors
var neighbors = HexMath.GetAllNeighbors(origin);

// Convert hex to pixel position
Vector2 pixel = HexMath.HexToPixel(hex, metadata);

// Rotate around a pivot (2 steps = 120°)
Vector3 rotated = HexMath.RotateBySteps(hex, pivot, steps: 2);
```

## Directions

Six directions in clockwise order (flat-top):

| Direction | Vector |
|-----------|--------|
| East | `(1, 0, -1)` |
| Southeast | `(1, -1, 0)` |
| Southwest | `(0, -1, 1)` |
| West | `(-1, 0, 1)` |
| Northwest | `(-1, 1, 0)` |
| Northeast | `(0, 1, -1)` |

## Requirements

- .NET 6.0+
- System.Numerics
