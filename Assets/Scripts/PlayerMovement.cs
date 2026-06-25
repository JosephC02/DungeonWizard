using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed;
    public float walkSpeed;
    public float sprintSpeed;

    public float groundDrag;

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
    bool BLOCKING;
    bool CASTING;
    bool GROUNDED;
    bool CANJUMP;
    bool AIRJUMP;

    [Header("Keybinds")]
    public KeyCode jumpKey = KeyCode.Space;
    public KeyCode sprintKey = KeyCode.LeftShift;
    public KeyCode crouchKey = KeyCode.LeftControl;
    public KeyCode primaryFire = KeyCode.Mouse0;
    public KeyCode secondaryFire = KeyCode.Mouse1;
    public KeyCode slot1 = KeyCode.Alpha1;
    public KeyCode slot2 = KeyCode.Alpha2;
    public KeyCode slot3 = KeyCode.Alpha3;
    public KeyCode slot4 = KeyCode.Alpha4;
    public KeyCode slot5 = KeyCode.Alpha5;
    public KeyCode slot6 = KeyCode.Alpha6;
    public KeyCode toggleCastMode = KeyCode.Q;

    [Header("Ground Check")]
    public float playerHeight;
    public LayerMask whatIsGround;

    [Header("Spellcasting")]
    private Array equippedComponents;
    private int selectedSlot;
    private bool castMode;

    public Transform orientaion; 

    float horizontalInput;
    float verticalInput; 

    Vector3 moveDirection;
    Vector3 flatVel;
    Vector3 limitedVel;

    Rigidbody playerRB;

    public MovementState state;

    public enum MovementState
    {
        walking,
        sprinting,
        air,
        crouching,
    }

    private void Start()
    {
        playerRB = GetComponent<Rigidbody>();
        playerRB.freezeRotation = true;
        CANJUMP = true;
        AIRJUMP = true;

        selectedSlot = 1;
        castMode = false;

        startYScale = transform.localScale.y;
    }

    private  void Update()
    {
        GROUNDED = Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f + 0.2f, whatIsGround);
        
        
        MyInput();
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
        CANJUMP = !CASTING && !BLOCKING;
        if (GROUNDED) {
            AIRJUMP = true;
        }
    }

    private void MyInput()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical"); 

        if(Input.GetKeyDown(jumpKey) && CANJUMP && (GROUNDED || AIRJUMP == true))
        {   
            Jump();
            if (!GROUNDED) {
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

        if(Input.GetKeyDown(primaryFire))
        {
            if (castMode) { Debug.Log("Cast mode lmb"); }
            else {  }
        }
        if (Input.GetKeyDown(secondaryFire))
        {
            if (castMode) { Debug.Log("Cast mode rmb"); }
            else{ BLOCKING = true; }
        }
        if (Input.GetKeyUp(secondaryFire))
        {
            if (castMode) { Debug.Log("Cast mode release rmb"); }
            else { BLOCKING = false; }
        }

        if (Input.GetKeyDown(slot1)){ selectedSlot = 1; }
        else if (Input.GetKeyDown(slot2)){ selectedSlot = 2; }
        else if (Input.GetKeyDown(slot3)){ selectedSlot = 3; }
        else if (Input.GetKeyDown(slot4)){ selectedSlot = 4; }
        else if (Input.GetKeyDown(slot5)){ selectedSlot = 5; }
        else if (Input.GetKeyDown(slot6)){ selectedSlot = 6; }

        if (Input.GetKeyDown(toggleCastMode))
        {
            castMode = !castMode;
        }
    }

    private void StateHandler()
    {
        
        if (GROUNDED)
        {
            if (Input.GetKey(crouchKey))
            {
                state = MovementState.crouching;
                moveSpeed = crouchSpeed;
            }
            else if (Input.GetKey(sprintKey) && !BLOCKING) 
            {
                state = MovementState.sprinting;
                moveSpeed = sprintSpeed;
            }
            else
            {
                state = MovementState.walking;
                if (BLOCKING) { moveSpeed = crouchSpeed; }
                else { moveSpeed = walkSpeed; }
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
