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
        if (StateMachine.Hit.CurrentState == Hit.State.NonHittable)
            return;

        _hpHandler?.TryConsumeHP(hitData.AttackData.Damage);
        _spHandler?.TryConsumeSP(hitData.AttackData.SpConsumption);
        Debug.Log($"{hitData}");

        if (hitData.AttackData == null)
            return;

        FeedbackManager.Instance.Apply(hitData.AttackData);
    }
}
