using System;
using UnityEngine;

[Serializable]
public class Resource
{
    [field: SerializeField] public int Max { get; private set; }
    [field: SerializeField] public int Current { get; private set; }

    // <Current, Max>
    public event Action<int, int> OnChanged;

    public Resource(int startVal, int maxVal)
    {
        Max = maxVal;
        Current = startVal;
    }

    public bool TryIncreaseCurrent(int val)
    {
        if (val < 0)
            return false;

        if (val == 0)
            return true;

        Current = Mathf.Min(Current + val, Max);
        OnChanged?.Invoke(Current, Max);
        return true;
    }

    public bool TryDecreaseCurrent(int val)
    {
        if (val < 0)
            return false;

        if (val == 0)
            return true;

        Current = Mathf.Max(Current - val, 0);
        OnChanged?.Invoke(Current, Max);

        return true;
    }

    public bool TryIncreaseMax(int val)
    {
        if (val < 0)
            return false;

        if (val == 0)
            return true;

        Max += val;
        OnChanged?.Invoke(Current, Max);
        return true;
    }

    public bool TryDecreaseMax(int val)
    {
        if (val < 0)
            return false;

        if (val == 0)
            return true;

        Max -= val;
        OnChanged?.Invoke(Current, Max);
        return true;
    }

    // Current가 충분할 때만 차감
    public bool TryConsume(int amount)
    {
        if (amount < 0 || Current < amount)
            return false;

        return TryDecreaseCurrent(amount);
    }
}
