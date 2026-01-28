using System.Numerics;

namespace Hexmath.Tests;

public class ConversionTests
{
    [Theory]
    [InlineData(0.1f, 0.1f, -0.2f)]
    [InlineData(2.7f, -1.3f, -1.4f)]
    [InlineData(-0.5f, 0.5f, 0.0f)]
    public void RoundToHex_AlwaysMaintainsConstraint(float x, float y, float z)
    {
        var fractional = new Vector3(x, y, z);
        var rounded = HMath.RoundToHex(fractional);

        Assert.True(HMath.IsValidHexCoordinate(rounded));
    }

    [Fact]
    public void RoundToHex_IntegerInput_ReturnsSame()
    {
        var integer = new Vector3(2, -1, -1);
        var rounded = HMath.RoundToHex(integer);

        Assert.Equal(integer, rounded);
    }

    [Theory]
    [InlineData(false)] // FlatTop
    [InlineData(true)]  // PointyTop
    public void HexToPixel_Origin_ReturnsZero(bool isPointyTop)
    {
        var meta = new HexMetaData(Size: 1.0f, IsPointyTop: isPointyTop);
        var pixel = HMath.HexToPixel(Vector3.Zero, meta);

        Assert.Equal(0, pixel.X, precision: 3);
        Assert.Equal(0, pixel.Y, precision: 3);
    }

    [Theory]
    [InlineData(0, 0, 0, false)]     // Origin, FlatTop
    [InlineData(0, 0, 0, true)]      // Origin, PointyTop
    [InlineData(1, 0, -1, false)]    // FlatTop
    [InlineData(1, 0, -1, true)]     // PointyTop
    [InlineData(-2, 3, -1, false)]   // FlatTop
    [InlineData(-2, 3, -1, true)]    // PointyTop
    [InlineData(5, -3, -2, false)]   // FlatTop
    [InlineData(5, -3, -2, true)]    // PointyTop
    public void PixelToHex_RoundTrip(float x, float y, float z, bool isPointyTop)
    {
        var original = new Vector3(x, y, z);
        var meta = new HexMetaData(Size: 1.0f, IsPointyTop: isPointyTop);
        var pixel = HMath.HexToPixel(original, meta);
        var roundTrip = HMath.PixelToHex(pixel, meta);

        Assert.Equal(original.X, roundTrip.X, precision: 3);
        Assert.Equal(original.Y, roundTrip.Y, precision: 3);
        Assert.Equal(original.Z, roundTrip.Z, precision: 3);
    }

    [Fact]
    public void PixelToHex_ZeroSize_ReturnsZero()
    {
        var zeroSizeMeta = new HexMetaData(Size: 0);
        var result = HMath.PixelToHex(new Vector2(100, 100), zeroSizeMeta);

        Assert.Equal(Vector3.Zero, result);
    }

    [Fact]
    public void ToAxial_ExtractsQR()
    {
        var cube = new Vector3(2, -1, -1);
        var axial = HMath.ToAxial(cube);

        Assert.Equal(2, axial.X);
        Assert.Equal(-1, axial.Y);
    }

    [Fact]
    public void ToCube_CreatesValidHex()
    {
        var axial = new Vector2(2, -1);
        var cube = HMath.ToCube(axial);

        Assert.Equal(2, cube.X);
        Assert.Equal(-1, cube.Y);
        Assert.Equal(-1, cube.Z);
        Assert.True(HMath.IsValidHexCoordinate(cube));
    }

    [Fact]
    public void ToAxial_ToCube_RoundTrip()
    {
        var original = new Vector3(3, -2, -1);
        var axial = HMath.ToAxial(original);
        var cube = HMath.ToCube(axial);

        Assert.Equal(original, cube);
    }

    [Fact]
    public void GetHexAtDistance_ReturnsCorrectHex()
    {
        var origin = Vector3.Zero;
        var east = HMath.Directions[0];

        var result = HMath.GetHexAtDistance(origin, east, 3);

        Assert.Equal(new Vector3(3, 0, -3), result);
    }

    [Fact]
    public void GetHexAtDistance_ZeroDistance_ReturnsOrigin()
    {
        var origin = new Vector3(1, -1, 0);
        var east = HMath.Directions[0];

        var result = HMath.GetHexAtDistance(origin, east, 0);

        Assert.Equal(origin, result);
    }

    [Fact]
    public void GetHexAtDistance_InvalidDirection_ThrowsException()
    {
        var origin = Vector3.Zero;
        var invalid = new Vector3(2, 0, -2);

        Assert.Throws<ArgumentException>(() => HMath.GetHexAtDistance(origin, invalid, 1));
    }

    [Fact]
    public void GetHexAtPixelDistance_ZeroDistance_ReturnsOrigin()
    {
        var origin = new Vector3(2, -1, -1);
        var meta = new HexMetaData(Size: 1.0f);
        var targetPixel = new Vector2(100, 100);

        var result = HMath.GetHexAtPixelDistance(origin, targetPixel, 0, meta);

        Assert.Equal(origin, result);
    }

    [Fact]
    public void GetHexAtPixelDistance_TargetAtOrigin_ReturnsOrigin()
    {
        var origin = Vector3.Zero;
        var meta = new HexMetaData(Size: 1.0f);
        var originPixel = HMath.HexToPixel(origin, meta);

        var result = HMath.GetHexAtPixelDistance(origin, originPixel, 1, meta);

        Assert.Equal(origin, result);
    }

    [Fact]
    public void GetHexAtPixelDistance_ReturnsValidHex()
    {
        var origin = Vector3.Zero;
        var meta = new HexMetaData(Size: 1.0f);
        var targetPixel = new Vector2(50, 25);

        var result = HMath.GetHexAtPixelDistance(origin, targetPixel, 2, meta);

        Assert.True(HMath.IsValidHexCoordinate(result));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void GetHexAtPixelDistance_MovesTowardTarget(bool isPointyTop)
    {
        var origin = Vector3.Zero;
        var meta = new HexMetaData(Size: 1.0f, IsPointyTop: isPointyTop);
        var east = HMath.Directions[0];
        var eastPixel = HMath.HexToPixel(east * 10, meta);

        var result = HMath.GetHexAtPixelDistance(origin, eastPixel, 1, meta);

        Assert.Equal(1, HMath.Distance(origin, result));
    }

    [Fact]
    public void GetHexAtPixelDistance_NegativeDistance_ReturnsOrigin()
    {
        var origin = new Vector3(1, -1, 0);
        var meta = new HexMetaData(Size: 1.0f);
        var targetPixel = new Vector2(100, 100);

        var result = HMath.GetHexAtPixelDistance(origin, targetPixel, -1, meta);

        Assert.Equal(origin, result);
    }
}
