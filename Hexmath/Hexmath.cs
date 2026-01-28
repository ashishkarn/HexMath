using System.Numerics;

namespace Hexmath
{
    #region Types

    /// <summary>
    /// Represents a hexagonal coordinate using axial coordinates (Q, R).
    /// The third cube coordinate S is computed as -Q - R, enforcing the constraint Q + R + S = 0.
    /// </summary>
    public readonly struct HexCoord(int q, int r) : IEquatable<HexCoord>
    {
        /// <summary>The Q coordinate (column in axial representation)</summary>
        public int Q { get; } = q;

        /// <summary>The R coordinate (row in axial representation)</summary>
        public int R { get; } = r;

        /// <summary>The S coordinate, computed as -Q - R (cube coordinate constraint)</summary>
        public int S => -Q - R;

        /// <summary>The origin coordinate (0, 0)</summary>
        public static readonly HexCoord Zero = new(0, 0);

        // Operators
        public static HexCoord operator +(HexCoord a, HexCoord b)
            => new(a.Q + b.Q, a.R + b.R);

        public static HexCoord operator -(HexCoord a, HexCoord b)
            => new(a.Q - b.Q, a.R - b.R);

        public static HexCoord operator *(HexCoord a, int scalar)
            => new(a.Q * scalar, a.R * scalar);

        public static HexCoord operator *(int scalar, HexCoord a)
            => new(a.Q * scalar, a.R * scalar);

        public static HexCoord operator -(HexCoord a)
            => new(-a.Q, -a.R);

        public static bool operator ==(HexCoord a, HexCoord b)
            => a.Q == b.Q && a.R == b.R;

        public static bool operator !=(HexCoord a, HexCoord b)
            => !(a == b);

        /// <summary>
        /// Converts to Vector3 for game engine interop.
        /// Returns (Q, R, S) where S = -Q - R.
        /// </summary>
        public Vector3 ToVector3() => new(Q, R, S);

        /// <summary>
        /// Creates a HexCoord from a Vector3, using X as Q and Y as R.
        /// The Z component is ignored (S is computed from Q and R).
        /// </summary>
        public static HexCoord FromVector3(Vector3 vec) => new((int)vec.X, (int)vec.Y);

        // Equality
        public bool Equals(HexCoord other) => Q == other.Q && R == other.R;

        public override bool Equals(object? obj)
            => obj is HexCoord other && Equals(other);

        public override int GetHashCode() => HashCode.Combine(Q, R);

        public override string ToString() => $"HexCoord({Q}, {R}, {S})";
    }

    /// <summary>
    /// Represents one of the six cardinal directions on a hexagonal grid.
    /// Values are ordered clockwise starting from East for flat-top hexagons.
    /// </summary>
    public enum HexDirection
    {
        /// <summary>East: offset (1, 0, -1)</summary>
        East = 0,

        /// <summary>SouthEast: offset (1, -1, 0)</summary>
        SouthEast = 1,

        /// <summary>SouthWest: offset (0, -1, 1)</summary>
        SouthWest = 2,

        /// <summary>West: offset (-1, 0, 1)</summary>
        West = 3,

        /// <summary>NorthWest: offset (-1, 1, 0)</summary>
        NorthWest = 4,

        /// <summary>NorthEast: offset (0, 1, -1)</summary>
        NorthEast = 5
    }

    /// <summary>
    /// Extension methods for HexDirection enum.
    /// </summary>
    public static class HexDirectionExtensions
    {
        private static readonly HexCoord[] Offsets =
        {
            new(1, 0),   // East
            new(1, -1),  // SouthEast
            new(0, -1),  // SouthWest
            new(-1, 0),  // West
            new(-1, 1),  // NorthWest
            new(0, 1)    // NorthEast
        };

        /// <summary>
        /// Gets the coordinate offset for this direction.
        /// </summary>
        public static HexCoord ToOffset(this HexDirection direction)
            => Offsets[(int)direction];

