namespace Hexmath.Tests;

public class AreaTests
{
    private static readonly HexCoord Origin = HexCoord.Zero;

    #region GetRing

    [Theory]
    [InlineData(1, 6)]
    [InlineData(2, 12)]
    [InlineData(3, 18)]
    [InlineData(5, 30)]
    public void GetRing_ReturnsCorrectCount(int radius, int expectedCount)
    {
        var ring = HMath.GetRing(Origin, radius).ToList();

        Assert.Equal(expectedCount, ring.Count);
    }

    [Fact]
    public void GetRing_ZeroRadius_ReturnsEmpty()
    {
        var ring = HMath.GetRing(Origin, 0).ToList();

        Assert.Empty(ring);
    }

    [Fact]
    public void GetRing_NegativeRadius_ReturnsEmpty()
    {
        var ring = HMath.GetRing(Origin, -1).ToList();

        Assert.Empty(ring);
    }

    [Fact]
    public void GetRing_AllHexesAtCorrectDistance()
    {
        int radius = 3;
        var ring = HMath.GetRing(Origin, radius);

        foreach (var hex in ring)
        {
            Assert.Equal(radius, HMath.Distance(Origin, hex));
        }
    }

    [Fact]
    public void GetRing_AllHexesSatisfyConstraint()
    {
        var ring = HMath.GetRing(Origin, 2);

        foreach (var hex in ring)
        {
            Assert.Equal(0, hex.Q + hex.R + hex.S);
        }
    }

    [Fact]
    public void GetRing_NoDuplicates()
    {
        var ring = HMath.GetRing(Origin, 3).ToList();
        var distinct = ring.Distinct().ToList();

        Assert.Equal(ring.Count, distinct.Count);
    }

    [Fact]
    public void GetRing_FromNonOrigin_WorksCorrectly()
    {
        var center = new HexCoord(3, -2);
        int radius = 2;
        var ring = HMath.GetRing(center, radius).ToList();

        Assert.Equal(12, ring.Count);
        foreach (var hex in ring)
        {
            Assert.Equal(radius, HMath.Distance(center, hex));
        }
    }

    #endregion

    #region GetSpiral

    [Fact]
    public void GetSpiral_IncludesOrigin_WhenRequested()
    {
        var spiral = HMath.GetSpiral(Origin, 2, includeOrigin: true).ToList();

        Assert.Contains(Origin, spiral);
    }

    [Fact]
    public void GetSpiral_ExcludesOrigin_WhenRequested()
    {
        var spiral = HMath.GetSpiral(Origin, 2, includeOrigin: false).ToList();

        Assert.DoesNotContain(Origin, spiral);
    }

    [Theory]
    [InlineData(0, true, 1)]
    [InlineData(1, true, 7)]
    [InlineData(2, true, 19)]
    [InlineData(3, true, 37)]
    public void GetSpiral_ReturnsCorrectCount(int maxRadius, bool includeOrigin, int expectedCount)
    {
        var spiral = HMath.GetSpiral(Origin, maxRadius, includeOrigin).ToList();

        Assert.Equal(expectedCount, spiral.Count);
    }

    [Theory]
    [InlineData(1, false, 6)]
    [InlineData(2, false, 18)]
    [InlineData(3, false, 36)]
    public void GetSpiral_WithoutOrigin_ReturnsCorrectCount(int maxRadius, bool includeOrigin, int expectedCount)
    {
        var spiral = HMath.GetSpiral(Origin, maxRadius, includeOrigin).ToList();

        Assert.Equal(expectedCount, spiral.Count);
    }

    [Fact]
    public void GetSpiral_NegativeRadius_ReturnsEmpty()
    {
        var spiral = HMath.GetSpiral(Origin, -1).ToList();

        Assert.Empty(spiral);
    }

    [Fact]
    public void GetSpiral_AllHexesSatisfyConstraint()
    {
        var spiral = HMath.GetSpiral(Origin, 3);

        foreach (var hex in spiral)
        {
            Assert.Equal(0, hex.Q + hex.R + hex.S);
        }
    }

    [Fact]
    public void GetSpiral_NoDuplicates()
    {
        var spiral = HMath.GetSpiral(Origin, 3).ToList();
        var distinct = spiral.Distinct().ToList();

        Assert.Equal(spiral.Count, distinct.Count);
    }

    [Fact]
    public void GetSpiral_FromNonOrigin_WorksCorrectly()
    {
        var center = new HexCoord(5, -3);
        var spiral = HMath.GetSpiral(center, 1, includeOrigin: true).ToList();

        Assert.Equal(7, spiral.Count);
        Assert.Contains(center, spiral);

        foreach (var hex in spiral.Where(h => h != center))
        {
            Assert.Equal(1, HMath.Distance(center, hex));
        }
    }

    [Fact]
    public void GetSpiral_AllHexesWithinMaxRadius()
    {
        int maxRadius = 4;
        var spiral = HMath.GetSpiral(Origin, maxRadius).ToList();

        foreach (var hex in spiral)
        {
            Assert.True(HMath.Distance(Origin, hex) <= maxRadius);
        }
    }

    #endregion
}
