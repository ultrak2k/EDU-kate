using UnityEngine;
using System.Collections;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(PlayerInput))]
public class PlayerController : MonoBehaviour
{

    [Header("Movement")]
    public float moveSpeed = 8f;

    [Header("Jump")]
    public float jumpForce = 16f;
    public LayerMask groundLayer;
    public Transform groundCheck;
    public float groundCheckRadius = 0.15f;

    [Header("Parry")]
    public float parryHangDuration = 0.2f;   // secs momentum is paused
    public float parryRotationSpeed = 720f;   // degrees per sec (720 = 1 sec as full spin at 0.5s)

    
    private Rigidbody2D rb;
    private bool isGrounded;
    private bool hasParried;          // used up parry this airtime?
    private bool isParrying;          // currently mid-parry animation?

    private float moveInput;   
    private bool jumpQueued;


    void Awake()
    {
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
        
        moveInput = value.Get<Vector2>().x;
    }

    void OnJump(InputValue value)
    {
        
        if (value.isPressed)
            jumpQueued = true;
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

        // Reset hasparried on landing
        if (isGrounded && !wasGrounded)
            hasParried = false;
    }


    void HandleMovement()
    {
        if (isParrying) return;
        rb.linearVelocity = new Vector2(moveInput * moveSpeed, rb.linearVelocity.y);
    }


    void HandleJumpAndParry()
    {
        if (!jumpQueued) return;
        jumpQueued = false;     // consume the input 

        if (isGrounded)
        {
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

        // Pause downward (and upward?) momentum
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
        rb.gravityScale = 0f;

        //Rotate exactly 360° over the duration
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
        rb.gravityScale = 1f;
        isParrying = false;
    }

    // Visualise grounch check
    void OnDrawGizmosSelected()
    {
        if (groundCheck == null) return;
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
    }
}