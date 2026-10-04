using Unity.VisualScripting;
using UnityEngine;

public class PlayerResourceManager : MonoBehaviour, IHpHandler, ISpHandler
{
    [SerializeField] PlayerStat _statData;

    [field:SerializeField] public Resource HP { get; private set; }
    [field: SerializeField] public Resource SP { get; private set; }

    void Awake()
    {
        HP = new Resource(_statData.MaxHP);
        SP = new Resource(_statData.MaxSP);
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
    #region SP

    public void IncreaseCurrentSP(int amount)
    {
        SP.TryIncreaseCurrent(amount);
    }
    public void DecreaseCurrentSP(int amount)
    {
        if (SP.TryDecreaseCurrent(amount))
        {
            // TODO : SP 0되면 탈진 (2초간 SP재생 중단)
        }
    }
    public void IncreaseMaxSP(int amount)
    {
        SP.TryIncreaseMax(amount);
    }
    public void DecreaseMaxSP(int amount)
    {
        SP.TryDecreaseMax(amount);
    }

    #endregion
}
