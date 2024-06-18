// Implement the logic for controlling a seven-segment display

[Flags]
public enum Segments
{
    A = 0b00000001,
    B = 0b00000010,
    C = 0b00000100,
    D = 0b00001000,
    E = 0b00010000,
    F = 0b00100000,
    G = 0b01000000,
}

public static class SegmentBits
{
    /// <summary>
    /// Check if the given combination of segments is valid
    /// </summary>
    /// <param name="s">The segments to check (e.g. `Segments.A`, `Segments.A | Segments.C`)</param>
    /// <returns>True if the combination is valid, false otherwise</returns>
    public static bool IsValidSegment(Segments s)
    {
        Segments allSegments = Segments.A | Segments.B | Segments.C | Segments.D | Segments.E | Segments.F | Segments.G;
        return s >= Segments.A && s <= allSegments;
    }

    /// <summary>
    /// Throws an exception if the given combination of segments is invalid
    /// </summary>
    /// <param name="s">The segments to check (e.g. `Segments.A`, `Segments.A | Segments.C`)</param>
    /// <exception cref="ArgumentException">Thrown if the combination is invalid</exception>
    public static void ThrowIfInvalidSegment(Segments s)
    {
        if (!IsValidSegment(s))
        {
            throw new ArgumentException("Invalid segment combination");
        }
    }

    public static Segments SetSegment(Segments segments, Segments segment)
    {
        ThrowIfInvalidSegment(segment);
        ThrowIfInvalidSegment(segments);

        return segments | segment;
    }

    public static Segments ClearSegment(Segments segments, Segments segment)
    {
        ThrowIfInvalidSegment(segment);
        ThrowIfInvalidSegment(segments);

        return segments & ~segment;
    }

    public static bool IsSet(Segments segments, Segments segmentToCheck)
    {
        ThrowIfInvalidSegment(segmentToCheck);
        ThrowIfInvalidSegment(segments);

        return (segments & segmentToCheck) == segmentToCheck;
    }

    public static Segments GetSegmentsForDigit(int digit)
    {
        return digit switch
        {
            0 => Segments.A | Segments.B | Segments.C | Segments.D | Segments.E | Segments.F,
            1 => Segments.B | Segments.C,
            2 => Segments.A | Segments.B | Segments.G | Segments.E | Segments.D,
            3 => Segments.A | Segments.B | Segments.C | Segments.D | Segments.G,
            4 => Segments.F | Segments.B | Segments.C | Segments.G,
            5 => Segments.A | Segments.F | Segments.G | Segments.C | Segments.D,
            6 => Segments.A | Segments.F | Segments.G | Segments.C | Segments.D | Segments.E,
            7 => Segments.A | Segments.B | Segments.C,
            8 => Segments.A | Segments.B | Segments.C | Segments.D | Segments.E | Segments.F | Segments.G,
            9 => Segments.A | Segments.B | Segments.C | Segments.D | Segments.F | Segments.G,
            _ => throw new ArgumentException("Invalid digit"),
        };
    }
}