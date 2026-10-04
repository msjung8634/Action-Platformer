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

    bool _isFlying;
    
    Action<ProjectileController, Collider2D> _onHit;
    Rigidbody2D _rigidbody;

    void Awake()
    {
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
        Action<ProjectileController, Collider2D> onHit = null)
    {
        _isFlying = true;

        SetDirection(targetPosition);
        Caster = caster;
        Owner = caster;

        TargetLayerMask = targetLayerMask;
        FlySpeed = Mathf.Max(0f, speed);
        _onHit = onHit;
    }

    public bool Reflect(Transform reflector, LayerMask reflectedTargetLayerMask, float speedMultiplier = 1f,
        Action<ProjectileController, Collider2D> onHit = null)
    {
        if (!_isFlying || Caster == null || reflector == null)
            return false;

        _reflectCount++;

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
        FlyDirection = (targetPosition - (Vector2)transform.position).normalized;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!_isFlying)
            return;

        int colliderlayer = 1 << other.gameObject.layer;

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
        _onHit?.Invoke(this, other);
        Despawn();

        // TODO : 패링 시 어떻게할지...
    }

    public void Despawn()
    {
        // 파괴 전 추가 이동 및 충돌 방지
        _isFlying = false;
        _onHit = null;

        Destroy(gameObject);
    }
}
