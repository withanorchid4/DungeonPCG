using UnityEngine;

public class KeyItem : MonoBehaviour //绑定在交互物上
{
    public CollectibleType type;

    public bool playerInRange = false;
    
    public Collector collector;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            collector = other.gameObject.GetComponent<Collector>();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            collector = null;
        }
    }

    private void Update()
    {
        if (playerInRange && Input.GetKeyDown(KeyCode.E))
        {
            if(IsObjectNearView())
            {
                collector.collectibleEvent.Invoke(type);
                gameObject.SetActive(false);
            }
        }
    }

    private bool IsObjectNearView()
    {
        Camera playerCamera = collector.mainCamera;
        Vector3 objectPosition = transform.position;
        Vector3 cameraPosition = playerCamera.transform.position;
        Vector3 direction = objectPosition - cameraPosition;

        var cameraForward = playerCamera.transform.forward;
        
        var angle = Vector3.Angle(direction, cameraForward);
        
        if(angle > 30f)
            return false;

        var dist = direction.magnitude;
        if(dist > 3f)
            return false;
        
        return true;
    }
}