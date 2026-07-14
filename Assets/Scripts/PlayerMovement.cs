using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed;
    public float walkSpeed;
    public float sprintSpeed;
    public float groundDrag;
    public float wallRunSpeed;

    public Transform orientaion;

    [Header("Jumping")]
    public float jumpForce;
    public float airMultiplier;

    [Header("Crouching")]
    public float crouchSpeed;
    public float crouchYScale;
    private float startYScale;

    [Header("Slope Physics")]
    public float maxSlopeAngle;
    private RaycastHit slopeHit;
    private bool exitSlope;

    [Header("Rules")]
    bool GROUNDED;
    bool CANJUMP;
    bool AIRJUMP;
    public bool WALLRUNNING;

    [Header("Keybinds")]
    public KeyCode jumpKey = KeyCode.Space;
    public KeyCode sprintKey = KeyCode.LeftShift;
    public KeyCode crouchKey = KeyCode.LeftControl;

    [Header("Ground Check")]
    public float playerHeight;
    public LayerMask whatIsGround;

    float horizontalInput;
    float verticalInput; 

    Vector3 moveDirection;
    Vector3 flatVel;
    Vector3 limitedVel;

    Rigidbody playerRB;

    private MovementState state;
    public enum MovementState
    {
        walking,
        sprinting,
        air,
        wallRunning,
        crouching,
    }

    private void Start()
    {
        playerRB = GetComponent<Rigidbody>();
        playerRB.freezeRotation = true;
        CANJUMP = true;
        AIRJUMP = true;

        startYScale = transform.localScale.y;
    }

    private  void Update()
    {
        GROUNDED = Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f + 0.2f, whatIsGround);

        ResolveInputs();
        SpeedControl();
        StateHandler();

        if (GROUNDED) 
            playerRB.linearDamping = groundDrag;
        else 
            playerRB.linearDamping = 0;

    }

    private void FixedUpdate()
    {
        MovePlayer();
        if (GROUNDED)
        {
            AIRJUMP = true;
        }
    }

    private void ResolveInputs()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical"); 

        if(Input.GetKeyDown(jumpKey) && CANJUMP && (GROUNDED || AIRJUMP == true))
        {   
            Jump();
            if (!GROUNDED) 
            {
                AIRJUMP = false;
            }
        }

        if (Input.GetKeyDown(crouchKey))
        {
            transform.localScale = new Vector3(transform.localScale.x, crouchYScale, transform.localScale.z);
            playerRB.AddForce(Vector3.down * 5f, ForceMode.Impulse);
        }
        else if (Input.GetKeyUp(crouchKey))
        {
            transform.localScale = new Vector3(transform.localScale.x, startYScale, transform.localScale.z);
        }
    }

    private void StateHandler()
    {
        
        if (WALLRUNNING) 
            {
                state = MovementState.wallRunning;
                moveSpeed = wallRunSpeed;
                Debug.Log("WALLRUNNINGGGGG");
            }


        if (GROUNDED)
        {

            if (Input.GetKey(crouchKey))
            {
                state = MovementState.crouching;
                moveSpeed = crouchSpeed;
            }
            else if (Input.GetKey(sprintKey)) 
            {
                state = MovementState.sprinting;
                moveSpeed = sprintSpeed;
            }
            else
            {
                moveSpeed = walkSpeed; 
            }
        }
        else 
        {
            state = MovementState.air;
        }
    }

    private void MovePlayer()
    {
        moveDirection = orientaion.forward * verticalInput + orientaion.right * horizontalInput;

        if (OnSlope() && !exitSlope)
        {
            playerRB.AddForce(GetSlopeMoveDir() * moveSpeed * 20f, ForceMode.Force);

            if (playerRB.linearVelocity.y > 0)
                playerRB.AddForce(Vector3.down * 110f, ForceMode.Force);
        }
        
        if (GROUNDED)
        {
            playerRB.AddForce(moveDirection.normalized * moveSpeed * 10f, ForceMode.Force);
        }
        else if (!GROUNDED)
        {
            playerRB.AddForce(moveDirection.normalized * moveSpeed * 10f * airMultiplier, ForceMode.Force);
        }

        playerRB.useGravity = !OnSlope();
    }   

    private void SpeedControl() 
    {
        flatVel = new Vector3(playerRB.linearVelocity.x, 0f, playerRB.linearVelocity.z);

        if (flatVel.magnitude > moveSpeed) 
        {
            limitedVel = flatVel.normalized * moveSpeed;
            playerRB.linearVelocity = new Vector3(limitedVel.x, playerRB.linearVelocity.y, limitedVel.z);
        }
    }

    private void Jump()
    {
        exitSlope = true;

        playerRB.linearVelocity = new Vector3(playerRB.linearVelocity.x, 0f, playerRB.linearVelocity.z);

        playerRB.AddForce(transform.up * jumpForce, ForceMode.Impulse);
    }

    private bool OnSlope()
    {
        if (Physics.Raycast(transform.position, Vector3.down, out slopeHit, playerHeight * 0.5f + 0.3f))
        {
            float angle = Vector3.Angle(Vector3.up, slopeHit.normal);
            return angle < maxSlopeAngle && angle != 0;
            
        }

        return false;
    }

    private Vector3 GetSlopeMoveDir() 
    {
        return Vector3.ProjectOnPlane(moveDirection, slopeHit.normal).normalized;
    }
}