        /// <summary>
        /// Gets the next direction in clockwise or counterclockwise order.
        /// </summary>
        public static HexDirection Next(this HexDirection direction, bool clockwise = true)
        {
            int offset = clockwise ? 1 : -1;
            int newIndex = (((int)direction + offset) % 6 + 6) % 6;
            return (HexDirection)newIndex;
        }

        /// <summary>
        /// Gets the opposite direction.
        /// </summary>
        public static HexDirection Opposite(this HexDirection direction)
            => (HexDirection)(((int)direction + 3) % 6);

        /// <summary>
        /// Converts to Vector3 for game engine interop.
        /// </summary>
        public static Vector3 ToVector3(this HexDirection direction)
            => Offsets[(int)direction].ToVector3();
    }

    #endregion

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
        /// All six hex directions in clockwise order.
        /// </summary>
        public static IReadOnlyList<HexDirection> AllDirections { get; } =
            Enum.GetValues<HexDirection>().ToList().AsReadOnly();

        #endregion

        #region Direction and Navigation

        /// <summary>
        /// Returns the next direction in clockwise or counterclockwise order.
        /// </summary>
        /// <param name="direction">The current hex direction</param>
        /// <param name="clockwise">True for clockwise rotation, false for counterclockwise</param>
        /// <returns>The next direction</returns>
        public static HexDirection NextDirection(HexDirection direction, bool clockwise = true)
            => direction.Next(clockwise);

        /// <summary>
        /// Gets a neighboring hex at the specified direction and distance from the origin.
        /// </summary>
        /// <param name="origin">The starting hex coordinate</param>
        /// <param name="direction">The direction to move in</param>
        /// <param name="distance">The number of hex steps to move (default: 1)</param>
        /// <returns>The hex coordinate at the specified position</returns>
        public static HexCoord Neighbor(HexCoord origin, HexDirection direction, int distance = 1)
            => origin + direction.ToOffset() * distance;

        /// <summary>
        /// Returns all 6 immediately adjacent hexes to the given origin hex.
        /// </summary>
        /// <param name="origin">The center hex coordinate</param>
        /// <returns>Enumerable of all adjacent hex coordinates</returns>
        public static IEnumerable<HexCoord> GetAllNeighbors(HexCoord origin)
        {
            foreach (HexDirection dir in AllDirections)
                yield return origin + dir.ToOffset();
        }

        /// <summary>
        /// Gets the closest hex direction to an arbitrary 2D direction vector.
        /// Useful for determining direction from pixel/world coordinates.
        /// </summary>
        /// <param name="direction">The direction vector to match (e.g., from pixel coordinates)</param>
        /// <returns>The closest hex direction</returns>
        /// <exception cref="ArgumentException">Thrown when direction is a zero vector</exception>
        public static HexDirection GetClosestHexDirection(Vector2 direction)
        {
            if (direction.LengthSquared() < Epsilon)
                throw new ArgumentException("Cannot determine direction from zero vector", nameof(direction));

            var normalized = Vector2.Normalize(direction);
            HexDirection closest = HexDirection.East;
            float smallestAngle = float.MaxValue;

            foreach (HexDirection dir in AllDirections)
            {
                var hexDir = dir.ToOffset();
                var hexDir2D = new Vector2(hexDir.Q, hexDir.R);
                var hexNormalized = Vector2.Normalize(hexDir2D);

                float similarity = Vector2.Dot(normalized, hexNormalized);
                float angle = MathF.Acos(Math.Clamp(similarity, -1f, 1f));

                if (angle < smallestAngle)
                {
                    smallestAngle = angle;
                    closest = dir;
                }
            }

            return closest;
        }

