using System;
using UnityEngine;

[Serializable]
public class Resource
{
    [field: SerializeField] public int Max { get; private set; }
    [field: SerializeField] public int Current { get; private set; }

    // <Current, Max>
    public event Action<int, int> OnChanged;

    public Resource(int maxVal)
    {
        Max = maxVal;
        Current = maxVal;
    }

    public bool TryIncreaseCurrent(int val)
    {
        if (val <= 0)
            return false;

        Current = Mathf.Min(Current + val, Max);
        OnChanged?.Invoke(Current, Max);
        return true;
    }

    public bool TryDecreaseCurrent(int val)
    {
        if (val <= 0)
            return false;

        Current = Mathf.Max(Current - val, 0);
        OnChanged?.Invoke(Current, Max);
        return true;
    }

    public bool TryIncreaseMax(int val)
    {
        if (val <= 0)
            return false;

        Max += val;
        OnChanged?.Invoke(Current, Max);
        return true;
    }

    public bool TryDecreaseMax(int val)
    {
        if (val <= 0)
            return false;

        Max -= val;
        OnChanged?.Invoke(Current, Max);
        return true;
    }
}
