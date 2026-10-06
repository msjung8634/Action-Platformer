using UnityEngine;

public class NecromancerVisual : EnemyVisualBase
{
    static readonly int _moveSpeedHash = Animator.StringToHash("moveSpeed");
    public void SetMoveSpeed(float value)
    {
        _animator.SetFloat(_moveSpeedHash, value);
    }

    static readonly int _magicMissileHash = Animator.StringToHash("magicMissile");
    public void PlayMagicMissile()
    {
        _animator.SetTrigger(_magicMissileHash);
    }
}
