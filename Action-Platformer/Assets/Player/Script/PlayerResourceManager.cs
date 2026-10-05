using System;
using UnityEngine;

public class PlayerResourceManager : MonoBehaviour, IHpHandler, ISpHandler
{
    [SerializeField] PlayerStat _statData;
    [field:SerializeField] public Resource HP { get; private set; }
    [field: SerializeField] public Resource SP { get; private set; }

    [Header("SP Regeneration")]
    [SerializeField, Min(0f)] int _spRegenPerSecond = 20;
    [SerializeField, Min(0f)] float _spRegenDelay = 0.8f;
    float _spRegenElapsed;
    float _spRegenStartTime;

    public event Action<int, int> OnHpChanged;
    public event Action<int, int> OnSpChanged;

    public event Action OnHit;
    public event Action OnDead;

    void Awake()
    {
        HP = new Resource(_statData.MaxHP, _statData.MaxHP);
        SP = new Resource(0, _statData.MaxSP);
    }

    void OnEnable()
    {
        WaveManager.Instance.OnRestartGame += OnRestartGame;
        HP.OnChanged += HpChanged;
        SP.OnChanged += SpChanged;
    }

    void OnDisable()
    {
        WaveManager.Instance.OnRestartGame -= OnRestartGame;
        HP.OnChanged -= HpChanged;
        SP.OnChanged -= SpChanged;
    }

    void OnRestartGame()
    {
        HP = new Resource(_statData.MaxHP, _statData.MaxHP);
        SP = new Resource(0, _statData.MaxSP);
        HP.OnChanged += HpChanged;
        SP.OnChanged += SpChanged;
        HpChanged(HP.Current, HP.Max);
        SpChanged(SP.Current, SP.Max);

        _spRegenElapsed = 0f;
        _spRegenStartTime = Time.time;
    }

    void Update()
    {
        RegenerateSP();
    }

    #region HP

    public bool TryConsumeHP(int amount)
    {
        if (!HP.TryDecreaseCurrent(amount))
            return false;

        if (HP.Current == 0)
        {
            OnDead?.Invoke();
        }
        else
        {
            OnHit?.Invoke();
        }

        return true;
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
        if (!SP.TryDecreaseCurrent(amount))
            return false;

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

        // 회복에 사용하고 남은시간은 보존
        _spRegenElapsed -= regenAmount * secondsPerPoint;
        regenAmount = Mathf.Min(regenAmount, SP.Max - SP.Current);
        SP.TryIncreaseCurrent(regenAmount);
    }

    #endregion
}
