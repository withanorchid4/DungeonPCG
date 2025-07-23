using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimatorHandler : MonoBehaviour
{
    public Animator anim;

    private int horizontal;

    private int vertical;
    
    // Start is called before the first frame update
    void Start()
    {
        anim = GetComponent<Animator>();
        horizontal = Animator.StringToHash("Horizontal");
        vertical = Animator.StringToHash("Vertical");
    }

    public void UpdateAnimatorValues(float horizontalInput, float verticalInput)
    {
        #region Vertical
        
        float v = 0;
        if (verticalInput > 0 && verticalInput < 0.55f)
        {
            v = 0.5f;
        }
        else if (verticalInput > 0.55f)
        {
            v = 1;
        }
        else if (verticalInput < 0 && verticalInput > -0.55f)
        {
            v = -0.5f;
        }
        else if (verticalInput < -0.55f)
        {
            v = -1;
        }
        else
        {
            v = 0;
        }

        #endregion
        
        #region Horizontal
        
        float h = 0;

        if (horizontalInput > 0 && horizontalInput < 0.55f)
        {
            h = 0.5f;
        }
        else if (horizontalInput > 0.55f)
        {
            h = 1;
        }
        else if (horizontalInput < 0 && horizontalInput > -0.55f)
        {
            h = -0.5f;
        }
        else if (horizontalInput < -0.55f)
        {
            h = -1;
        }
        else
        {
            h = 0;
        }
        #endregion

        anim.SetFloat(vertical, -1, 0.1f, Time.deltaTime);
        anim.SetFloat(horizontal, h, 0.1f, Time.deltaTime);
        
        // Debug.Log("v: " + anim.GetFloat(vertical));
        // Debug.Log("h: " + anim.GetFloat(horizontal));
    }
    
    // Update is called once per frame
    void Update()
    {
        
        
    }
}