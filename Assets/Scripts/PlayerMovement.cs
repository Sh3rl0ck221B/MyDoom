using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class PlayerMovement : MonoBehaviour
{
    public CharacterController controller;

    //Movement
    public float speed = 12f ;
    private Vector3 move;

    //Jump
    public float jumpHeight = 3f;
    public float gravity = -9.81f;
    private Vector3 velocity;

    //DoubleJump
    public Transform feet;
    public LayerMask ground;
    public float distance = 0.4f;
    public bool doubleJump = false;
    public bool isGrounded;

    //Dash
    public float dashTime = 0.25f;
    public float dashSpeed = 20f;
    private float startCoolDown;

    //Trampoline
    public LayerMask trampoline;

    //Grabbling hook
    public Camera cam;
    public Transform debugHitTransform;
    public Vector3 hookShotPosition;
    private float velocityY;

    //Climb
    private bool canClimb;
    public LayerMask climbable;
    private float speedUpDown = 3.2f;
    
    //StateHandling
    private State state;
    private enum State
    {
        Normal,
        HookshotFlyingPlayer,
        Climbing
    }

    void Awake()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        canClimb = checkEnvironment(climbable);

        if (canClimb)
        {
            state = State.Climbing;
        }
        else
        {
            controller.enabled = true;
        }
        
        
        
        switch (state)
        { 
            case State.Normal:
                HandleCharacterMovement();
                HandleHookshotStart();
                break;
            case State.HookshotFlyingPlayer:
                HandleHookShotMovement();
                break;
            case State.Climbing:
                Climbing();
                HandleHookshotStart();
                break;
        }
    }

    private void Climbing()
    {
        controller.enabled = false;
        if (Input.GetKey("w"))
        {
            transform.position += Vector3.up / speedUpDown;
        }

        if (Input.GetKey("s"))
        {
            transform.position += Vector3.down / speedUpDown;
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            controller.enabled = true;
            StartCoroutine(Dash(false));
        }
    }

    private void HandleCharacterMovement()
    {
        isGrounded = checkEnvironment(ground);

        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }
        
        Move();

        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            Debug.Log("Jump was pressed");
            Jump(jumpHeight);
            doubleJump = true;
        }
        else
        {
            if (Input.GetButtonDown("Jump") && doubleJump)
            {
                Jump(jumpHeight);
                doubleJump = false;
            }
        }

        if (checkEnvironment(trampoline))
        {
            Jump(jumpHeight * 2);
        }

        velocity.y  += gravity * Time.deltaTime;
        
        controller.Move(velocity * Time.deltaTime);

        if (Input.GetMouseButtonDown(0))
        {
            StartCoroutine(Dash(true));
        }
    }
    
    IEnumerator Dash(bool forward)
    {
        float startTime = Time.time;

        while (Time.time < startTime + dashTime)
        {
            if (forward)
            {
                controller.Move( transform.forward * dashSpeed * Time.deltaTime);
            }
            else
            {
                controller.Move( transform.forward * dashSpeed * Time.deltaTime);
            }
            yield return null;
        }
    }
    
    private void Jump(float height)
    {
        velocity.y = Mathf.Sqrt(height * -2f * gravity);
    }
    
    private void Move()
    {
        float xInput = Input.GetAxis("Horizontal");
        float zInput = Input.GetAxis("Vertical");
        
        move = transform.right * xInput + transform.forward * zInput;
        
        controller.Move(move * speed * Time.deltaTime);
    }

    private bool checkEnvironment(int ground)
    {
        return Physics.CheckSphere(feet.position, distance, ground);
    }

    private void HandleHookshotStart()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (Physics.Raycast(cam.transform.position, cam.transform.forward, out RaycastHit raycastHit))
            {
                debugHitTransform.position = raycastHit.point;
                hookShotPosition = raycastHit.point;
                state = State.HookshotFlyingPlayer;
            };
        }
    }

    private void HandleHookShotMovement()
    {
        Vector3 hookshotDir = (hookShotPosition - transform.position).normalized;

        float hookShotSpped = Mathf.Clamp(Vector3.Distance(transform.position, hookShotPosition), 10, 40);
        
        controller.Move(hookshotDir * hookShotSpped * 2f * Time.deltaTime);
        
        float reachedHookshotPosition = 1f;
        if (Vector3.Distance(transform.position, hookShotPosition) < reachedHookshotPosition)
        {
            state = State.Normal;
        }

        if (Input.GetKeyDown(KeyCode.E))
        {
            state = State.Normal;
        }
        
    }
    



}
