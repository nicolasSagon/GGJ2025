using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float jumpForce = 5f;
    private Vector2 moveInput;
    private Rigidbody2D rb;
    private bool isTouchingWallRightWall;
    private bool isTouchingWallLeftWall;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        CheckWallCollision();
    }

    public void FixedUpdate()
{
    if (!isTouchingWallRightWall && !isTouchingWallLeftWall)
    {
        Vector2 targetVelocity = new Vector2(moveInput.x * moveSpeed, rb.linearVelocity.y);
        rb.linearVelocity = targetVelocity;
    }
    else if (isTouchingWallRightWall && moveInput.x < 0)
    {
        Vector2 targetVelocity = new Vector2(moveInput.x * moveSpeed, rb.linearVelocity.y);
        rb.linearVelocity = targetVelocity;
    }
    else if (isTouchingWallLeftWall && moveInput.x > 0)
    {
        Vector2 targetVelocity = new Vector2(moveInput.x * moveSpeed, rb.linearVelocity.y);
        rb.linearVelocity = targetVelocity;
    }
}

    public void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if (context.performed && Mathf.Approximately(rb.linearVelocity.y, 0))
        {
            rb.AddForce(new Vector2(0f, jumpForce), ForceMode2D.Impulse);
        }
    }

    public void OnAttack(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            // Perform attack logic here
            Debug.Log("Attack!");
        }
    }

    private void CheckWallCollision()
    {
        float rayLength = 1f;
        Vector2 position = transform.position;
        Vector2 direction = new Vector2(1, 0);
        Vector2 direction2 = new Vector2(-1, 0);

        RaycastHit2D hit = Physics2D.Raycast(position, direction, rayLength, LayerMask.GetMask("Default"));
        if (hit.collider == null) {
            Debug.DrawRay(position, direction * rayLength, Color.red);
        } else {
            Debug.DrawRay(position, direction * rayLength, Color.green);
        }

        RaycastHit2D hit2 = Physics2D.Raycast(position, direction2, rayLength, LayerMask.GetMask("Default"));
        if (hit2.collider == null) {
            Debug.DrawRay(position, direction2 * rayLength, Color.red);
        } else {
            Debug.DrawRay(position, direction2 * rayLength, Color.green);
        }

        if (hit.collider != null) {
            isTouchingWallRightWall = true;
        } else {
            isTouchingWallRightWall = false;
        }

        if (hit2.collider != null) {
            isTouchingWallLeftWall = true;
        } else {
            isTouchingWallLeftWall = false;
        }
    }
}