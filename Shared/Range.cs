namespace CocoaAni.Files.M3U8.Shared;

public class Range
{
    public Range(int start, int count)
    {
        Start = start;
        Count = count;
    }

    public int Count { get; set; }
    public int Start { get; set; }
}