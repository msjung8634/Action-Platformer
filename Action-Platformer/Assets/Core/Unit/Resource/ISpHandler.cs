using System;

public interface ISpHandler
{
    public Resource SP { get; }
    void IncreaseCurrentSP(int amount);
    void DecreaseCurrentSP(int amount);
    void IncreaseMaxSP(int amount);
    void DecreaseMaxSP(int amount);
    public event Action<int, int> OnSpChanged;
}
