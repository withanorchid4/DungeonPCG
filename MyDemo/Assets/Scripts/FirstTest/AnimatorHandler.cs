using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VectorLuo
{
    public class AnimatorHandler : MonoBehaviour
    {
        public Animator anim;

        private int vertical;

        private int horizonal;

        public bool canRotate;

        public void Initialize()
        {
            anim = GetComponent<Animator>();
            vertical = Animator.StringToHash("Vertical");
            horizonal = Animator.StringToHash("Horizonal");
        }

        public void UpdateAnimatorValues(float verticalMovement, float horizontalMovement)
        {
            #region Vertical

            float v = 0;
            if (verticalMovement > 0 && verticalMovement < 0.55f)
            {
                v = 0.5f;
            }
            else if (verticalMovement > 0.55f)
            {
                v = 1;
            }
            else if (verticalMovement < 0 && verticalMovement > -0.55f)
            {
                v = -0.5f;
            }
            else if (verticalMovement < -0.55f)
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

            if (horizontalMovement > 0 && horizontalMovement < 0.55f)
            {
                h = 0.5f;
            }
            else if (horizontalMovement > 0.55f)
            {
                h = 1;
            }
            else if (horizontalMovement < 0 && horizontalMovement > -0.55f)
            {
                h = -0.5f;
            }
            else if (horizontalMovement < -0.55f)
            {
                h = -1;
            }
            else
            {
                h = 0;
            }
            #endregion
            
            Debug.Log("v: " + v + ", h: " + h);
            anim.SetFloat(vertical, v, 0.1f, Time.deltaTime); //0.1f是lerp速度
            anim.SetFloat(horizonal, h, 0.1f, Time.deltaTime);
            //怎么看实际设置多少值
            Debug.Log("v: " + anim.GetFloat(vertical));
            Debug.Log("h: " + anim.GetFloat(horizonal));
        }

        public void CanRotate()
        {
            canRotate = true;
        }

        public void StopRotate()
        {
            canRotate = false;
        }
        
        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }
    }

}
