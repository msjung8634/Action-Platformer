using System;

public interface IFpHandler
{
    public Resource FP { get; }
    public void IncreaseFp(int amount);
    public event Action<int, int> OnFpChanged;
    public event Action OnNotEnoughFp;
}