        /// <summary>
        /// Determines whether two hex coordinates lie on the same straight line along one of the six hex directions.
        /// </summary>
        /// <param name="hexA">The first hex coordinate</param>
        /// <param name="hexB">The second hex coordinate</param>
        /// <returns>True if the two hexes are aligned in a straight line; otherwise, false</returns>
        public static bool AreInStraightLine(HexCoord hexA, HexCoord hexB)
        {
            if (hexA == hexB) return true;

            var diff = hexB - hexA;
            int dQ = diff.Q, dR = diff.R, dS = diff.S;

            // On a straight line if one of Q, R, S is zero
            return dQ == 0 || dR == 0 || dS == 0;
        }

        #endregion

        #region Distance and Area Operations

        /// <summary>
        /// Calculates the Manhattan distance between two hex coordinates.
        /// </summary>
        /// <param name="from">Starting hex coordinate</param>
        /// <param name="to">Destination hex coordinate</param>
        /// <returns>The distance in hex units</returns>
        public static int Distance(HexCoord from, HexCoord to)
        {
            var diff = to - from;
            return Math.Max(Math.Max(Math.Abs(diff.Q), Math.Abs(diff.R)), Math.Abs(diff.S));
        }

        /// <summary>
        /// Returns all hexes forming a ring at the specified radius from the origin.
        /// </summary>
        /// <param name="origin">The center hex coordinate</param>
        /// <param name="radius">The radius of the ring (must be positive)</param>
        /// <returns>Enumerable of hex coordinates forming the ring</returns>
        public static IEnumerable<HexCoord> GetRing(HexCoord origin, int radius)
        {
            if (radius <= 0) yield break;

            var current = origin + HexDirection.NorthWest.ToOffset() * radius;

            foreach (HexDirection dir in AllDirections)
            {
                for (int j = 0; j < radius; j++)
                {
                    yield return current;
                    current = current + dir.ToOffset();
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
        public static IEnumerable<HexCoord> GetSpiral(HexCoord origin, int maxRadius, bool includeOrigin = true)
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
        public static Vector2 HexToPixel(HexCoord hex, HexMetaData metaData)
        {
            float x, y;

            if (metaData.IsPointyTop)
            {
                x = (Sqrt3 * hex.Q + Sqrt3 * 0.5f * hex.R) * metaData.Size * metaData.HorizontalStretch;
                y = 1.5f * hex.R * metaData.Size * metaData.VerticalStretch;
            }
            else
            {
                x = 1.5f * hex.Q * metaData.Size * metaData.HorizontalStretch;
                y = (Sqrt3 * 0.5f * hex.Q + Sqrt3 * hex.R) * metaData.Size * metaData.VerticalStretch;
            }

            return new Vector2(x, y);
        }

        /// <summary>
        /// Converts 2D pixel coordinates in local grid space to hex coordinates.
        /// </summary>
        /// <param name="pixel">The 2D local position to convert</param>
        /// <param name="metaData">Hex grid metadata containing size and layout information</param>
        /// <returns>Hex coordinate</returns>
        public static HexCoord PixelToHex(Vector2 pixel, HexMetaData metaData)
        {
            if (metaData.Size == 0) return HexCoord.Zero;

            float px = pixel.X / (metaData.Size * metaData.HorizontalStretch);
            float py = pixel.Y / (metaData.Size * metaData.VerticalStretch);

            float q, r;

            if (metaData.IsPointyTop)
            {
                q = Sqrt3 / 3.0f * px - 1.0f / 3.0f * py;
                r = 2.0f / 3.0f * py;
            }
            else
            {
                q = 2.0f / 3.0f * px;
                r = -(1.0f / 3.0f) * px + Sqrt3 / 3.0f * py;
            }

            return RoundToHex(q, r);
        }

        /// <summary>
        /// Rounds fractional hex coordinates to the nearest valid integer hex coordinates.
        /// Maintains the hex constraint that q + r + s = 0.
        /// </summary>
        /// <param name="q">Fractional Q coordinate</param>
        /// <param name="r">Fractional R coordinate</param>
        /// <returns>Rounded hex coordinates</returns>
        internal static HexCoord RoundToHex(float q, float r)
        {
            float s = -q - r;

            int rQ = (int)MathF.Round(q);
            int rR = (int)MathF.Round(r);
            int rS = (int)MathF.Round(s);

            float dQ = MathF.Abs(rQ - q);
            float dR = MathF.Abs(rR - r);
            float dS = MathF.Abs(rS - s);

            if (dQ > dR && dQ > dS)
                rQ = -rR - rS;
            else if (dR > dS)
                rR = -rQ - rS;
            // else S is derived from Q and R anyway

            return new HexCoord(rQ, rR);
        }

        #endregion

        #region Distance-Based Movement

        /// <summary>
        /// Gets a hex at a specific distance in one of the six hex grid directions.
        /// </summary>
        /// <param name="origin">Starting hex coordinate</param>
        /// <param name="direction">The hex direction to move in</param>
        /// <param name="distance">Distance in hex units (1 = adjacent, 2 = two hexes away, etc.)</param>
        /// <returns>Hex coordinate at the specified distance</returns>
        public static HexCoord GetHexAtDistance(HexCoord origin, HexDirection direction, int distance)
        {
            if (distance <= 0) return origin;
            return origin + direction.ToOffset() * distance;
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
        public static HexCoord GetHexAtPixelDistance(HexCoord origin, Vector2 targetPixel, float distance, HexMetaData metaData)
        {
            if (distance <= 0) return origin;

            var originPixel = HexToPixel(origin, metaData);
            var pixelDirection = targetPixel - originPixel;

            if (pixelDirection.LengthSquared() < Epsilon) return origin;

            var normalizedDir = Vector2.Normalize(pixelDirection);
            var effectiveStepSize = CalculateEffectiveStepSize(normalizedDir, metaData);
            var stepPixel = originPixel + normalizedDir * effectiveStepSize * distance;

            return PixelToHex(stepPixel, metaData);
        }

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
        /// Positive steps rotate counterclockwise, negative steps rotate clockwise.
        /// </summary>
        /// <param name="hexToRotate">The hex coordinate to rotate</param>
        /// <param name="origin">The center point of rotation</param>
        /// <param name="steps">Number of 60-degree steps to rotate</param>
        /// <returns>The rotated hex coordinate</returns>
        public static HexCoord RotateBySteps(HexCoord hexToRotate, HexCoord origin, int steps)
        {
            var relative = hexToRotate - origin;
            int normalizedSteps = ((steps % 6) + 6) % 6;

            var rotated = normalizedSteps switch
            {
                0 => relative,
                1 => new HexCoord(-relative.R, -relative.S),
                2 => new HexCoord(relative.S, relative.Q),
                3 => new HexCoord(-relative.Q, -relative.R),
                4 => new HexCoord(relative.R, relative.S),
                5 => new HexCoord(-relative.S, -relative.Q),
                _ => relative
            };

            return rotated + origin;
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
        public static HexCoord RotateToMatch(HexCoord orientFrom, HexCoord orientTo, HexCoord hexToRotate, HexCoord origin, bool? forceClockwise = null)
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

        private static (int ClockwiseSteps, int CounterClockwiseSteps) GetRotationSteps(HexCoord from, HexCoord to, HexCoord origin)
        {
            if (from == origin || to == origin || Distance(from, origin) != Distance(to, origin))
                return (-1, -1);

            var relativeFrom = from - origin;
            var relativeTo = to - origin;

            for (int i = 0; i < 6; i++)
            {
                if (relativeFrom == relativeTo)
                {
                    int ccwSteps = (6 - i) % 6;
                    return (i, ccwSteps);
                }
                // Rotate by 1 step (counterclockwise)
                relativeFrom = new HexCoord(-relativeFrom.R, -relativeFrom.S);
            }

            return (-1, -1);
        }

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
