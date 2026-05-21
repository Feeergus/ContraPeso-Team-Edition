using UnityEngine;
using UnityEngine.InputSystem;
using System;

[RequireComponent(typeof(Rigidbody), typeof(Collider))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float speed = 8f;
    public float jumpForce = 12f;

    [Header("Jump Settings")]
    public int maxJumps = 1;

    [Header("Gravity")]
    public float baseGravity = 20f;
    public float fallMultiplier = 2.5f;
    public float lowJumpMultiplier = 2f;

    [Header("Rotation")]
    public Transform visualModel;
    public float rotationSpeed = 10f;

    [Header("Ground Detection")]
    public float rayDistance = 0.2f;
    public float rayOffsetMultiplier = 0.4f;
    public LayerMask groundLayer;

    [Header("Physics")]
    public float maxForce = 10f;

    // 🔥 EVENTOS (clave para conectar sistemas)
    public Action OnResizeSmall;
    public Action OnResizeNormal;
    public Action OnResizeLarge;

    private Rigidbody rb;
    private Collider col;

    private Vector3 movementInput;
    private Vector2 moveInput;
    private bool isGrounded;

    private int jumpCount = 0;
    private bool jumpAllowedByState = true;

    private PlayerInputActions inputActions;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        col = GetComponent<Collider>();

        rb.useGravity = false;
        rb.linearDamping = 0f;
        rb.angularDamping = 0.05f;

        inputActions = new PlayerInputActions();
    }

    void OnEnable()
    {
        inputActions.Enable();

        // Movimiento y salto
        inputActions.Player.Jump.performed += OnJump;

        // 🔥 Resize
        inputActions.Player.ResizeSmall.performed += _ => OnResizeSmall?.Invoke();
        inputActions.Player.ResizeNormal.performed += _ => OnResizeNormal?.Invoke();
        inputActions.Player.ResizeLarge.performed += _ => OnResizeLarge?.Invoke();
    }

    void OnDisable()
    {
        inputActions.Player.Jump.performed -= OnJump;

        inputActions.Player.ResizeSmall.performed -= _ => OnResizeSmall?.Invoke();
        inputActions.Player.ResizeNormal.performed -= _ => OnResizeNormal?.Invoke();
        inputActions.Player.ResizeLarge.performed -= _ => OnResizeLarge?.Invoke();

        inputActions.Disable();
    }

    void Update()
    {
        HandleInput();
    }

    void FixedUpdate()
    {
        CheckGround();
        HandleMovement();
        ApplyCustomGravity();
        HandleRotation();
    }

    void HandleInput()
    {
        moveInput = inputActions.Player.Move.ReadValue<Vector2>();

        movementInput = Vector3.ClampMagnitude(
            new Vector3(moveInput.x, 0f, moveInput.y),
            1f
        );
    }

    void HandleMovement()
    {
        Vector3 targetVelocity = new Vector3(
            movementInput.x * speed,
            rb.linearVelocity.y,
            movementInput.z * speed
        );

        Vector3 velocityChange = targetVelocity - rb.linearVelocity;
        velocityChange.y = 0f;
        velocityChange = Vector3.ClampMagnitude(velocityChange, maxForce);

        rb.AddForce(velocityChange, ForceMode.VelocityChange);
    }

    void ApplyCustomGravity()
    {
        float gravity = baseGravity;

        if (rb.linearVelocity.y < 0)
            gravity *= fallMultiplier;
        else if (rb.linearVelocity.y > 0 && !inputActions.Player.Jump.IsPressed())
            gravity *= lowJumpMultiplier;

        rb.AddForce(Vector3.down * gravity, ForceMode.Acceleration);
    }

    void OnJump(InputAction.CallbackContext context)
    {
        TryJump();
    }

    void TryJump()
    {
        if (!jumpAllowedByState) return;
        if (jumpCount >= maxJumps) return;
        if (!isGrounded) return;

        rb.AddForce(Vector3.up * jumpForce, ForceMode.VelocityChange);
        jumpCount++;
    }

    void CheckGround()
    {
        Vector3 origin = GetRayOrigin();
        float width = col.bounds.extents.x * rayOffsetMultiplier;

        bool centerHit = Physics.Raycast(origin, Vector3.down, rayDistance, groundLayer);
        bool leftHit = Physics.Raycast(origin - transform.right * width, Vector3.down, rayDistance, groundLayer);
        bool rightHit = Physics.Raycast(origin + transform.right * width, Vector3.down, rayDistance, groundLayer);

        isGrounded = centerHit || leftHit || rightHit;

        if (isGrounded)
            jumpCount = 0;
    }

    Vector3 GetRayOrigin()
    {
        Vector3 center = col.bounds.center;
        float extentsY = col.bounds.extents.y;

        return center - Vector3.up * extentsY + Vector3.up * 0.05f;
    }

    void HandleRotation()
    {
        if (visualModel == null) return;
        if (movementInput == Vector3.zero) return;

        Quaternion targetRotation = Quaternion.LookRotation(movementInput);
        visualModel.rotation = Quaternion.Lerp(
            visualModel.rotation,
            targetRotation,
            rotationSpeed * Time.deltaTime
        );
    }

    public void SetMovementStats(float newSpeed, float newJumpForce, bool allowJump)
    {
        speed = newSpeed;
        jumpForce = newJumpForce;
        jumpAllowedByState = allowJump;
    }
}