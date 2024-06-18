/*

00:  -------- 
01: |        |
02: |        |
03: |        |
04:  -------- 
05: |        |
06: |        |
07: |        |
08:  -------- 

*/

public static class Display
{
    public static void DrawLineOfDigit(int digit, int line, Terminal terminal)
    {
        var segments = SegmentBits.GetSegmentsForDigit(digit);

        switch (line)
        {
            case 0:
                DrawHorizontalLine(terminal, segments, Segments.A);
                break;
            case 1:
            case 2:
            case 3:
                DrawVerticalLine(terminal, segments, (Segments.F, Segments.B));
                break;
            case 4:
                DrawHorizontalLine(terminal, segments, Segments.G);
                break;
            case 5:
            case 6:
            case 7:
                DrawVerticalLine(terminal, segments, (Segments.E, Segments.C));
                break;
            case 8:
                DrawHorizontalLine(terminal, segments, Segments.D);
                break;
        }
    }

    public static void DrawNumber(int number, Terminal terminal)
    {
        var digits = number.ToString();

        for (var j = 0; j < 9; j++)
        {
            for (var i = 0; i < digits.Length; i++)
            {
                var digit = int.Parse(digits[i].ToString());
                DrawLineOfDigit(digit, j, terminal);
                terminal.Write("   ");
            }
            
            terminal.WriteLine();
        }
    }

    private static void DrawHorizontalLine(Terminal terminal, Segments segments, Segments segment)
    {
        terminal.Write(" ");
        for (var i = 0; i < 8; i++)
        {
            if (SegmentBits.IsSet(segments, segment)) { terminal.Write("-"); }
            else { terminal.Write(" "); }
        }
        terminal.Write(" ");
    }

    private static void DrawVerticalLine(Terminal terminal, Segments segments, (Segments, Segments) segmentPair)
    {
        if (SegmentBits.IsSet(segments, segmentPair.Item1)) { terminal.Write("|"); }
        else { terminal.Write(" "); }
        for (var i = 0; i < 8; i++) { terminal.Write(" "); }
        if (SegmentBits.IsSet(segments, segmentPair.Item2)) { terminal.Write("|"); }
        else { terminal.Write(" "); }
    }

}