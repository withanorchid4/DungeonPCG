using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;
using Quaternion = UnityEngine.Quaternion;
using Vector3 = UnityEngine.Vector3;

namespace VectorLuo
{
    public class PlayerLocomotions : MonoBehaviour
    {
        private Transform cameraObject;

        private InputHandler inputHandler;

        private Vector3 moveDirection;

        [HideInInspector] public Transform myTransform;
        [HideInInspector] public AnimatorHandler animatorHandler;

        public new Rigidbody rigidbody;

        public GameObject normalCamera;

        [Header("Stats")] [SerializeField] private float movementSpeed = 5;

        [SerializeField] private float rotationSpeed = 10;
        // Start is called before the first frame update
        public void Start()
        {
            rigidbody = GetComponent<Rigidbody>();
            inputHandler = GetComponent<InputHandler>();
            animatorHandler = GetComponentInChildren<AnimatorHandler>();
            cameraObject = Camera.main.transform;
            myTransform = transform;
            animatorHandler.Initialize();

        }


        public void Update()
        {
            //Debug.Log("update");
            float delta = Time.deltaTime;
            
            inputHandler.TickInput(delta);

            moveDirection = cameraObject.forward * inputHandler.vertical;
            moveDirection += cameraObject.right * inputHandler.horizontal;

            float speed = movementSpeed;
            moveDirection *= speed;

            Vector3 projectedVelocity = Vector3.ProjectOnPlane(moveDirection, normalVector);
            rigidbody.velocity = projectedVelocity;
            
            animatorHandler.UpdateAnimatorValues(inputHandler.moveAmount, 0);
            
            if (animatorHandler.canRotate)
            {
                HandleRotation(delta);
            }

        }
        #region Movement

        private Vector3 normalVector = Vector3.up;
        private Vector3 targetPosition;
        
        private void HandleRotation(float delta) //处理的是人物的旋转，得到人物移动后的rotation
        {
            Vector3 targetDir = Vector3.zero;
            float moveOverride = inputHandler.moveAmount;

            targetDir = cameraObject.forward * inputHandler.vertical; //随着相机
            targetDir += cameraObject.right * inputHandler.horizontal;  //前移之后右移，累积矢量和，得到人物朝向
            
            targetDir.Normalize();
            targetDir.y = 0;

            if (targetDir == Vector3.zero)
            {
                targetDir = myTransform.forward;
            }

            float rs = rotationSpeed;

            Quaternion tr = Quaternion.LookRotation(targetDir);  //朝向转四元数
            Quaternion targetRotation = Quaternion.Slerp(myTransform.rotation, tr, rs * delta);  //插值

            myTransform.rotation = targetRotation;


        }
        
        
        #endregion


    }
}
