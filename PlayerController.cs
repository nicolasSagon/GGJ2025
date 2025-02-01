using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using Unity.VisualScripting;

public class PlayerController : MonoBehaviour
{
    public Color playerColor = Color.green;
    public StuckBar stuckBar;
    public float moveSpeed = 5f;
    public float jumpForce = 5f;
    public int unstuckForce = 5;
    private Vector2 moveInput;
    private Rigidbody rb;
    private bool isTouchingWallRightWall;
    private bool isTouchingWallLeftWall;
    private int stuckValue = 100;
    private bool isLastMoveToRight = false;

    private PlayerState playerState = PlayerState.Idle;

    private List<GameObject> ballsInRange = new List<GameObject>();

    private Animator animator;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        CheckWallCollision();
    }

    public void FixedUpdate()
    {
        if (playerState != PlayerState.Stuck)
        {
            if (!isTouchingWallRightWall && !isTouchingWallLeftWall && moveInput.x != 0)
            {
                Vector2 targetVelocity = new Vector2(moveInput.x * moveSpeed, rb.linearVelocity.y);
                playerState = PlayerState.Walking;
                rb.linearVelocity = targetVelocity;
            }
            else if (isTouchingWallRightWall && moveInput.x < 0)
            {
                Vector2 targetVelocity = new Vector2(moveInput.x * moveSpeed, rb.linearVelocity.y);
                playerState = PlayerState.Walking;
                rb.linearVelocity = targetVelocity;
            }
            else if (isTouchingWallLeftWall && moveInput.x > 0)
            {
                Vector2 targetVelocity = new Vector2(moveInput.x * moveSpeed, rb.linearVelocity.y);
                playerState = PlayerState.Walking;
                rb.linearVelocity = targetVelocity;
            } else {
                if (Mathf.Approximately(rb.linearVelocity.y, 0)) {
                    if (playerState != PlayerState.Idle) {
                        Debug.Log("Reset to idle");
                        playerState = PlayerState.Idle;
                    }
                }
                
            }
        }
        updateAnimator();
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
        stuckBar.UpdateStuckBar(stuckValue);
        if (playerState == PlayerState.Stuck) {
            if(context.started) {
                if (moveInput.x > 0) {
                    isLastMoveToRight = true;
                    stuckValue -= unstuckForce;
                } else if (moveInput.x < 0) {
                    isLastMoveToRight = false;
                    stuckValue -= unstuckForce;
                }
            } else {
                if (moveInput.x > 0 && !isLastMoveToRight) {
                    stuckValue -= unstuckForce;
                    isLastMoveToRight = true;
                } else if (moveInput.x < 0 && isLastMoveToRight) {
                    stuckValue -= unstuckForce;
                    isLastMoveToRight = false;
                }
            }
            if (stuckValue <= 0)
            {
                playerState = PlayerState.Idle;
                stuckBar.UpdateStuckBar(stuckValue);
            }
        }
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if (context.performed && Mathf.Approximately(rb.linearVelocity.y, 0))
        {
            playerState = PlayerState.Jumping;
            rb.AddForce(new Vector2(0f, jumpForce), ForceMode.Impulse);
        }
    }

    public void OnAttack(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            playerState = PlayerState.Attacking;
            // Perform attack logic here
            // Hit all balls in range
            foreach (GameObject ball in ballsInRange)
            {
                if (ball.IsDestroyed() == true) {
                    continue;
                }
                Rigidbody ballRb = ball.GetComponent<Rigidbody>();
                if (ballRb != null)
                {
                    Vector2 hitVector = moveInput.normalized;
                    if (hitVector == Vector2.zero)
                    {
                        // If the player is not moving, hit up
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
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("BallHitbox"))
        {
            BubbleController ball = collision.gameObject.GetComponent<BubbleController>();
            if (ball.GetCurrentOwner() != this && ball.GetCurrentOwner() != null)
            {
                GotHit(ball);
            }
        }
    }

    void GotHit(BubbleController ball)
    {
        Debug.Log("Got hit by: " + ball.name + " owned by: " + ball.GetComponent<BubbleController>().GetCurrentOwner().name);
        OnPlayerGetStuck();
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Ball") && ballsInRange.Contains(other.transform.parent.gameObject))
        {
            ballsInRange.Remove(other.transform.parent.gameObject);
        }
    }

    private void CheckWallCollision()
    {
        float rayLength = 1f;
        Vector3 position = transform.position;
        Vector3 direction = new Vector3(1, 0);
        Vector3 direction2 = new Vector3(-1, 0);

        RaycastHit hit;
        Physics.Raycast(position, direction, out hit, rayLength, LayerMask.GetMask("Default"));
        if (hit.collider == null)
        {
            Debug.DrawRay(position, direction * rayLength, Color.red);
        }
        else
        {
            Debug.DrawRay(position, direction * rayLength, Color.green);
        }

        RaycastHit hit2;
        Physics.Raycast(position, direction2, out hit2, rayLength, LayerMask.GetMask("Default"));
        if (hit2.collider == null)
        {
            Debug.DrawRay(position, direction2 * rayLength, Color.red);
        }
        else
        {
            Debug.DrawRay(position, direction2 * rayLength, Color.green);
        }

        if (hit.collider != null)
        {
            isTouchingWallRightWall = true;
        }
        else
        {
            isTouchingWallRightWall = false;
        }

        if (hit2.collider != null)
        {
            isTouchingWallLeftWall = true;
        }
        else
        {
            isTouchingWallLeftWall = false;
        }
    }

    public void OnPlayerGetStuck()
    {
        playerState = PlayerState.Stuck;
        stuckValue = 100;
        stuckBar.UpdateStuckBar(stuckValue);
    }

    public void updateAnimator() {
        switch(playerState) {
            case PlayerState.Walking: {
                animator.SetBool("isRunning", true);
                animator.SetBool("isJumping", false);
                break;
            }
            case PlayerState.Idle: {
                animator.SetBool("isRunning", false);
                animator.SetBool("isJumping", false);
                break;
            }
            case PlayerState.Jumping: {
                animator.SetBool("isJumping", true);
                animator.SetBool("isRunning", false);
                break;
            }
        }
        
    }
}