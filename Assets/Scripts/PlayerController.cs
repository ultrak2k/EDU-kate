using UnityEngine;
using System.Collections;
using UnityEngine.InputSystem;
using System;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(PlayerInput))]
public class PlayerController : MonoBehaviour
{

    [Header("Movement")]
    public float moveSpeed = 8f;

    [Header("Jump")]
    public float jumpForce = 16f;
    public LayerMask groundLayer;
    public LayerMask oneWayPlatformLayer;
    public Transform groundCheck;
    public float groundCheckRadius = 0.15f;

    [Header("Parry")]
    public float parryHangDuration = 0.2f;   // secs momentum/grav is paused
    public float parryRotationSpeed = 720f;   // degrees per sec (720 = 1 sec as full spin at 0.5s)

    [Header("Dash")]
    public float dashForce = 20f;
    public float dashDuration = 0.15f;      // how long gravity/momentum is suppressed
    public float dashCooldown = 0.6f;

    private float lastDashTime = -99f;      
    private float facingDirection = 1f;

    [Header("Coyote Time")] //apparently this is what its called??
    public float coyoteTime = 0.12f;        // seconds after leaving ground you can still jump
    private float coyoteTimeCounter;

    private Rigidbody2D rb;
    private bool isGrounded;
    private bool hasParried;          // used up parry this airtime?
    private bool isParrying;          // currently mid-parry animation?
    private bool isDashing;           // currently mid-dash

    private float moveInput;   
    private float verticalMoveInput;   
    private bool jumpQueued;

    [SerializeField]
    private Collider2D playerCollider;

    public event Action OnDash;         //handles what to do on a dash outside of this script

    void Awake()
    {
        //playerCollider = GetComponent<Collider2D>(); //redundant for now, could be used if we have an actual collider for the guy
        rb = GetComponent<Rigidbody2D>();

        // Auto-create a ground-check child if one wasn't assigned
        if (groundCheck == null)
        {
            GameObject gc = new GameObject("GroundCheck");
            gc.transform.SetParent(transform);
            gc.transform.localPosition = new Vector3(0f, -0.55f, 0f); // not sure if this works ngl
            groundCheck = gc.transform;
        }
    }
    void OnMove(InputValue value)
    {

        Vector2 input = value.Get<Vector2>();
        moveInput = input.x;
        verticalMoveInput = input.y;
        if (moveInput != 0f)
            facingDirection = Mathf.Sign(moveInput);
    }

    void OnJump(InputValue value)
    {
        
        if (value.isPressed)
            jumpQueued = true;
    }

    void OnSprint(InputValue value)
    {
        if (value.isPressed && !isDashing && Time.time >= lastDashTime + dashCooldown)
            StartCoroutine(DoDash());
    }

    void Update()
    {
        CheckGrounded();
        HandleJumpAndParry();
    }

    void FixedUpdate()
    {
        HandleMovement();
    }

   
    void CheckGrounded()
    {
        bool wasGrounded = isGrounded;
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
        bool isPlatformed = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, oneWayPlatformLayer);

        if (isGrounded || isPlatformed)
        {
            isGrounded = true; //platforms on a seperate layer, still need "GROUND" layer for parry reset and stuff
        }
        if (isGrounded)
            coyoteTimeCounter = coyoteTime;     // reset the window while grounded
        else
            coyoteTimeCounter -= Time.deltaTime;

        // Reset hasparried on landing
        if (isGrounded && !wasGrounded)
            hasParried = false;
    }


    void HandleMovement()
    {
        if (isParrying || isDashing) return;
        rb.linearVelocity = new Vector2(moveInput * moveSpeed, rb.linearVelocity.y);
    }


    void HandleJumpAndParry()
    {
        if (!jumpQueued) return;
        jumpQueued = false;     // consume the input 
        
        bool holdingDown = verticalMoveInput < -0.5f;
        Debug.Log($"isGrounded={isGrounded}  verticalInput={verticalMoveInput}  holdingDown={holdingDown}");
        if (isGrounded && holdingDown)
        {
            StartCoroutine(DropThrough());
            return;
        }

        bool canJump = isGrounded || coyoteTimeCounter > 0f;

        if (canJump)
        {
            coyoteTimeCounter = 0f;
            Jump();
        }
        else if (!hasParried && !isParrying)
        {
            StartCoroutine(DoParry());
        }
    }


    void Jump()
    {
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
    }


    IEnumerator DoParry()
    {
        hasParried = true;
        isParrying = true;

        GetComponent<ParryDetector>()?.SetParryActive(true);
        // Pause downward (and upward?) momentum
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
        rb.gravityScale = 0f;

        //Rotate exactly 360� over the duration
        float elapsed = 0f;
        float totalRotation = 0f;
        float targetRotation = 360f;

        while (elapsed < parryHangDuration)
        {
            elapsed += Time.deltaTime;
            float rotationThisFrame = (targetRotation / parryHangDuration) * Time.deltaTime;
            totalRotation += rotationThisFrame;
            transform.Rotate(0f, 0f, rotationThisFrame);
            yield return null;
        }

        // Snap to clean angle (make sure there arent decimals lying around)
        float startAngle = transform.eulerAngles.z - totalRotation;
        transform.eulerAngles = new Vector3(0f, 0f, startAngle + targetRotation);

        // become god and reapply gravity
        rb.gravityScale = 1.1f;
        GetComponent<ParryDetector>()?.SetParryActive(false);
        isParrying = false;
    }
    IEnumerator DoDash()
    {
        Debug.Log("DASH!");
        isDashing = true;
        lastDashTime = Time.time;

        // Use current input direction, or if no input, dash in facing direction
        float direction = moveInput != 0f ? Mathf.Sign(moveInput) : facingDirection;

        rb.gravityScale = 0f;
        rb.linearVelocity = new Vector2(direction * dashForce, 0f);

        OnDash.Invoke();

        yield return new WaitForSeconds(dashDuration);

        rb.gravityScale = 1f;
        // Bleed off horizontal speed so it doesn't feel goofy aah after the dash
        rb.linearVelocity = new Vector2(moveInput * moveSpeed, rb.linearVelocity.y);
        isDashing = false;
    }

    IEnumerator DropThrough()
    {
        // Grab the actual platform colliders under the player's feet
        Collider2D[] platforms = Physics2D.OverlapCircleAll(
            groundCheck.position, groundCheckRadius, oneWayPlatformLayer
        );

        foreach (Collider2D platform in platforms)
            Physics2D.IgnoreCollision(playerCollider, platform, true);

        rb.linearVelocity = new Vector2(rb.linearVelocity.x, -2f);

        yield return new WaitForSeconds(0.3f);

        foreach (Collider2D platform in platforms)
            Physics2D.IgnoreCollision(playerCollider, platform, false);
    }

    // Visualise grounch check
    void OnDrawGizmosSelected()
    {
        if (groundCheck == null) return;
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
    }
}