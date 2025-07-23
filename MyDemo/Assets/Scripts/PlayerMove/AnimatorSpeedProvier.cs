using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimatorSpeedProvier : MonoBehaviour
{
    private Animator anim;

    private float currentSpeed;
    

    [Header("下蹲参数")]
    public float crouchHeightRatio = 0.6f;    // 下蹲时高度比例（1/3）
    public float horizontalOffset = 0.3f;     // 水平前移距离
    public float crouchTransitionSpeed = 5f;  // 下蹲过渡速度
    
    
    private Vector3 originalLocalPosition;    // 原始本地位置
    private Vector3 targetLocalPosition;      // 目标本地位置
    // Start is called before the first frame update

    private PlayerMovement playerMoveMent;
    void Start()
    {
        anim = GetComponent<Animator>();
        originalLocalPosition = Camera.main.transform.localPosition;
        playerMoveMent = GetComponentInParent<PlayerMovement>();
    }

    // Update is called once per frame
    void Update()
    {
        float horizontalInput = Input.GetAxis("Horizontal");
        float verticalInput = Input.GetAxis("Vertical");
        
        if (Input.GetKey(KeyCode.LeftControl) && playerMoveMent.isGround)
        {
            anim.SetBool("IsCrouch", true);
            ToggleCrouch();
        }
        else
        {
            anim.SetBool("IsCrouch", false);
            targetLocalPosition = originalLocalPosition;
        }
        UpdateCameraPosition();
        currentSpeed = Mathf.Sqrt(horizontalInput * horizontalInput + verticalInput * verticalInput);
        //Debug.Log("cur speed : " + currentSpeed);
        
        anim.SetFloat("Speed", currentSpeed);
    }
    
    void ToggleCrouch()
    {
        // 计算目标位置：高度降低到1/3，并沿面朝方向水平偏移
        Vector3 crouchPosition = originalLocalPosition;
        crouchPosition.y *= crouchHeightRatio;
        crouchPosition += Vector3.forward * horizontalOffset; // 假设Player面朝Z轴正方向
        targetLocalPosition = crouchPosition;
    }

    void UpdateCameraPosition()
    {
        Vector3 newPos = Vector3.Lerp(
            Camera.main.transform.localPosition,
            targetLocalPosition,
            Time.deltaTime * crouchTransitionSpeed);
        Camera.main.transform.localPosition = newPos;
    }
    
    
}
