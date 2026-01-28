using System.Numerics;

namespace Hexmath
{
    /// <summary>
    /// Core hex grid mathematical operations and coordinate transformations.
    /// Provides comprehensive functionality for working with hexagonal grids including
    /// coordinate conversions, distance calculations, pathfinding, and rotations.
    /// </summary>
    public static class HMath
    {
        #region Constants

        private static readonly float Sqrt3 = MathF.Sqrt(3.0f);
        private const float Epsilon = 0.001f;

        /// <summary>
        /// Read-only collection of all six hexagonal direction vectors in clockwise order for flat-top hexagons.
        /// Order: EAST, SOUTHEAST, SOUTHWEST, WEST, NORTHWEST, NORTHEAST
        /// </summary>
        public static readonly IReadOnlyList<Vector3> Directions =
        [
            new Vector3(1, 0, -1),   // EAST
            new Vector3(1, -1, 0),   // SOUTHEAST
            new Vector3(0, -1, 1),   // SOUTHWEST
            new Vector3(-1, 0, 1),   // WEST
            new Vector3(-1, 1, 0),   // NORTHWEST
            new Vector3(0, 1, -1)    // NORTHEAST
        ];

        #endregion

        #region Direction and Navigation

        /// <summary>
        /// Returns the next direction in clockwise or counterclockwise order from the current direction.
        /// </summary>
        /// <param name="currentDirection">The current hex direction vector</param>
        /// <param name="clockwise">True for clockwise rotation, false for counterclockwise</param>
        /// <returns>The next direction vector, or null if the current direction is invalid</returns>
        public static Vector3? NextDirection(Vector3 currentDirection, bool clockwise = true)
        {
            int index = -1;
            for (int i = 0; i < Directions.Count; i++)
            {
                if (Directions[i] == currentDirection)
                {
                    index = i;
                    break;
                }
            }
            
            if (index == -1) return null;

            int offset = clockwise ? 1 : -1;
            int newIndex = (index + offset + Directions.Count) % Directions.Count;
            return Directions[newIndex];
        }

        /// <summary>
        /// Gets a neighboring hex at the specified direction and distance from the origin.
        /// </summary>
        /// <param name="origin">The starting hex coordinate</param>
        /// <param name="direction">The direction vector to move in</param>
        /// <param name="distance">The number of hex steps to move (default: 1)</param>
        /// <returns>The hex coordinate at the specified position</returns>
        public static Vector3 Neighbor(Vector3 origin, Vector3 direction, int distance = 1) 
            => origin + direction * distance;

        /// <summary>
        /// Returns all 6 immediately adjacent hexes to the given origin hex.
        /// </summary>
        /// <param name="origin">The center hex coordinate</param>
        /// <returns>Enumerable of all adjacent hex coordinates</returns>
        public static IEnumerable<Vector3> GetAllNeighbors(Vector3 origin)
        {
            origin = RoundToHex(origin);
            return Directions.Select(dir => origin + dir);
        }

        /// <summary>
        /// Gets the closest hex direction to an arbitrary direction vector.
        /// </summary>
        /// <param name="direction">The direction vector to match</param>
        /// <returns>The closest hex direction, or Vector3.Zero if direction is zero</returns>
        public static Vector3 GetClosestHexDirection(Vector3 direction)
        {
            if (direction.LengthSquared() < Epsilon)
                return Vector3.Zero;

            if (Directions.Contains(direction))
                return direction;

            Vector3 closestDirection = Directions[0];
            float smallestAngleDiff = float.MaxValue;

            foreach (var hexDirection in Directions)
            {
                // System.Numerics: Normalize is static, Dot is static
                float similarity = Vector3.Dot(Vector3.Normalize(direction), Vector3.Normalize(hexDirection));

                // System.MathF for floats, Math.Clamp for range
                float angleDiff = MathF.Acos(Math.Clamp(similarity, -1f, 1f));

                if (angleDiff < smallestAngleDiff)
                {
                    smallestAngleDiff = angleDiff;
                    closestDirection = hexDirection;
                }
            }

            return closestDirection;
        }

