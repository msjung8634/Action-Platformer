using System;

public interface IHpHandler
{
    Resource HP { get; }
    void IncreaseCurrentHP(int amount);
    void DecreaseCurrentHP(int amount);
    void IncreaseMaxHP(int amount);
    void DecreaseMaxHP(int amount);
    public event Action<int, int> OnHpChanged;
}
