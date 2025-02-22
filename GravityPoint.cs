using UnityEngine;
using UnityEngine.InputSystem;

public class GravityPoint : MonoBehaviour
{
    [Header("Rope Settings")]
    public GameObject ropeProjectilePrefab;
    public float maxRopeLength = 100f;
    public float minRopeLength = 5f;
    public float ropeSpeed = 50f;
    public float changeRopeLengthSpeed = 5f;
    public float tensionForceFactor = 1f;
    public float horizontalTensionForceFactor = 2f;
    public float movementControlForce = 1f;
    [Range(0f, 100f)] public float ropeLengthPercentage = 15f;
    public float maxRopePullSpeed = 20f;
    public LayerMask attachableSurfaces;
    public PlayerStatsSO isRopeAttached;
    public int playerID;

    [Header("Visual Settings")]
    public LineRenderer lineRenderer;

    private Rigidbody2D playerRigidbody;
    private GameObject ropeProjectile;
    private Rigidbody2D ropeProjectileRigidbody;
    private Vector2 ropeAnchorPoint;
    private bool isRopeShooting = false;
    private float currentRopeLength;
    private float capturedRopeLength;
    private Vector2 lastValidAimDirection = Vector2.right;
    private bool isReturningToPlayer = false;
    private PlayerInput playerInput;

    void Start()
    {
        isRopeAttached.playerInfo[playerID].isRopeAttached = false;
        playerRigidbody = GetComponent<Rigidbody2D>();
        playerInput = GetComponent<PlayerInput>();
        currentRopeLength = maxRopeLength;

        ropeProjectile = Instantiate(ropeProjectilePrefab, transform.position, Quaternion.identity);
        ropeProjectile.transform.SetParent(transform);
        ropeProjectile.name = "GravityPoint";
        ropeProjectile.SetActive(false);
        ropeProjectileRigidbody = ropeProjectile.GetComponent<Rigidbody2D>();

        if (lineRenderer != null)
        {
            lineRenderer.enabled = false;
        }
    }

    void Update()
    {
        if (playerInput.actions["Grapple"].WasPressedThisFrame())
        {
            if (isRopeAttached.playerInfo[playerID].isRopeAttached || isRopeShooting)
            {
                DetachRope();
            }
            else
            {
                Vector2 direction = GetAimDirection();
                if (direction != Vector2.zero)
                {
                    StartRopeShoot(direction);
                }
            }
        }

        if (isRopeShooting && ropeProjectile.activeSelf)
        {
            HandleRopeProjectile();
        }

        if (isRopeAttached.playerInfo[playerID].isRopeAttached)
        {
            HandleRopePhysics();
            HandlePlayerMovementOnRope();
            UpdateLineRenderer();
        }
    }

    Vector2 GetAimDirection()
    {
        Vector2 direction;

        if (playerInput.currentControlScheme == "Keyboard & Mouse")
        {
            Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            direction = (mousePosition - transform.position).normalized;
        }
        else
        {
            direction = playerInput.actions["Aim"].ReadValue<Vector2>().normalized;

            // אם הסטיק במרכז (לא זז), נשתמש בכיוון האחרון שנרשם מעל 70% גודל
            if (direction.magnitude < 0.7f)
            {
                return lastValidAimDirection;
            }
            else
            {
                lastValidAimDirection = direction; // עדכון כיוון אחרון תקף
            }
        }

        return direction;
    }

    void StartRopeShoot(Vector3 direction)
    {
        isRopeShooting = true;
        isReturningToPlayer = false;
        ropeProjectile.transform.position = transform.position;
        ropeProjectile.SetActive(true);
        ropeProjectileRigidbody.linearVelocity = direction * ropeSpeed;
        ropeProjectileRigidbody.gravityScale = 0f;

        if (lineRenderer != null)
        {
            lineRenderer.enabled = true;
            lineRenderer.positionCount = 2;
            lineRenderer.SetPosition(0, transform.position);
            lineRenderer.SetPosition(1, ropeProjectile.transform.position);
        }
    }