        /// <summary>
        /// Determines whether two hex coordinates lie on the same straight line along one of the six hex directions.
        /// </summary>
        /// <param name="hexA">The first hex coordinate.</param>
        /// <param name="hexB">The second hex coordinate.</param>
        /// <returns>True if the two hexes are aligned in a straight line; otherwise, false.</returns>
        public static bool AreInStraightLine(Vector3 hexA, Vector3 hexB)
        {
            if (hexA == hexB)
                return true;

            Vector3 diff = hexB - hexA;

            foreach (var dir in Directions)
            {
                // Floating point comparisons with slight tolerance is usually better, 
                // but for integer-aligned hex grids, direct comparison often works if logic is clean.
                // Using standard epsilon for robustness.
                if (IsCrossProductZero(diff, dir) && IsSameDirection(diff, dir))
                    return true;
            }
            return false;
        }

        private static bool IsCrossProductZero(Vector3 a, Vector3 b)
        {
            return MathF.Abs(a.X * b.Y - a.Y * b.X) < Epsilon &&
                   MathF.Abs(a.Y * b.Z - a.Z * b.Y) < Epsilon &&
                   MathF.Abs(a.Z * b.X - a.X * b.Z) < Epsilon;
        }

        private static bool IsSameDirection(Vector3 diff, Vector3 dir)
        {
            return (Math.Sign(diff.X) == Math.Sign(dir.X) || dir.X == 0) &&
                   (Math.Sign(diff.Y) == Math.Sign(dir.Y) || dir.Y == 0) &&
                   (Math.Sign(diff.Z) == Math.Sign(dir.Z) || dir.Z == 0);
        }

        #endregion

        #region Distance and Area Operations

        /// <summary>
        /// Calculates the Manhattan distance between two hex coordinates.
        /// </summary>
        /// <param name="from">Starting hex coordinate</param>
        /// <param name="to">Destination hex coordinate</param>
        /// <returns>The distance in hex units</returns>
        public static int Distance(Vector3 from, Vector3 to) =>
            (int)(MathF.Abs(from.X - to.X) + MathF.Abs(from.Y - to.Y) + MathF.Abs(from.Z - to.Z)) / 2;

        /// <summary>
        /// Returns all hexes forming a ring at the specified radius from the origin.
        /// </summary>
        /// <param name="origin">The center hex coordinate</param>
        /// <param name="radius">The radius of the ring (must be positive)</param>
        /// <returns>Enumerable of hex coordinates forming the ring</returns>
        public static IEnumerable<Vector3> GetRing(Vector3 origin, int radius)
        {
            if (radius <= 0) yield break;
            
            Vector3 current = origin + Directions[4] * radius;
            for (int i = 0; i < 6; i++)
            {
                for (int j = 0; j < radius; j++)
                {
                    yield return current;
                    current += Directions[i];
                }
            }
        }

        /// <summary>
        /// Returns all hexes in a spiral pattern from origin up to the maximum radius.
        /// </summary>
        /// <param name="origin">The center hex coordinate</param>
        /// <param name="maxRadius">The maximum radius to include</param>
        /// <param name="includeOrigin">Whether to include the origin hex in the result</param>
        /// <returns>Enumerable of hex coordinates in spiral order</returns>
        public static IEnumerable<Vector3> GetSpiral(Vector3 origin, int maxRadius, bool includeOrigin = true)
        {
            if (maxRadius < 0) yield break;
            if (includeOrigin) yield return origin;
            
            for (int r = 1; r <= maxRadius; r++)
                foreach (var hex in GetRing(origin, r))
                    yield return hex;
        }

        #endregion

        #region Coordinate Conversions

