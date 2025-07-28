using UnityEngine;
using System.Collections.Generic;

public class RuntimeInstanceTester : MonoBehaviour
{
    [SerializeField]
    public GameObject prefab;
    [SerializeField]
    public int instanceCount = 4;
    
    [SerializeField]
    public BookShelfInstancer bookShelfInstancer;
    
    void Start()
    {
        bookShelfInstancer = FindObjectOfType<BookShelfInstancer>();
        if (bookShelfInstancer == null)
        {
            Debug.LogError("No BookShelfInstancer found");
            return;
        }
        for (int i = 0; i < instanceCount; i++)
        {
            var instanceLocalPosition = new Vector3(8 + i * 2, 0, 0);
            bookShelfInstancer.RegisterInstance(prefab, instanceLocalPosition, Quaternion.identity, Vector3.one);
        }
    }
    
    
}
