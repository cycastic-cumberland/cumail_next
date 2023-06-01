namespace CumailNEXT.Components.Paralex;

public class SafeFlag
{
    private bool _actualValue;
    public bool Flag
    {
        get => _actualValue;
        set => _actualValue = value;
    }
    public SafeFlag(bool startingValue = false)
    {
        _actualValue = startingValue;
    }
    public void WaitToFinish()
    {
        while (!Flag) { continue; }
    }
}