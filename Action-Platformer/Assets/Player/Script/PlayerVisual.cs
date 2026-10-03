using UnityEngine;

public class PlayerVisual : MonoBehaviour
{
    [SerializeField] SpriteRenderer _renderer;
    [SerializeField] Animator _animator;

    public PlayerInput.MoveDirection FaceDirection { get; private set; } = PlayerInput.MoveDirection.Right;

    public void SetFacingDirection(PlayerInput.MoveDirection direction)
    {
        switch (direction)
        {
            case PlayerInput.MoveDirection.Right:
                FaceDirection = PlayerInput.MoveDirection.Right;
                _renderer.flipX = false;
                break;
            case PlayerInput.MoveDirection.Left:
                FaceDirection = PlayerInput.MoveDirection.Left;
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

    static readonly int _ascendHash = Animator.StringToHash("ascend");
    static readonly int _descendHash = Animator.StringToHash("descend");
    static readonly int _landHash = Animator.StringToHash("land");
    public void PlayAscend()
    {
        _animator.SetTrigger(_ascendHash);
    }
    public void PlayDescend()
    {
        _animator.SetTrigger(_descendHash);
    }
    public void PlayLand()
    {
        _animator.SetTrigger(_landHash);
    }

    static readonly int _groundAattackHash = Animator.StringToHash("groundAttack");
    static readonly int _comboIndexHash = Animator.StringToHash("comboIndex");
    public void PlayGroundAttack(int comboIndex)
    {
        _animator.SetInteger(_comboIndexHash, comboIndex);
        _animator.SetTrigger(_groundAattackHash);
    }

    static readonly int _dodgeHash = Animator.StringToHash("dodge");
    public void PlayDodge()
    {
        _animator.SetTrigger(_dodgeHash);
    }
}
