using System;
using UnityEngine;

public class ProjectileController : MonoBehaviour
{
    [Header("Collision")]
    [SerializeField] LayerMask _blockLayerMask;
    public LayerMask TargetLayerMask { get; private set; }

    int _reflectCount;
    public Transform Caster { get; private set; }
    public Transform Owner { get; private set; }

    public float FlySpeed { get; private set; }
    public Vector2 FlyDirection { get; private set; }
    public bool IgnoreYOffset { get; private set; }

    bool _isFlying;
    Action<ProjectileController, Collider2D> _onHit;

    ProjectileVisual _visual;
    Rigidbody2D _rigidbody;

    void Awake()
    {
        TryGetComponent(out _visual);
        TryGetComponent(out _rigidbody);
    }

    void FixedUpdate()
    {
        if (!_isFlying)
            return;

        Vector2 nextPosition = _rigidbody.position + FlyDirection * FlySpeed * Time.fixedDeltaTime;
        _rigidbody.MovePosition(nextPosition);
    }

    public void Initialize(Transform caster, Vector2 targetPosition, LayerMask targetLayerMask, float speed,
        bool ignoreYOffset = true, Action<ProjectileController, Collider2D> onHit = null)
    {
        _isFlying = true;

        IgnoreYOffset = ignoreYOffset;
        SetDirection(targetPosition);
        Caster = caster;
        Owner = caster;

        TargetLayerMask = targetLayerMask;
        FlySpeed = Mathf.Max(0f, speed);

        _onHit = onHit;
    }

    public bool Reflect(Transform reflector, LayerMask reflectedTargetLayerMask, float speedMultiplier = 1f,
        bool ignoreYOffset = true, Action<ProjectileController, Collider2D> onHit = null)
    {
        if (!_isFlying || Caster == null || reflector == null)
            return false;

        _reflectCount++;

        IgnoreYOffset = ignoreYOffset;
        SetDirection(Caster.position);
        Caster = reflector;
        Owner = reflector;

        TargetLayerMask = reflectedTargetLayerMask;
        FlySpeed *= speedMultiplier;

        _onHit = onHit;

        return true;
    }

    void SetDirection(Vector2 targetPosition)
    {
        Vector2 dirToTarget = targetPosition - (Vector2)transform.position;
        if (IgnoreYOffset)
        {
            dirToTarget.y = 0f;
        }
        FlyDirection = dirToTarget.normalized;

        float angle = Mathf.Atan2(FlyDirection.y, FlyDirection.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, angle);
    }

    void OnTriggerEnter2D(Collider2D collider)
    {
        if (!_isFlying)
            return;

        int colliderlayer = 1 << collider.gameObject.layer;

        // blockLayer 닿으면 소멸
        if ((_blockLayerMask.value & colliderlayer) != 0)
        {
            Despawn();
            return;
        }

        // targetLayer 아니면 무시
        if ((TargetLayerMask.value & colliderlayer) == 0)
            return;

        // 명중 시
        if (collider.TryGetComponent(out DamageReceiver damageReceiver))
        {
            switch (damageReceiver.StateMachine.Hit.CurrentState)
            {
                // target이 피격 가능상태
                case Hit.State.Hittable:
                    _onHit?.Invoke(this, collider);
                    Disable();
                    _visual.PlayDisappear();
                    break;
                // target이 피격 불가상태
                case Hit.State.NonHittable:
                    break;
            }
        }
    }

    void Despawn()
    {
        Disable();
        Destroy(gameObject);
    }
    void Disable()
    {
        // 파괴 전 추가 이동 및 충돌 방지
        _isFlying = false;
        _onHit = null;
    }

    void AnimEvent_DisappearEnd()
    {
        Destroy(gameObject);
    }
}
