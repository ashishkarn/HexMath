using System.Numerics;

namespace Hexmath.Tests;

public class DistanceTests
{
    private static readonly HexCoord Origin = HexCoord.Zero;

    [Fact]
    public void Distance_FromOriginToEachDirection_IsOne()
    {
        foreach (var dir in HMath.AllDirections)
        {
            Assert.Equal(1, HMath.Distance(Origin, dir.ToOffset()));
        }
    }

    [Fact]
    public void Distance_IsSymmetric()
    {
        var hexA = new HexCoord(3, -2);
        var hexB = new HexCoord(-1, 2);

        Assert.Equal(HMath.Distance(hexA, hexB), HMath.Distance(hexB, hexA));
    }

    [Fact]
    public void Distance_ToSameHex_IsZero()
    {
        var hex = new HexCoord(2, -1);
        Assert.Equal(0, HMath.Distance(hex, hex));
    }

    [Theory]
    [InlineData(0, 0, 2, -1, 2)]
    [InlineData(0, 0, 3, -3, 3)]
    [InlineData(1, 0, -1, 0, 2)]
    public void Distance_CalculatesCorrectly(int q1, int r1, int q2, int r2, int expected)
    {
        var from = new HexCoord(q1, r1);
        var to = new HexCoord(q2, r2);

        Assert.Equal(expected, HMath.Distance(from, to));
    }

    [Fact]
    public void Neighbor_MovesInCorrectDirection()
    {
        var result = HMath.Neighbor(Origin, HexDirection.East);

        Assert.Equal(new HexCoord(1, 0), result);
    }

    [Fact]
    public void Neighbor_WithDistance_MovesMultipleSteps()
    {
        var result = HMath.Neighbor(Origin, HexDirection.East, 3);

        Assert.Equal(new HexCoord(3, 0), result);
        Assert.Equal(-3, result.S);
    }

    [Theory]
    [InlineData(HexDirection.East, 1, 0)]
    [InlineData(HexDirection.West, -1, 0)]
    [InlineData(HexDirection.SouthEast, 1, -1)]
    [InlineData(HexDirection.NorthWest, -1, 1)]
    public void Neighbor_AllDirections_MoveCorrectly(HexDirection direction, int expectedQ, int expectedR)
    {
        var result = HMath.Neighbor(Origin, direction);

        Assert.Equal(expectedQ, result.Q);
        Assert.Equal(expectedR, result.R);
    }

    [Fact]
    public void GetAllNeighbors_ReturnsSixHexes()
    {
        var neighbors = HMath.GetAllNeighbors(Origin).ToList();

        Assert.Equal(6, neighbors.Count);
    }

    [Fact]
    public void GetAllNeighbors_AllAtDistanceOne()
    {
        var neighbors = HMath.GetAllNeighbors(Origin);

        foreach (var neighbor in neighbors)
        {
            Assert.Equal(1, HMath.Distance(Origin, neighbor));
        }
    }

    [Fact]
    public void GetAllNeighbors_AllUnique()
    {
        var neighbors = HMath.GetAllNeighbors(Origin).ToList();

        Assert.Equal(6, neighbors.Distinct().Count());
    }

    [Fact]
    public void GetAllNeighbors_FromNonOrigin_ReturnsCorrectNeighbors()
    {
        var center = new HexCoord(2, -1);
        var neighbors = HMath.GetAllNeighbors(center).ToList();

        Assert.Equal(6, neighbors.Count);
        foreach (var neighbor in neighbors)
        {
            Assert.Equal(1, HMath.Distance(center, neighbor));
        }
    }

    [Fact]
    public void AreInStraightLine_SameHex_ReturnsTrue()
    {
        var hex = new HexCoord(2, -1);
        Assert.True(HMath.AreInStraightLine(hex, hex));
    }

    [Fact]
    public void AreInStraightLine_AlignedHexes_East_ReturnsTrue()
    {
        var hexA = Origin;
        var hexB = new HexCoord(3, 0); // 3 steps EAST (dR = 0)

        Assert.True(HMath.AreInStraightLine(hexA, hexB));
    }

    [Fact]
    public void AreInStraightLine_AlignedHexes_SouthEast_ReturnsTrue()
    {
        var hexA = Origin;
        var hexB = new HexCoord(2, -2); // steps along SE (dS = 0)

        Assert.True(HMath.AreInStraightLine(hexA, hexB));
    }

    [Fact]
    public void AreInStraightLine_AlignedHexes_SouthWest_ReturnsTrue()
    {
        var hexA = Origin;
        var hexB = new HexCoord(0, -3); // steps along SW (dQ = 0)

        Assert.True(HMath.AreInStraightLine(hexA, hexB));
    }

    [Fact]
    public void AreInStraightLine_NonAlignedHexes_ReturnsFalse()
    {
        var hexA = Origin;
        var hexB = new HexCoord(2, -1); // Not on a straight line from origin

        Assert.False(HMath.AreInStraightLine(hexA, hexB));
    }

    [Fact]
    public void GetClosestHexDirection_PositiveX_ReturnsEast()
    {
        var direction = new Vector2(1, 0);
        var result = HMath.GetClosestHexDirection(direction);

        Assert.Equal(HexDirection.East, result);
    }

    [Fact]
    public void GetClosestHexDirection_NegativeX_ReturnsWest()
    {
        var direction = new Vector2(-1, 0);
        var result = HMath.GetClosestHexDirection(direction);

        Assert.Equal(HexDirection.West, result);
    }

    [Fact]
    public void GetClosestHexDirection_ScaledDirection_ReturnsSame()
    {
        var direction = new Vector2(100, 0);
        var result = HMath.GetClosestHexDirection(direction);

        Assert.Equal(HexDirection.East, result);
    }

    [Fact]
    public void GetClosestHexDirection_ZeroVector_ThrowsException()
    {
        Assert.Throws<ArgumentException>(() => HMath.GetClosestHexDirection(Vector2.Zero));
    }
}