        /// <summary>
        /// Converts hex coordinates to 2D pixel coordinates in local grid space.
        /// </summary>
        /// <param name="hex">The hex coordinate to convert</param>
        /// <param name="metaData">Hex grid metadata containing size and layout information</param>
        /// <returns>2D local position as Vector2</returns>
        public static Vector2 HexToPixel(Vector3 hex, HexMetaData metaData)
        {
            float x, y;
            
            if (metaData.IsPointyTop)
            {
                // Pointy-top: x = sqrt(3) * q + sqrt(3)/2 * r, y = 3/2 * r
                x = (Sqrt3 * hex.X + Sqrt3 * 0.5f * hex.Y) * metaData.Size * metaData.HorizontalStretch;
                y = 1.5f * hex.Y * metaData.Size * metaData.VerticalStretch;
            }
            else
            {
                // Flat-top: x = 3/2 * q, y = sqrt(3)/2 * q + sqrt(3) * r
                x = 1.5f * hex.X * metaData.Size * metaData.HorizontalStretch;
                y = (Sqrt3 * 0.5f * hex.X + Sqrt3 * hex.Y) * metaData.Size * metaData.VerticalStretch;
            }
            
            return new Vector2(x, y);
        }

        /// <summary>
        /// Converts 2D pixel coordinates in local grid space to hex coordinates.
        /// </summary>
        /// <param name="pixel">The 2D local position to convert</param>
        /// <param name="metaData">Hex grid metadata containing size and layout information</param>
        /// <returns>Hex coordinate as Vector3</returns>
        public static Vector3 PixelToHex(Vector2 pixel, HexMetaData metaData)
        {
            if (metaData.Size == 0) return Vector3.Zero;

            // Unscale the pixel coordinates
            float px = pixel.X / (metaData.Size * metaData.HorizontalStretch);
            float py = pixel.Y / (metaData.Size * metaData.VerticalStretch);

            float q, r;

            if (metaData.IsPointyTop)
            {
                // Inverse of pointy-top: q = sqrt(3)/3 * x - 1/3 * y, r = 2/3 * y
                q = Sqrt3 / 3.0f * px - 1.0f / 3.0f * py;
                r = 2.0f / 3.0f * py;
            }
            else
            {
                // Inverse of flat-top: q = 2/3 * x, r = -1/3 * x + sqrt(3)/3 * y
                q = 2.0f / 3.0f * px;
                r = -(1.0f / 3.0f) * px + Sqrt3 / 3.0f * py;
            }

            return RoundToHex(new Vector3(q, r, -q - r));
        }

        /// <summary>
        /// Rounds fractional hex coordinates to the nearest valid integer hex coordinates.
        /// Maintains the hex constraint that q + r + s = 0.
        /// </summary>
        /// <param name="fractionalHex">Fractional hex coordinates</param>
        /// <returns>Rounded hex coordinates as Vector3</returns>
        public static Vector3 RoundToHex(Vector3 fractionalHex)
        {
            float rQ = MathF.Round(fractionalHex.X);
            float rR = MathF.Round(fractionalHex.Y);
            float rS = MathF.Round(fractionalHex.Z);

            float dQ = MathF.Abs(rQ - fractionalHex.X);
            float dR = MathF.Abs(rR - fractionalHex.Y);
            float dS = MathF.Abs(rS - fractionalHex.Z);

            if (dQ > dR && dQ > dS)
                rQ = -rR - rS;
            else if (dR > dS)
                rR = -rQ - rS;
            else
                rS = -rQ - rR;

            return new Vector3(rQ, rR, rS);
        }

        /// <summary>
        /// Converts cube coordinates (Vector3) to axial coordinates (Vector2).
        /// </summary>
        /// <param name="cube">Cube coordinates as Vector3</param>
        /// <returns>Axial coordinates as Vector2</returns>
        public static Vector2 ToAxial(Vector3 cube) => new(cube.X, cube.Y);

        /// <summary>
        /// Converts axial coordinates (Vector2) to cube coordinates (Vector3).
        /// </summary>
        /// <param name="axial">Axial coordinates as Vector2</param>
        /// <returns>Cube coordinates as Vector3</returns>
        public static Vector3 ToCube(Vector2 axial) => new(axial.X, axial.Y, -axial.X - axial.Y);

        #endregion

        #region Distance-Based Movement

