using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VectorLuo
{
    public class InputHandler : MonoBehaviour
    {
        public float horizontal;
        public float vertical;
        public float moveAmount;
        public float mouseX;
        public float mouseY;

        private PlayerControls inputActions;
        private CameraHandler cameraHandler;

        private Vector2 movementInput; //从设备读入输入值x,y
        private Vector2 cameraInput;

        private void Awake()
        {
            cameraHandler = CameraHandler.instance;
        }

        private void FixedUpdate()
        {
            float delta = Time.fixedDeltaTime;
        
            if (cameraHandler != null)
            {
                cameraHandler.FollowTarget(delta);
                cameraHandler.HandlerCameraRotation(delta, mouseX, mouseY);
            }
        }

        // private void Update()
        // {
        //     var deltaTime = Time.deltaTime;
        //     if (cameraHandler != null)
        //     {
        //         cameraHandler.FollowTarget(deltaTime);
        //         cameraHandler.HandlerCameraRotation(deltaTime, mouseX, mouseY);
        //     }
        // }
        public void OnEnable()
        {
            if (inputActions == null)
            {
                inputActions = new PlayerControls();
                inputActions.PlayerMovement.Movement.performed += inputActions =>
                {
                    movementInput = inputActions.ReadValue<Vector2>();
                    //Debug.Log("move: " + movementInput);
                };  //Lambda表达式，输入=>输出
                inputActions.PlayerMovement.Camera.performed += i => cameraInput = i.ReadValue<Vector2>();
                
            }
            
            inputActions.Enable();
        }

        public void OnDisable()
        {
            inputActions.Disable();
        }

        public void TickInput(float delta)
        {
            //Debug.Log("Tick!" + delta);
            MoveInput(delta);
        }

        private void MoveInput(float delta)
        {
            horizontal = movementInput.x;
            vertical = movementInput.y;
            moveAmount = Mathf.Clamp01(Mathf.Abs(horizontal) + Mathf.Abs(vertical));
            mouseX = cameraInput.x;
            mouseY = cameraInput.y;
        }
    }
}
