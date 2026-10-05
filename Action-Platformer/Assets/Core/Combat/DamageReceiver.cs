using UnityEngine;

public class DamageReceiver : MonoBehaviour, IDamageable
{
    [field: SerializeField] public Transform AimPoint { get; private set; }
    [field: SerializeField] public UnitStateMachine StateMachine { get; private set; }
    [SerializeField] MonoBehaviour _resourceHandler;
    IHpHandler _hpHandler;

    void Awake()
    {
        _hpHandler = _resourceHandler as IHpHandler;
    }

    public void TakeDamage(HitData hitData)
    {
        if (StateMachine.Hit.CurrentState == Hit.State.NonHittable)
            return;

        _hpHandler?.DecreaseHp(hitData.AttackData.Damage);
        // TODO : 화염 속성치 부여
        //Debug.Log($"{hitData}");

        if (hitData.AttackData == null)
            return;

        FeedbackManager.Instance.Apply(hitData.AttackData);
    }
}
