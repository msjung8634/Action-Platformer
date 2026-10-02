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

    public void TryIncreaseCurrent(int val, out bool isSuccess)
    {
        if (val <= 0)
        {
            isSuccess = false;
            return;
        }

        isSuccess = true;
        Current = Mathf.Min(Current + val, Max);
        OnChanged?.Invoke(Current, Max);
    }

    public void TryDecreaseCurrent(int val, out bool isSuccess)
    {
        if (val <= 0)
        {
            isSuccess = false;
            return;
        }

        isSuccess = true;
        Current = Mathf.Max(Current - val, 0);
        OnChanged?.Invoke(Current, Max);
    }

    public void TryIncreaseMax(int val, out bool isSuccess)
    {
        if (val <= 0)
        {
            isSuccess = false;
            return;
        }

        isSuccess = true;
        Max += val;
        OnChanged?.Invoke(Current, Max);
    }

    public void TryDecreaseMax(int val, out bool isSuccess)
    {
        if (val <= 0)
        {
            isSuccess = false;
            return;
        }

        isSuccess = true;
        Max -= val;
        OnChanged?.Invoke(Current, Max);
    }
}
