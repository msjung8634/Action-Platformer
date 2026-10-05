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
        if (StateMachine.Hit.CurrentState == Hit.State.NonHittable ||
            hitData.AttackData == null)
            return;

        _hpHandler?.DecreaseHp(hitData.AttackData.Damage);

        // Enemy인 경우
        if (_resourceHandler is EnemyResourceManager)
        {
            // Player(Attacker)의 IFpHanlder 탐색
            var fpHandler = hitData.Attacker.GetComponentInChildren<IFpHandler>();
            if (fpHandler != null)
            {
                fpHandler?.IncreaseFp(hitData.AttackData.FpGain);
            }
        }

        FeedbackManager.Instance.Apply(hitData.AttackData);
    }
}
