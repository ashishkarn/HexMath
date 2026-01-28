using System.Numerics;

namespace Hexmath.Tests;

public class HexCoordTests
{
    #region Construction and Properties

    [Fact]
    public void Constructor_SetsQAndR()
    {
        var coord = new HexCoord(3, -2);

        Assert.Equal(3, coord.Q);
        Assert.Equal(-2, coord.R);
    }

    [Fact]
    public void S_IsComputedCorrectly()
    {
        var coord = new HexCoord(3, -2);

        Assert.Equal(-1, coord.S);
    }

    [Theory]
    [InlineData(0, 0, 0)]
    [InlineData(1, -1, 0)]
    [InlineData(2, -3, 1)]
    [InlineData(-5, 2, 3)]
    public void S_AlwaysSatisfiesConstraint(int q, int r, int expectedS)
    {
        var coord = new HexCoord(q, r);

        Assert.Equal(expectedS, coord.S);
        Assert.Equal(0, coord.Q + coord.R + coord.S);
    }

    [Fact]
    public void Zero_IsOrigin()
    {
        Assert.Equal(0, HexCoord.Zero.Q);
        Assert.Equal(0, HexCoord.Zero.R);
        Assert.Equal(0, HexCoord.Zero.S);
    }

    #endregion

    #region Operators

    [Fact]
    public void Addition_CombinesCoordinates()
    {
        var a = new HexCoord(1, 2);
        var b = new HexCoord(3, -1);

        var result = a + b;

        Assert.Equal(4, result.Q);
        Assert.Equal(1, result.R);
    }

    [Fact]
    public void Subtraction_SubtractsCoordinates()
    {
        var a = new HexCoord(5, 2);
        var b = new HexCoord(3, -1);

        var result = a - b;

        Assert.Equal(2, result.Q);
        Assert.Equal(3, result.R);
    }

    [Theory]
    [InlineData(2, 3, 2, 4, 6)]
    [InlineData(1, -1, 3, 3, -3)]
    [InlineData(-2, 4, 0, 0, 0)]
    public void Multiplication_ScalesCoordinates(int q, int r, int scalar, int expectedQ, int expectedR)
    {
        var coord = new HexCoord(q, r);

        var result = coord * scalar;

        Assert.Equal(expectedQ, result.Q);
        Assert.Equal(expectedR, result.R);
    }

    [Fact]
    public void Multiplication_IsCommutative()
    {
        var coord = new HexCoord(2, -3);

        var result1 = coord * 4;
        var result2 = 4 * coord;

        Assert.Equal(result1, result2);
    }

    [Fact]
    public void Negation_NegatesCoordinates()
    {
        var coord = new HexCoord(3, -2);

        var result = -coord;

        Assert.Equal(-3, result.Q);
        Assert.Equal(2, result.R);
    }

    [Fact]
    public void Equality_TrueForSameCoordinates()
    {
        var a = new HexCoord(2, -1);
        var b = new HexCoord(2, -1);

        Assert.True(a == b);
        Assert.False(a != b);
    }

    [Fact]
    public void Equality_FalseForDifferentCoordinates()
    {
        var a = new HexCoord(2, -1);
        var b = new HexCoord(2, -2);

        Assert.False(a == b);
        Assert.True(a != b);
    }

    #endregion

    #region Equality and Hashing

    [Fact]
    public void Equals_TrueForSameValues()
    {
        var a = new HexCoord(1, 2);
        var b = new HexCoord(1, 2);

        Assert.True(a.Equals(b));
        Assert.True(a.Equals((object)b));
    }

    [Fact]
    public void Equals_FalseForDifferentValues()
    {
        var a = new HexCoord(1, 2);
        var b = new HexCoord(1, 3);

        Assert.False(a.Equals(b));
    }

    [Fact]
    public void Equals_FalseForNull()
    {
        var coord = new HexCoord(1, 2);

        Assert.False(coord.Equals(null));
    }

    [Fact]
    public void GetHashCode_SameForEqualCoordinates()
    {
        var a = new HexCoord(5, -3);
        var b = new HexCoord(5, -3);

        Assert.Equal(a.GetHashCode(), b.GetHashCode());
    }

    [Fact]
    public void GetHashCode_DifferentForDifferentCoordinates()
    {
        var a = new HexCoord(5, -3);
        var b = new HexCoord(-3, 5);

        Assert.NotEqual(a.GetHashCode(), b.GetHashCode());
    }

    #endregion

    #region Conversions

    [Theory]
    [InlineData(0, 0)]
    [InlineData(2, -1)]
    [InlineData(-3, 1)]
    [InlineData(5, -3)]
    public void ToVector3_ReturnsCorrectValues(int q, int r)
    {
        var coord = new HexCoord(q, r);
        int expectedS = -q - r;

        var vec = coord.ToVector3();

        Assert.Equal(q, vec.X);
        Assert.Equal(r, vec.Y);
        Assert.Equal(expectedS, vec.Z);
    }

    [Fact]
    public void ToVector3_MaintainsConstraint()
    {
        var coord = new HexCoord(7, -4);

        var vec = coord.ToVector3();

        Assert.Equal(0, vec.X + vec.Y + vec.Z);
    }

    #endregion

    #region ToString

    [Fact]
    public void ToString_FormatsCorrectly()
    {
        var coord = new HexCoord(3, -2);

        var str = coord.ToString();

        Assert.Equal("HexCoord(3, -2, -1)", str);
    }

    #endregion
}
