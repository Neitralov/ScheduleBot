namespace ScheduleBot.Structs;

/// <summary>Представляет промежуток времени, указанный в часах от 0 до 23.</summary>
/// <exception cref="ArgumentException">Start не может быть больше end.</exception>
/// <exception cref="ArgumentException">Start не может быть большее 23.</exception>
/// <exception cref="ArgumentException">End не может быть большее 23.</exception>
public readonly struct HoursRange
{
    private readonly uint _start;
    private readonly uint _end;
    
    public HoursRange(uint start, uint end)
    {
        _start = start <= 23
            ? start
            : throw new ArgumentException("Параметр start не может быть большее 23");

        _end = end <= 23 
            ? end
            : throw new ArgumentException("Параметр end не может быть большее 23");
        
        if (start > end)
            throw new ArgumentException("Параметр start не может быть больше параметра end");
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