using System;
using UnityEngine;

public class EnemyResourceManager : MonoBehaviour, IHpHandler
{
    [SerializeField] EnemyStat _statData;

    [field: SerializeField] public Resource HP { get; private set; }

    public event Action<int, int> OnHpChanged;
    public event Action OnHit;
    public event Action OnDead;

    void Awake()
    {
        HP = new Resource(_statData.MaxHP);
    }

    void OnEnable()
    {
        HP.OnChanged += HpChanged;
    }

    void OnDisable()
    {
        HP.OnChanged -= HpChanged;
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
}
