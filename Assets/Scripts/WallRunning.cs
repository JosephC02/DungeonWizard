using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallRunning : MonoBehaviour
{
    [Header("Wallrunning")]
    public LayerMask whatIsWall;
    public LayerMask whatIsGround;
    public float wallJumpUpForce;
    public float wallJumpSideForce;
    public float wallRunForce;
    public float wallRunTime;
    private float wallRunTimer;

    [Header("Input")]
    private float horizontalInput;
    private float verticalInput;
    public KeyCode jumpKey = KeyCode.Space;

    [Header("Detection")]
    public float wallCheckDistance;
    public float minJumpHeight;
    private RaycastHit leftWallHit;
    private RaycastHit rightWallHit;
    private bool wallLeft;
    private bool wallRight;

    [Header("References")]
    public Transform orientation;
    private PlayerMovement pm;
    private Rigidbody playerRB;


    private void Start()
    {
        playerRB = GetComponent<Rigidbody>();
        pm = GetComponent<PlayerMovement>();
    }

    private void Update()
    {
        WallCheck();
        StateMachine();
    }   

    private void FixedUpdate()
    {
        if (pm.WALLRUNNING)
        {
            WallRunningMovement();
        }
    }

    private void WallCheck()
    {
        wallRight = Physics.Raycast(transform.position, orientation.right, out rightWallHit, wallCheckDistance, whatIsWall);
        wallLeft = Physics.Raycast(transform.position, -orientation.right, out leftWallHit, wallCheckDistance, whatIsWall);
        
    }

    private bool AboveGround()
    {
        return !Physics.Raycast(transform.position, Vector3.down, minJumpHeight, whatIsGround);
    }

    private void StateMachine()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");


        if((wallLeft || wallRight) && verticalInput > 0 && AboveGround())
        {
            Debug.Log("WALLRUNNINGGGGG");
            if (!pm.WALLRUNNING)
                StartWallRun();
            
            if (Input.GetKeyDown(jumpKey)) {
                WallJump();
            }
        }

        else
        {
            if (pm.WALLRUNNING)
                StopWallRun();
        }
    }

    private void StartWallRun()
    {
        pm.WALLRUNNING = true;
        Debug.Log("WALLRUNNINGGGGG");
    }

    
    private void WallRunningMovement()
    {
        playerRB.useGravity = false;
        playerRB.linearVelocity = new Vector3(playerRB.linearVelocity.x, 0f, playerRB.linearVelocity.z);

        Vector3 wallNormal = wallRight ? rightWallHit.normal : leftWallHit.normal;

        Vector3 wallForward = Vector3.Cross(wallNormal, transform.up);

        if ((orientation.forward - wallForward).magnitude > (orientation.forward - -wallForward).magnitude)
            wallForward = -wallForward;

        playerRB.AddForce(wallForward * wallRunForce, ForceMode.Force);

        if (!(wallLeft && horizontalInput > 0) && !(wallRight && horizontalInput < 0))
            playerRB.AddForce(-wallNormal * 100, ForceMode.Force);
    }

    private void StopWallRun()
    {
        pm.WALLRUNNING = false;
    }

    private void WallJump()
    {
        Vector3 wallNormal = wallRight ? rightWallHit.normal : leftWallHit.normal;

        Vector3 forceToApply = transform.up * wallJumpUpForce + wallNormal * wallJumpSideForce;

        playerRB.linearVelocity = new Vector3(playerRB.linearVelocity.x, 0f, playerRB.linearVelocity.z);
        playerRB.AddForce(forceToApply, ForceMode.Impulse);
    }



}
