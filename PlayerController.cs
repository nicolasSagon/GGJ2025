using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class PlayerController : MonoBehaviour
{
    public Color playerColor = Color.green;
    public float moveSpeed = 5f;
    public float jumpForce = 5f;
    private Vector2 moveInput;
    private Rigidbody rb;
    private bool isTouchingWallRightWall;
    private bool isTouchingWallLeftWall;

    private List<GameObject> ballsInRange = new List<GameObject>();

    void Start()
    {
        rb = GetComponent<Rigidbody>();
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
            rb.AddForce(new Vector2(0f, jumpForce), ForceMode.Impulse);
        }
    }

    public void OnAttack(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            // Perform attack logic here
            Debug.Log("Attack!");
            // Hit all balls in range
            foreach (GameObject ball in ballsInRange)
            {
                Rigidbody ballRb = ball.GetComponent<Rigidbody>();
                if (ballRb != null)
                {
                    Vector2 hitVector = moveInput.normalized;
                    if (hitVector == Vector2.zero) {
                        // If the player is not moving, hit in the direction of the ball
                        hitVector = new Vector2(0, 1);
                    }

                    ball.GetComponent<BubbleController>().HitBall(this, hitVector);
                }
            }
        }
    }


    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Ball") && !ballsInRange.Contains(other.transform.parent.gameObject))
        {
            ballsInRange.Add(other.transform.parent.gameObject);
            Debug.Log("Ball entered range: " + other.transform.parent.gameObject);
        }
        if (other.CompareTag("BallHitbox"))
        {
            if (other.transform.parent.gameObject.GetComponent<BubbleController>().GetCurrentOwner() != this && other.transform.parent.gameObject.GetComponent<BubbleController>().GetCurrentOwner() != null) {
                Debug.Log("Ball hit: " + other.transform.parent.gameObject);
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Ball") && ballsInRange.Contains(other.transform.parent.gameObject))
        {
            ballsInRange.Remove(other.transform.parent.gameObject);
            Debug.Log("Ball exited range: " + other.transform.parent.gameObject);
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