    void HandleRopeProjectile()
    {
        if (!isReturningToPlayer)
        {
            if (Vector2.Distance(transform.position, ropeProjectile.transform.position) >= maxRopeLength)
            {
                isReturningToPlayer = true;
                ropeProjectileRigidbody.linearVelocity = Vector2.zero;
                ropeProjectileRigidbody.gravityScale = 1f;
            }
        }
        else
        {
            Vector2 directionToPlayer = ((Vector2)transform.position - (Vector2)ropeProjectile.transform.position).normalized;
            if (Vector2.Distance(transform.position, ropeProjectile.transform.position) >= 5)
            {
                ropeProjectileRigidbody.AddForce(directionToPlayer * ropeSpeed / 4, ForceMode2D.Force);
            }
        }

        RaycastHit2D hit = Physics2D.Raycast(ropeProjectile.transform.position, ropeProjectileRigidbody.linearVelocity.normalized, 0.1f, attachableSurfaces);
        if (hit.collider != null)
        {
            ropeAnchorPoint = hit.point;
            isRopeAttached.playerInfo[playerID].isRopeAttached = true;
            isRopeShooting = false;
            ropeProjectileRigidbody.linearVelocity = Vector2.zero;
            ropeProjectileRigidbody.gravityScale = 0f;
            capturedRopeLength = Mathf.Max(Vector2.Distance(transform.position, ropeAnchorPoint), minRopeLength);
            currentRopeLength = capturedRopeLength * (ropeLengthPercentage / 100f);
            UpdateLineRenderer();
        }

        UpdateLineRenderer();
    }

    void HandleRopePhysics()
    {
        if (isRopeAttached.playerInfo[playerID].isRopeAttached && !IsValidCollider(ropeAnchorPoint))
        {
            DetachRope();
            return;
        }

        if (!isRopeAttached.playerInfo[playerID].isRopeAttached) return;

        Vector2 directionToAnchor = (ropeAnchorPoint - (Vector2)transform.position).normalized;
        float distanceToAnchor = Vector2.Distance(transform.position, ropeAnchorPoint);
        float baseLength = currentRopeLength;

        if (distanceToAnchor > baseLength)
        {
            float tensionForce = (distanceToAnchor - baseLength) * tensionForceFactor;
            playerRigidbody.AddForce(directionToAnchor * tensionForce, ForceMode2D.Force);
        }

        float horizontalOffset = transform.position.x - ropeAnchorPoint.x;
        float horizontalTensionForce = horizontalOffset * horizontalTensionForceFactor;
        playerRigidbody.AddForce(Vector2.left * horizontalTensionForce, ForceMode2D.Force);

        Vector2 clampedVelocity = Vector2.ClampMagnitude(playerRigidbody.linearVelocity, maxRopePullSpeed);
        playerRigidbody.linearVelocity = clampedVelocity;
    }

    void HandlePlayerMovementOnRope()
    {
        Vector2 moveInput = playerInput.actions["Move"].ReadValue<Vector2>();

        if (moveInput.y > 0)
        {
            currentRopeLength = Mathf.Max(minRopeLength, currentRopeLength - Time.deltaTime * changeRopeLengthSpeed);
        }
        else if (moveInput.y < 0)
        {
            currentRopeLength = Mathf.Min(maxRopeLength, currentRopeLength + Time.deltaTime * changeRopeLengthSpeed);
        }

        if (moveInput.x > 0)
        {
            playerRigidbody.AddForce(Vector2.right * movementControlForce, ForceMode2D.Force);
        }
        else if (moveInput.x < 0)
        {
            playerRigidbody.AddForce(Vector2.left * movementControlForce, ForceMode2D.Force);
        }
    }

    bool IsValidCollider(Vector2 point)
    {
        Collider2D collider = Physics2D.OverlapPoint(point, attachableSurfaces);
        return collider != null;
    }

    public void DetachRope()
    {
        isRopeAttached.playerInfo[playerID].isRopeAttached = false;
        isRopeShooting = false;
        isReturningToPlayer = false;
        ropeProjectile.SetActive(false);
        if (lineRenderer != null)
        {
            lineRenderer.enabled = false;
        }
    }

    void UpdateLineRenderer()
    {
        if (lineRenderer != null && (isRopeAttached.playerInfo[playerID].isRopeAttached || isRopeShooting))
        {
            lineRenderer.SetPosition(0, transform.position);
            lineRenderer.SetPosition(1, isRopeShooting ? ropeProjectile.transform.position : ropeAnchorPoint);
        }
    }
}
