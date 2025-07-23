
using System;
using UnityEngine;

public class DoorControl : MonoBehaviour
{
    public GameObject door;

    private bool willOpen;

    public float targetAngle = 0f; // 目标角度
    public float openAngle = 90f; // 打开时的角度
    public float closeAngle = 0f;  // 关闭时的角度
    public float openSpeed = 200f; // 每秒旋转速度（度/秒）
    
    private float speed;

    private bool needKey;

    private bool isOpen;

    public bool playerInRange;

    public void OnValidate()
    {
        //Debug.Log("Door Enable!!");
        var smallDoor = FindSmallDoorInChildren(transform.gameObject);
        if (smallDoor == null)
        {
            Debug.LogError("出错，在这个门节点下没有smalldoor:" + transform.name);
        }

        if (transform.gameObject.name.Contains("open"))
        {
            isOpen = true;
        }
        else
        {
            isOpen = false;
        }

        door = smallDoor;
        targetAngle = door.transform.localRotation.eulerAngles.y;
    }

    public void Start()
    {
        Debug.Log("Start!!");
        var smallDoor = FindSmallDoorInChildren(transform.gameObject);
        if (smallDoor == null)
        {
            Debug.LogError("出错，在这个门节点下没有smalldoor:" + transform.name);
        }
        
        if (transform.gameObject.name.Contains("open"))
        {
            isOpen = true;
        }
        else
        {
            isOpen = false;
        }


        door = smallDoor;
    }


    public void Update()
    {
        if (playerInRange && Input.GetKeyDown(KeyCode.E))
        {
            if (transform.gameObject.name.Contains("Exit"))
            {
                if(!isOpen)
                {
                    Open();
                    isOpen = true;
                }
            }
            else
            {
                if (isOpen)
                {
                    Close();
                    isOpen = false;
                }
                else
                {
                    Open();
                    isOpen = true;
                }
            }
        }

        float currentY = door.transform.localRotation.eulerAngles.y;
        float newY = Mathf.MoveTowardsAngle(currentY, targetAngle, openSpeed * Time.deltaTime);
        door.transform.localRotation = Quaternion.Euler(0, newY, 0);

        if (transform.gameObject.name.Contains("Exit") &&
            Mathf.Approximately(door.transform.localRotation.eulerAngles.y, openAngle))
        {
            GameManager.Instance.GameOver(true);
        }
    }


    public void Open()
    {
        targetAngle = openAngle;
    }

    public void Close()
    {
        targetAngle = closeAngle;
    }

    private GameObject FindSmallDoorInChildren(GameObject root)
    {
        if (root.name == "small_door")
        {
            return root;
        }
        var trans = root.transform;
        foreach (Transform child in trans)
        {
            GameObject res = FindSmallDoorInChildren(child.gameObject);
            if (res != null)
                return res;
        }

        return null;
    }
}