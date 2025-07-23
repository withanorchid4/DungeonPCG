using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float speed = 4f;
    public float gravity = -9.8f;
    public float jumpHeight = 2f;
    public float rotationSpeed = 10f;

    public float horizontalInput;
    public float verticalInput;
    public Vector3 velocity;

    public CharacterController characterController;

    public Transform groundCheckPoint;
    public float checkRadius = 0.4f;
    public LayerMask groundLayer;

    public bool isGround;

    private AnimatorHandler animatorHandler;
    
    // Start is called before the first frame update
    void Start()
    {
        characterController = GetComponent<CharacterController>();
        animatorHandler = GetComponentInChildren<AnimatorHandler>();
    }

    // Update is called once per frame
    void Update()
    {
        float delta = Time.deltaTime;

        isGround = Physics.CheckSphere(groundCheckPoint.position, checkRadius, groundLayer.value);

        //Debug.Log($"IsGround: {isGround}"); // 调试日志
        
        if(isGround && velocity.y < 0)
        {
            velocity.y = -2f;
            if (Input.GetKey(KeyCode.LeftControl))
            {
                speed = 1f;
            }
            else
            {
                speed = 4f;
            }
        }
        
        horizontalInput = Input.GetAxis("Horizontal");
        verticalInput = Input.GetAxis("Vertical");
        //animatorHandler.UpdateAnimatorValues(horizontalInput, verticalInput);

        Vector3 movement = transform.right * horizontalInput + transform.forward * verticalInput;

        characterController.Move(movement * speed * delta);
        
        if (Input.GetButtonDown("Jump") && isGround)
        {
            if (Input.GetKey(KeyCode.LeftControl))
            {
                //说明在下蹲，不能跳跃
                return;
            }
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }

        velocity.y += gravity * delta;

        characterController.Move(velocity * delta);
    }
}
