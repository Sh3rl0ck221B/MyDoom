using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;
using UnityEngine.Serialization;

public class PlayerMovement : MonoBehaviourPun, IPunObservable
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

    //Trampoline
    public LayerMask trampoline;

    //Grabbling hook
    public Camera cam;
    private Vector3 hookShotPosition;
    public Transform debugHitPosition;

    //Climb
    private bool canClimb;
    public LayerMask climbable;
    private float speedUpDown = 3.2f;
    private bool isJumping;
    
    //Visibility
    public GameObject healthbar;
    
    //Shooting
    private float health = 1f;
    public GameObject bulletPrefab;
    
    //Bomb
    public GameObject bombPrefab;
    
    //Respawn
    private GameManager manager;

    
    //StateHandling
    private State state;
    private enum State
    {
        Normal,
        HookshotFlyingPlayer,
        Climbing
    }

    void Start()
    {

        manager = GameObject.Find("GameManger").GetComponent<GameManager>();
        
        
        if (photonView.IsMine)
        {
            
        }
        else
        {
            cam.enabled = false;
            cam.gameObject.GetComponent<AudioListener>().enabled = false;
            gameObject.GetComponent<PlayerMovement>().enabled = false;
            gameObject.GetComponent<MouseLook>().enabled = false;
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.K))
        {
            manager.Respawn();
            PhotonNetwork.Destroy(gameObject);
        }
        
        switch (state)
        { 
            case State.Normal:
                CharacterMovement();
                StartGrapplingGun();
                StartClimbing();
                Shoot();
                Bomb();
                break;
            case State.HookshotFlyingPlayer:
                HandleHookShotMovement();
                Shoot();
                break;
            case State.Climbing:
                Climbing();
                StartGrapplingGun();
                Shoot();
                break;
        }
    }

    private void Bomb()
    {
        if (Input.GetKeyDown(KeyCode.B))
        {
            GameObject bomb = Instantiate(bombPrefab, cam.transform.position, Quaternion.identity);
            Rigidbody rigidbody = bomb.GetComponent<Rigidbody>();
            rigidbody.AddForce(transform.forward * 1000);
        }
    }
    
    private void StartGrapplingGun()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (Physics.Raycast(cam.transform.position, cam.transform.forward, out RaycastHit raycastHit))
            {
                hookShotPosition = raycastHit.point;
                state = State.HookshotFlyingPlayer;
            };
        }
    }
    
    private void HandleHookShotMovement()
    {
        Vector3 direction = (hookShotPosition - transform.position).normalized;
        float speed = Mathf.Clamp(Vector3.Distance(transform.position, hookShotPosition), 10, 40);
        
        controller.Move(direction * speed * 2f * Time.deltaTime);

        if (Vector3.Distance(transform.position, hookShotPosition) < 1)
        {
            state = State.Normal;
        }
    }
    
    private bool checkEnvironment(int ground)
    {
        return Physics.CheckSphere(feet.position, distance, ground);
    }
    
  
    
    private void CharacterMovement()
    {
        isGrounded = checkEnvironment(ground);
        
        if (isGrounded && velocity.y < 0)
        {
            velocity = Vector3.zero;
            velocity.y = -2f;
        }
        
        Move();

        if (Input.GetButtonDown("Jump") && isGrounded)
        {
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

        velocity.y += gravity * Time.deltaTime;
        
        controller.Move(velocity * Time.deltaTime);

        if (Input.GetMouseButtonDown(0))
        {
            StartCoroutine(Dash());
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

    private void Shoot()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            GameObject bullet = PhotonNetwork.Instantiate(bulletPrefab.name, cam.transform.position + cam.transform.forward, Quaternion.identity,0);
            bullet.GetComponent<Rigidbody>().AddForce(cam.transform.forward * 150, ForceMode.Impulse);
        }
    }
    
    void updateHealthBar()
    {
        Vector3 oldScale = healthbar.transform.localScale;
        healthbar.transform.localScale = new Vector3(health, oldScale.y, oldScale.z);
    }
    
    public void hit()
    {
        if (!photonView.IsMine)
        {
            return;
        }
        
        health -= 0.2f;
        if (health <= 0)
        {
            manager.Respawn();
            PhotonNetwork.Destroy(photonView.gameObject);
            health = 0f;
            
        }
        updateHealthBar();
    }
    
 
    
    
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(health);
        }
        else
        {
            health = (float) stream.ReceiveNext();
            updateHealthBar();
        }
    }

    private void StartClimbing()
    {
        canClimb = checkEnvironment(climbable);

        if (canClimb)
        {
            state = State.Climbing;
        }
    }

    private void Climbing()
    {
        canClimb = checkEnvironment(climbable);
        
        if (canClimb)
        {
            isJumping = false;
            if (Input.GetKey("w"))
            {
                transform.position += Vector3.up / speedUpDown;
            }

            if (Input.GetKey("s"))
            {
                transform.position += Vector3.down / speedUpDown;
            }
            
            if (Input.GetButtonDown("Jump"))
            {
                isJumping = true;
            }
        }
        else
        {
            if (state != State.HookshotFlyingPlayer)
            {
                state = State.Normal;
            }
        }
        
        if(!checkEnvironment(ground) && isJumping)
        {
            velocity = -transform.forward * Mathf.Sqrt(5 * -2f * gravity);
            controller.Move(velocity * Time.deltaTime);
        }

        if (checkEnvironment(ground))
        {
            isJumping = false;
        }
    }

  
    
    IEnumerator Dash()
    {
        float startTime = Time.time;

        while (Time.time < startTime + dashTime)
        {
            controller.Move( transform.forward * dashSpeed * Time.deltaTime);
            
            yield return null;
        }
    }
    
    
   

   

    

   
    



}
