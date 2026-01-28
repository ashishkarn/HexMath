using System.Numerics;

namespace Hexmath.Tests;

public class ConversionTests
{
    #region RoundToHex

    [Theory]
    [InlineData(0.1f, 0.1f)]
    [InlineData(2.7f, -1.3f)]
    [InlineData(-0.5f, 0.5f)]
    public void RoundToHex_AlwaysMaintainsConstraint(float q, float r)
    {
        var rounded = HMath.RoundToHex(q, r);

        Assert.Equal(0, rounded.Q + rounded.R + rounded.S);
    }

    [Fact]
    public void RoundToHex_IntegerInput_ReturnsSame()
    {
        var rounded = HMath.RoundToHex(2.0f, -1.0f);

        Assert.Equal(2, rounded.Q);
        Assert.Equal(-1, rounded.R);
        Assert.Equal(-1, rounded.S);
    }

    [Theory]
    [InlineData(1.4f, -0.6f, 1, 0)]      // Rounds to nearest valid hex
    [InlineData(-2.2f, 1.1f, -2, 1)]
    [InlineData(0.0f, 0.0f, 0, 0)]
    [InlineData(0.9f, 0.1f, 1, 0)]       // Near (1, 0)
    [InlineData(-0.9f, -0.1f, -1, 0)]    // Near (-1, 0)
    public void RoundToHex_RoundsToNearestHex(float q, float r, int expectedQ, int expectedR)
    {
        var rounded = HMath.RoundToHex(q, r);

        Assert.Equal(expectedQ, rounded.Q);
        Assert.Equal(expectedR, rounded.R);
    }

    #endregion

    #region HexToPixel

    [Theory]
    [InlineData(false)] // FlatTop
    [InlineData(true)]  // PointyTop
    public void HexToPixel_Origin_ReturnsZero(bool isPointyTop)
    {
        var meta = new HexMetaData(Size: 1.0f, IsPointyTop: isPointyTop);
        var pixel = HMath.HexToPixel(HexCoord.Zero, meta);

        Assert.Equal(0, pixel.X, precision: 3);
        Assert.Equal(0, pixel.Y, precision: 3);
    }

    [Fact]
    public void HexToPixel_FlatTop_EastNeighbor()
    {
        var meta = new HexMetaData(Size: 1.0f, IsPointyTop: false);
        var hex = new HexCoord(1, 0);

        var pixel = HMath.HexToPixel(hex, meta);

        Assert.True(pixel.X > 0);
    }

    [Fact]
    public void HexToPixel_PointyTop_EastNeighbor()
    {
        var meta = new HexMetaData(Size: 1.0f, IsPointyTop: true);
        var hex = new HexCoord(1, 0);

        var pixel = HMath.HexToPixel(hex, meta);

        Assert.True(pixel.X > 0);
    }

    #endregion

    #region PixelToHex

    [Theory]
    [InlineData(0, 0, false)]      // Origin, FlatTop
    [InlineData(0, 0, true)]       // Origin, PointyTop
    [InlineData(1, 0, false)]      // FlatTop
    [InlineData(1, 0, true)]       // PointyTop
    [InlineData(-2, 3, false)]     // FlatTop
    [InlineData(-2, 3, true)]      // PointyTop
    [InlineData(5, -3, false)]     // FlatTop
    [InlineData(5, -3, true)]      // PointyTop
    public void PixelToHex_RoundTrip(int q, int r, bool isPointyTop)
    {
        var original = new HexCoord(q, r);
        var meta = new HexMetaData(Size: 1.0f, IsPointyTop: isPointyTop);
        var pixel = HMath.HexToPixel(original, meta);
        var roundTrip = HMath.PixelToHex(pixel, meta);

        Assert.Equal(original.Q, roundTrip.Q);
        Assert.Equal(original.R, roundTrip.R);
        Assert.Equal(original.S, roundTrip.S);
    }

    [Fact]
    public void PixelToHex_ZeroSize_ReturnsZero()
    {
        var zeroSizeMeta = new HexMetaData(Size: 0);
        var result = HMath.PixelToHex(new Vector2(100, 100), zeroSizeMeta);

        Assert.Equal(HexCoord.Zero, result);
    }

