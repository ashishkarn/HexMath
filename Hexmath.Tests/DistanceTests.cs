using System.Numerics;

namespace Hexmath.Tests;

public class DistanceTests
{
    private static readonly Vector3 Origin = Vector3.Zero;

    [Fact]
    public void Distance_FromOriginToEachDirection_IsOne()
    {
        foreach (var dir in HMath.Directions)
        {
            Assert.Equal(1, HMath.Distance(Origin, dir));
        }
    }

    [Fact]
    public void Distance_IsSymmetric()
    {
        var hexA = new Vector3(3, -2, -1);
        var hexB = new Vector3(-1, 2, -1);

        Assert.Equal(HMath.Distance(hexA, hexB), HMath.Distance(hexB, hexA));
    }

    [Fact]
    public void Distance_ToSameHex_IsZero()
    {
        var hex = new Vector3(2, -1, -1);
        Assert.Equal(0, HMath.Distance(hex, hex));
    }

    [Theory]
    [InlineData(0, 0, 0, 2, -1, -1, 2)]
    [InlineData(0, 0, 0, 3, -3, 0, 3)]
    [InlineData(1, 0, -1, -1, 0, 1, 2)]
    public void Distance_CalculatesCorrectly(float x1, float y1, float z1, float x2, float y2, float z2, int expected)
    {
        var from = new Vector3(x1, y1, z1);
        var to = new Vector3(x2, y2, z2);

        Assert.Equal(expected, HMath.Distance(from, to));
    }

    [Fact]
    public void Neighbor_MovesInCorrectDirection()
    {
        var east = HMath.Directions[0]; // EAST (1, 0, -1)
        var result = HMath.Neighbor(Origin, east);

        Assert.Equal(east, result);
    }

    [Fact]
    public void Neighbor_WithDistance_MovesMultipleSteps()
    {
        var east = HMath.Directions[0];
        var result = HMath.Neighbor(Origin, east, 3);

        Assert.Equal(new Vector3(3, 0, -3), result);
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
    public void NextDirection_Clockwise_ReturnsCorrectSequence()
    {
        var current = HMath.Directions[0]; // EAST

        for (int i = 1; i < 6; i++)
        {
            current = HMath.NextDirection(current, clockwise: true)!.Value;
            Assert.Equal(HMath.Directions[i], current);
        }
    }

    [Fact]
    public void NextDirection_CounterClockwise_ReturnsCorrectSequence()
    {
        var current = HMath.Directions[0]; // EAST
        current = HMath.NextDirection(current, clockwise: false)!.Value;

        Assert.Equal(HMath.Directions[5], current); // NORTHEAST
    }

    [Fact]
    public void NextDirection_InvalidDirection_ReturnsNull()
    {
        var invalid = new Vector3(5, 5, 5);
        Assert.Null(HMath.NextDirection(invalid));
    }

    [Fact]
    public void AreInStraightLine_SameHex_ReturnsTrue()
    {
        var hex = new Vector3(2, -1, -1);
        Assert.True(HMath.AreInStraightLine(hex, hex));
    }

    [Fact]
    public void AreInStraightLine_AlignedHexes_ReturnsTrue()
    {
        var hexA = Origin;
        var hexB = new Vector3(3, 0, -3); // 3 steps EAST

        Assert.True(HMath.AreInStraightLine(hexA, hexB));
    }

    [Fact]
    public void AreInStraightLine_NonAlignedHexes_ReturnsFalse()
    {
        var hexA = Origin;
        var hexB = new Vector3(2, -1, -1); // Not on a straight line from origin

        Assert.False(HMath.AreInStraightLine(hexA, hexB));
    }

    [Fact]
    public void GetClosestHexDirection_ExactDirection_ReturnsSame()
    {
        var east = HMath.Directions[0];
        Assert.Equal(east, HMath.GetClosestHexDirection(east));
    }

    [Fact]
    public void GetClosestHexDirection_ScaledDirection_ReturnsNormalized()
    {
        var scaledEast = new Vector3(5, 0, -5);
        var expected = HMath.Directions[0]; // EAST

        Assert.Equal(expected, HMath.GetClosestHexDirection(scaledEast));
    }

    [Fact]
    public void GetClosestHexDirection_ZeroVector_ReturnsZero()
    {
        Assert.Equal(Vector3.Zero, HMath.GetClosestHexDirection(Vector3.Zero));
    }
}
