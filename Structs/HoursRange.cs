namespace ScheduleBot.Structs;

/// <summary>Представляет промежуток времени, указанный в часах.</summary>
public readonly struct HoursRange
{
    private readonly int _start;
    private readonly int _end;
    
    public HoursRange(int start, int end)
    {
        _start = start;
        _end = end;
    }

    private bool Equals(HoursRange other)
    {
        return _start == other._start && _end == other._end;
    }

    public override bool Equals(object? obj)
    {
        return obj is HoursRange other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(_start, _end);
    }

    public static bool operator ==(HoursRange a, int b) => (b >= a._start) && (b <= a._end);
    public static bool operator !=(HoursRange a, int b) => !(b >= a._start) || !(b <= a._end);

    public static bool operator ==(int b, HoursRange a) => (b >= a._start) && (b <= a._end);
    public static bool operator !=(int b, HoursRange a) => !(b >= a._start) || !(b <= a._end);
}