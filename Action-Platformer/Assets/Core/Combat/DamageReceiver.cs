using UnityEngine;

public class DamageReceiver : MonoBehaviour, IDamageable
{
    [field: SerializeField] public Transform AimPoint { get; private set; }
    [field: SerializeField] public UnitStateMachine StateMachine { get; private set; }
    [SerializeField] MonoBehaviour _hpHandlerObject;
    [SerializeField] MonoBehaviour _spHandlerObject;
    IHpHandler _hpHandler;
    ISpHandler _spHandler;

    void Awake()
    {
        _hpHandler = _hpHandlerObject as IHpHandler;
        _spHandler = _spHandlerObject as ISpHandler;
    }

    public void TakeDamage(HitData hitData)
    {
        _hpHandler?.DecreaseCurrentHP(hitData.AttackData.Damage);
        _spHandler?.DecreaseCurrentSP(hitData.AttackData.SpConsumption);
    }
}
