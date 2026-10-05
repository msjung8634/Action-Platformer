using System;

public interface IHpHandler
{
    Resource HP { get; }
    bool TryConsumeHP(int amount);
    public event Action<int, int> OnHpChanged;
}
