using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float gravity = 3.5f; // grav force  
    public float speed = 25f; // player movement speed
    [Range(0.1f, 1f)] public float crouchSpeedMultplier = 0.5f; // crouch speed
    public float jumpForce = 0.5f; // jumpiung force

    private CharacterController controller;
    private Vector3 motion;
    private float currentSpeed = 0;
    private float velocity = 0;
    private bool crouching = false;
    private bool isGrounded = false;

    // awake is called before the start method
    private void Awake()
    {
        controller = GetComponent<CharacterController>();
    }

    // Start is called before the first frame update
    void Start()
    {
        currentSpeed = speed;

    }

    private void FixedUpdate()
    {
        motion = Vector3.zero;
        isGrounded = controller.isGrounded;

        if (isGrounded == true)
        {
            velocity = -gravity * Time.deltaTime;
        }
        else
        {
            velocity -= gravity * Time.deltaTime;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (isGrounded == true)
        {
            if (crouching == false)
            {
                if (Input.GetKeyDown(KeyCode.Space) == true)
                {
                    velocity = jumpForce;
                }
                else if (Input.GetKeyDown(KeyCode.C) == true)
                {
                    crouching = true;
                    currentSpeed = speed * crouchSpeedMultplier;
                    controller.height = 1;

                }
            }
            if(crouching == true)
            {
                if(Input.GetKeyUp(KeyCode.C) == true)
                {
                    crouching = false;
                    currentSpeed = speed;
                    controller.height = 2;
                }
            }
        }
        ApplyMovement();
    }
    void ApplyMovement()
    {
        float inputX = Input.GetAxisRaw("Vertical") * currentSpeed;
        float inputY = Input.GetAxisRaw("Horizontal") * currentSpeed;
        motion += transform.forward * inputX * Time.deltaTime;
        motion += transform.right * inputY * Time.deltaTime;
        motion. y += velocity * Time.deltaTime; 
        controller.Move(motion);

    }
}
