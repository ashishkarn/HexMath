using System.Numerics;

namespace Hexmath.Tests;

public class ValidationTests
{
    [Theory]
    [InlineData(0, 0, 0, true)]
    [InlineData(1, -1, 0, true)]
    [InlineData(1, 0, -1, true)]
    [InlineData(2, -3, 1, true)]
    [InlineData(-5, 2, 3, true)]
    [InlineData(1, 1, 1, false)]
    [InlineData(1, 0, 0, false)]
    [InlineData(0, 1, 1, false)]
    public void IsValidHexCoordinate_ReturnsExpected(float x, float y, float z, bool expected)
    {
        var coord = new Vector3(x, y, z);
        Assert.Equal(expected, HMath.IsValidHexCoordinate(coord));
    }

    [Fact]
    public void IsValidHexDirection_AllDirectionsAreValid()
    {
        foreach (var dir in HMath.Directions)
        {
            Assert.True(HMath.IsValidHexDirection(dir));
        }
    }

    [Fact]
    public void IsValidHexDirection_InvalidDirectionReturnsFalse()
    {
        var invalid = new Vector3(2, 0, -2);
        Assert.False(HMath.IsValidHexDirection(invalid));
    }

    [Fact]
    public void IsValidHexDirection_ZeroVectorReturnsFalse()
    {
        Assert.False(HMath.IsValidHexDirection(Vector3.Zero));
    }

    [Fact]
    public void Directions_HasSixElements()
    {
        Assert.Equal(6, HMath.Directions.Count);
    }

    [Fact]
    public void Directions_AllSatisfyHexConstraint()
    {
        foreach (var dir in HMath.Directions)
        {
            Assert.True(HMath.IsValidHexCoordinate(dir));
        }
    }
}
