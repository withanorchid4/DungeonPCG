using System;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Events;
[System.Serializable]
public enum CollectibleType { KeyFragment, ShoeFragment}
//收集事件
[System.Serializable]
public class CollectibleEvent: UnityEvent<CollectibleType>{}

public class Collector : MonoBehaviour
{
    public Camera mainCamera;
    
    public CollectibleEvent collectibleEvent = new CollectibleEvent();
    public Dictionary<CollectibleType, int> collectedItems = new Dictionary<CollectibleType, int>();
    public List<CollectibleType> collectedTypes = new List<CollectibleType>();

    public void Awake()
    {
        collectibleEvent.RemoveListener(Collect);
        collectibleEvent.AddListener(Collect);

        mainCamera = GetComponentInChildren<Camera>();
    }

    private void OnDestroy()
    {
        collectibleEvent.RemoveListener(Collect);
    }

    public void Collect(CollectibleType type)
    {
        if (collectedItems.ContainsKey(type))
        {
            collectedItems[type]++;
        }
        else
        {
            collectedItems.Add(type, 1);
        }
        collectedTypes.Add(type);
        Debug.Log("Collected " + type);
    }

    public bool CanExit()
    {
        return collectedItems.TryGetValue(CollectibleType.KeyFragment, out var keyNum) ? keyNum >= ObjectPlacementSystem.maxKeyCount - 1 : false;
    }
    
    
}

