using System;

public interface IHpHandler
{
    Resource HP { get; }
    public void DecreaseHp(int amount);
    public event Action<int, int> OnHpChanged;
    public event Action OnHit;
    public event Action OnDead;
}
