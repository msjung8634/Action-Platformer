using UnityEngine;

public class PlayerVisual : MonoBehaviour
{
    [SerializeField] SpriteRenderer _renderer;
    [SerializeField] Animator _animator;

    public void SetFacingDirection(PlayerInput.MoveDirection direction)
    {
        switch (direction)
        {
            case PlayerInput.MoveDirection.Right:
                _renderer.flipX = false;
                break;
            case PlayerInput.MoveDirection.Left:
                _renderer.flipX = true;
                break;
            default:
                break;
        }
    }

    static readonly int _moveSpeedHash = Animator.StringToHash("moveSpeed");
    public void SetMoveSpeed(float speed)
    {
        _animator.SetFloat(_moveSpeedHash, speed);
    }

    static readonly int _groundAattackHash = Animator.StringToHash("groundAttack");
    static readonly int _comboIndexHash = Animator.StringToHash("comboIndex");
    public void PlayGroundAttack(int comboIndex)
    {
        _animator.SetInteger(_comboIndexHash, comboIndex);
        _animator.SetTrigger(_groundAattackHash);
    }
}
