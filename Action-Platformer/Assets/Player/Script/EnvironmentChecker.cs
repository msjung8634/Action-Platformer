using UnityEngine;

public class EnvironmentChecker : MonoBehaviour
{
    [Header("Ground Check")]
    [SerializeField] LayerMask _groundLayer;
    [SerializeField] Transform _groundCheck;
    [SerializeField] float _groundCheckRadius = 0.15f;

    [Header("Wall Check")]
    [SerializeField] LayerMask _wallLayer;
    [SerializeField] Transform _wallCheck;
    [SerializeField] Vector2 _wallCheckSize = new(0.1f, 0.8f);

    [field:SerializeField] public bool IsGrounded { get; private set; }
    [field: SerializeField] public bool IsTouchingWall { get; private set; }

    void FixedUpdate()
    {
        CheckGround();
        CheckWall();
    }

    void CheckGround()
    {
        IsGrounded = Physics2D.OverlapCircle(
            _groundCheck.position,
            _groundCheckRadius,
            _groundLayer
        );
    }

    void CheckWall()
    {
        IsTouchingWall = Physics2D.OverlapBox(
            _wallCheck.position,
            _wallCheckSize,
            0f,
            _groundLayer
        );
    }

    void OnDrawGizmosSelected()
    {
        if (_groundCheck != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(
                _groundCheck.position,
                _groundCheckRadius
            );
        }

        if (_wallCheck != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(
                _wallCheck.position,
                _wallCheckSize
            );
        }
    }
}
