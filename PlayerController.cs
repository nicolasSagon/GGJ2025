using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.Experimental.GraphView;
using System;

public class PlayerController : MonoBehaviour
{
    public Color playerColor = Color.green;
    public StuckBar stuckBar;
    public bool isLogging = false;
    public bool isStartingFacingRight = true;
    public float initialSpeed = 10f;
    private float currentSpeed;
    public float boostSpeed = 18f;
    public float boostDuration = 0.05f;
    public float jumpForce = 30f;
    public float jumpForceFactor = 0.1f;
    public int unstuckForce = 5;
    public float attackCooldown = 0.2f;
    private float lastAttackTime = -Mathf.Infinity; // Time when the last attack occurred
    private Vector2 moveInput;
    private float lastDirection = 0; // Tracks the last movement direction (-1 for left, 1 for right, 0 for no movement)    private float lastDirectionChangeTime;
    private float lastBoostTime = -Mathf.Infinity;
    private bool isBoosting = false;
    private Rigidbody rb;
    private bool isTouchingWallRightWall;
    private bool isTouchingWallLeftWall;
    private int stuckValue = 100;
    private bool isLastMoveToRight = false;
    private bool isGrounded = false;
    private bool isJumping = false;
    private bool timeoutForJumpAnim = false;
    public float maxJumpTime = 0.25f; // Maximum time the jump button can be held
    private float jumpTimeCounter; // Tracks how long the jump button has been held

    private PlayerState playerState = PlayerState.Idle;

    private List<GameObject> ballsInRange = new();

    private Animator animator;

    public AudioClip jumpSound, attackSound, hitSound, missedAttackSound;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();
        currentSpeed = initialSpeed;
        if (isStartingFacingRight == false)
        {
            lastDirection = -1;
        } else {
            lastDirection = 1;
        }
    }

    void Update()
    {
        CheckWallCollision();
    }

    public void FixedUpdate()
    {
        HandleJumping();
        // Handle boost duration
        if (isBoosting && (Time.time >= lastBoostTime + boostDuration))
        {
            EndBoost();
        }
        if (playerState != PlayerState.Stuck)
        {  
            if (!isTouchingWallRightWall && !isTouchingWallLeftWall && moveInput.x != 0)
            {
                Vector2 targetVelocity = new Vector2(moveInput.x * currentSpeed, rb.linearVelocity.y);
                if (isGrounded && !timeoutForJumpAnim) {
                    playerState = PlayerState.Walking;
                }
                rb.linearVelocity = targetVelocity;
            }
            else if (isTouchingWallRightWall && moveInput.x < 0)
            {
                Vector2 targetVelocity = new Vector2(moveInput.x * currentSpeed, rb.linearVelocity.y);
                if (isGrounded && !timeoutForJumpAnim) {
                    playerState = PlayerState.Walking;
                }
                rb.linearVelocity = targetVelocity;
            }
            else if (isTouchingWallLeftWall && moveInput.x > 0)
            {
                Vector2 targetVelocity = new Vector2(moveInput.x * currentSpeed, rb.linearVelocity.y);
                if (isGrounded && !timeoutForJumpAnim) {
                    playerState = PlayerState.Walking;
                }
                rb.linearVelocity = targetVelocity;
            } else {
                if (isGrounded && !timeoutForJumpAnim) {
                    playerState = PlayerState.Idle;
                }
            }
        }
        updateAnimator();
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        float moveX = context.ReadValue<Vector2>().x;
        float currentDirection = Mathf.Sign(moveX);
        // Check if the player has changed direction
        if (moveX != 0 && currentDirection != lastDirection)
        {

            // Check if enough time has passed since the last boost
            if (Time.time >= lastBoostTime + boostDuration)
            {
                StartBoost();
            }
            lastDirection = currentDirection; // Update the direction tracking
            lastBoostTime = Time.time;
            transform.RotateAround(transform.localPosition, transform.up, 180f);
        }

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
                stuckBar.UpdateStuckBar(stuckValue);
            }
        }
    }

    void StartBoost()
    {
        isBoosting = true;
        currentSpeed = boostSpeed;
    }

    void EndBoost()
    {
        isBoosting = false;
        currentSpeed = initialSpeed;
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if (context.started && isGrounded)
        {
            playerState = PlayerState.Jumping;
            StartCoroutine(waitForJumpAnim());
            isJumping = true;
            jumpTimeCounter = maxJumpTime;
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, jumpForce); // Apply initial jump force
            AudioController.PlaySound(jumpSound);
        }

        // Stop the jump when the jump button is released
        if (context.canceled)
        {
            isJumping = false;
        }      
    }

    void HandleJumping() {
        if (isJumping)
        {
            // If the jump button is held, apply additional force over time

            if (jumpTimeCounter > 0)
            {
                rb.linearVelocity += new Vector3(rb.linearVelocity.x, jumpForce*jumpForceFactor); // Continue applying force
                jumpTimeCounter -= Time.deltaTime; // Reduce the remaining jump time
            }
            else
            {
                isJumping = false; // Stop the jump when the time runs out
            }
        }
    }

    public void OnAttack(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            // Check if enough time has passed since the last attack
            if (Time.time <= lastAttackTime + attackCooldown)
            {
                Debug.Log("Attack on cooldown");
                return;
            }
            playerState = PlayerState.Attacking;
            // Perform attack logic here
            // Hit all balls in range
            if (ballsInRange.Count == 0) {
                AudioController.PlaySound(missedAttackSound);
            }
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
                    AudioController.PlaySound(attackSound);
                }
            }
            animator.SetTrigger("Attack");
            lastAttackTime = Time.time; // Update the last attack time
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
        AudioController.PlaySound(hitSound);
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
        float groundRayLength = 1f;
        Vector3 position = transform.position;
        Vector3 direction = new Vector3(1, 0);
        Vector3 direction2 = new Vector3(-1, 0);
        Vector3 directionGround = new Vector3(0, -1);

        RaycastHit hit;
        Physics.Raycast(position, direction, out hit, rayLength, LayerMask.GetMask("Default"));
        if (hit.collider == null)
        {
            Debug.DrawRay(position, direction * rayLength, Color.red);
            isTouchingWallRightWall = false;
        }
        else
        {
            Debug.DrawRay(position, direction * rayLength, Color.green);
            isTouchingWallRightWall = true;
        }

        RaycastHit hit2;
        Physics.Raycast(position, direction2, out hit2, rayLength, LayerMask.GetMask("Default"));
        if (hit2.collider == null)
        {
            Debug.DrawRay(position, direction2 * rayLength, Color.red);
            isTouchingWallLeftWall = false;
        }
        else
        {
            Debug.DrawRay(position, direction2 * rayLength, Color.green);
            isTouchingWallLeftWall = true;
        }

        RaycastHit hit3;
        Physics.Raycast(position, directionGround, out hit3, groundRayLength, LayerMask.GetMask("Default"));
        if (hit3.collider == null)
        {
            Debug.DrawRay(position, directionGround * groundRayLength, Color.red);
            isGrounded = false;
        }
        else
        {
            Debug.DrawRay(position, directionGround * groundRayLength, Color.green);
            isGrounded = true;
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

    public System.Collections.IEnumerator waitForJumpAnim() {
        timeoutForJumpAnim = true;
        yield return new WaitForSeconds(0.2f);
        timeoutForJumpAnim = false;
    }
}