        /// <summary>
        /// Gets a hex at a specific distance in one of the six hex grid directions.
        /// </summary>
        /// <param name="origin">Starting hex coordinate</param>
        /// <param name="direction">One of the six hex directions from HexApi.Directions</param>
        /// <param name="distance">Distance in hex units (1 = adjacent, 2 = two hexes away, etc.)</param>
        /// <returns>Hex coordinate at the specified distance</returns>
        /// <exception cref="ArgumentException">Thrown when direction is not one of the six hex directions</exception>
        public static Vector3 GetHexAtDistance(Vector3 origin, Vector3 direction, int distance)
        {
            if (!Directions.Contains(direction))
                throw new ArgumentException("Direction must be one of the six hex directions", nameof(direction));

            if (distance <= 0) return origin;

            return RoundToHex(origin + direction * distance);
        }

        /// <summary>
        /// Gets a hex at a specific distance in the direction from origin towards a target pixel position.
        /// Useful for mouse-based direction calculations where you have a target world position.
        /// </summary>
        /// <param name="origin">Starting hex coordinate</param>
        /// <param name="targetPixel">Target pixel/world position to calculate direction towards</param>
        /// <param name="distance">Distance in hex units (1.0f = adjacent, 2.0f = two hexes away, etc.)</param>
        /// <param name="metaData">Hex metadata containing size and stretch information</param>
        /// <returns>Hex coordinate at the specified distance towards the target pixel</returns>
        public static Vector3 GetHexAtPixelDistance(Vector3 origin, Vector2 targetPixel, float distance, HexMetaData metaData)
        {
            if (distance <= 0) return origin;

            var originPixel = HexToPixel(origin, metaData);
            var pixelDirection = targetPixel - originPixel;

            // Handle case where target is at origin
            if (pixelDirection.LengthSquared() < Epsilon) return origin;

            // System.Numerics: Vector2.Normalize
            var normalizedDir = Vector2.Normalize(pixelDirection);
            var effectiveStepSize = CalculateEffectiveStepSize(normalizedDir, metaData);
            var stepPixel = originPixel + normalizedDir * effectiveStepSize * distance;

            return RoundToHex(PixelToHex(stepPixel, metaData));
        }


        /// <summary>
        /// Calculates the effective step size for a direction, accounting for stretch factors.
        /// </summary>
        /// <param name="direction">Normalized direction vector</param>
        /// <param name="metaData">Hex metadata containing stretch factors</param>
        /// <returns>Effective step size considering stretch factors</returns>
        private static float CalculateEffectiveStepSize(Vector2 direction, HexMetaData metaData)
        {
            var stretchedDir = new Vector2(
                direction.X * metaData.HorizontalStretch,
                direction.Y * metaData.VerticalStretch
            );

            return metaData.Size * stretchedDir.Length();
        }

        #endregion

        #region Rotation Operations

        /// <summary>
        /// Rotates a hex coordinate around an origin by the specified number of 60-degree steps.
        /// </summary>
        /// <param name="hexToRotate">The hex coordinate to rotate</param>
        /// <param name="origin">The center point of rotation</param>
        /// <param name="steps">Number of 60-degree steps to rotate (positive = clockwise, negative = counterclockwise)</param>
        /// <returns>The rotated hex coordinate</returns>
        public static Vector3 RotateBySteps(Vector3 hexToRotate, Vector3 origin, int steps)
        {
            Vector3 relativePos = hexToRotate - origin;
            int normalizedSteps = ((steps % 6) + 6) % 6;
            
            Vector3 rotatedRelativePos = normalizedSteps switch
            {
                0 => relativePos,
                1 => new Vector3(-relativePos.Y, -relativePos.Z, -relativePos.X),
                2 => new Vector3(relativePos.Z, relativePos.X, relativePos.Y),
                3 => new Vector3(-relativePos.X, -relativePos.Y, -relativePos.Z),
                4 => new Vector3(relativePos.Y, relativePos.Z, relativePos.X),
                5 => new Vector3(-relativePos.Z, -relativePos.X, -relativePos.Y),
                _ => relativePos
            };
            
            return rotatedRelativePos + origin;
        }