    [Theory]
    [InlineData(1.0f)]
    [InlineData(2.0f)]
    [InlineData(0.5f)]
    public void PixelToHex_DifferentSizes_RoundTrip(float size)
    {
        var original = new HexCoord(3, -2);
        var meta = new HexMetaData(Size: size);
        var pixel = HMath.HexToPixel(original, meta);
        var roundTrip = HMath.PixelToHex(pixel, meta);

        Assert.Equal(original, roundTrip);
    }

    #endregion

    #region GetHexAtDistance

    [Fact]
    public void GetHexAtDistance_ReturnsCorrectHex()
    {
        var origin = HexCoord.Zero;

        var result = HMath.GetHexAtDistance(origin, HexDirection.East, 3);

        Assert.Equal(new HexCoord(3, 0), result);
        Assert.Equal(-3, result.S);
    }

    [Fact]
    public void GetHexAtDistance_ZeroDistance_ReturnsOrigin()
    {
        var origin = new HexCoord(1, -1);

        var result = HMath.GetHexAtDistance(origin, HexDirection.East, 0);

        Assert.Equal(origin, result);
    }

    [Theory]
    [InlineData(HexDirection.East, 2, 2, 0)]
    [InlineData(HexDirection.West, 2, -2, 0)]
    [InlineData(HexDirection.SouthEast, 3, 3, -3)]
    [InlineData(HexDirection.NorthWest, 3, -3, 3)]
    public void GetHexAtDistance_AllDirections(HexDirection direction, int distance, int expectedQ, int expectedR)
    {
        var result = HMath.GetHexAtDistance(HexCoord.Zero, direction, distance);

        Assert.Equal(expectedQ, result.Q);
        Assert.Equal(expectedR, result.R);
    }

    #endregion

    #region GetHexAtPixelDistance

    [Fact]
    public void GetHexAtPixelDistance_ZeroDistance_ReturnsOrigin()
    {
        var origin = new HexCoord(2, -1);
        var meta = new HexMetaData(Size: 1.0f);
        var targetPixel = new Vector2(100, 100);

        var result = HMath.GetHexAtPixelDistance(origin, targetPixel, 0, meta);

        Assert.Equal(origin, result);
    }

    [Fact]
    public void GetHexAtPixelDistance_TargetAtOrigin_ReturnsOrigin()
    {
        var origin = HexCoord.Zero;
        var meta = new HexMetaData(Size: 1.0f);
        var originPixel = HMath.HexToPixel(origin, meta);

        var result = HMath.GetHexAtPixelDistance(origin, originPixel, 1, meta);

        Assert.Equal(origin, result);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void GetHexAtPixelDistance_MovesTowardTarget(bool isPointyTop)
    {
        var origin = HexCoord.Zero;
        var meta = new HexMetaData(Size: 1.0f, IsPointyTop: isPointyTop);
        var eastHex = new HexCoord(10, 0);
        var eastPixel = HMath.HexToPixel(eastHex, meta);

        var result = HMath.GetHexAtPixelDistance(origin, eastPixel, 1, meta);

        Assert.Equal(1, HMath.Distance(origin, result));
    }

    [Fact]
    public void GetHexAtPixelDistance_NegativeDistance_ReturnsOrigin()
    {
        var origin = new HexCoord(1, -1);
        var meta = new HexMetaData(Size: 1.0f);
        var targetPixel = new Vector2(100, 100);

        var result = HMath.GetHexAtPixelDistance(origin, targetPixel, -1, meta);

        Assert.Equal(origin, result);
    }

    [Fact]
    public void GetHexAtPixelDistance_ReturnsValidHex()
    {
        var origin = HexCoord.Zero;
        var meta = new HexMetaData(Size: 1.0f);
        var targetPixel = new Vector2(50, 25);

        var result = HMath.GetHexAtPixelDistance(origin, targetPixel, 2, meta);

        // HexCoord constraint is always valid by construction
        Assert.Equal(0, result.Q + result.R + result.S);
    }

    #endregion
}
