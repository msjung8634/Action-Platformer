using UnityEngine;

public class EnemyResourceManager : MonoBehaviour, IHpHandler
{
    [SerializeField] EnemyStat _statData;

    [field: SerializeField] public Resource HP { get; private set; }

    void Awake()
    {
        HP = new Resource(_statData.MaxHP);
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
            // TODO : HP 0되면 사망
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

    #endregion

    // TODO : SP 0되면 탈진 (2초간 SP재생 중단)
}
