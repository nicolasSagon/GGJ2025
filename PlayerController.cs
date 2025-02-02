using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.Experimental.GraphView;
using System;
using System.Collections;

public class PlayerController : MonoBehaviour
{
    public Color playerColor = Color.green;
    public float playerHealth = 100f;
    public StuckBar stuckBar;
    public float verticalRayLength = 1f, horizontalRayLength = 1f;
    public float wallJumpVerticalFactor = 0.5f, wallJumpHorizontalFactor = 2f;
    public bool isLogging = false;
    public bool isStartingFacingRight = true;
    public float initialSpeed = 10f;
    private float currentSpeed;
    public float boostSpeed = 18f;
    public float boostDuration = 0.05f;
    public float jumpForce = 30f;
    public float jumpForceFactor = 0.1f;
    public int unstuckForce = 5;
    public float freezeTime = 0.1f;
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
        }
        else
        {
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
                if (isGrounded && !timeoutForJumpAnim)
                {
                    playerState = PlayerState.Walking;
                }
                rb.linearVelocity = targetVelocity;
            }
            else if (isTouchingWallRightWall && moveInput.x < 0)
            {
                Vector2 targetVelocity = new Vector2(moveInput.x * currentSpeed, rb.linearVelocity.y);
                if (isGrounded && !timeoutForJumpAnim)
                {
                    playerState = PlayerState.Walking;
                }
                rb.linearVelocity = targetVelocity;
            }
            else if (isTouchingWallLeftWall && moveInput.x > 0)
            {
                Vector2 targetVelocity = new Vector2(moveInput.x * currentSpeed, rb.linearVelocity.y);
                if (isGrounded && !timeoutForJumpAnim)
                {
                    playerState = PlayerState.Walking;
                }
                rb.linearVelocity = targetVelocity;
            }
            else
            {
                if (isGrounded && !timeoutForJumpAnim)
                {
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
        if (playerState == PlayerState.Stuck)
        {
            if (context.started)
            {
                if (moveInput.x > 0)
                {
                    isLastMoveToRight = true;
                    stuckValue -= unstuckForce;
                }
                else if (moveInput.x < 0)
                {
                    isLastMoveToRight = false;
                    stuckValue -= unstuckForce;
                }
            }
            else
            {
                if (moveInput.x > 0 && !isLastMoveToRight)
                {
                    stuckValue -= unstuckForce;
                    isLastMoveToRight = true;
                }
                else if (moveInput.x < 0 && isLastMoveToRight)
                {
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
        if (context.started && (isGrounded || IsTouchingWall()))
        {
            playerState = PlayerState.Jumping;
            StartCoroutine(waitForJumpAnim());
            isJumping = true;
            jumpTimeCounter = maxJumpTime;
            // Apply initial jump force
            if (IsTouchingWall() && !isGrounded)
            {
                float xForceSign = isTouchingWallLeftWall ? 1 : -1;
                rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0); // Reset y velocity for consistent jump height
                Vector3 targetVelocity = new Vector3(xForceSign * jumpForce * wallJumpHorizontalFactor, jumpForce * wallJumpVerticalFactor);

                // Start a coroutine to smooth the wall jump
                StartCoroutine(SmoothWallJump(targetVelocity));
            }
            else {
                rb.linearVelocity = new Vector3(rb.linearVelocity.x, jumpForce);
            }
            AudioController.PlaySound(jumpSound);
        }

        // Stop the jump when the jump button is released
        if (context.canceled)
        {
            isJumping = false;
        }
    }
private IEnumerator SmoothWallJump(Vector3 targetForce)
{
    float elapsedTime = 0f;
    float wallJumpSmoothTime = 0.1f;
    Vector3 forceApplied = Vector3.zero;

    while (elapsedTime < wallJumpSmoothTime)
    {
        elapsedTime += Time.deltaTime;
        float t = elapsedTime / wallJumpSmoothTime; // Normalized time (0 to 1)

        // Calculate the force to apply this frame
        Vector3 forceThisFrame = Vector3.Lerp(Vector3.zero, targetForce, t) - forceApplied;
        rb.AddForce(forceThisFrame, ForceMode.VelocityChange);

        // Track the total force applied so far
        forceApplied += forceThisFrame;

        yield return null;
    }

    // Ensure the final force is applied exactly
    rb.AddForce(targetForce - forceApplied, ForceMode.VelocityChange);
}

    void HandleJumping()
    {
        if (isJumping)
        {
            // If the jump button is held, apply additional force over time

            if (jumpTimeCounter > 0)
            {
                rb.linearVelocity += new Vector3(rb.linearVelocity.x, jumpForce * jumpForceFactor); // Continue applying force
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
            if (ballsInRange.Count == 0)
            {
                AudioController.PlaySound(missedAttackSound);
            } else {
                StartCoroutine(FreezePlayer());
            }
            foreach (GameObject ball in ballsInRange)
            {
                if (ball.IsDestroyed() == true)
                {
                    continue;
                }
                Rigidbody ballRb = ball.GetComponent<Rigidbody>();
                if (ballRb != null)
                {
                    Vector2 hitVector = moveInput.normalized;
                    if (hitVector == Vector2.zero)
                    {
                        // If the player is not moving, hit forward
                        hitVector = transform.forward;
                    }

                    ball.GetComponent<BubbleController>().HitBall(this, hitVector);
                    AudioController.PlaySound(attackSound);
                }
            }
            animator.SetTrigger("Attack");
            lastAttackTime = Time.time; // Update the last attack time
        }
    }

    public void addBallInRange(GameObject ball) {
        if (!ballsInRange.Contains(ball.transform.parent.gameObject)){
            ballsInRange.Add(ball.transform.parent.gameObject);
        }
    }

    public void removeBallInRange(GameObject ball) {
        if (ballsInRange.Contains(ball.transform.parent.gameObject))
        {
            ballsInRange.Remove(ball.transform.parent.gameObject);
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
        Destroy(ball.gameObject);
        AudioController.PlaySound(hitSound);
        ApplyDamage(ball.GetDamage());
    }

    private void ApplyDamage(float damage)
    {
        if (playerHealth > 0)
        {
            playerHealth -= damage;
            Debug.Log("Health: " + playerHealth);
        }
        else
        {
            Die();
        }
    }

    private void Die()
    {
        Debug.Log("Player died");
        playerState = PlayerState.Dead;
    }

    void OnTriggerExit(Collider other)
    {
        
    }

    private bool IsTouchingWall(){
        return isTouchingWallRightWall || isTouchingWallLeftWall;
    }

    private void CheckWallCollision()
    {
        float horizontalRayLength = 2f;
        float verticalRayLength = 1f;
        Vector3 position = transform.position;
        Vector3 direction = new Vector3(1, 0);
        Vector3 direction2 = new Vector3(-1, 0);
        Vector3 directionGround = new Vector3(0, -1);

        RaycastHit hit;
        Physics.Raycast(position, direction, out hit, horizontalRayLength, LayerMask.GetMask("Wall"));
        if (hit.collider == null)
        {
            Debug.DrawRay(position, direction * horizontalRayLength, Color.red);
            isTouchingWallRightWall = false;
        }
        else
        {
            Debug.DrawRay(position, direction * horizontalRayLength, Color.green);
            isTouchingWallRightWall = true;
        }

        RaycastHit hit2;
        Physics.Raycast(position, direction2, out hit2, horizontalRayLength, LayerMask.GetMask("Wall"));
        if (hit2.collider == null)
        {
            Debug.DrawRay(position, direction2 * horizontalRayLength, Color.red);
            isTouchingWallLeftWall = false;
        }
        else
        {
            Debug.DrawRay(position, direction2 * horizontalRayLength, Color.green);
            isTouchingWallLeftWall = true;
        }

        RaycastHit hit3;
        Physics.Raycast(position, directionGround, out hit3, verticalRayLength, LayerMask.GetMask("Wall"));
        if (hit3.collider == null)
        {
            Debug.DrawRay(position, directionGround * verticalRayLength, Color.red);
            isGrounded = false;
        }
        else
        {
            Debug.DrawRay(position, directionGround * verticalRayLength, Color.green);
            isGrounded = true;
        }
    }

    public void updateAnimator()
    {
        switch (playerState)
        {
            case PlayerState.Walking:
                {
                    animator.SetBool("isRunning", true);
                    animator.SetBool("isJumping", false);
                    break;
                }
            case PlayerState.Idle:
                {
                    animator.SetBool("isRunning", false);
                    animator.SetBool("isJumping", false);
                    break;
                }
            case PlayerState.Jumping:
                {
                    animator.SetBool("isJumping", true);
                    animator.SetBool("isRunning", false);
                    break;
                }
        }
    }

    public System.Collections.IEnumerator waitForJumpAnim()
    {
        timeoutForJumpAnim = true;
        yield return new WaitForSeconds(0.2f);
        timeoutForJumpAnim = false;
    }

    public System.Collections.IEnumerator FreezePlayer()
    {
        var playerVelocity = rb.linearVelocity;
        var playerConstraints = rb.constraints;
        rb.constraints = RigidbodyConstraints.FreezeAll;
        yield return new WaitForSeconds(freezeTime);
        rb.constraints =  playerConstraints;
        rb.linearVelocity = playerVelocity;
    }
}