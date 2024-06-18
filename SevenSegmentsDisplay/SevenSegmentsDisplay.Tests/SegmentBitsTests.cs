public class SegmentBitsTests
{
    [Theory]
    [InlineData(Segments.A)]
    [InlineData(Segments.B)]
    [InlineData(Segments.C)]
    [InlineData(Segments.D)]
    [InlineData(Segments.E)]
    [InlineData(Segments.F)]
    [InlineData(Segments.G)]
    [InlineData(Segments.A | Segments.B)]
    [InlineData(Segments.A | Segments.B | Segments.C)]
    public void IsValidSegment_ValidSegments_ReturnsTrue(Segments s)
    {
        // Act
        bool result = SegmentBits.IsValidSegment(s);

        // Assert
        Assert.True(result);
    }

    [Theory]
    [InlineData((Segments)0b10000000)] // Invalid segment
    [InlineData((Segments)0b11111111)] // Invalid segment
    public void IsValidSegment_InvalidSegments_ReturnsFalse(Segments s)
    {
        // Act
        bool result = SegmentBits.IsValidSegment(s);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void ThrowIfInvalidSegment_ValidSegment_DoesNotThrow()
    {
        SegmentBits.ThrowIfInvalidSegment(Segments.A);
    }

    [Fact]
    public void ThrowIfInvalidSegment_InvalidSegment_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => SegmentBits.ThrowIfInvalidSegment((Segments)128));
    }

    [Fact]
    public void SetSegment_ValidSegments_ReturnsCorrectResult()
    {
        var result = SegmentBits.SetSegment(Segments.A, Segments.B);
        Assert.Equal(Segments.A | Segments.B, result);
    }

    [Fact]
    public void ClearSegment_ValidSegments_ReturnsCorrectResult()
    {
        var result = SegmentBits.ClearSegment(Segments.A | Segments.B, Segments.B);
        Assert.Equal(Segments.A, result);
    }

    [Fact]
    public void IsSet_SegmentIsSet_ReturnsTrue()
    {
        var result = SegmentBits.IsSet(Segments.A | Segments.B, Segments.B);
        Assert.True(result);
    }

    [Fact]
    public void IsSet_SegmentIsNotSet_ReturnsFalse()
    {
        var result = SegmentBits.IsSet(Segments.A, Segments.B);
        Assert.False(result);
    }

    [Theory]
    [InlineData(0, Segments.A | Segments.B | Segments.C | Segments.D | Segments.E | Segments.F)]
    [InlineData(1, Segments.B | Segments.C)]
    [InlineData(2, Segments.A | Segments.B | Segments.G | Segments.E | Segments.D)]
    public void GetSegmentsForDigit_ValidDigit_ReturnsCorrectSegments(int digit, Segments expected)
    {
        var result = SegmentBits.GetSegmentsForDigit(digit);
        Assert.Equal(expected, result);
    }

    [Fact]
    public void GetSegmentsForDigit_InvalidDigit_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => SegmentBits.GetSegmentsForDigit(10));
    }

    [Fact]
    public void SetSegment_InvalidSegment_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => SegmentBits.SetSegment(Segments.A, (Segments)128));
        Assert.Throws<ArgumentException>(() => SegmentBits.SetSegment((Segments)128, Segments.A));
    }

    [Fact]
    public void ClearSegment_InvalidSegment_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => SegmentBits.ClearSegment(Segments.A, (Segments)128));
        Assert.Throws<ArgumentException>(() => SegmentBits.ClearSegment((Segments)128, Segments.A));
    }

    [Fact]
    public void IsSet_InvalidSegment_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => SegmentBits.IsSet(Segments.A, (Segments)128));
        Assert.Throws<ArgumentException>(() => SegmentBits.IsSet((Segments)128, Segments.A));
    }
}