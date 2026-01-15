# \# HexMath

# 

# A comprehensive C# library for hexagonal grid mathematics, providing coordinate transformations, distance calculations, pathfinding utilities, and rotation operations.

# 

# \## Features

# 

# \- \*\*Cube coordinate system\*\* using `Vector3` (q, r, s) where q + r + s = 0

# \- \*\*Coordinate conversions\*\* between hex, pixel, axial, and cube systems

# \- \*\*Distance calculations\*\* and area operations (rings, spirals)

# \- \*\*Rotation operations\*\* around arbitrary pivot points

# \- \*\*Direction utilities\*\* for navigation and alignment checks

# \- \*\*Support for both flat-top and pointy-top\*\* hex orientations

# \- \*\*Configurable stretch factors\*\* for non-uniform grids

# 

# \## Installation

# 

# Add the source file to your project and include the namespace:

# 

# ```csharp

# using Hexmath.src.HexMath;

# ```

# 

# \## Quick Start

# 

# ```csharp

# // Create grid metadata

# var metadata = new HexMetaData(

# &nbsp;   Size: 32.0f,

# &nbsp;   IsPointyTop: true,

# &nbsp;   HorizontalStretch: 1.5f,

# &nbsp;   VerticalStretch: 1.0f

# );

# 

# // Work with hex coordinates

# var origin = new Vector3(0, 0, 0);

# var target = new Vector3(2, -1, -1);

# 

# // Calculate distance

# int distance = HexMath.Distance(origin, target); // Returns 2

# 

# // Get all neighbors

# var neighbors = HexMath.GetAllNeighbors(origin);

# 

# // Convert to pixel coordinates

# Vector2 pixelPos = HexMath.HexToPixel(target, metadata);

# ```

# 

# \## Coordinate System

# 

# HexMath uses \*\*cube coordinates\*\* where each hex is represented as a `Vector3(q, r, s)` with the constraint that `q + r + s = 0`.

# 

# \### Directions

# 

# Six directional constants are provided in clockwise order (flat-top orientation):

# 

# | Direction | Vector |

# |-----------|--------|

# | East | `(1, 0, -1)` |

# | Southeast | `(1, -1, 0)` |

# | Southwest | `(0, -1, 1)` |

# | West | `(-1, 0, 1)` |

# | Northwest | `(-1, 1, 0)` |

# | Northeast | `(0, 1, -1)` |

# 

# \## API Reference

# 

# \### Navigation

# 

# ```csharp

# // Get next direction (clockwise or counterclockwise)

# Vector3? next = HexMath.NextDirection(currentDir, clockwise: true);

# 

# // Get neighbor in a direction

# Vector3 neighbor = HexMath.Neighbor(origin, direction, distance: 1);

# 

# // Get all 6 adjacent hexes

# IEnumerable<Vector3> neighbors = HexMath.GetAllNeighbors(origin);

# 

# // Find closest hex direction to an arbitrary vector

# Vector3 closest = HexMath.GetClosestHexDirection(arbitraryVector);

# 

# // Check if two hexes are aligned

# bool aligned = HexMath.AreInStraightLine(hexA, hexB);

# ```

# 

# \### Distance \& Area

# 

# ```csharp

# // Manhattan distance between hexes

# int dist = HexMath.Distance(from, to);

# 

# // Get hexes forming a ring at radius

# IEnumerable<Vector3> ring = HexMath.GetRing(origin, radius: 3);

# 

# // Get hexes in spiral pattern

# IEnumerable<Vector3> spiral = HexMath.GetSpiral(origin, maxRadius: 5, includeOrigin: true);

# ```

# 

# \### Coordinate Conversions

# 

# ```csharp

# // Hex to pixel (local space)

# Vector2 pixel = HexMath.HexToPixel(hex, metadata);

# 

# // Pixel to hex

# Vector3 hex = HexMath.PixelToHex(pixel, metadata);

# 

# // Round fractional coordinates to nearest valid hex

# Vector3 rounded = HexMath.RoundToHex(fractionalHex);

# 

# // Convert between cube and axial

# Vector2 axial = HexMath.ToAxial(cube);

# Vector3 cube = HexMath.ToCube(axial);

# ```

# 

# \### Distance-Based Movement

# 

# ```csharp

# // Get hex at specific distance in a direction

# Vector3 target = HexMath.GetHexAtDistance(origin, direction, distance: 3);

# 

# // Get hex at distance toward a pixel position (useful for mouse input)

# Vector3 target = HexMath.GetHexAtPixelDistance(origin, mousePos, distance: 2.0f, metadata);

# ```

# 

# \### Rotation

# 

# ```csharp

# // Rotate by 60-degree steps (positive = clockwise)

# Vector3 rotated = HexMath.RotateBySteps(hex, pivot, steps: 2);

# 

# // Rotate to match orientation between two reference hexes

# Vector3 rotated = HexMath.RotateToMatch(fromRef, toRef, hexToRotate, pivot);

# ```

# 

# \### Validation

# 

# ```csharp

# // Check if coordinate satisfies q + r + s = 0

# bool valid = HexMath.IsValidHexCoordinate(coord);

# 

# // Check if vector is one of the six hex directions

# bool isDirection = HexMath.IsValidHexDirection(coord);

# ```

# 

# \## HexMetaData

# 

# Configure grid properties using the `HexMetaData` record:

# 

# ```csharp

# var metadata = new HexMetaData(

# &nbsp;   Size: 32.0f,           // Base hex radius

# &nbsp;   IsPointyTop: true,     // true = pointy-top, false = flat-top

# &nbsp;   HorizontalStretch: 1.5f,

# &nbsp;   VerticalStretch: 1.0f

# );

# ```

# 

# \## Examples

# 

# \### Finding Path Along Ring

# 

# ```csharp

# var center = new Vector3(0, 0, 0);

# foreach (var hex in HexMath.GetRing(center, radius: 2))

# {

# &nbsp;   // Process each hex on the ring

# &nbsp;   Console.WriteLine($"Hex at {hex}, distance: {HexMath.Distance(center, hex)}");

# }

# ```

# 

# \### Mouse-Based Hex Selection

# 

# ```csharp

# // Convert mouse position to hex

# Vector2 mouseWorld = GetMouseWorldPosition();

# Vector3 hoveredHex = HexMath.PixelToHex(mouseWorld, metadata);

# 

# // Get hex 3 steps toward mouse from player

# Vector3 targetHex = HexMath.GetHexAtPixelDistance(playerHex, mouseWorld, 3.0f, metadata);

# ```

# 

# \### Rotating a Formation

# 

# ```csharp

# var pivot = new Vector3(0, 0, 0);

# var formation = new List<Vector3> { /\* hex positions \*/ };

# 

# // Rotate entire formation 120 degrees clockwise (2 steps)

# var rotatedFormation = formation

# &nbsp;   .Select(h => HexMath.RotateBySteps(h, pivot, steps: 2))

# &nbsp;   .ToList();

# ```

