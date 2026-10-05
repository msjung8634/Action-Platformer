using System;
using UnityEngine;

public class PlayerResourceManager : MonoBehaviour, IHpHandler, ISpHandler
{
    [SerializeField] PlayerStat _statData;

    [field:SerializeField] public Resource HP { get; private set; }
    [field: SerializeField] public Resource SP { get; private set; }

    public event Action<int, int> OnHpChanged;
    public event Action<int, int> OnSpChanged;
    public event Action OnHit;
    public event Action OnDead;


    void Awake()
    {
        HP = new Resource(_statData.MaxHP);
        SP = new Resource(_statData.MaxSP);
    }

    void OnEnable()
    {
        HP.OnChanged += HpChanged;
        SP.OnChanged += SpChanged;
    }

    void OnDisable()
    {
        HP.OnChanged -= HpChanged;
        SP.OnChanged -= SpChanged;
    }

    #region HP

    public void IncreaseCurrentHP(int amount)
    {
        HP.TryIncreaseCurrent(amount);
    }
    public void DecreaseCurrentHP(int amount)
    {
        if (HP.TryDecreaseCurrent(amount))
        {
            if (HP.Current == 0)
            {
                OnDead?.Invoke();
            }
            else
            {
                OnHit?.Invoke();
            }
        }
    }
    public void IncreaseMaxHP(int amount)
    {
        HP.TryIncreaseMax(amount);
    }
    public void DecreaseMaxHP(int amount)
    {
        HP.TryDecreaseMax(amount);
    }

    void HpChanged(int current, int max)
    {
        OnHpChanged?.Invoke(current, max);
    }

    #endregion
    #region SP

    public void IncreaseCurrentSP(int amount)
    {
        SP.TryIncreaseCurrent(amount);
    }
    public void DecreaseCurrentSP(int amount)
    {
        SP.TryDecreaseCurrent(amount);
    }
    public void IncreaseMaxSP(int amount)
    {
        SP.TryIncreaseMax(amount);
    }
    public void DecreaseMaxSP(int amount)
    {
        SP.TryDecreaseMax(amount);
    }

    void SpChanged(int current, int max)
    {
        OnSpChanged?.Invoke(current, max);
    }

    #endregion
}