        /// <summary>
        /// Rotates a hex to match the orientation between two reference hexes around an origin.
        /// </summary>
        /// <param name="orientFrom">Source orientation reference hex</param>
        /// <param name="orientTo">Target orientation reference hex</param>
        /// <param name="hexToRotate">The hex coordinate to rotate</param>
        /// <param name="origin">The center point of rotation</param>
        /// <param name="forceClockwise">Optional: force clockwise (true) or counterclockwise (false) rotation</param>
        /// <returns>The rotated hex coordinate</returns>
        /// <exception cref="ArgumentException">Thrown when rotation parameters are invalid</exception>
        public static Vector3 RotateToMatch(Vector3 orientFrom, Vector3 orientTo, Vector3 hexToRotate, Vector3 origin, bool? forceClockwise = null)
        {
            if (orientFrom == orientTo) return hexToRotate;

            var (cwSteps, ccwSteps) = GetRotationSteps(orientFrom, orientTo, origin);
            if (cwSteps == -1)
                throw new ArgumentException($"Invalid rotation from {orientFrom} to {orientTo} around {origin}");

            int steps = forceClockwise.HasValue
                ? (forceClockwise.Value ? cwSteps : -ccwSteps)
                : (cwSteps <= ccwSteps ? cwSteps : -ccwSteps);

            return RotateBySteps(hexToRotate, origin, steps);
        }

        /// <summary>
        /// Calculates clockwise and counterclockwise step counts needed for rotation between two orientations.
        /// </summary>
        /// <param name="from">Source orientation hex</param>
        /// <param name="to">Target orientation hex</param>
        /// <param name="origin">Center of rotation</param>
        /// <returns>Tuple containing (clockwise steps, counterclockwise steps), or (-1, -1) if invalid</returns>
        private static (int ClockwiseSteps, int CounterClockwiseSteps) GetRotationSteps(Vector3 from, Vector3 to, Vector3 origin)
        {
            if (from == origin || to == origin || Distance(from, origin) != Distance(to, origin))
                return (-1, -1);

            Vector3 relativeFrom = from - origin;
            Vector3 relativeTo = to - origin;

            for (int i = 0; i < 6; i++)
            {
                // Replacement for Godot's IsEqualApprox. 
                // Since hex coordinates usually end up as integers, a small epsilon is safe.
                if (Vector3.DistanceSquared(relativeFrom, relativeTo) < Epsilon)
                {
                    int ccwSteps = (6 - i) % 6;
                    return (i, ccwSteps);
                }
                relativeFrom = new Vector3(-relativeFrom.Y, -relativeFrom.Z, -relativeFrom.X);
            }

            return (-1, -1);
        }

        #endregion

        #region Validation Operations

        /// <summary>
        /// Checks whether a Coordinate is a valid Hex Coordinate.
        /// </summary>
        /// <param name="coord">The coordinate to check.</param>
        /// <returns>True if Coordinate is a valid Hex Coordinate otherwise False.</returns>
        public static bool IsValidHexCoordinate(Vector3 coord) 
            => MathF.Abs(coord.X + coord.Y + coord.Z) < Epsilon;

        /// <summary>
        /// Checks whether a Coordinate is a valid Hex Direction.
        /// </summary>
        /// <param name="coord">The coordinate to check.</param>
        /// <returns>True if Coordinate is a valid Hex Direction otherwise False.</returns>
        public static bool IsValidHexDirection(Vector3 coord) 
            => Directions.Contains(coord);

        #endregion
    }

    /// <summary>
    /// Metadata structure containing hex grid configuration parameters.
    /// Defines the size, orientation, and stretch factors for hex grid conversions.
    /// </summary>
    /// <param name="Size">The base size/radius of hexagons</param>
    /// <param name="IsPointyTop">True for pointy-top orientation, false for flat-top</param>
    /// <param name="HorizontalStretch">Horizontal scaling factor</param>
    /// <param name="VerticalStretch">Vertical scaling factor</param>
    public record struct HexMetaData
    (
        float Size = 1.0f,
        bool IsPointyTop = true,
        float HorizontalStretch = 1.0f,
        float VerticalStretch = 1.0f
    );
}
