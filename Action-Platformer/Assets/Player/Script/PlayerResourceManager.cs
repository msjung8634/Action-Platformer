using System;
using UnityEngine;

public class PlayerResourceManager : MonoBehaviour, IHpHandler, ISpHandler, IFpHandler
{
    [SerializeField] PlayerStat _statData;
    [field:SerializeField] public Resource HP { get; private set; }
    [field: SerializeField] public Resource SP { get; private set; }
    [field: SerializeField] public Resource FP { get; private set; }

    [Header("SP Regeneration")]
    [SerializeField, Min(0f)] int _spRegenPerSecond = 20;
    [SerializeField, Min(0f)] float _spRegenDelay = 0.8f;
    float _spRegenElapsed;
    float _spRegenStartTime;

    public event Action<int, int> OnHpChanged;
    public event Action OnHit;
    public event Action OnDead;
    public event Action<int, int> OnSpChanged;
    public event Action OnNotEnoughSp;
    public event Action<int, int> OnFpChanged;
    public event Action OnNotEnoughFp;

    void Awake()
    {
        HP = new Resource(_statData.MaxHP, _statData.MaxHP);
        SP = new Resource(_statData.MaxSP, _statData.MaxSP);
        FP = new Resource(0, _statData.MaxFP);
    }

    void OnEnable()
    {
        HP.OnChanged += HpChanged;
        SP.OnChanged += SpChanged;
        FP.OnChanged += FpChanged;

        if (WaveManager.Instance == null) return;
        WaveManager.Instance.OnRestartGame += OnRestartGame;
    }

    void OnDisable()
    {
        HP.OnChanged -= HpChanged;
        SP.OnChanged -= SpChanged;
        FP.OnChanged -= FpChanged;

        if (WaveManager.Instance == null) return;
        WaveManager.Instance.OnRestartGame -= OnRestartGame;
    }

    void OnRestartGame()
    {
        HP = new Resource(_statData.MaxHP, _statData.MaxHP);
        SP = new Resource(_statData.MaxSP, _statData.MaxSP);
        FP = new Resource(0, _statData.MaxFP);
        HP.OnChanged += HpChanged;
        SP.OnChanged += SpChanged;
        FP.OnChanged += FpChanged;
        HpChanged(HP.Current, HP.Max);
        SpChanged(SP.Current, SP.Max);
        FpChanged(FP.Current, FP.Max);

        _spRegenElapsed = 0f;
        _spRegenStartTime = Time.time;
    }

    void Update()
    {
        RegenerateSP();
    }

    #region HP

    public void DecreaseHp(int amount)
    {
        HP.TryDecreaseCurrent(amount);
        if (HP.Current == 0)
        {
            OnDead?.Invoke();
        }
        else
        {
            OnHit?.Invoke();
        }
    }

    void HpChanged(int current, int max)
    {
        OnHpChanged?.Invoke(current, max);
    }

    #endregion
    #region SP

    void SpChanged(int current, int max)
    {
        OnSpChanged?.Invoke(current, max);
    }

    public bool TryConsumeSP(int amount)
    {
        if (!SP.TryConsume(amount))
        {
            return false;
        }

        // SP 소모 시, 재생 초기화
        _spRegenStartTime = Time.time + _spRegenDelay;
        _spRegenElapsed = 0f;

        return true;
    }

    void RegenerateSP()
    {
        // 재생 불가
        if (HP.Current <= 0
            || SP.Current >= SP.Max
            || _spRegenPerSecond <= 0)
        {
            _spRegenElapsed = 0f;
            return;
        }

        if (Time.time < _spRegenStartTime)
            return;

        _spRegenElapsed += Time.deltaTime;

        float secondsPerPoint = 1f / _spRegenPerSecond;
        int regenAmount = Mathf.FloorToInt(_spRegenElapsed / secondsPerPoint);
        if (regenAmount <= 0)
            return;

        regenAmount = Mathf.Min(regenAmount, SP.Max - SP.Current);
        if (SP.TryIncreaseCurrent(regenAmount))
        {
            _spRegenElapsed -= regenAmount * secondsPerPoint;
        }
    }

    #endregion
    #region FP

    public void IncreaseFp(int amount)
    {
        FP.TryIncreaseCurrent(amount);
        // TODO : 필요 시 화염게이지 단계에 따른 이벤트 추가
    }

    void FpChanged(int current, int max)
    {
        OnFpChanged?.Invoke(current, max);
    }

    public bool TryConsumeFP(int amount)
    {
        if (!FP.TryConsume(amount))
        {
            return false;
        }

        // TODO : FP 자동감소 초기화
        return true;
    }

    #endregion

    public bool TryConsumeResource(AttackData attackData)
    {
        if (attackData == null)
            return false;

        int spCost = attackData.SpConsumption;
        int fpCost = attackData.FpConsumption;

        if (spCost < 0 || fpCost < 0)
            return false;

        // 확인
        bool notEnoughSP = SP.Current < spCost;
        bool notEnoughFP = FP.Current < fpCost;
        if (notEnoughSP)
        {
            OnNotEnoughSp?.Invoke();
        }
        if (notEnoughFP)
        {
            OnNotEnoughFp?.Invoke();
        }

        if (notEnoughSP || notEnoughFP)
            return false;

        // 소비
        if (spCost > 0)
            TryConsumeSP(spCost);

        if (fpCost > 0)
            TryConsumeFP(fpCost);

        return true;
    }

    public bool TryConsumeResource(MoveData moveData)
    {
        if (moveData == null)
            return false;

        int spCost = moveData.SpConsumption;

        if (spCost < 0)
            return false;

        // 확인
        bool notEnoughSP = SP.Current < spCost;
        if (notEnoughSP)
        {
            OnNotEnoughSp?.Invoke();
        }

        if (notEnoughSP)
            return false;

        // 소비
        if (spCost > 0)
            TryConsumeSP(spCost);

        return true;
    }
}
