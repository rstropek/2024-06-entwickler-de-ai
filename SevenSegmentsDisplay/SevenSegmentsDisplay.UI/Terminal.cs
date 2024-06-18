// Class that encapsulates all IO operations for the terminal.
// This class will be used for testing purposes.

public class Terminal
{
    // Clear Screen
    public void ClearScreen()
    {
        Console.Clear();
    }

    // Write without newline
    public void Write(string text)
    {
        Console.Write(text);
    }

    // Write newline
    public void WriteLine()
    {
        Console.WriteLine();
    }
}
