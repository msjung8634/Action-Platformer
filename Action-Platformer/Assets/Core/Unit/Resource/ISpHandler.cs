using System;

public interface ISpHandler
{
    public Resource SP { get; }
    public bool TryConsumeSP(int amount);
    public event Action<int, int> OnSpChanged;
    public event Action OnNotEnoughSp;
}
