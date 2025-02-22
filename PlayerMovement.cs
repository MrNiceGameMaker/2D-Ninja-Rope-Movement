using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float maxMoveSpeed = 10f;
    public float acceleration = 15f;
    public float deacceleration = 10f;
    public float airControlFactor = 0.5f;
    public float movementControlForce = 5f; // כוח שליטה בזמן תנועה

    [Header("Jump Settings")]
    public float jumpForce = 15f;
    public int maxJumps = 1;
    private int jumpCount;

    [Header("Wall Climbing Settings")]
    public float wallSlideSpeed = 3f;
    public float wallJumpForce = 15f;
    public float wallJumpDirectionForce = 10f;
    public LayerMask wallLayer;

    [Header("Ground Check")]
    public LayerMask groundLayer;
    public float groundCheckRadius = 0.2f;
    public Transform groundCheck;

    [Header("Slope Handling")]
    public float maxSlopeAngle = 45f; // זווית מדרון מקסימלית
    public float stepHeight = 0.3f; // גובה מדרגה מקסימלי

    [Header("Wall Check")]
    public Transform[] wallChecks; // מערך נקודות לבדיקה מול קירות
    public float wallCheckDistance = 0.5f;

    [Header("Collision Bounce Settings")]
    public float bounceForce = 10f; // כוח הקפיצה
    public float maxBounceForce = 20f; // כוח מקסימלי לקפיצה

    [Header("Rope Integration")]
    public PlayerStatsSO isRopeAttached;
    public int playerID;

    private Rigidbody2D rb;
    private bool isGrounded;
    private bool isTouchingWall;
    private bool isWallSliding;
    private Vector2 moveDirection;
    private PlayerInput playerInput;
    private RaycastHit2D slopeHit; // משתנה לבדיקת זווית המדרון
    private bool isAffectedByShockwave = false; // האם השחקן מושפע מגל הדף?

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        playerInput = GetComponent<PlayerInput>();
        jumpCount = maxJumps;
    }

    void Update()
    {
        moveDirection = playerInput.actions["Move"].ReadValue<Vector2>(); // קבלת קלט תנועה
        CheckGround();
        CheckWall();
        HandleNormalMovement();
    }

    void HandleNormalMovement()
    {
        if (playerInput.actions["Jump"].WasPressedThisFrame())
        {
            if (isGrounded || jumpCount > 0)
            {
                Jump();
            }
            else if (isWallSliding)
            {
                WallJump();
            }
        }

        if (isTouchingWall && !isGrounded && rb.linearVelocity.y < 0)
        {
            isWallSliding = true;
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, -wallSlideSpeed);
        }
        else
        {
            isWallSliding = false;
        }
    }

    void FixedUpdate()
    {
        if (!isRopeAttached.playerInfo[playerID].isRopeAttached && !isAffectedByShockwave) // אם אין חבל או גל הדף, נמשיך בתנועה רגילה
        {
            Move();
        }
    }

    void Move()
    {
        float targetSpeed = moveDirection.x * maxMoveSpeed;

        // בדיקת מדרון
        if (CheckSlope())
        {
            float slopeAngle = Vector2.Angle(Vector2.up, slopeHit.normal);

            if (slopeAngle <= maxSlopeAngle)
            {
                if (moveDirection.x != 0) // תנועה על המדרון רק אם יש קלט
                {
                    Vector2 slopeDirection = new Vector2(slopeHit.normal.y, -slopeHit.normal.x);
                    rb.AddForce(slopeDirection.normalized * targetSpeed, ForceMode2D.Force);
                }
                else
                {
                    rb.linearVelocity = new Vector2(0, rb.linearVelocity.y); // עצירה מוחלטת על הקרקע
                }
                return;
            }
        }

        float speedDifference = targetSpeed - rb.linearVelocity.x;
        float accelerationRate = isGrounded ? acceleration : acceleration * airControlFactor;

        if (moveDirection.x != 0)
        {
            float movement = Mathf.Clamp(speedDifference * accelerationRate, -maxMoveSpeed, maxMoveSpeed);
            rb.AddForce(new Vector2(movement, 0), ForceMode2D.Force);
        }

        if (moveDirection.x == 0 && isGrounded && !isAffectedByShockwave)
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        }
    }

    void Jump()
    {
        rb.linearVelocity += new Vector2(0, jumpForce);
        jumpCount--;
    }

    void WallJump()
    {
        Vector2 jumpDirection = new Vector2(moveDirection.x * wallJumpDirectionForce, wallJumpForce);
        rb.linearVelocity = jumpDirection;
        isWallSliding = false;
    }

    void CheckGround()
    {
        bool isTouchingGround = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
        isGrounded = isTouchingGround && rb.linearVelocity.y <= 0;

        if (isGrounded)
        {
            jumpCount = maxJumps;
        }
    }

    void CheckWall()
    {
        isTouchingWall = false;

        foreach (Transform wallCheck in wallChecks)
        {
            if (Physics2D.Raycast(wallCheck.position, transform.right * Mathf.Sign(moveDirection.x), wallCheckDistance, wallLayer))
            {
                isTouchingWall = true;
                jumpCount = maxJumps;
                break;
            }
        }
    }

    bool CheckSlope()
    {
        slopeHit = Physics2D.Raycast(groundCheck.position, Vector2.down, groundCheckRadius * 2, groundLayer);

        if (slopeHit.collider != null)
        {
            float slopeAngle = Vector2.Angle(Vector2.up, slopeHit.normal);
            return slopeAngle <= maxSlopeAngle;
        }

        return false;
    }

    public void ApplyShockwaveForce(Vector2 force)
    {
        rb.AddForce(force, ForceMode2D.Impulse);
        isAffectedByShockwave = true;
        Invoke(nameof(ResetShockwaveEffect), 0.3f);
    }

    private void ResetShockwaveEffect()
    {
        isAffectedByShockwave = false;
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        if (groundCheck != null)
        {
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }

        Gizmos.color = Color.blue;
        if (wallChecks != null)
        {
            foreach (Transform wallCheck in wallChecks)
            {
                Gizmos.DrawLine(wallCheck.position, wallCheck.position + Vector3.right * wallCheckDistance * Mathf.Sign(moveDirection.x));
            }
        }
    }
}
