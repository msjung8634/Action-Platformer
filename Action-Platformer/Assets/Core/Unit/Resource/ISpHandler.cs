using System;

public interface ISpHandler
{
    public Resource SP { get; }
    bool TryConsumeSP(int amount);
    public event Action<int, int> OnSpChanged;
}
